using ConsoleFrameBuffer;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleFrameBuffer.Test {

    internal class Program {

        private struct Point {
            public int X { get; set; }
            public int Y { get; set; }
        }

        private bool _running = true;
        private const int _width = 80;
        private const int _height = 40;

        private ConsoleFrameBuffer _bufferMap = new ConsoleFrameBuffer(0, 0, _width, _height);

        private ConsoleKeyInfo _keyPressed;

        private Point player = new Point();

        public Program() {
            while (_running) {
                Update();
                Render();
            }
        }

        private void Render() {
            _bufferMap.Clear();

            _bufferMap.Write(player.X, player.Y, "@", ConsoleColor.Red);

            _bufferMap.DrawBuffer();
        }

        private void Update() {
            if (Console.KeyAvailable) {
                _keyPressed = Console.ReadKey(true);

                if (_keyPressed.Key == ConsoleKey.W)
                    player.Y--;
                if (_keyPressed.Key == ConsoleKey.D)
                    player.X++;
                if (_keyPressed.Key == ConsoleKey.S)
                    player.Y++;
                if (_keyPressed.Key == ConsoleKey.A)
                    player.X--;

                if (_keyPressed.Key == ConsoleKey.Escape) { _running = false; }
            }
        }

        private static void Main(string[] args) {
            Console.BufferWidth = _width;
            Console.BufferHeight = _height;
            Console.WindowWidth = _width;
            Console.WindowHeight = _height;

            Console.CursorVisible = false;

            Program prog = new Program();
        }
    }
}