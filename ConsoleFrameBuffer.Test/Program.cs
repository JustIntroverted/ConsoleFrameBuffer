/*
    I'll add some comments into the test project at some point.  For now,
    follow the instructions in the README.md file to see how to set a project
    up using ConsoleFrameBuffer.
*/

using System;
using System.Collections.Generic;
using System.Diagnostics;
using ConsoleFrameBuffer.Test.Mapping;
using ConsoleFrameBuffer.Test.Utility;

namespace ConsoleFrameBuffer.Test {

    internal class Program {
        // frame variables
        private ConsoleFrame _rootFrame;
        private ConsoleFrame _logsFrame;
        private ConsoleFrame _mapFrame;
        private ConsoleFrame _statsFrame;

        // root frame width and height
        private const int WIDTH = 80;
        private const int HEIGHT = 25;

        // list that will contain the logs displayed
        private List<string> _logs = new List<string>();

        // fps variables
        private Stopwatch _sw = new Stopwatch();
        private TimeSpan _sample;
        private long _frames;
        private float _value;

        // player variables
        private Point _player = new Point();
        private Camera _playerCamera = new Camera();
        private string _playerName = string.Empty;
        private Point _mousePos = new Point();

        // map variable
        private CaveMap _caveMap;

        public static Random RandomNumber = new Random();

        public Program() {
            // create the different frames we'll be using
            _rootFrame = new ConsoleFrame(0, 0, WIDTH, HEIGHT);
            _logsFrame = new ConsoleFrame(0, HEIGHT - 6, WIDTH, 5);
            _mapFrame = new ConsoleFrame(0, 5, WIDTH, HEIGHT - 11);
            _statsFrame = new ConsoleFrame(0, 0, WIDTH, 5);

            // adjust the settings for the root frame
            _rootFrame.SetConsoleTitle("ConsoleFrameBuffer.Test");
            _rootFrame.SetCursorVisibility(1, false);

            // clear it
            _rootFrame.Clear();

            // create events for the root frame
            _rootFrame.Update += _rootBuffer_Update;
            _rootFrame.Render += _rootBuffer_Render;
            _rootFrame.Key_Pressed += _rootBuffer_Key_Pressed;
            _rootFrame.Key_Released += _rootBuffer_Key_Released;
            _rootFrame.Mouse_Moved += _rootBuffer_Mouse_Moved;
            _rootFrame.MouseButton_Clicked += _rootBuffer_MouseButton_Clicked;
            _rootFrame.MouseButton_DoubleClicked += _rootBuffer_MouseButton_DoubleClicked;

            // get the player's name by creating a temporary frame that'll use ReadLine()
            using (ConsoleFrame frame = new ConsoleFrame(0, 0, WIDTH, 2)) {
                frame.Write(0, 0, "Player Name: \n", ConsoleColor.White, ConsoleColor.Black, true);
                frame.SetCursorVisibility(100, true);
                frame.WriteBuffer();

                while (string.IsNullOrWhiteSpace(_playerName)) {
                    frame.SetCursorPosition(0, 1);
                    string n = frame.ReadLine().Trim();
                    _playerName = n.Length > 12 ? n.Substring(0, 12) : n;
                }

                frame.SetConsoleTitle("Player: " + _playerName);

                frame.Clear();
                frame.SetCursorVisibility(1, false);
                frame.Write(0, 0, "Generating cave...", ConsoleColor.White);
                frame.WriteBuffer();
            }

            // generate a cave-like map
            _caveMap = new CaveMap();
            _caveMap.Generate(80, 80);

            // set the players position on the map, then adjust the camera
            _player = _caveMap.UpFloorPosition;
            _playerCamera.FixCamera(_player, _mapFrame.Width, _mapFrame.Height - 5);

            // set the variables for fps count
            _sample = TimeSpan.FromSeconds(1);
            _value = 0;
            _frames = 0;
            _sw = Stopwatch.StartNew();

            // start the loop for the root frame
            _rootFrame.Run();

            // dispose of the frames when the loop ends
            _mapFrame.Dispose();
            _statsFrame.Dispose();
            _logsFrame.Dispose();

            // clear the root frame
            _rootFrame.Clear();

            // now dispose of the root frame
            _rootFrame.Dispose();
        }

        private static void Main(string[] args) {
            Program prog = new Program();
        }

