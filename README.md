# **Description:** #

Allows you to write to the console window at a much faster rate by way of PInvoke.  
For example, if you wanted to make a game using the C# console window, 
then this would allow you to do so without all the flickering you get 
from clearing the screen.

# **Setting Up Your Own Project:** #

```
#!c#
using ConsoleFrameBuffer;
using System;

namespace YourProject {

    internal class Program {

        // width and height for the buffer frame
        private const int _width = 80;

        private const int _height = 25;

        // create new buffer frame
        private RootFrameBuffer _rootBuffer = new RootFrameBuffer(0, 0, _width, _height);

        // player variables
        private int _playerX = 0;

        private int _playerY = 0;
        private string _playerID = "@";

        public Program() {
            // create the events for the buffer
            _rootBuffer.Update_Event += _rootBuffer_Update_Event;
            _rootBuffer.Render_Event += _rootBuffer_Render_Event;
            _rootBuffer.KeyPressed_Event += _rootBuffer_KeyPressed_Event;

            // run the buffer's update and render events in a loop
            _rootBuffer.Run();

            // clears the console window after running Stop()
            // only really needed if you're running the exe
            // through another console and want to clear
            // the screen after exit
            Console.Clear();
        }

        private void _rootBuffer_KeyPressed_Event(VirtualKeys KeyPressed, ControlKeyState KeyModifiers) {
            // moves the "@" around the screen
            if (KeyPressed == VirtualKeys.W)
                _playerY--;
            if (KeyPressed == VirtualKeys.D)
                _playerX++;
            if (KeyPressed == VirtualKeys.S)
                _playerY++;
            if (KeyPressed == VirtualKeys.A)
                _playerX--;

            // ends the program
            if (KeyPressed == VirtualKeys.Escape)
                _rootBuffer.Stop();
        }

        private void _rootBuffer_Update_Event() {
            // maybe some game logic here?
        }

        private void _rootBuffer_Render_Event() {
            // clear the buffer frame
            _rootBuffer.Clear();

            // write the player data to the buffer frame
            _rootBuffer.Write(_playerX, _playerY, _playerID, ConsoleColor.Cyan);

            // finally, draw/write the buffer frame to the console window
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