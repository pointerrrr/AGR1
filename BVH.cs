using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static template.GlobalLib;
using OpenTK;
using static OpenTK.Vector3;

namespace template
{
    public class BVH : Primitive
    {
        public bool IsLeafNode = false;
        public static int BinCount = 10, MaxSplitDepth = 10;
        public int CurrentSplitDepth = 0;
        public float SplitCost = float.PositiveInfinity;
        //public (Vector3, Vector3) BoundingVolume;
        public List<Primitive> Primitives;
        public BVH Left, Right;

        public BVH(List<Primitive> primitives)
        {
            Primitives = primitives;
        }

        public void Construct()
        {
            BoundingBox = GetBoundingVolume(Primitives);
            subDivide();
        }

        public override Intersection Intersect(Ray ray)
        {
            var intersection = IntersectAABB(BoundingBox, ray);
            if (intersection != null)
            {
                return IntersectSubNode(ray);
            }
            else
                return null;
        }

        public Intersection IntersectSubNode(Ray ray)
        {
            if(IsLeafNode)
            {
                var nearest = new Intersection { length = float.PositiveInfinity };

                for(int i = 0; i < Primitives.Count; i++)
                {
                    var intersection = Primitives[i].Intersect(ray);
                    if (intersection != null && intersection.length < nearest.length)
                        nearest = intersection;
                }

                if (nearest.primitive != null)
                    return nearest;
                return null;
            }

            var intersectLeft = IntersectAABB(Left.BoundingBox, ray);
            var intersectRight = IntersectAABB(Right.BoundingBox, ray);

            if (intersectLeft == null && intersectRight == null)
                return null;

            if (intersectLeft == null)
                return Right.IntersectSubNode(ray);

            if (intersectRight == null)
                return Left.IntersectSubNode(ray);

            if (intersectLeft.length > intersectRight.length)
            {
                intersectRight = Right.IntersectSubNode(ray);

                if (intersectRight == null)
                    return Left.IntersectSubNode(ray);
                else
                    return intersectRight;
            }
            else
            {
                intersectLeft = Left.IntersectSubNode(ray);

                if (intersectLeft == null)
                    return Right.IntersectSubNode(ray);
                else
                    return intersectLeft;
            }
        }




