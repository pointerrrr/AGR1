using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using static OpenTK.Vector3;
using static template.Constants;

namespace template
{
    class Pathtracer
    {
        public Camera Camera;
        public List<Primitive> Scene = new List<Primitive>();

        public int[,] result;
        public int Height, Width;
        public float AspectRatio;
        public Skybox Skydome;

        public Pathtracer(int numThreads, int height = 512, int width = 512)
        {
            Height = height;
            Width = width;
            AspectRatio = width / ((float)height);
            Camera = new Camera(new Vector3(), new Vector3(0, 0, -1), AspectRatio);
            Skydome = new Skybox("../../assets/stpeters_probe.jpg");
            result = new int[Width, Height];

            MakeScene();
        }

        public void Trace(Surface screen, int threadId, int numthreads)
        {
            int sqr = (int)Math.Sqrt(numthreads);
            int fromX = (threadId % sqr) * Width / sqr;
            int toX = ((threadId % sqr) + 1) * Width / sqr;
            int fromY = (threadId / sqr) * Height / sqr;
            int toY = ((threadId / sqr) + 1) * Height / sqr;
            for (int x = fromX; x < toX; x++)
            {
                for (int y = fromY; y < toY; y++)
                {
                    Ray ray = new Ray();
                    ray.position = Camera.Position;

                    Vector3 horizontal = Camera.Screen.TopRigth - Camera.Screen.TopLeft;
                    Vector3 vertical = Camera.Screen.BottomLeft - Camera.Screen.TopLeft;
                    Vector3 pixelLocation = Camera.Screen.TopLeft + horizontal / Width * x + vertical / Height * y;

                    Matrix4 rotation = Matrix4.CreateRotationX(Camera.XRotation);
                    rotation *= Matrix4.CreateRotationY(Camera.YRotation);
                    Matrix4 translation = Matrix4.CreateTranslation(Camera.Position);

                    pixelLocation = Transform(pixelLocation, rotation);
                    pixelLocation = Transform(pixelLocation, translation);

                    ray.direction = Normalize(pixelLocation - Camera.Position);
                    result[x, y] = VecToInt(TraceRay(ray));
                }
            }
        }

        public Vector3 TraceRay(Ray ray, int recursionDepth = 0)
        {
            if (recursionDepth > MaxRecursion)
                return new Vector3();

            var nearest = new Intersection { length = float.PositiveInfinity };


            foreach (var primitive in Scene)
            {
                var intersection = primitive.Intersect(ray);
                if (intersection != null && intersection.length < nearest.length)
                    nearest = intersection;
            }

            if (nearest.primitive == null)
                return new Vector3();

            if (nearest.primitive.Material.IsLight)
                return nearest.primitive.Material.Emittance;

            var newRayDir = DiffuseReflection(nearest.normal);

            var newRay = new Ray { position = nearest.Position, direction = newRayDir };

            var BRDF = nearest.primitive.Material.Albedo / (float)Math.PI;

            var tracedRay = TraceRay(newRay, ++recursionDepth);

            var Ei =  tracedRay * Dot(nearest.normal, newRayDir);

            if ((BRDF.X > 0 || BRDF.Y > 0 || BRDF.Z > 0))// && (Ei.X > 0 || Ei.Y > 0 || Ei.Z > 0))
                ;

            return (float)Math.PI * 2f * BRDF + Ei;
        }

        private Vector3 DiffuseReflection(Vector3 Normal)
        {
            var Random = new Random();
            float z = (float)Random.NextDouble();
            float theta = (float)( Random.NextDouble() * Math.PI * 2 - Math.PI);

            float x = (float) Math.Cos(theta);
            float y = (float) Math.Sin(theta);

            Vector3 result = new Vector3(x, y, z);

            Matrix4 rotation = Matrix4.CreateRotationX(Normal.X);
            rotation *= Matrix4.CreateRotationY(Normal.Y);
            rotation *= Matrix4.CreateRotationZ(Normal.Z);

            return Transform(result, rotation);
        }

        private void MakeScene()
        {
            Scene.Add(new Sphere(new Vector3(3, 0, -10), 1) { Material = new Material { color = new Vector3(1, 0, 0), Reflectivity = 0f, Albedo = new Vector3(0f, 0f, 1f) } });
            Scene.Add(new Sphere(new Vector3(-3, 0, -10), 1) { Material = new Material { color = new Vector3(0, 1, 0), Reflectivity = 0f, Albedo = new Vector3(0f, 01f, 0f) } });
            Scene.Add(new Sphere(new Vector3(0, 0, -10), 1) { Material = new Material { color = new Vector3(0, 0, 1), Reflectivity = 0f, Albedo = new Vector3(1f, 0f, 0f) } });

            Scene.Add(new Sphere(new Vector3(0, 0, 2), 1) { Material = new Material {Emittance = new Vector3(100, 100, 100), Albedo = new Vector3(0.5f, 0.5f, 0.5f), IsLight = true } } );

            Scene.Add(new Plane(new Vector3(0, -2, -20), new Vector3(0, 1, 0)) { Material = new Material { color = new Vector3(1, 1, 1), Albedo = new Vector3(1,1,1) } });
        }
    }

    public class PathLight
    {
        public Vector3 Color;
        public Vector3 Position;
        public float Radius;

        public PathLight(Vector3 position, Vector3 color, float radius = 1f)
        {
            Position = position;
            Color = color;
            Radius = radius;
        }

    }
}