        private void _rootBuffer_Key_Pressed(VirtualKeys Key, ControlKeyState KeyModifers) {
            addLog("Key Pressed: " + (KeyModifers > 0 ? (KeyModifers.ToString() + " + " + Key.ToString()) : Key.ToString()));

            if (Key == VirtualKeys.W &&
                _caveMap.IsWalkable(_player.X, _player.Y - 1))
                _player.Y--;
            if (Key == VirtualKeys.D &&
                _caveMap.IsWalkable(_player.X + 1, _player.Y))
                _player.X++;
            if (Key == VirtualKeys.S &&
                _caveMap.IsWalkable(_player.X, _player.Y + 1))
                _player.Y++;
            if (Key == VirtualKeys.A &&
                _caveMap.IsWalkable(_player.X - 1, _player.Y))
                _player.X--;

            if (Key == VirtualKeys.Tab) {
                _rootFrame.SetCursorVisibility(1, !_rootFrame.CursorVisible);
                addLog("Cursor Visible: " + _rootFrame.CursorVisible.ToString());
            }

            if (KeyModifers == ControlKeyState.ShiftPressed) {
                // KEY: SHIFT + . = >
                if (Key == VirtualKeys.OEMPeriod) {
                    if (_caveMap.DownFloorPosition == _player) {
                        using (ConsoleFrame frame = new ConsoleFrame(5, 10, 50, 3)) {
                            frame.Write(1, 1, "Do you wish to drop down a floor? yes/no\n", ConsoleColor.White, ConsoleColor.Black, true);
                            frame.WriteBuffer();

                            VirtualKeys ans = frame.ReadAsVirtualKey();

                            if (ans == VirtualKeys.Y) {
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
                if (Key == VirtualKeys.Q) {
                    using (ConsoleFrame frame = new ConsoleFrame(5, 10, 50, 3)) {
                        frame.Write(1, 1, "Are you sure you want to quit? yes/no\n", ConsoleColor.White, ConsoleColor.Black, true);
                        frame.WriteBuffer();

                        VirtualKeys ans = frame.ReadAsVirtualKey();

                        if (ans == VirtualKeys.Y) {
                            _rootFrame.Stop();
                            _caveMap = null;
                            Program prog = new Program();
                        }
                    }
                }
            }
        }

        private void _rootBuffer_Key_Released(VirtualKeys Key, ControlKeyState KeyModifers) {
            addLog("Key Released: " + (KeyModifers > 0 ? (KeyModifers.ToString() + " + " + Key.ToString()) : Key.ToString()));
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
            _rootFrame.Clear();
            _statsFrame.Clear();
            _mapFrame.Clear();
            _logsFrame.Clear();

            for (int i = 0; i < WIDTH; i++) {
                _statsFrame.Write(i, 0, "=");
                _statsFrame.Write(i, 2, "=");
            }

            _statsFrame.Write(0, 1, "+");
            _statsFrame.Write(WIDTH - 1, 1, "+");
            _statsFrame.Write(2, 1, string.Format("x:{0} - y:{1} // rx:{2} - ry:{3} // rw:{4} - rh:{5} // cw:{6} - ch:{7}",
                _player.X, _player.Y, _rootFrame.X, _rootFrame.Y, _rootFrame.Width,
                _rootFrame.Height, Console.WindowWidth, Console.WindowHeight), ConsoleColor.White);
            _statsFrame.Write(WIDTH - string.Format("fps: {0}", (int)_value).Length - 2, 1,
                string.Format("fps: {0}", (int)_value), ConsoleColor.Yellow);

            string help = "Use WASD to move '@' around.  Use ARROW KEYS to move the frame around.";
            _statsFrame.Write(WIDTH / 2 - (help.Length / 2), 3, help, ConsoleColor.White);

            DrawMap();
            writeLogs();

            _mapFrame.Write(_player.X - _playerCamera.X, _player.Y - _playerCamera.Y, "@", ConsoleColor.Red);

            ConsoleFrame.CopyBuffer(_statsFrame, _rootFrame);
            ConsoleFrame.CopyBuffer(_mapFrame, _rootFrame);
            ConsoleFrame.CopyBuffer(_logsFrame, _rootFrame);

            _rootFrame.Write(_mousePos.X, _mousePos.Y, "#", ConsoleColor.Red);

            _rootFrame.WriteBuffer();

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
            _playerCamera.FixCamera(_player, _mapFrame.Width, _mapFrame.Height - 5);
        }

        private void addLog(string log) {
            _logs.Insert(0, log);
        }

        private void DrawMap() {
            for (int x = 0; x < _mapFrame.Width; x++) {
                for (int y = 0; y < _mapFrame.Height; y++) {
                    if (!_caveMap.IsOutOfBounds(x + _playerCamera.X, y + _playerCamera.Y)) {
                        Tile tmpTile = _caveMap.Tiles[x + _playerCamera.X, y + _playerCamera.Y];

                        if (tmpTile.IsVisible) {
                            _mapFrame.Write(x, y, tmpTile.ID, tmpTile.ForegroundColor, tmpTile.BackgroundColor);
                        } else if (tmpTile.IsExplored) {
                            _mapFrame.Write(x, y, tmpTile.ID, Tile.DarkenTile(tmpTile.ForegroundColor), Tile.DarkenTile(tmpTile.BackgroundColor));
                        }
                    }
                }
            }
        }

        private void writeLogs() {
            for (int i = 0; i < _logsFrame.Height; i++) {
                if (i < _logs.Count)
                    _logsFrame.Write(1, (_logsFrame.Height - 1) - i, _logs[i], ConsoleColor.White);
            }
        }
    }
}