using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using static template.GlobalLib;
using static OpenTK.Vector3;
using System.IO;

namespace template
{
    public abstract class Tracer
    {
        public Camera Camera;
        public List<Primitive> Scene = new List<Primitive>();
        public List<Light> Lights = new List<Light>();
        public Skybox Skydome;
        public int[,] result;
        public int Height, Width;
        public float AspectRatio;
        public int SamplesTaken;

        public Tracer(int numThreads, int height = 512, int width = 512)
        {
            Height = height;
            Width = width;
            AspectRatio = width / ((float)height);
            Camera = new Camera(new Vector3(), new Vector3(0, 0, -1), AspectRatio);
            Skydome = new Skybox("../../assets/skydome.png");
            result = new int[Width, Height];
        }

        public abstract void Trace(Surface screen, int threadId, int numthreads);

        protected abstract Vector3 TraceRay(Ray ray, int threadId, int recursionDepth = 0);

        protected Vector3 reflect(Ray ray, Intersection intersection, int recursionDepth, int threadId)
        {
            var reflectionRay = Normalize(ReflectRay(ray.direction, intersection.normal));
            Ray reflection = new Ray() { direction = reflectionRay, position = intersection.Position + reflectionRay * 0.0001f };
            return TraceRay(reflection, threadId, ++recursionDepth) * intersection.primitive.Material.Reflectivity;
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

        public List<Primitive> ReadObj(string path)
        {
            var vertices = new List<Vector3>();
            var triangles = new List<Primitive>();
            using (StreamReader streamReader = new StreamReader(path))
            {
                string line;
                

                while((line = streamReader.ReadLine()) != null)
                {
                    line = line.Trim();
                    line = line.Replace("  ", " ");
                    var par = line.Split(' ');

                    switch (par[0])
                    {
                        case "v":
                            vertices.Add(new Vector3(float.Parse(par[1]), float.Parse(par[2]), float.Parse(par[3])));
                            break;
                        case "f":
                            int index1 = int.Parse(par[1].Split('/')[0]) - 1;
                            int index2 = int.Parse(par[2].Split('/')[0]) - 1;
                            int index3 = int.Parse(par[3].Split('/')[0]) - 1;
                            triangles.Add(new Vertex(vertices[index1], vertices[index2], vertices[index3]) { Material = new Material { color = new Vector3(1,0,0)} });
                            break;
                        default:
                            break;
                    }
                }
                streamReader.Close();

            }

            return triangles;
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

        public Vector3 IntersectionColor;

    }
}
