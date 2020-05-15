using System;
using System.Collections.Generic;
using ConsoleFrameBuffer.Test.Mapping;
using ConsoleFrameBuffer.Test.Utility;

namespace ConsoleFrameBuffer.Test.Frames {

    public class RootFrame : ConsoleFrame {
        // frame variables
        private ConsoleFrame _logsFrame;
        private ConsoleFrame _mapFrame;
        private ConsoleFrame _statsFrame;

        // root frame width and height
        private const int WIDTH = 80;
        private const int HEIGHT = 25;

        // list that will contain the logs displayed
        private List<string> _logs = new List<string>();

        // player variables
        private Point _player = new Point();
        private Camera _playerCamera = new Camera();
        private string _playerName = string.Empty;
        private Point _mousePos = new Point();

        // map variable
        private CaveMap _caveMap;

        public RootFrame() {
            // create the different frames we'll be using
            _logsFrame = new ConsoleFrame(0, HEIGHT - 6, WIDTH, 5);
            _statsFrame = new ConsoleFrame(0, 0, 80, 5);
            _mapFrame = new ConsoleFrame(0, 5, WIDTH, HEIGHT - 11);

            // add the child frames
            ChildFrames.Add(_logsFrame);
            ChildFrames.Add(_statsFrame);
            ChildFrames.Add(_mapFrame);

            // adjust the settings for the root frame
            SetConsoleTitle("ConsoleFrameBuffer.Test");
            SetCursorVisibility(1, false);

            // create events for the root frame
            Update += RootFrame_Update;
            Render += RootFrame_Render;
            Key_Pressed += RootFrame_Key_Pressed;
            Key_Released += RootFrame_Key_Released;
            Mouse_Moved += RootFrame_Mouse_Moved;
            MouseButton_Clicked += RootFrame_MouseButton_Clicked;
            MouseButton_DoubleClicked += RootFrame_MouseButton_DoubleClicked;

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

            // start the loop for the root frame
            Run();

            // dispose of the frames when the loop ends
            _mapFrame.Dispose();
            _statsFrame.Dispose();
            _logsFrame.Dispose();

            // clear the root frame
            Clear();

            // now dispose of the root frame
            Dispose();
        }

        private void RootFrame_Update() {
            if (_caveMap != null)
                _caveMap.ComputeFOV(_player);

            _playerCamera.FixCamera(_player, _mapFrame.Width, _mapFrame.Height - 5);
        }

        private void RootFrame_Render() {
            Clear();
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
                _player.X, _player.Y, X, Y, Width,
                Height, Width, Height), ConsoleColor.White);
            _statsFrame.Write(
                WIDTH - string.Format("fps: {0}", FPS).Length - 2, 1,
                string.Format("fps: {0}", FPS), ConsoleColor.Yellow);

            string help = "Use WASD to move '@' around.  Use ARROW KEYS to move the map frame around.";
            _statsFrame.Write(WIDTH / 2 - (help.Length / 2), 3, help, ConsoleColor.White);

            drawMap();
            writeLogs();

            _mapFrame.Write(_player.X - _playerCamera.X, _player.Y - _playerCamera.Y, "@", ConsoleColor.Red);
            _mapFrame.Write(_mousePos.X - _mapFrame.X, _mousePos.Y - _mapFrame.Y, "#", ConsoleColor.Red);
        }

        private void RootFrame_Key_Pressed(VirtualKeys Key, ControlKeyState KeyModifiers) {
            addLog("Key Pressed: " + (KeyModifiers > 0 ? (KeyModifiers.ToString() + " + " + Key.ToString()) : Key.ToString()));

            // get player movement keys, then make sure the map is walkable
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

            // move the map frame around
            if (Key == VirtualKeys.Up)
                _mapFrame.Y--;
            if (Key == VirtualKeys.Right)
                _mapFrame.X++;
            if (Key == VirtualKeys.Down)
                _mapFrame.Y++;
            if (Key == VirtualKeys.Left)
                _mapFrame.X--;

            // hide, or unhide the console cursor
            if (Key == VirtualKeys.Tab) {
                SetCursorVisibility(1, !CursorVisible);
                addLog("Cursor Visible: " + CursorVisible.ToString());
            }

            // check to see if there's a downfloor.  if there is, generate a new map
            if (KeyModifiers == ControlKeyState.ShiftPressed) {
                // KEY: SHIFT + . = >
                if (Key == VirtualKeys.OEMPeriod) {
                    if (_caveMap.DownFloorPosition == _player) {
                        string msg = "Do you wish to drop down a floor? yes/no\n";
                        using (ConsoleFrame frame = new ConsoleFrame((Width / 2) - (msg.Length / 2), 10, msg.Length, 3)) {
                            frame.SetCursorVisibility(100, true);
                            frame.Write(0, 1, msg, ConsoleColor.White, ConsoleColor.DarkBlue, true);
                            frame.WriteBuffer();

                            VirtualKeys ans = frame.ReadAsVirtualKey();

                            if (ans == VirtualKeys.Y) {
                                frame.Clear();
                                frame.Write(1, 1, "Generating cave...", ConsoleColor.White);
                                frame.WriteBuffer();

                                _caveMap.Generate(80, 80);
                                _player = _caveMap.UpFloorPosition;
                            }

                            frame.SetCursorVisibility(1, false);
                        }
                    }
                }
            }

            // see if the player wants to quit
            if (KeyModifiers == ControlKeyState.LeftCtrlPressed ||
                KeyModifiers == ControlKeyState.RightCtrlPressed) {
                // KEYS: CTRL + Q
                if (Key == VirtualKeys.Q) {
                    string msg = "Are you sure you want to quit? yes/no\n";
                    using (ConsoleFrame frame = new ConsoleFrame((Width / 2) - (msg.Length / 2), 10, msg.Length, 3)) {
                        frame.SetCursorVisibility(100, true);
                        frame.Write(0, 1, msg, ConsoleColor.White, ConsoleColor.DarkBlue, true);
                        frame.WriteBuffer();

                        VirtualKeys ans = frame.ReadAsVirtualKey();

                        if (ans == VirtualKeys.Y) {
                            Stop();
                            _caveMap = null;
                        }

                        frame.SetCursorVisibility(1, false);
                    }
                }
            }
        }

        private void RootFrame_Key_Released(VirtualKeys Key, ControlKeyState KeyModifiers) {
            addLog("Key Released: " + (KeyModifiers > 0 ? (KeyModifiers.ToString() + " + " + Key.ToString()) : Key.ToString()));
        }

        private void RootFrame_Mouse_Moved(int X, int Y) {
            _mousePos.X = X;
            _mousePos.Y = Y;
        }

        private void RootFrame_MouseButton_Clicked(int X, int Y, VirtualKeys ButtonState) {
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

        private void RootFrame_MouseButton_DoubleClicked(int X, int Y, VirtualKeys ButtonState) {
            addLog(string.Format("MouseButton DoubleClicked: X:{0},Y:{1} - {2}", X + _playerCamera.X, Y + _playerCamera.Y - 5, ButtonState.ToString()));
        }

        private void drawMap() {
            if (_caveMap == null) return;

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

        private void addLog(string log) {
            _logs.Insert(0, log);
        }

        private void writeLogs() {
            for (int i = 0; i < _logsFrame.Height; i++) {
                if (i < _logs.Count)
                    _logsFrame.Write(1, (_logsFrame.Height - 1) - i, _logs[i], ConsoleColor.White);
            }
        }
    }
}