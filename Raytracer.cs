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

        public int[,] Trace()
        {
            int[,] result = new int[512, 512];
            for (int i = 0; i < 512; i++)
            {
                for (int j = 0; j < 512; j++)
                {
                    result[i,j] = TraceRay(i, j);
                }
            }
            return result;
        }

        private int TraceRay(int xCoordinate, int yCoordinate)
        {
            Ray ray = new Ray();
            ray.position = Camera.Position;

            ray.direction = new Vector3((xCoordinate - 256f) / 512, (yCoordinate - 256f) / 512f, Camera.Position.Z -1) - Camera.Position;
            ray.direction.Normalize();

            Intersection nearest = new Intersection { length = float.PositiveInfinity };

            foreach (var primitive in Scene)
            {
                var intersection = primitive.Intersect(ray);
                if (intersection != null && intersection.length < nearest.length)
                    nearest = intersection;
            }

            if (nearest.primitive == null)
                return 0;

            return VecToInt(nearest.primitive.Material.color);
        }
        

        private int VecToInt(Vector3 vector)
        {
            int R = vector.X > 1 ? 255 : (int)vector.X * 255;
            int G = vector.Y > 1 ? 255 : (int)vector.Y * 255;
            int B = vector.Z > 1 ? 255 : (int)vector.Z * 255;
            return (R << 16) + (G << 8) + B;
        }

        private void MakeScene()
        {
            Scene.Add(new Sphere(new Vector3(3, 0, -10), 1) { Material = new Material { color = new Vector3(1, 0, 0) } });
            Scene.Add(new Sphere(new Vector3(-3, 0, -10), 1) { Material = new Material { color = new Vector3(0, 1, 0) } });
            Scene.Add(new Sphere(new Vector3(0, 0, -10), 1) { Material = new Material { color = new Vector3(0, 0, 1) } });
            Scene.Add(new Plane(new Vector3(0, -5, -12), new Vector3(0, 1, 0)) { Material = new Material { color = new Vector3(0,1,1)} });
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
