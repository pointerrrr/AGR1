using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace template
{
    public class Camera
    {
        public Vector3 Position;
        public Vector3 Direction;
        public Screen Screen;

        public float FOV;
        public float FocalDistance;
        public float ApertureSize;

    }

    public class Screen
    {
        public Vector3 TopLeft, TopRigth, BottomLeft, BottomRight;
        
        public Screen(Vector3 topleft, Vector3 topright, Vector3 bottomleft, Vector3 bottomright)
        {
            TopLeft = topleft;
            TopRigth = topright;
            BottomLeft = bottomleft;
            BottomRight = bottomright;
        }
    }
}
