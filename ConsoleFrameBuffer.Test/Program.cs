using System;
using System.Diagnostics;

namespace ConsoleFrameBuffer.Test {

    internal class Program {

        private struct Point {
            public int X { get; set; }
            public int Y { get; set; }
        }

        private bool _running = true;
        private const int _width = 80;
        private const int _height = 25;

        private ConsoleFrameBuffer _bufferStats = new ConsoleFrameBuffer(0, 0, _width, 1);
        private ConsoleFrameBuffer _bufferMap = new ConsoleFrameBuffer(0, 1, _width, _height - 1);

        private ConsoleKeyInfo _keyPressed;

        private Point _player = new Point();

        private string[] _tileMap = new string[_width * _height];

        private Random _rndNum = new Random();
        private Stopwatch _sw = new Stopwatch();
        private long _frames;
        private float _value;
        private TimeSpan _sample;

        public Program() {
            for (int i = 0; i < _width * _height; i++) {
                _tileMap[i] = new string('.', 1);
            }

            _sample = TimeSpan.FromSeconds(1);
            _value = 0;
            _frames = 0;
            _sw = Stopwatch.StartNew();

            while (_running) {
                Update();
                Render();
            }
        }

        private void Update() {
            if (_sw.Elapsed > _sample) {
                _value = (float)(_frames / _sw.Elapsed.TotalSeconds);

                _sw.Reset();
                _sw.Start();
                _frames = 0;
            }

            if (Console.KeyAvailable) {
                _keyPressed = Console.ReadKey(true);

                if (_keyPressed.Key == ConsoleKey.W)
                    _player.Y--;
                if (_keyPressed.Key == ConsoleKey.D)
                    _player.X++;
                if (_keyPressed.Key == ConsoleKey.S)
                    _player.Y++;
                if (_keyPressed.Key == ConsoleKey.A)
                    _player.X--;

                if (_keyPressed.Key == ConsoleKey.Escape) { _running = false; }
            }
        }

        private void Render() {
            _bufferStats.Clear();
            _bufferMap.Clear();

            _bufferStats.Write(0, 0, String.Format("x:{0} - y:{1}", _player.X, _player.Y), ConsoleColor.White);
            _bufferStats.Write(_width - String.Format("fps: {0}", _value).Length, 0, String.Format("fps: {0}", _value), ConsoleColor.Yellow);

            DrawMap();

            _bufferMap.Write(_player.X, _player.Y, "@", ConsoleColor.Red);

            _bufferStats.DrawBuffer();
            _bufferMap.DrawBuffer();

            _frames++;
        }

        private ConsoleColor color = ConsoleColor.Gray;
        private int _colortick = 0;

        private void DrawMap() {
            if (_colortick >= 2000) {
                color = RandomColor();
                _colortick = 0;
            }

            for (int x = 0; x < _width; x++)
                for (int y = 0; y < _height - 1; y++) {
                    _bufferMap.Write(x, y, _tileMap[x + _width * y], color);
                }

            _colortick++;
        }

        private ConsoleColor RandomColor() {
            ConsoleColor[] color = {
                ConsoleColor.Blue, ConsoleColor.Cyan, ConsoleColor.DarkBlue,
                ConsoleColor.DarkCyan, ConsoleColor.DarkGray, ConsoleColor.DarkGreen,
                ConsoleColor.DarkMagenta, ConsoleColor.DarkRed, ConsoleColor.DarkYellow,
                ConsoleColor.Gray, ConsoleColor.Green, ConsoleColor.Magenta, ConsoleColor.Red,
                ConsoleColor.White, ConsoleColor.Yellow
            };

            return color[_rndNum.Next(0, color.Length)];
        }

        private static void Main(string[] args) {
            Console.Title = "ConsoleFrameBuffer Test";

            //Console.BufferWidth = _width;
            //Console.BufferHeight = _height;
            //Console.WindowWidth = _width;
            //Console.WindowHeight = _height;

            Console.CursorVisible = false;

            Program prog = new Program();
        }
    }
}