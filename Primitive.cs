using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using static OpenTK.Vector3;
using static template.Constants;

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

            var intersection = new Intersection();

            intersection.primitive = this;
            intersection.ray = ray;


            if (C.Length < Radius)
            {
                t += (float)Math.Sqrt(Radius2 - p2);

                intersection.length = t - 0.0001f;
                intersection.Position = intersection.length * ray.direction + ray.position;
                intersection.normal = Normalize(intersection.Position - Position);

                return intersection;
            }

            t -= (float)Math.Sqrt(Radius2 - p2);

            if (((t < ray.length) && (t > 0)))
            {

                intersection.length = t - 0.0001f;
                intersection.Position = intersection.length * ray.direction + ray.position;
                intersection.normal = Normalize(intersection.Position - Position);
                

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
            float t = (Dot(Position - ray.position, Normal))  / par;

            if (Math.Abs(par) < 0.0001f || t < 0)
                return null;

            var intersection = new Intersection();

            intersection.length = t - 0.0001f;
            intersection.primitive = this;
            intersection.ray = ray;
            intersection.normal = par > 0 ? -Normal : Normal;
            intersection.Position = ray.position + ray.direction * intersection.length;

            return intersection;

        }
    }

    // adapted from https://en.wikipedia.org/wiki/M%C3%B6ller%E2%80%93Trumbore_intersection_algorithm
    public class Vertex : Primitive
    {
        public Vector3 Point1, Point2, Point3, Normal;

        public Vertex(Vector3 p1, Vector3 p2, Vector3 p3)
        {
            Point1 = p1;
            Point2 = p2;
            Point3 = p3;

            Normal = Normalize( Cross(p2 - p1, p3 - p1));
        }

        public override Intersection Intersect(Ray ray)
        {
            Vector3 edge1, edge2, h, s, q;
            float a, f, u, v;

            edge1 = Point2 - Point1;
            edge2 = Point3 - Point1;
            h = Cross(ray.direction, edge2);
            a = Dot(edge1, h);
            if (a > -Epsilon && a < Epsilon)
                return null;

            f = 1f / a;

            s = ray.position - Point1;

            u = f * Dot(s, h);
            if (u < 0 || u > 1)
                return null;

            q = Cross(s, edge1);
            v = f * Dot(ray.direction, q);
            if (v < 0 || u + v > 1)
                return null;
            float t = f * Dot(edge2, q);
            if (t < Epsilon)
                return null;
            var intersection = new Intersection();
            intersection.length = t - Epsilon;
            intersection.Position = ray.position + (intersection.length * ray.direction);
            if (Dot(Normal, ray.direction) > 0)
                intersection.normal = -Normal;
            else
                intersection.normal = Normal;
            
            intersection.primitive = this;
            intersection.ray = ray;
            return intersection;
        }
    }

    public class Torus : Primitive
    {
        public float Radius, Thickness;

        public override Intersection Intersect(Ray ray)
        {
            
            throw new NotImplementedException();
        }
    }
}
