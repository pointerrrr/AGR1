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
    public class Raytracer
    {
        public Camera Camera;
        public List<Primitive> Scene = new List<Primitive>();
        public List<Light> Lights = new List<Light>();
        public Skybox Skydome;
        public int[,] result;
        public int Height, Width;
        public float AspectRatio;

        public Raytracer(int numThreads, int height = 512, int width = 512)
        {
            Height = height;
            Width = width;
            AspectRatio = width / ((float) height);
            Camera = new Camera(new Vector3(), new Vector3(0,0,-1), AspectRatio);
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
                    Vector3 pixelLocation = Camera.Screen.TopLeft + horizontal / Width  * x + vertical / Height * y;

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

        private Vector3 TraceRay(Ray ray, int recursionDepth = 0)
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
                return Skybox(ray);

            var illumination = new Vector3();

            foreach (var light in Lights)
            {

                if (castShadowRay(light, nearest.Position + nearest.normal * 0.001f))
                {
                    var distance = (light.Position - nearest.Position).Length;
                    var attenuation = 1f /  (distance * distance);
                    var nDotL = Dot(nearest.normal, Normalize(light.Position - nearest.Position));

                    if (nDotL < 0)
                        continue;
                    illumination += nDotL * attenuation * light.Color;
                }

            }
            if (nearest.primitive.Material.Reflectivity != 0)
            {
                reflectColor = reflect(ray, nearest, recursionDepth);
            }

            if(nearest.primitive.Material.RefractionIndex != 0)
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
                    refractColor = reflect(ray, nearest, recursionDepth);
                else
                    refractColor = TraceRay(new Ray() { direction = Normalize(snell * ray.direction + (snell * thetaOne - (float)Math.Sqrt(internalReflection)) * primitiveNormal), position = nearest.Position + ray.direction * 0.002f }, recursionDepth++);
            }


            return nearest.primitive.Material.color * (1 - nearest.primitive.Material.Reflectivity) * illumination + reflectColor + refractColor;
        }

        

        private Vector3 reflect(Ray ray, Intersection intersection, int recursionDepth)
        {
            var reflectionRay = Normalize(ReflectRay(ray.direction, intersection.normal));
            Ray reflection = new Ray() { direction = reflectionRay, position = intersection.Position + reflectionRay * 0.0001f };
            return TraceRay(reflection, ++recursionDepth) * intersection.primitive.Material.Reflectivity;
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
        
        

        private void MakeScene()
        {
            Scene.Add(new Sphere(new Vector3(3, 0, -10), 1) { Material = new Material { color = new Vector3(1, 0, 0), Reflectivity = 0f } });
            Scene.Add(new Sphere(new Vector3(-3, -2, -10), 1) { Material = new Material { color = new Vector3(0, 1, 0), Reflectivity = 0f } });
            Scene.Add(new Sphere(new Vector3(0, 0, -10), 1) { Material = new Material { color = new Vector3(0, 0, 1), Reflectivity = 0f } });

            
            Scene.Add(new Plane(new Vector3(0, -2, -20), new Vector3(0, 1, 0)) { Material = new Material { color = new Vector3(1,1,1), } });

            Lights.Add(new Light(new Vector3(0, 0, 0), new Vector3(100, 100, 100)));

            Scene.Add(new Sphere(new Vector3(0, 0, -5), 1) { Material = new Material { color = new Vector3(0f, 0, 0), RefractionIndex = 1.333f } });

            Scene.Add(new Vertex(new Vector3(-1, 2, -5), new Vector3(1, 2, -5), new Vector3(0, 1, -5)) { Material = new Material { color = new Vector3(1, 0, 0), Reflectivity = 0 } });
            Scene.Add(new Vertex(new Vector3(-1, 2, 5), new Vector3(1, 2, 5), new Vector3(0, 1, 5)) { Material = new Material { color = new Vector3(1, 0, 0), Reflectivity = 0 } });
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

    public class Ray
    {
        public float length = float.PositiveInfinity;

        public Vector3 direction;
        public Vector3 position;
        public Vector3 color;
    }

    public class Intersection
    {
        public Ray ray;
        public Primitive primitive;
        public Vector3 normal;
        public Vector3 Position;

        public float length;

    }

    public class Material
    {
        public Vector3 color;
        public Vector3 Emittance;
        public bool IsLight;
        public float Reflectivity;
        public float RefractionIndex;
        public Vector3 Albedo;
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
  

    // skybox for when rays hit nothing
    public class Skybox
    {
        public Texture Texture { get; set; }

        // initialize texture via string
        public Skybox(string path)
        {
            Texture = new Texture(path);
        }
    }

    // texture for all primitives
    public class Texture
    {
        // 2d-array of vector3's to make the image accesible in multiple threads
        public Vector3[,] Image { get; set; }
        // bitmap used for final texture (changes the Image array when the bitmap is changed as well)
        public Bitmap Bitmap
        {
            get { return Bitmap; }
            set
            {
                Image = new Vector3[value.Width, value.Height];
                for (int i = 0; i < value.Width; i++)
                    for (int j = 0; j < value.Height; j++)
                    {
                        Color color = value.GetPixel(i, j);
                        Image[i, j] = new Vector3((float)color.R / 255, (float)color.G / 255, (float)color.B / 255);
                    }
            }
        }

        // initialize texture via string
        public Texture(string path)
        {
            Bitmap image = new Bitmap(path);
            Bitmap = image;
        }
    }
}
