using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;

namespace template {

	class Game
	{
		public Surface screen;

		public void Init()
		{
			screen.Clear( 0x000000 );
		}

		public void Tick()
		{
			//screen.Print( "hello world!", 2, 2, 0xffffff );
			// render stuff over the backbuffer (OpenGL, sprites)
			var raytracer = new Raytracer();
			var bitmap = raytracer.Trace();

			for (int i = 0; i < 512; i++)
			{
				for (int j = 0; j < 512; j++)
				{
					screen.Pixel(i, j, RGBToInt(bitmap.GetPixel(i, j)));				
				}
			}
		}

		public void Render()
		{
			
		}

		private int RGBToInt(Color color)
        {
			return (color.R << 16) + (color.G << 8) + color.B;
		}
	}

} // namespace Template
