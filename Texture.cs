using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenTK;

namespace template
{
    // skybox for when rays hit nothing
    public class Skybox
    {
        public Texture Texture { get; set; }

        // initialize texture via string
        public Skybox(string path)
        {
            Texture = new Texture(path);
        }
    }

    // texture for all primitives
    public class Texture
    {
        // 2d-array of vector3's to make the image accesible in multiple threads
        public Vector3[,] Image { get; set; }
        // bitmap used for final texture (changes the Image array when the bitmap is changed as well)
        public Bitmap Bitmap
        {
            get { return Bitmap; }
            set
            {
                Image = new Vector3[value.Width, value.Height];
                for (int i = 0; i < value.Width; i++)
                    for (int j = 0; j < value.Height; j++)
                    {
                        Color color = value.GetPixel(i, j);
                        Image[i, j] = new Vector3((float)color.R / 255, (float)color.G / 255, (float)color.B / 255);
                    }
            }
        }

        // initialize texture via string
        public Texture(string path)
        {
            Bitmap image = new Bitmap(path);
            Bitmap = image;
        }
    }

}
