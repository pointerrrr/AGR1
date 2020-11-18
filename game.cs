using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Diagnostics;
using OpenTK;
using OpenTK.Graphics;
using OpenTK.Graphics.OpenGL;
using OpenTK.Input;
using System.Threading;

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

			Controls(OpenTK.Input.Keyboard.GetState());
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
			float movementDistance = 0.25f;
			var camera = raytracer.Camera;
			currentKeyState = key;
			if (currentKeyState[Key.W])
				camera.Reposition( new Vector3(0, 0, -movementDistance));
			if (currentKeyState[Key.A])
				camera.Reposition(new Vector3(-movementDistance, 0, 0));
			if (currentKeyState[Key.S])
				camera.Reposition(new Vector3(0, 0, movementDistance));
			if (currentKeyState[Key.D])
				camera.Reposition(new Vector3(movementDistance, 0, 0));
			if (currentKeyState[Key.E])
				camera.Reposition(new Vector3(0, movementDistance, 0));
			if (currentKeyState[Key.Q])
				camera.Reposition(new Vector3(0, -movementDistance, 0));

			if (currentKeyState[Key.Left])
				camera.YRotation += (float)Math.PI / 10f;
			if (currentKeyState[Key.Right])
				camera.YRotation -= (float)Math.PI / 10f;
			if (currentKeyState[Key.Up])
				camera.XRotation = (float)(camera.XRotation + Math.PI / 10f > Math.PI / 5f ? Math.PI / 2f : camera.XRotation + Math.PI / 10f);
			if (currentKeyState[Key.Down])
				camera.XRotation = (float)(camera.XRotation + Math.PI / 10f < -Math.PI / 5f ? -Math.PI / 2f : camera.XRotation - Math.PI / 10f);

			if (currentKeyState[Key.BracketLeft])
			{
				camera.FOV = raytracer.Camera.FOV + 5 > 160 ? 160 : camera.FOV + 5;
				camera.UpdateScreen();
			}
			if (currentKeyState[Key.BracketRight])
			{
				camera.FOV = raytracer.Camera.FOV - 5 < 20 ? 20 : camera.FOV - 5;
				camera.UpdateScreen();
			}
			prevKeyState = key;
		}

		private bool checkNewKeyPress(Key key)
        {
			return currentKeyState[key] && (currentKeyState[key] != prevKeyState[key]);
		}
	}

} // namespace Template
