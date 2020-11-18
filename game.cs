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
		int numThreads = 4;
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
			trace();
			draw();
			
		}

		public void Tick()
		{
			

			
		}
		public void trace()
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

		public void draw()
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
			var camera = raytracer.Camera;
			currentKeyState = key;
			bool keypressed = false;
			if (checkNewKeyPress(Key.W))
			{
				raytracer.Camera.Reposition(new Vector3(camera.Direction.X, 0, camera.Direction.Z * 0.1f));
				keypressed = true;
			}
			if (checkNewKeyPress(Key.A))
			{
				raytracer.Camera.Reposition(new Vector3(camera.Direction.Z, 0, -camera.Direction.X * 0.1f) * -1);
				keypressed = true;
			}
			if (checkNewKeyPress(Key.S))
			{
				raytracer.Camera.Reposition(new Vector3(camera.Direction.X, 0, camera.Direction.Z * 0.1f) * -1);
				keypressed = true;
			}
			if (checkNewKeyPress(Key.D))
			{
				raytracer.Camera.Reposition(new Vector3(camera.Direction.Z, 0, -camera.Direction.X * 0.1f));
				keypressed = true;
			}

			prevKeyState = key;
            if (keypressed)
            {
				trace();
				draw();
            }
			
		}

		private bool checkNewKeyPress(Key key)
        {
			return currentKeyState[key] && (currentKeyState[key] != prevKeyState[key]);
		}
	}

} // namespace Template
