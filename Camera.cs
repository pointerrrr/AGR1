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
        private float screenDistance;
        public float FocalDistance;
        public float ApertureSize;

        public Camera(Vector3 position, Vector3 direction, float fov = 120)
        {
            Position = position;
            Direction = direction;
            FOV = fov;
            createScreen();
        }

        public void Reposition(Vector3 vector)
        {
            Position += vector;
            updateScreen();
        }

        private void updateScreen()
        {
            Screen.TopLeft = Position + Direction * screenDistance + new Vector3(-1, 1, 0);
            Screen.TopRigth = Position + Direction * screenDistance + new Vector3(1, 1, 0);
            Screen.BottomLeft = Position + Direction * screenDistance + new Vector3(-1, -1, 0);
            Screen.BottomRight = Position + Direction * screenDistance + new Vector3(1, -1, 0);
        }

        private void createScreen()
        {
            screenDistance = 1 / (float)Math.Tan((FOV * (Math.PI / 180)) / 2);
            var leftTop = Position + Direction * screenDistance + new Vector3(-1,1,0) ;
            var rightTop = Position + Direction * screenDistance + new Vector3(1, 1, 0) ;
            var leftBottom = Position + Direction * screenDistance + new Vector3(-1, -1, 0);
            var rightBottom = Position + Direction * screenDistance + new Vector3(1, -1, 0);
            Screen = new Screen(leftTop, rightTop, leftBottom, rightBottom);
        }

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
