using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Drawing;
using System.Threading;
using static OpenTK.Vector3;
using static template.GlobalLib;

namespace template
{
    public class Raytracer : Tracer
    {
        public Raytracer(int numThreads, int height = 512, int width = 512) : base(numThreads, height, width)
        {
            MakeScene();
        }

        private void MakeScene()
        {
            var texture1 = new Texture("../../assets/checkers.png");
            var texture2 = new Texture("../../assets/globe.jpg");
            var texture3 = new Texture("../../assets/triangle.jpg");
            var texture4 = new Texture("../../assets/capsule0.jpg");
            var texture5 = new Texture("../../assets/kunai.png");
            var texture6 = new Texture("../../assets/fractal.jpg");
            var objFile = "../../assets/basic_box.obj";

            Lights.Add(new Light(new Vector3(0, 0, 0), new Vector3(100, 100, 100)));

            //Lights.Add(new Light(new Vector3(0, 0, -50), new Vector3(10000, 10000, 10000)));
            //Lights.Add(new Light(new Vector3(-30, 0, -10), new Vector3(10000, 10000, 10000)));

            Scene.AddRange(  ReadObj(objFile, Matrix4.CreateScale(0.1f) * Matrix4.CreateTranslation(new Vector3(0, -1, 0)), texture6));

            return;
            Scene.Add(new Sphere(new Vector3(3, 0, -10), 1) { Material = new Material { color = new Vector3(1, 0, 0), Reflectivity = 0f } });
            Scene.Add(new Sphere(new Vector3(-3, -2, -10), 1) { Material = new Material { color = new Vector3(0, 1, 0), Reflectivity = 0f } });
            Scene.Add(new Sphere(new Vector3(0, 0, -10), 1) { Material = new Material { color = new Vector3(0, 0, 1), Reflectivity = 0f } });


            Scene.Add(new Plane(new Vector3(0, -2, -20), new Vector3(0, 1, 0)) { Material = new Material { color = new Vector3(1, 1, 1), Texture = texture1 } });

            

            Scene.Add(new Sphere(new Vector3(0, 0, -5), 1) { Material = new Material { color = new Vector3(0f, 0, 0), RefractionIndex = 1.333f } });

            Scene.Add(new Vertex(new Vector3(-1, 2, -5), new Vector3(1, 2, -5), new Vector3(0, 1, -5)) { Material = new Material { color = new Vector3(1, 0, 0), Reflectivity = 0, Texture = texture3 } });
            Scene.Add(new Vertex(new Vector3(-1, 2, 5), new Vector3(1, 2, 5), new Vector3(0, 1, 5)) { Material = new Material { color = new Vector3(1, 0, 0), Reflectivity = 0, Texture = texture3 } });


            Scene.Add(new Sphere(new Vector3(0, 0, -20), 5) { Material = new Material { color = new Vector3(1, 1, 1), Texture = texture2 } });
        }

        public override void Trace(Surface screen, int threadId, int numthreads)
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
                    Vector3 aaResult = new Vector3();
                    float AAsqrt = (float)Math.Sqrt(AA);
                    for (float aax = 0; aax < AAsqrt; aax++)
                    {
                        for (float aay = 0; aay < AAsqrt; aay++)
                        {
                            Ray ray = new Ray();
                            ray.position = Camera.Position;

                            Vector3 horizontal = Camera.Screen.TopRigth - Camera.Screen.TopLeft;
                            Vector3 vertical = Camera.Screen.BottomLeft - Camera.Screen.TopLeft;
                            Vector3 pixelLocation = Camera.Screen.TopLeft + horizontal / Width * (x + aax * (1f/AAsqrt) - 0.5f) + vertical / Height * (y + aay * ( 1f/AAsqrt) - 0.5f);

                            Matrix4 rotation = Matrix4.CreateRotationX(Camera.XRotation);
                            rotation *= Matrix4.CreateRotationY(Camera.YRotation);
                            Matrix4 translation = Matrix4.CreateTranslation(Camera.Position);

                            pixelLocation = Transform(pixelLocation, rotation);
                            pixelLocation = Transform(pixelLocation, translation);

                            ray.direction = Normalize(pixelLocation - Camera.Position);
                            aaResult += TraceRay(ray, threadId);
                        }
                    }
                    aaResult /= AA;
                    result[x, y] = VecToInt(aaResult);
                }
            }
        }

        protected override Vector3 TraceRay(Ray ray, int threadId, int recursionDepth = 0)
        {
            Vector3 reflectColor = new Vector3();
            Vector3 refractColor = new Vector3();


            if (recursionDepth > 10)
                return new Vector3();
            Intersection nearest = new Intersection { length = float.PositiveInfinity };

            foreach (var primitive in Scene)
            {
                var intersection = primitive.Intersect(ray);
                if (intersection != null && intersection.length < nearest.length)
                    nearest = intersection;
            }

            //TODO add skybox here
            if (nearest.primitive == null)
                return new Vector3();

            var illumination = new Vector3();

            foreach (var light in Lights)
            {

                if (castShadowRay(light, nearest.Position))
                {
                    var distance = (light.Position - nearest.Position).Length;
                    var attenuation = 1f / (distance * distance);
                    var nDotL = Dot(nearest.normal, Normalize(light.Position - nearest.Position));

                    if (nDotL < 0)
                        continue;
                    illumination += nDotL * attenuation * light.Color;
                }

            }
            if (nearest.primitive.Material.Reflectivity != 0)
            {
                reflectColor = reflect(ray, nearest, recursionDepth, threadId);
            }

            if (nearest.primitive.Material.RefractionIndex != 0)
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
                    refractColor = reflect(ray, nearest, recursionDepth, threadId);
                else
                    refractColor = TraceRay(new Ray() { direction = Normalize(snell * ray.direction + (snell * thetaOne - (float)Math.Sqrt(internalReflection)) * primitiveNormal), position = nearest.Position + ray.direction * 0.002f }, threadId, recursionDepth++);
            }

            if (nearest.primitive.Material.Texture != null)
                return nearest.IntersectionColor * (1 - nearest.primitive.Material.Reflectivity) * illumination + reflectColor + refractColor; ;
            return nearest.primitive.Material.color * (1 - nearest.primitive.Material.Reflectivity) * illumination + reflectColor + refractColor;
        }

        private bool castShadowRay(Light light, Vector3 position)
        {
            var distToLight = (light.Position - position).Length;
            foreach (var primitive in Scene)
            {
                var intersection = primitive.Intersect(new Ray { position = position, direction = Normalize(light.Position - position) });
                if (intersection != null && intersection.length < distToLight && intersection.length > 0.0001f)
                    return false;
            }
            return true;
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

    

    public class Light
    {
        public Vector3 Color;
        public Vector3 Position;

        public Light(Vector3 position, Vector3 color)
        {
            Position = position;
            Color = color;
        }
    }


    // TODO
  

    
}
