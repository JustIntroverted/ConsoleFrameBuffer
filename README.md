# **Description:** #

Allows you to write to the console window at a much faster rate by way of PInvoke.  
For example, if you wanted to make a game using the C# console window, 
then this would allow you to do so without all the flickering you get 
from clearing the screen.

# **Setting Up Your Own Project:** #

```
#!c#


using System;
using ConsoleFrameBuffer;

namespace ProjectName {

	class Program {
		
		// simple bool varible for the loop
        private bool _running = true;
		
		// width and height for the buffer frame 
        private const int _width = 80;
        private const int _height = 50;

		// create new buffer frame
        private FrameBuffer _rootBuffer = new FrameBuffer(0, 0, _width, _height);

		// variable to hold what key has been pressed
        private ConsoleKeyInfo _keyPressed;

		// player variables
        private int _playerX = 0;
		private int _playerY = 0;
		private string _playerID = "@";

        public Program() {
			// simple loop that calls Update() and Render()
            while (_running) {
                Update();
                Render();
            }

			// clears the console window after _running = false
			// only really needed if you're running the exe
			// through another console type and want to clear
			// the screen after exit
            Console.Clear();
        }
		
		private void Update() {		
            if (Console.KeyAvailable) {
                _keyPressed = Console.ReadKey(true);

				// moves the buffer frame around the screen
                if (_keyPressed.Key == ConsoleKey.UpArrow)
                    _rootBuffer.Y--;
                if (_keyPressed.Key == ConsoleKey.DownArrow)
                    _rootBuffer.Y++;
                if (_keyPressed.Key == ConsoleKey.LeftArrow)
                    _rootBuffer.X--;
                if (_keyPressed.Key == ConsoleKey.RightArrow)
                    _rootBuffer.X++;

				// moves the "@" around the screen
                if (_keyPressed.Key == ConsoleKey.W)
                    _playerY--;
                if (_keyPressed.Key == ConsoleKey.D)
                    _playerX++;
                if (_keyPressed.Key == ConsoleKey.S)
                    _playerY++;
                if (_keyPressed.Key == ConsoleKey.A)
                    _playerX--;

				// ends the program
                if (_keyPressed.Key == ConsoleKey.Escape)
                    _running = false;
            }
		}
		
		private void Render() {
			// clear the buffer frame
			_rootBuffer.Clear();
			
			// write the player data to the buffer frame
            _rootBuffer.Write(_playerX, _playerY, _playerID, ConsoleColor.Cyan);
			
			// finally, write the buffer frame to the console window
			_rootBuffer.WriteBuffer();
		}
		
		private static void Main(string[] args) {
            Console.Title = "New Console Game Project!";

            Console.BufferWidth = _width;
            Console.BufferHeight = _height;
            Console.WindowWidth = _width;
            Console.WindowHeight = _height;

            Console.CursorVisible = false;

            Program prog = new Program();
        }
	}
}
```