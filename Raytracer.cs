using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using System.Drawing;
using static OpenTK.Vector3;

namespace template
{
    public class Raytracer
    {
        public Camera Camera;
        public List<Primitive> Scene = new List<Primitive>();
        public List<Light> Lights = new List<Light>();

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
                    Ray ray = new Ray();
                    ray.position = Camera.Position;

                    ray.direction = new Vector3((i - 256f) / 512, (j - 256f) / 512f, Camera.Position.Z - 1) - Camera.Position;
                    ray.direction.Normalize();
                    result[i,j] = VecToInt(TraceRay(ray));
                }
            }
            return result;
        }

        private Vector3 TraceRay(Ray ray, int recursionDepth = 0)
        {
            if (recursionDepth > 10)
                return new Vector3();
            Intersection nearest = new Intersection { length = float.PositiveInfinity };

            foreach (var primitive in Scene)
            {
                var intersection = primitive.Intersect(ray);
                if (intersection != null && intersection.length < nearest.length)
                    nearest = intersection;
            }

            if (nearest.primitive == null)
                return new Vector3();

            var illumination = new Vector3();

            foreach (var light in Lights)
            {
                if (!castShadowRay(light, nearest.Position + Normalize(light.Position - nearest.Position) * 0.0001f))
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
                var reflectionRay = Normalize(reflectRay(ray.direction, nearest.normal));
                Ray reflection = new Ray() { direction = reflectionRay, position = nearest.Position + reflectionRay * 0.0001f };
                return nearest.primitive.Material.color * (1 - nearest.primitive.Material.Reflectivity) * illumination
                    + (TraceRay(reflection, ++recursionDepth) * nearest.primitive.Material.Reflectivity);
            }

            

            return nearest.primitive.Material.color * illumination;
        }

        private bool castShadowRay(Light light, Vector3 position)
        {
            foreach (var primitive in Scene)
            {
                var intersection = primitive.Intersect(new Ray { position = position, direction = Normalize(position - light.Position) });
                if (intersection != null)
                    return false;
            }
            return true;
        }
        
        private Vector3 reflectRay(Vector3 rayDirection, Vector3 normal)
        {
            return rayDirection - 2 * Dot(rayDirection, normal) * normal;
        }

        private int VecToInt(Vector3 vector)
        {
            int R = vector.X > 1 ? 255 : (int)(vector.X * 255);
            int G = vector.Y > 1 ? 255 : (int)(vector.Y * 255);
            int B = vector.Z > 1 ? 255 : (int)(vector.Z * 255);
            return (R << 16) + (G << 8) + B;
        }

        private void MakeScene()
        {
            Scene.Add(new Sphere(new Vector3(3, 0, -5), 1) { Material = new Material { color = new Vector3(1, 0, 0), Reflectivity = 0f } });
            Scene.Add(new Sphere(new Vector3(-3, 0, -5), 1) { Material = new Material { color = new Vector3(0, 1, 0), Reflectivity = 0f } });
            Scene.Add(new Sphere(new Vector3(0, 0, -5), 1) { Material = new Material { color = new Vector3(0, 0, 1), Reflectivity = 1f } });

            
            Scene.Add(new Plane(new Vector3(0, -1, -20), new Vector3(0, -1f, 0)) { Material = new Material { color = new Vector3(1,1,1)} });

            Lights.Add(new Light(new Vector3(0,0,0), new Vector3(10, 10, 10)));
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
        public float Reflectivity;
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
}
