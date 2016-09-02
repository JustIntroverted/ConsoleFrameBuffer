/*
    I'll add some comments into the test project at some point.  For now,
    follow the instructions in the README.md file to see how to set a project
    up using ConsoleFrameBuffer.
*/

using ConsoleFrameBuffer.Test.Mapping;
using ConsoleFrameBuffer.Test.Utility;
using System;
using System.Diagnostics;

namespace ConsoleFrameBuffer.Test {

    internal class Program {
        private bool _running = true;
        private const int _width = 80;
        private const int _height = 50;

        private FrameBuffer _rootBuffer = new FrameBuffer(0, 0, _width, _height);
        private FrameBuffer _bufferStats = new FrameBuffer(0, 0, _width, 5);
        private FrameBuffer _bufferMap = new FrameBuffer(0, 5, _width, _height - 5);

        private ConsoleKeyInfo _keyPressed;

        private Point _player = new Point();
        private Camera _playerCamera = new Camera();

        private CaveMap _caveMap = new CaveMap();
        private Array colors = Enum.GetValues(typeof(ConsoleColor));

        public static Random RandomNumber = new Random();
        private Stopwatch _sw = new Stopwatch();
        private long _frames;
        private float _value;
        private TimeSpan _sample;

        public Program() {
            _caveMap.Generate(500, 100);

            _player.X = _caveMap.StartPos.X;
            _player.Y = _caveMap.StartPos.Y;

            _playerCamera.FixCamera(_player, _bufferMap.Width, _bufferMap.Height);

            _sample = TimeSpan.FromSeconds(1);
            _value = 0;
            _frames = 0;
            _sw = Stopwatch.StartNew();

            while (_running) {
                Update();
                Render();
            }

            Console.Clear();
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

                if (_keyPressed.Key == ConsoleKey.UpArrow)
                    _rootBuffer.Y--;
                if (_keyPressed.Key == ConsoleKey.DownArrow)
                    _rootBuffer.Y++;
                if (_keyPressed.Key == ConsoleKey.LeftArrow)
                    _rootBuffer.X--;
                if (_keyPressed.Key == ConsoleKey.RightArrow)
                    _rootBuffer.X++;

                if (_keyPressed.Key == ConsoleKey.W &&
                    _caveMap.IsFloor(_player.X, _player.Y - 1))
                    _player.Y--;
                if (_keyPressed.Key == ConsoleKey.D &&
                    _caveMap.IsFloor(_player.X + 1, _player.Y))
                    _player.X++;
                if (_keyPressed.Key == ConsoleKey.S &&
                    _caveMap.IsFloor(_player.X, _player.Y + 1))
                    _player.Y++;
                if (_keyPressed.Key == ConsoleKey.A &&
                    _caveMap.IsFloor(_player.X - 1, _player.Y))
                    _player.X--;

                if (_keyPressed.Key == ConsoleKey.Escape)
                    _running = false;
            }

            _caveMap.ComputeFOV(_player);
            _playerCamera.FixCamera(_player, _bufferMap.Width, _bufferMap.Height);
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
            _bufferStats.Write(2, 1, String.Format("x:{0} - y:{1} // rx:{2} - ry:{3}",
                _player.X, _player.Y, _rootBuffer.X, _rootBuffer.Y), ConsoleColor.White);
            _bufferStats.Write(_width - String.Format("fps: {0}", (int)_value).Length - 2, 1,
                String.Format("fps: {0}", (int)_value), ConsoleColor.Yellow);

            string help = "Use WASD to move '@' around.  Use ARROW KEYS to move the frame around.";
            _bufferStats.Write(_width / 2 - (help.Length / 2), 3, help, ConsoleColor.White);

            DrawMap();

            _bufferMap.Write(_player.X - _playerCamera.X, _player.Y - _playerCamera.Y, "@", ConsoleColor.Red);

            FrameBuffer.CopyBuffer(_bufferStats, _rootBuffer);
            FrameBuffer.CopyBuffer(_bufferMap, _rootBuffer);

            _rootBuffer.WriteBuffer();

            _frames++;
        }

        private int _colortick = 0;

        private void DrawMap() {
            ConsoleColor color = ConsoleColor.Gray;

            if (_colortick >= 5000) {
                color = Tile.DarkenTile(RandomColor());
                _colortick = 0;
            }

            for (int x = 0; x < _bufferMap.Width; x++) {
                for (int y = 0; y < _bufferMap.Height; y++) {
                    if (!_caveMap.IsOutOfBounds(x + _playerCamera.X, y + _playerCamera.Y)) {
                        Tile tmpTile = _caveMap.Tiles[x + _playerCamera.X, y + _playerCamera.Y];

                        if (tmpTile.IsVisible) {
                            _bufferMap.Write(x, y, tmpTile.ID, tmpTile.ForegroundColor, tmpTile.BackgroundColor);
                        } else if (tmpTile.IsExplored) {
                            _bufferMap.Write(x, y, tmpTile.ID, Tile.DarkenTile(tmpTile.ForegroundColor), Tile.DarkenTile(tmpTile.BackgroundColor));
                        }
                    }
                }
            }

            _colortick++;
        }

        private ConsoleColor RandomColor() {
            return (ConsoleColor)colors.GetValue(RandomNumber.Next(0, colors.Length));
        }

        private static void Main(string[] args) {
            Console.Title = "ConsoleFrameBuffer.Test";

            Console.BufferWidth = _width;
            Console.BufferHeight = _height;
            Console.WindowWidth = _width;
            Console.WindowHeight = _height;

            Console.CursorVisible = false;

            Program prog = new Program();
        }
    }
}