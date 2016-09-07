/*
    I'll add some comments into the test project at some point.  For now,
    follow the instructions in the README.md file to see how to set a project
    up using ConsoleFrameBuffer.
*/

using ConsoleFrameBuffer.Test.Mapping;
using ConsoleFrameBuffer.Test.Utility;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace ConsoleFrameBuffer.Test {

    internal class Program {
        public static Random RandomNumber = new Random();
        private static int _height = 25;
        private static Stopwatch _sw = new Stopwatch();
        private static int _width = 80;
        private RootFrameBuffer _bufferLogs = new RootFrameBuffer(0, _height - 6, _width, 5);
        private RootFrameBuffer _bufferMap = new RootFrameBuffer(0, 5, _width, _height - 11);
        private RootFrameBuffer _bufferStats = new RootFrameBuffer(0, 0, _width, 5);
        private CaveMap _caveMap = new CaveMap();
        private long _frames;
        private List<string> _logs = new List<string>();
        private Point _mousePos = new Point();
        private Point _player = new Point();
        private Camera _playerCamera = new Camera();
        private string _playerName = string.Empty;
        private RootFrameBuffer _rootBuffer = new RootFrameBuffer(0, 0, _width, _height);
        private TimeSpan _sample;
        private float _value;
        private Array colors = Enum.GetValues(typeof(ConsoleColor));

        public Program() {
            Console.Clear();

            _rootBuffer.Update += _rootBuffer_Update;
            _rootBuffer.Render += _rootBuffer_Render;
            _rootBuffer.Key_Pressed += _rootBuffer_Key_Pressed;
            _rootBuffer.Key_Released += _rootBuffer_Key_Released;
            _rootBuffer.Mouse_Moved += _rootBuffer_Mouse_Moved;
            _rootBuffer.MouseButton_Clicked += _rootBuffer_MouseButton_Clicked;
            _rootBuffer.MouseButton_DoubleClicked += _rootBuffer_MouseButton_DoubleClicked;

            using (RootFrameBuffer frame = new RootFrameBuffer(0, 0, 30, 1)) {
                frame.Write(0, 0, "Player Name: \n", ConsoleColor.White, ConsoleColor.Black, true);
                frame.WriteBuffer();

                while (_playerName.Trim().Length == 0) {
                    _playerName = _rootBuffer.ReadLine();
                }

                Console.Title = _playerName;

                frame.Write(0, 0, "Generating cave...", ConsoleColor.White);
                frame.WriteBuffer();
            }

            _caveMap.Generate(80, 80);

            _player.X = _caveMap.UpFloorPosition.X;
            _player.Y = _caveMap.UpFloorPosition.Y;

            _playerCamera.FixCamera(_player, _bufferMap.Width, _bufferMap.Height - 5);

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

        private static void Main(string[] args) {
            Console.Title = "ConsoleFrameBuffer.Test";
            Console.CursorVisible = false;

            Program prog = new Program();
        }

        private void _rootBuffer_Key_Pressed(VirtualKeys KeyPressed, ControlKeyState KeyModifers) {
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

            if (KeyPressed == VirtualKeys.Tab) {
                _rootBuffer.SetCursorVisibility(1, !_rootBuffer.CursorVisible);
                addLog("Cursor Visible: " + _rootBuffer.CursorVisible.ToString());
            }

            if (KeyModifers == ControlKeyState.ShiftPressed) {
                // KEY: SHIFT + . = >
                if (KeyPressed == VirtualKeys.OEMPeriod) {
                    if (_caveMap.DownFloorPosition == _player) {
                        using (RootFrameBuffer frame = new RootFrameBuffer(5, 10, 50, 3)) {
                            frame.Write(1, 1, "Do you wish to drop down a floor? yes/no\n", ConsoleColor.White, ConsoleColor.Black, true);
                            frame.WriteBuffer();

                            char ans = frame.ReadKey();

                            if (ans == 'y' || ans == 'Y') {
                                frame.Write(0, 0, "Generating cave...", ConsoleColor.White);
                                frame.WriteBuffer();

                                _caveMap.Generate(80, 80);
                                _player = _caveMap.UpFloorPosition;
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
                        frame.Write(1, 1, "Are you sure you want to quit? yes/no\n", ConsoleColor.White, ConsoleColor.Black, true);
                        frame.WriteBuffer();

                        VirtualKeys ans = frame.ReadAsVirtualKey();
                        addLog(ans.ToString() + " - " + ((int)ans).ToString());
                        if (ans == VirtualKeys.Y) {
                            _rootBuffer.Stop();
                        }
                    }
                }
            }
        }

        private void _rootBuffer_Key_Released(VirtualKeys KeyReleased, ControlKeyState KeyModifers) {
            addLog("Key Released: " + (KeyModifers > 0 ? (KeyModifers.ToString() + " + " + KeyReleased.ToString()) : KeyReleased.ToString()));
        }

        private void _rootBuffer_Mouse_Moved(int X, int Y) {
            _mousePos.X = X;
            _mousePos.Y = Y;
        }

        private void _rootBuffer_MouseButton_Clicked(int X, int Y, VirtualKeys ButtonState) {
            addLog(string.Format("MouseButton Clicked: X:{0},Y:{1} - {2}", X + _playerCamera.X, Y + _playerCamera.Y - 5, ButtonState.ToString()));

            if (ButtonState == VirtualKeys.LeftButton) {
                int mousePosMapX = X + _playerCamera.X;
                int mousePosMapY = Y + _playerCamera.Y - 5;

                if (!_caveMap.IsOutOfBounds(mousePosMapX, mousePosMapY)) {
                    Tile tmpTile = _caveMap.Tiles[mousePosMapX, mousePosMapY];

                    if (!tmpTile.IsVisible) return;

                    string tileDetails = string.Format("ID: [{0}], Walkable: {1}, Wall: {2}, IsVisible: {3}, IsExplored: {4}",
                        tmpTile.ID, tmpTile.Walkable, tmpTile.Wall, tmpTile.IsVisible, tmpTile.IsExplored);

                    addLog(tileDetails);
                }
            }
        }

        private void _rootBuffer_MouseButton_DoubleClicked(int X, int Y, VirtualKeys ButtonState) {
            addLog(string.Format("MouseButton DoubleClicked: X:{0},Y:{1} - {2}", X + _playerCamera.X, Y + _playerCamera.Y - 5, ButtonState.ToString()));
        }

        private void _rootBuffer_Render() {
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

        private void _rootBuffer_Update() {
            if (_sw.Elapsed > _sample) {
                _value = (float)(_frames / _sw.Elapsed.TotalSeconds);

                _sw.Reset();
                _sw.Start();
                _frames = 0;
            }

            _caveMap.ComputeFOV(_player);
            _playerCamera.FixCamera(_player, _bufferMap.Width, _bufferMap.Height - 5);
        }

        private void addLog(string log) {
            _logs.Insert(0, log);
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

        private ConsoleColor RandomColor() {
            return (ConsoleColor)colors.GetValue(RandomNumber.Next(0, colors.Length));
        }

        private void writeLogs() {
            for (int i = 0; i < _bufferLogs.Height; i++) {
                if (i < _logs.Count)
                    _bufferLogs.Write(1, (_bufferLogs.Height - 1) - i, _logs[i], ConsoleColor.White);
            }
        }
    }
}