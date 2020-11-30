using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using static OpenTK.Vector3;
using static template.GlobalLib;

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
        Random[] random;

        public Pathtracer(int numThreads, int height = 512, int width = 512)
        {
            random = new Random[numThreads];
            for (int i = 0; i < numThreads; i++)
                random[i] = new Random();
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

                    float samples = 10;

                    Vector3 jemoeder = new Vector3();

                    for(int i = 0; i < samples; i++)
                    {
                        jemoeder  += TraceRay(ray, threadId, 0);
                    }

                    jemoeder /= samples;

                    result[x, y] = VecToInt(jemoeder);
                }
            }
        }

        public Vector3 TraceRay(Ray ray, int threadId, int recursionDepth = 0)
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

            if (nearest.primitive.Material.Reflectivity > 0)
            {
                float chance = (float) random[threadId].NextDouble();
                if (chance < nearest.primitive.Material.Reflectivity)
                {
                    var reflectRay = new Ray { direction = ReflectRay(ray.direction, nearest.normal), position = nearest.Position };
                    return TraceRay(reflectRay, threadId, ++recursionDepth) * nearest.primitive.Material.color;
                }
            }

            if(nearest.primitive.Material.RefractionIndex != 0 )
            {
                float refractionCurrentMaterial = 1.00027717f;
                float refractionIndexNextMaterial = nearest.primitive.Material.RefractionIndex;
                Vector3 primitiveNormal = nearest.normal;

                float thetaOne = Math.Min(1, Math.Max(Dot(ray.direction, primitiveNormal), -1));

                if (thetaOne < 0)
                    thetaOne *= -1;
                else
                {
                    primitiveNormal *= -1;
                    float temp = refractionCurrentMaterial;
                    refractionCurrentMaterial = refractionIndexNextMaterial;
                    refractionIndexNextMaterial = temp;
                }

                float snell = refractionCurrentMaterial / refractionIndexNextMaterial;

                float internalReflection = 1 - snell * snell * (1 - thetaOne * thetaOne);

                if (internalReflection < 0)
                    return reflect(ray, nearest, threadId, recursionDepth);
                else
                    return TraceRay(new Ray() { direction = Normalize(snell * ray.direction + (snell * thetaOne - (float)Math.Sqrt(internalReflection)) * primitiveNormal), position = nearest.Position + ray.direction * 0.002f }, threadId, recursionDepth++);
            }

            var newDirection = DiffuseReflection(nearest.normal, threadId);

            var newRay = new Ray { direction = newDirection, position = nearest.Position + nearest.normal * 0.001f, length = float.PositiveInfinity };

            float p = (float)( 1f / (2f * Math.PI));

            float cos_theta = Dot(newDirection, nearest.normal);

            Vector3 BRDF = nearest.primitive.Material.color / (float)Math.PI;

            Vector3 incoming = TraceRay(newRay, threadId, ++recursionDepth);

            return (BRDF * incoming * cos_theta / p);

            //return (float)Math.PI * 2f * BRDF + Ei;
        }

        private Vector3 reflect(Ray ray, Intersection intersection, int threadId, int recursionDepth)
        {
            var reflectionRay = Normalize(ReflectRay(ray.direction, intersection.normal));
            Ray reflection = new Ray() { direction = reflectionRay, position = intersection.Position + reflectionRay * 0.0001f };
            return TraceRay(reflection, threadId, ++recursionDepth) * intersection.primitive.Material.Reflectivity;
        }

        private Vector3 DiffuseReflection(Vector3 Normal, int threadId)
        {
            /*
            var Random = new Random();
            float z = (float)Random.NextDouble();
            float theta = (float)( Random.NextDouble() * Math.PI * 2 - Math.PI);

            float x = (float) Math.Cos(theta);
            float y = (float) Math.Sin(theta);

            Vector3 result = new Vector3(x, y, z);

            Matrix4 rotation = Matrix4.CreateRotationX(Normal.X);
            rotation *= Matrix4.CreateRotationY(Normal.Y);
            rotation *= Matrix4.CreateRotationZ(Normal.Z);

            return Normalize(Transform(result, rotation));
            */

            Vector3 b3 = Normalize(Normal);
            Vector3 different = Math.Abs(b3.X) < 0.5f ? new Vector3(1, 0, 0) : new Vector3(0, 1, 0);

            Vector3 b1 = Normalize(Cross(b3, different));
            Vector3 b2 = Cross(b1, b3);

            float z = (float)random[threadId].NextDouble();
            float r = (float)Math.Sqrt(1f - z * z);

            float theta = (float)( random[threadId].NextDouble() * Math.PI * 2f - Math.PI);

            float x = (float)(r * Math.Cos(theta));
            float y = (float)(r * Math.Sin(theta));

            return Normalize(x * b1 + y * b2 + z * b3);
        }

        private void MakeScene()
        {
            Scene.Add(new Sphere(new Vector3(3, 0, -10), 1) { Material = new Material { color = new Vector3(1, 1, 1), Reflectivity = 1f, Albedo = new Vector3(0f, 0f, 1f) } });
            Scene.Add(new Sphere(new Vector3(-3, 0, -10), 1) { Material = new Material { color = new Vector3(0, 1, 0), Reflectivity = 0f, Albedo = new Vector3(0f, 01f, 0f) } });
            Scene.Add(new Sphere(new Vector3(0, 0, -10), 1) { Material = new Material { color = new Vector3(0, 0, 1), Reflectivity = 0f, Albedo = new Vector3(1f, 0f, 0f), RefractionIndex = 1.5f } });

            Scene.Add(new Sphere(new Vector3(0, 5, -10), 2) { Material = new Material {Emittance = new Vector3(15, 15, 15), Albedo = new Vector3(0.5f, 0.5f, 0.5f), IsLight = true } } );

            Scene.Add(new Plane(new Vector3(0, -20, -20), new Vector3(0, 1, 0)) { Material = new Material { color = new Vector3(1, 1, 1), Albedo = new Vector3(1,1,1) } });
        }

        private Vector3 Skybox(Ray ray)
        {
            // flipping the image
            Vector3 d = -ray.direction;
            float r = (float)((1d / Math.PI) * Math.Acos(d.Z) / Math.Sqrt(d.X * d.X + d.Y * d.Y));
            // find the coordinates
            float u = r * d.X + 1;
            float v = r * d.Y + 1;
            // scale the coordinates to image size
            int iu = (int)(u * Skydome.Texture.Image.GetLength(0) / 2);
            int iv = (int)(v * Skydome.Texture.Image.GetLength(1) / 2);
            // fail safe to make sure we're inside of the image coordinates
            if (iu >= Skydome.Texture.Image.GetLength(0) || iu < 0)
                iu = 0;
            if (iv >= Skydome.Texture.Image.GetLength(1) || iv < 0)
                iv = 0;
            // return the color
            return Skydome.Texture.Image[iu, iv];
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
