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

        private ConsoleFrameBuffer _rootBuffer = new ConsoleFrameBuffer(0, 0, _width, _height);
        private ConsoleFrameBuffer _bufferStats = new ConsoleFrameBuffer(0, 0, _width, 3);
        private ConsoleFrameBuffer _bufferMap = new ConsoleFrameBuffer(0, 3, _width, _height - 3);

        private ConsoleKeyInfo _keyPressed;

        private Point _player = new Point();

        private string[] _tileMap = new string[_width * _height];
        private char[] _tileChars = new char[] { ',', '.', '`', '+' };
        private Array colors = Enum.GetValues(typeof(ConsoleColor));

        private Random _rndNum = new Random();
        private Stopwatch _sw = new Stopwatch();
        private long _frames;
        private float _value;
        private TimeSpan _sample;

        public Program() {
            ShuffleTiles();

            _sample = TimeSpan.FromSeconds(1);
            _value = 0;
            _frames = 0;
            _sw = Stopwatch.StartNew();

            while (_running) {
                Update();
                Render();
            }
        }

        private void ShuffleTiles() {
            for (int i = 0; i < _width * _height; i++) {
                _tileMap[i] = new string(_tileChars[_rndNum.Next(0, _tileChars.Length)], 1);
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

                if (_keyPressed.Key == ConsoleKey.UpArrow) {
                    _rootBuffer.Y--;
                    //_rootBuffer.SetBufferPosition(_rootBuffer.X, _rootBuffer.Y);
                }

                if (_keyPressed.Key == ConsoleKey.DownArrow) {
                    _rootBuffer.Y++;
                    //_rootBuffer.SetBufferPosition(_rootBufferPos.X, _rootBufferPos.Y);
                }

                if (_keyPressed.Key == ConsoleKey.LeftArrow) {
                    _rootBuffer.X--;
                    //_rootBuffer.SetBufferPosition(_rootBufferPos.X, _rootBufferPos.Y);
                }

                if (_keyPressed.Key == ConsoleKey.RightArrow) {
                    _rootBuffer.X++;
                    //_rootBuffer.SetBufferPosition(_rootBufferPos.X, _rootBufferPos.Y);
                }

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
            _rootBuffer.Clear();
            _bufferStats.Clear();
            _bufferMap.Clear();

            for (int i = 0; i < _width; i++) {
                _bufferStats.Write(i, 0, "=");
                _bufferStats.Write(i, 2, "=");
            }

            _bufferStats.Write(0, 1, "+");
            _bufferStats.Write(_width - 1, 1, "+");
            _bufferStats.Write(2, 1, String.Format("x:{0} - y:{1} // rx:{2} - ry:{3}", _player.X, _player.Y, _rootBuffer.X, _rootBuffer.Y), ConsoleColor.White);
            _bufferStats.Write(_width - String.Format("fps: {0}", (int)_value).Length - 2, 1, String.Format("fps: {0}", (int)_value), ConsoleColor.Yellow);

            DrawMap();

            _bufferMap.Write(_player.X, _player.Y, "@", ConsoleColor.Red);

            ConsoleFrameBuffer.CopyBuffer(_bufferStats, _rootBuffer);
            ConsoleFrameBuffer.CopyBuffer(_bufferMap, _rootBuffer);

            _rootBuffer.DrawBuffer();

            _frames++;
        }

        private ConsoleColor color = ConsoleColor.Gray;
        private int _colortick = 0;
        private ConsoleColor _bgcolor = ConsoleColor.Black;
        private int _tileshuffletick = 0;

        private void DrawMap() {
            if (_colortick >= 5000) {
                color = RandomColor();
                _bgcolor = RandomColor();
                _colortick = 0;
            }

            if (_tileshuffletick >= 50) {
                ShuffleTiles();
                _tileshuffletick = 0;
            }

            for (int x = 0; x < _width; x++) {
                for (int y = 0; y < _height - 1; y++) {
                    _bufferMap.Write(x, y, _tileMap[x + _width * y], color, _bgcolor);
                }
            }

            _colortick++;
            _tileshuffletick++;
        }

        private ConsoleColor RandomColor() {
            return (ConsoleColor)colors.GetValue(_rndNum.Next(0, colors.Length));
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