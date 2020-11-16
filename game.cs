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
		Raytracer raytracer;
		int[,] pixels;

		public void Init()
		{
			screen.Clear( 0x000000 );
			raytracer = new Raytracer();
		}

		public void Tick()
		{
			//screen.Print( "hello world!", 2, 2, 0xffffff );
			// render stuff over the backbuffer (OpenGL, sprites)
			
			pixels = raytracer.Trace();

			for (int i = 0; i < 512; i++)
			{
				for (int j = 0; j < 512; j++)
				{
					screen.Pixel(i, j, pixels[i, j]);				
				}
			}
		}

		public void Render()
		{
			
		}
	}

} // namespace Template
