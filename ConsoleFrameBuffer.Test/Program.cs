/*
    I'll add some comments into the test project at some point.  For now,
    follow the instructions in the README.md file to see how to set a project
    up using ConsoleFrameBuffer.
*/

using ConsoleFrameBuffer.API;
using ConsoleFrameBuffer.Test.Mapping;
using ConsoleFrameBuffer.Test.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace ConsoleFrameBuffer.Test {

    internal class Program {
        private static int _width = 80;
        private static int _height = 25;

        private RootFrameBuffer _rootBuffer = new RootFrameBuffer(0, 0, _width, _height);
        private RootFrameBuffer _bufferStats = new RootFrameBuffer(0, 0, _width, 5);
        private RootFrameBuffer _bufferMap = new RootFrameBuffer(0, 5, _width, _height - 11);
        private RootFrameBuffer _bufferLogs = new RootFrameBuffer(0, _height - 6, _width, 5);

        private string _playerName = string.Empty;
        private Point _player = new Point();
        private Camera _playerCamera = new Camera();
        private Point _mousePos = new Point();

        private CaveMap _caveMap = new CaveMap();
        private Array colors = Enum.GetValues(typeof(ConsoleColor));

        public static Random RandomNumber = new Random();
        private static Stopwatch _sw = new Stopwatch();
        private long _frames;
        private float _value;
        private TimeSpan _sample;

        private List<string> _logs = new List<string>();

        public Program() {
            _rootBuffer.Update_Event += _rootBuffer_Update_Event;
            _rootBuffer.Render_Event += _rootBuffer_Render_Event;
            _rootBuffer.KeyPressed_Event += _rootBuffer_KeyPressed_Event;
            _rootBuffer.KeyReleased_Event += _rootBuffer_KeyReleased_Event;
            _rootBuffer.MouseMove_Event += _rootBuffer_MouseMove_Event;

            using (RootFrameBuffer frame = new RootFrameBuffer(0, 0, 30, 1)) {
                frame.Write(0, 0, "Player Name: ", ConsoleColor.White, ConsoleColor.Black, true);
                frame.WriteBuffer();

                while (_playerName.Trim().Length == 0) {
                    _playerName = _rootBuffer.ReadLine();
                }

                Console.Title = _playerName;

                frame.Write(0, 0, "Generating cave...", ConsoleColor.White);
                frame.WriteBuffer();
            }

            _caveMap.Generate(500, 100);

            _player.X = _caveMap.StartPos.X;
            _player.Y = _caveMap.StartPos.Y;

            _playerCamera.FixCamera(_player, _bufferMap.Width, _bufferMap.Height);

            _sample = TimeSpan.FromSeconds(1);
            _value = 0;
            _frames = 0;
            _sw = Stopwatch.StartNew();

            _rootBuffer.Run();

            _bufferMap.Dispose();
            _bufferStats.Dispose();
            _bufferLogs.Dispose();
            _rootBuffer.Dispose();

            Console.Clear();
        }

        private void _rootBuffer_KeyReleased_Event(VirtualKeys KeyReleased, ControlKeyState KeyModifers) {
            addLog("Key Released: " + (KeyModifers > 0 ? (KeyModifers.ToString() + " + " + KeyReleased.ToString()) : KeyReleased.ToString()));
        }

        private void _rootBuffer_MouseMove_Event(int X, int Y) {
            _mousePos.X = X;
            _mousePos.Y = Y;
        }

        private void _rootBuffer_KeyPressed_Event(VirtualKeys KeyPressed, ControlKeyState KeyModifers) {
            addLog("Key Pressed: " + (KeyModifers > 0 ? (KeyModifers.ToString() + " + " + KeyPressed.ToString()) : KeyPressed.ToString()));

            if (KeyPressed == VirtualKeys.W &&
                _caveMap.IsWalkable(_player.X, _player.Y - 1))
                _player.Y--;
            if (KeyPressed == VirtualKeys.D &&
                _caveMap.IsWalkable(_player.X + 1, _player.Y))
                _player.X++;
            if (KeyPressed == VirtualKeys.S &&
                _caveMap.IsWalkable(_player.X, _player.Y + 1))
                _player.Y++;
            if (KeyPressed == VirtualKeys.A &&
                _caveMap.IsWalkable(_player.X - 1, _player.Y))
                _player.X--;

            if (KeyModifers == ControlKeyState.ShiftPressed) {
                // KEY: SHIFT + . = >
                if (KeyPressed == VirtualKeys.OEMPeriod) {
                    if (_caveMap.DownFloor == _player) {
                        using (RootFrameBuffer frame = new RootFrameBuffer(5, 10, 50, 3)) {
                            string ans = string.Empty;

                            frame.Write(1, 1, "Do you wish to drop down a floor? yes/no", ConsoleColor.White, ConsoleColor.Black, true);
                            frame.WriteBuffer();

                            frame.SetCursorPosition(1, frame.Height - 1);

                            ans = frame.Read().Trim().ToLower();

                            if (ans == "y") {
                                frame.Write(0, 0, "Generating cave...", ConsoleColor.White);
                                frame.WriteBuffer();

                                _caveMap.Generate(500, 100);
                                _player = _caveMap.StartPos;
                            }
                        }
                    }
                }
            }

            if (KeyModifers == ControlKeyState.LeftCtrlPressed ||
                KeyModifers == ControlKeyState.RightCtrlPressed) {
                // KEYS: CTRL + Q
                if (KeyPressed == VirtualKeys.Q) {
                    using (RootFrameBuffer frame = new RootFrameBuffer(5, 10, 50, 3)) {
                        string ans = string.Empty;

                        frame.Write(1, 1, "Are you sure you want to quit? yes/no", ConsoleColor.White, ConsoleColor.Black, true);
                        frame.WriteBuffer();

                        frame.SetCursorPosition(1, frame.Height - 1);

                        ans = frame.Read().Trim().ToLower();

                        if (ans == "y") {
                            _rootBuffer.Stop();
                        }
                    }
                }
            }
        }

        private void _rootBuffer_Update_Event() {
            if (_sw.Elapsed > _sample) {
                _value = (float)(_frames / _sw.Elapsed.TotalSeconds);

                _sw.Reset();
                _sw.Start();
                _frames = 0;
            }

            // resizes frames and console settings based on console width/height
            if (Console.WindowWidth != _width || Console.WindowHeight != _height) {
                _width = Console.WindowWidth;
                _height = Console.WindowHeight;
                _rootBuffer.ResizeBuffer(_width, _height);
                _bufferStats.ResizeBuffer(_width, 5);
                _bufferMap.ResizeBuffer(_width, _height - 5);
                _bufferLogs.ResizeBuffer(_width, _height - 6);

                // TODO 9: fix out of range error on resize
                if (_width >= 0 && _height >= 0) {
                    Console.BufferWidth = _width;
                    Console.BufferHeight = _height;
                    Console.WindowWidth = _width;
                    Console.WindowHeight = _height;
                }
            }

            _caveMap.ComputeFOV(_player);
            _playerCamera.FixCamera(_player, _bufferMap.Width, _bufferMap.Height);
        }

        private void _rootBuffer_Render_Event() {
            _rootBuffer.Clear();
            _bufferStats.Clear();
            _bufferMap.Clear();
            _bufferLogs.Clear();

            for (int i = 0; i < _width; i++) {
                _bufferStats.Write(i, 0, "=");
                _bufferStats.Write(i, 2, "=");
            }

            _bufferStats.Write(0, 1, "+");
            _bufferStats.Write(_width - 1, 1, "+");
            _bufferStats.Write(2, 1, string.Format("x:{0} - y:{1} // rx:{2} - ry:{3} // rw:{4} - rh:{5} // cw:{6} - ch:{7}",
                _player.X, _player.Y, _rootBuffer.X, _rootBuffer.Y, _rootBuffer.Width,
                _rootBuffer.Height, Console.WindowWidth, Console.WindowHeight), ConsoleColor.White);
            _bufferStats.Write(_width - string.Format("fps: {0}", (int)_value).Length - 2, 1,
                string.Format("fps: {0}", (int)_value), ConsoleColor.Yellow);

            string help = "Use WASD to move '@' around.  Use ARROW KEYS to move the frame around.";
            _bufferStats.Write(_width / 2 - (help.Length / 2), 3, help, ConsoleColor.White);

            DrawMap();
            writeLogs();

            _bufferMap.Write(_player.X - _playerCamera.X, _player.Y - _playerCamera.Y, "@", ConsoleColor.Red);

            RootFrameBuffer.CopyBuffer(_bufferStats, _rootBuffer);
            RootFrameBuffer.CopyBuffer(_bufferMap, _rootBuffer);
            RootFrameBuffer.CopyBuffer(_bufferLogs, _rootBuffer);

            _rootBuffer.Write(_mousePos.X, _mousePos.Y, "#", ConsoleColor.Red);

            _rootBuffer.WriteBuffer();

            _frames++;
        }

        private void DrawMap() {
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
        }

        private void addLog(string log) {
            _logs.Insert(0, log);
        }

        private void writeLogs() {
            for (int i = 0; i < _bufferLogs.Height; i++) {
                if (i < _logs.Count)
                    _bufferLogs.Write(1, (_bufferLogs.Height - 1) - i, _logs[i], ConsoleColor.White);
            }
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

            //Console.CursorVisible = false;

            Program prog = new Program();
        }
    }
}