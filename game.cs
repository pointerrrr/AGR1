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
		int numThreads = 9;
		Thread[] t;
		Thread screenThread;
		public int Width = 512;
		public int Height = 512;


		private KeyboardState prevKeyState, currentKeyState;

		public Game(int width, int height)
        {
			Width = width;
			Height = height;
        }

		public void Init()
		{
			screen.Clear( 0x000000 );

			raytracer = new Raytracer(numThreads, Height, Width);
			t = new Thread[numThreads];
			Trace();
			Draw();
			
		}

		public void Tick()
		{
			Controls(Keyboard.GetState());			
		}

		public void Trace()
        {
			for (int i = 0; i < numThreads; i++)
			{
				t[i] = new Thread(startTracing);
			}
			for (int i = 0; i < numThreads; i++)
			{
				t[i].Start(i);
			}
			for (int i = 0; i < numThreads; i++)
			{
				t[i].Join();
			}
		}

		public void Draw()
        {
			for (int i = 0; i < Width; i++)
			{
				for (int j = 0; j < Height; j++)
				{
					screen.Pixel(i, j, raytracer.result[i, j]);
				}
			}
		}

		private void startTracing(object mt)
        {
			raytracer.Trace(screen, (int) mt, numThreads);
		}

		public void Render()
		{
			
		}

		public void Controls(KeyboardState key)
        {
			float movementDistance = 0.25f;
			var camera = raytracer.Camera;
			currentKeyState = key;
			bool keyPressed = false;
			if (currentKeyState[Key.W])
			{
				camera.Reposition(new Vector3(0, 0, -movementDistance));
				keyPressed = true;
			}
			if (currentKeyState[Key.A])
			{
				camera.Reposition(new Vector3(-movementDistance, 0, 0));
				keyPressed = true;
			}
			if (currentKeyState[Key.S])
			{
				camera.Reposition(new Vector3(0, 0, movementDistance));
				keyPressed = true;
			}
			if (currentKeyState[Key.D])
			{
				camera.Reposition(new Vector3(movementDistance, 0, 0));
				keyPressed = true;
			}
			if (currentKeyState[Key.E])
			{
				camera.Reposition(new Vector3(0, movementDistance, 0));
				keyPressed = true;
			}
			if (currentKeyState[Key.Q])
			{
				camera.Reposition(new Vector3(0, -movementDistance, 0));
				keyPressed = true;
			}

			if (currentKeyState[Key.Left])
			{
				camera.YRotation += (float)Math.PI / 10f;
				keyPressed = true;
			}
			if (currentKeyState[Key.Right])
			{
				camera.YRotation -= (float)Math.PI / 10f;
				keyPressed = true;
			}
			if (currentKeyState[Key.Up])
			{
				camera.XRotation = (float)(camera.XRotation + Math.PI / 10f > Math.PI / 5f ? Math.PI / 2f : camera.XRotation + Math.PI / 10f);
				keyPressed = true;
			}
			if (currentKeyState[Key.Down])
			{
				camera.XRotation = (float)(camera.XRotation + Math.PI / 10f < -Math.PI / 5f ? -Math.PI / 2f : camera.XRotation - Math.PI / 10f);
				keyPressed = true;
			}

			if (currentKeyState[Key.BracketLeft])
			{
				camera.FOV = raytracer.Camera.FOV + 5 > 160 ? 160 : camera.FOV + 5;
				camera.UpdateScreen();
				keyPressed = true;
			}
			if (currentKeyState[Key.BracketRight])
			{
				camera.FOV = raytracer.Camera.FOV - 5 < 20 ? 20 : camera.FOV - 5;
				camera.UpdateScreen();
				keyPressed = true;
			}
			prevKeyState = key;
            if (keyPressed)
            {
				Trace();
				Draw();
            }			
		}

		private bool checkNewKeyPress(Key key)
        {
			return currentKeyState[key] && (currentKeyState[key] != prevKeyState[key]);
		}
	}

} // namespace Template
