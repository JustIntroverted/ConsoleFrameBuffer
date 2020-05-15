# **Description:**

Allows you to write to the console window at a much faster rate.  If you wanted to make a game (e.g., roguelike) using the C# console window, then this would allow you to do so without all the flickering you get from clearing the screen.

# **Setting Up Your Own Project:**

```c#
using System;
using ConsoleFrameBuffer;

namespace YourProject 
{
    internal class Program 
	{
        // width and height for the frame
        private const int WIDTH = 80;
        private const int HEIGHT = 25;

        // declare new frame
        private ConsoleFrame RootFrame;

        // player variables
        private int _playerX, _playerY = 0;
        private string _playerID = "@";

        public Program() 
		{
            // create new frame
            RootFrame = new ConsoleFrame(0, 0, WIDTH, HEIGHT);

            // adjust some settings for the frame
            RootFrame.SetCursorVisibility(1, false);

            // create the events for the frame
            RootFrame.Update += RootFrame_Update;
            RootFrame.Render += RootFrame_Render;
            RootFrame.Key_Pressed += RootFrame_Key_Pressed;

            // run the frame's update and render events in a loop
            RootFrame.Run();

            // clears the console window after running Stop()
            // only really needed if you're running the exe
            // through another console and want to clear
            // the screen after exit
            Console.Clear();
        }

        private void RootFrame_Key_Pressed(VirtualKeys Key, ControlKeyState KeyModifiers) 
		{
            // moves the "@" around the screen
            if (Key == VirtualKeys.W)
                _playerY--;
            if (Key == VirtualKeys.D)
                _playerX++;
            if (Key == VirtualKeys.S)
                _playerY++;
            if (Key == VirtualKeys.A)
                _playerX--;

            // ends the program
            if (Key == VirtualKeys.Escape)
                RootFrame.Stop();
        }

        private void RootFrame_Update() 
		{
            // maybe some game logic here?
        }

        private void RootFrame_Render() 
		{
            // clear the buffer frame
            RootFrame.Clear();

            // write the player data to the buffer frame
            RootFrame.Write(_playerX, _playerY, _playerID, ConsoleColor.Cyan);

            // finally, draw/write the buffer frame to the console window
            RootFrame.WriteBuffer();
        }

        private static void Main(string[] args) 
		{
            Console.Title = "New Console Game Project!";

            new Program();
        }
    }
}
```