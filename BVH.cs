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
    public class BVH
    {
        public bool IsLeafNode = false;
        public int BinCount = 10, MaxSplitDepth = 10;
        public float SplitCost = float.PositiveInfinity;
        public (Vector3, Vector3) BoundingVolume;
        public List<Primitive> Primitives;
        public BVH Left, Right;

        public BVH(List<Primitive> primitives)
        {
            Primitives = primitives;
        }

        public void Construct()
        {
            BoundingVolume = GetBoundingVolume(Primitives);
            subDivide();
        }

        public Intersection intersect()
        {
            return null;
        }

        private void subDivide()
        {
            (var bbMin, var bbMax) = BoundingVolume;

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

                switch (plane)
                {
                    case SplitPlane.X:
                        float leftXDist = binSize * i + binSize / 2f;
                        float rightXDist = distance - leftXDist;
                        surfaceAreaLeft = leftXDist * yDist * 2 + leftXDist * zDist * 2 + yDist * zDist * 2;
                        surfaceAreaRight = rightXDist * yDist * 2 + rightXDist * zDist * 2 + yDist * zDist * 2;
                        break;
                    case SplitPlane.Y:
                        float leftYDist = binSize * i + binSize / 2f;
                        float rightYDist = distance - leftYDist;
                        surfaceAreaLeft = xDist * leftYDist * 2 + xDist * zDist * 2 + leftYDist * zDist * 2;
                        surfaceAreaRight = xDist * rightYDist * 2 + xDist * zDist * 2 + rightYDist * zDist * 2;
                        break;
                    case SplitPlane.Z:
                        float leftZDist = binSize * i + binSize / 2f;
                        float rightZDist = distance - leftZDist;
                        surfaceAreaLeft = xDist * yDist * 2 + xDist * leftZDist * 2 + yDist * leftZDist * 2;
                        surfaceAreaRight = xDist * yDist * 2 + xDist * rightZDist * 2 + yDist * rightZDist * 2;
                        break;
                }

                var costLeft = CalculateCosts(left);
                var costRight = CalculateCosts(right);

                var cost = 0.125f + surfaceAreaLeft / surfaceAreaRight * costLeft + surfaceAreaRight / surfaceArea * costRight;

                if (cost < bestSplitCost)
                {
                    bestSplitCost = cost;
                    bestSplit = i;
                    bestLeft = left;
                    bestRight = right;
                }
            }

            // n primitives > max primitives in node or min cost < leaf cost
            if (bestSplitCost < SplitCost)
            {
                // TODO
                Left = new BVH(bestLeft);
                Right = new BVH(bestRight);
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
    }

    public enum SplitPlane
    {
        X,
        Y,
        Z
    }
}
