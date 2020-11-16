using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using static OpenTK.Vector3;

namespace template
{
    public abstract class Primitive
    {
        public Material Material;

        public abstract Intersection Intersect(Ray ray);

    }

    public class Sphere : Primitive
    {
        public Vector3 Position;
        public float Radius;
        public float Radius2;

        public Sphere(Vector3 position, float radius)
        {
            Position = position;
            Radius = radius;
            Radius2 = radius * radius;
        }

        public override Intersection Intersect(Ray ray)
        {
            // efficient ray / sphere intersection adapted from the lecture slides
            Vector3 C = Position - ray.position;

            float t = Dot(C, ray.direction);
            Vector3 Q = C - t * ray.direction;
            float p2 = Dot(Q, Q);

            if (p2 > Radius2)
                return null;

            t -= (float)Math.Sqrt(Radius2 - p2);

            if ((t < ray.length) && (t > 0))
            {
                var intersection = new Intersection();

                intersection.length = t;
                intersection.primitive = this;
                intersection.ray = ray;

                return intersection;
            }
            return null;            
        }
    }

    public class Plane : Primitive
    {
        public Vector3 Position;
        public Vector3 Normal;

        public Plane(Vector3 position, Vector3 normal)
        {
            Position = position;
            Normal = normal;
        }

        public override Intersection Intersect(Ray ray)
        {
            float par = Dot(ray.direction, Normal);
            float t = (Dot(Position - ray.position, Normal) * -1)  / par;

            if(Math.Abs(par) > 0.0001f && t > 0)
            {
                var intersection = new Intersection();

                intersection.length = t;
                intersection.primitive = this;
                intersection.ray = ray;

                return intersection;
            }
            return null;
        }
    }

    public class Vertex : Primitive
    {
        public Vector3 Point1, Point2, Point3;

        public override Intersection Intersect(Ray ray)
        {

            
            return null;
        }
    }
}
