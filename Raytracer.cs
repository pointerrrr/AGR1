using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Drawing;

namespace template
{
    public class Raytracer
    {
        public Camera Camera;
        public List<Primitive> Scene = new List<Primitive>();

        public Raytracer()
        {
            Camera = new Camera();
            Camera.Screen = new Screen(new Vector3(-1, 1, -1), new Vector3(1, 1, -1), new Vector3(-1, -1, -1), new Vector3(1, -1, -1));

            MakeScene();
        }

        public Bitmap Trace()
        {
            Bitmap result = new Bitmap(512, 512);
            for (int i = 0; i < 512; i++)
            {
                for (int j = 0; j < 512; j++)
                {
                    Ray ray = new Ray();
                    ray.position = Camera.Position;

                    ray.direction = new Vector3((i - 256f) / 512, (j - 256f) / 512f, -1) - Camera.Position;
                    ray.direction.Normalize();

                    List<Intersection> intersects = new List<Intersection>();

                    foreach (var primitive in Scene)
                    {
                        var intersection = primitive.Intersect(ray);
                        if (intersection != null)
                            intersects.Add(intersection);
                    }

                    if (intersects.Count == 0)
                        continue;

                    Intersection a = intersects[0];

                    for (int k = 1; k < intersects.Count; k++)
                    {
                        if (intersects[k].length < a.length)
                            a = intersects[k];
                    }

                    result.SetPixel(i, j, VecToRGB(a.primitive.Material.color));

                }
            }
            result.Save("lmao.png");
            return result;
        }

        

        private Color VecToRGB(Vector3 vector)
        {
            int R = vector.X > 1 ? 255 : (int)vector.X * 255;
            int G = vector.Y > 1 ? 255 : (int)vector.Y * 255;
            int B = vector.Z > 1 ? 255 : (int)vector.Z * 255;
            return Color.FromArgb(R, G, B);
        }

        private void MakeScene()
        {
            Scene.Add(new Sphere(new Vector3(3, 0, -10), 1) { Material = new Material { color = new Vector3(1, 0, 0) } });
            Scene.Add(new Sphere(new Vector3(-3, 0, -10), 1) { Material = new Material { color = new Vector3(0, 1, 0) } });
            Scene.Add(new Sphere(new Vector3(0, 0, -10), 1) { Material = new Material { color = new Vector3(0, 0, 1) } });           
            
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

        public float length;

    }

    public class Material
    {
        public Vector3 color;
        public float Reflectivity;
    }

    public class Light
    {
        public Vector3 color;
        public Vector3 position;
    }
}
