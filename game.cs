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

		private KeyboardState prevKeyState, currentKeyState;

		public void Init()
		{
			screen.Clear( 0x000000 );
			raytracer = new Raytracer();
		}

		public void Tick()
		{
			//screen.Print( "hello world!", 2, 2, 0xffffff );
			// render stuff over the backbuffer (OpenGL, sprites)
			
			pixels = raytracer.Trace(screen);

			//for (int i = 0; i < 512; i++)
			//{
			//	for (int j = 0; j < 512; j++)
			//	{
			//		screen.Pixel(i, j, pixels[i, j]);				
			//	}
			//}
		}

		public void Render()
		{
			
		}

		public void Controls(KeyboardState key)
        {
			var camera = raytracer.Camera;
			currentKeyState = key;
			if (checkNewKeyPress(Key.W))
				raytracer.Camera.Reposition( new Vector3(camera.Direction.X, 0, camera.Direction.Z * 0.1f));
			if (checkNewKeyPress(Key.A))
				raytracer.Camera.Reposition(new Vector3(camera.Direction.Z, 0, -camera.Direction.X * 0.1f) * -1); ;
			if (checkNewKeyPress(Key.S))
				raytracer.Camera.Reposition(new Vector3(camera.Direction.X, 0, camera.Direction.Z * 0.1f) * -1); ;
			if (checkNewKeyPress(Key.D))
				raytracer.Camera.Reposition(new Vector3(camera.Direction.Z, 0, -camera.Direction.X * 0.1f)); ;

			prevKeyState = key;
		}

		private bool checkNewKeyPress(Key key)
        {
			return currentKeyState[key] && (currentKeyState[key] != prevKeyState[key]);
		}
	}

} // namespace Template