        private void subDivide()
        {
            if(CurrentSplitDepth > MaxSplitDepth)
            {
                IsLeafNode = true;
                return;
            }
            (var bbMin, var bbMax) = BoundingBox;

            var xDist = Math.Abs(bbMin.X - bbMax.X);
            var yDist = Math.Abs(bbMin.Y - bbMax.Y);
            var zDist = Math.Abs(bbMin.Z - bbMax.Z);

            float surfaceArea = xDist * yDist * 2 + xDist * zDist * 2 + yDist * zDist * 2;

            SplitPlane plane = SplitPlane.X;

            if (xDist >= yDist && xDist >= zDist)
                plane = SplitPlane.X;

            if (yDist > xDist && yDist >= zDist)
                plane = SplitPlane.Y;

            if (zDist > xDist && zDist > yDist)
                plane = SplitPlane.Z;

            float distance = 0;
            float start = 0;

            switch (plane)
            {
                case SplitPlane.X:
                    distance = xDist;
                    start = bbMin.X;
                    break;
                case SplitPlane.Y:
                    distance = yDist;
                    start = bbMin.Y;
                    break;
                case SplitPlane.Z:
                    distance = zDist;
                    start = bbMin.Z;
                    break;
            }

            float binSize = distance / BinCount;

            int bestSplit = 0;
            float bestSplitCost = float.PositiveInfinity;
            float bestCostLeft = float.PositiveInfinity;
            float bestCostRight = float.PositiveInfinity;
            (Vector3, Vector3) boundingLeft = (new Vector3(), new Vector3()), boundingRight = (new Vector3(), new Vector3());
            List<Primitive> bestLeft = null;
            List<Primitive> bestRight = null;
            

            for (int i = 0; i < BinCount; i++)
            {
                List<Primitive> left = new List<Primitive>();
                List<Primitive> right = new List<Primitive>();

                for (int j = 0; j < Primitives.Count; j++)
                {
                    switch (plane)
                    {
                        case SplitPlane.X:
                            if (Primitives[j].Centroid.X < start + binSize * i + binSize / 2f)
                                left.Add(Primitives[j]);
                            else
                                right.Add(Primitives[j]);
                            break;
                        case SplitPlane.Y:
                            if (Primitives[j].Centroid.Y < start + binSize * i + binSize / 2f)
                                left.Add(Primitives[j]);
                            else
                                right.Add(Primitives[j]);
                            break;
                        case SplitPlane.Z:
                            if (Primitives[j].Centroid.Z < start + binSize * i + binSize / 2f)
                                left.Add(Primitives[j]);
                            else
                                right.Add(Primitives[j]);
                            break;
                    }
                }

                float surfaceAreaLeft = 0f;
                float surfaceAreaRight = 0f;

                float leftXDist;
                float rightXDist;
                float leftYDist;
                float rightYDist;
                float leftZDist;
                float rightZDist;
                switch (plane)
                {
                    case SplitPlane.X:
                        leftXDist = binSize * i + binSize / 2f;
                        rightXDist = distance - leftXDist;
                        surfaceAreaLeft = leftXDist * yDist * 2 + leftXDist * zDist * 2 + yDist * zDist * 2;
                        surfaceAreaRight = rightXDist * yDist * 2 + rightXDist * zDist * 2 + yDist * zDist * 2;
                        
                        break;
                    case SplitPlane.Y:
                        leftYDist = binSize * i + binSize / 2f;
                        rightYDist = distance - leftYDist;
                        surfaceAreaLeft = xDist * leftYDist * 2 + xDist * zDist * 2 + leftYDist * zDist * 2;
                        surfaceAreaRight = xDist * rightYDist * 2 + xDist * zDist * 2 + rightYDist * zDist * 2;
                        boundingLeft = (new Vector3(), new Vector3());
                        boundingRight = (new Vector3(), new Vector3());
                        break;
                    case SplitPlane.Z:
                        leftZDist = binSize * i + binSize / 2f;
                        rightZDist = distance - leftZDist;
                        surfaceAreaLeft = xDist * yDist * 2 + xDist * leftZDist * 2 + yDist * leftZDist * 2;
                        surfaceAreaRight = xDist * yDist * 2 + xDist * rightZDist * 2 + yDist * rightZDist * 2;
                        boundingLeft = (new Vector3(), new Vector3());
                        boundingRight = (new Vector3(), new Vector3());
                        break;
                }

                var costLeft = CalculateCosts(left);
                var costRight = CalculateCosts(right);

                var cost = 0.125f + surfaceAreaLeft / surfaceAreaRight * costLeft + surfaceAreaRight / surfaceArea * costRight;

                if (cost < bestSplitCost)
                {
                    bestSplitCost = cost;
                    bestSplit = i;
                    bestCostLeft = costLeft;
                    bestCostRight = costRight;
                    bestLeft = left;
                    bestRight = right;
                }
            }

            // n primitives > max primitives in node or min cost < leaf cost
            if (bestSplitCost < SplitCost)
            {
                // TODO
                switch(plane)
                {
                    case SplitPlane.X:
                        boundingLeft = (new Vector3(bbMin.X, bbMin.Y, bbMin.Z), new Vector3(bbMin.X + binSize * bestSplit + binSize / 2f, bbMax.Y, bbMax.Z));
                        boundingRight = (new Vector3(bbMin.X + binSize * bestSplit + binSize / 2f, bbMin.Y, bbMin.Z), new Vector3(bbMax.X, bbMax.Y, bbMax.Z));
                        break;
                    case SplitPlane.Y:
                        boundingLeft = (new Vector3(bbMin.X, bbMin.Y, bbMin.Z), new Vector3(bbMax.X, bbMin.Y + binSize * bestSplit + binSize / 2f, bbMax.Z));
                        boundingRight = (new Vector3(bbMin.X, bbMin.Y + binSize * bestSplit + binSize / 2f, bbMin.Z), new Vector3(bbMax.X, bbMax.Y, bbMax.Z));
                        break;
                    case SplitPlane.Z:
                        boundingLeft = (new Vector3(bbMin.X, bbMin.Y, bbMin.Z), new Vector3(bbMax.X, bbMax.Y, bbMin.Z + binSize * bestSplit + binSize / 2f));
                        boundingRight = (new Vector3(bbMin.X, bbMin.Y, bbMin.Z + binSize * bestSplit + binSize / 2f), new Vector3(bbMax.X, bbMax.Y, bbMax.Z));
                        break;
                }
                

                Left = new BVH(bestLeft) { SplitCost = bestCostLeft, BoundingBox = boundingLeft, CurrentSplitDepth = CurrentSplitDepth + 1 };
                Right = new BVH(bestRight) { SplitCost = bestCostRight, BoundingBox = boundingRight, CurrentSplitDepth = CurrentSplitDepth + 1};
                Left.subDivide();
                Right.subDivide();
            }
            else
            {
                IsLeafNode = true;
            }
        }

        private float CalculateCosts(List<Primitive> primitives)
        {
            return primitives.Count;
        }

        public override void GetTexture(Intersection intersection)
        {
            throw new NotImplementedException();
        }
    }

    public enum SplitPlane
    {
        X,
        Y,
        Z
    }
}
