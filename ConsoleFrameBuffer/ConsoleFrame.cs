﻿namespace ConsoleFrameBuffer {

    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Text;
    using Microsoft.Win32.SafeHandles;

    public enum MouseWheeled {
        Up, Down
    }

    public class ConsoleFrame : IDisposable {
        #region protected variables
        protected CharInfo[] _buffer;
        protected short _bufferheight;
        protected short _bufferwidth;
        protected Coord _cursorPosition;
        protected bool _disposed = false;
        protected SafeFileHandle _hConsoleIn;
        protected SafeFileHandle _hConsoleOut;
        protected SMALL_RECT _rect;
        protected bool _running = false;
        protected int _tabStop;
        protected short _x;
        protected short _y;
        #endregion protected variables

        #region private variables
        // fps variables
        private Stopwatch _sw = new Stopwatch();
        private TimeSpan _sample;
        private long _frames;
        private float _value;
        #endregion private variables

        #region delegates/events

        public delegate void KeyPressedDelegate(VirtualKeys Key, ControlKeyState KeyModifiers);
        public delegate void KeyReleasedDelegate(VirtualKeys Key, ControlKeyState KeyModifiers);
        public delegate void MouseButtonDoubleClickedDelegate(int X, int Y, VirtualKeys ButtonState);
        public delegate void MouseButtonClickedDelegate(int X, int Y, VirtualKeys ButtonState);
        public delegate void MouseMovedDelegate(int X, int Y);
        public delegate void MouseWheeledDelegate(MouseWheeled Direction);
        public delegate void RenderDelegate();
        public delegate void UpdateDelegate();

        public event KeyPressedDelegate Key_Pressed;
        public event KeyReleasedDelegate Key_Released;
        public event MouseButtonDoubleClickedDelegate MouseButton_DoubleClicked;
        public event MouseButtonClickedDelegate MouseButton_Clicked;
        public event MouseMovedDelegate Mouse_Moved;
        public event MouseWheeledDelegate Mouse_Wheeled;
        public event RenderDelegate Render;
        public event UpdateDelegate Update;
        #endregion delegates/events

        #region public variables
        public bool CursorVisible { get { return getCursorVisibility(); } }
        public int CursorX { get; set; }
        public int CursorY { get; set; }
        public int Height { get { return _bufferheight; } protected set { _bufferheight = (short)(value < 0 ? 0 : value); } }
        public int Width { get { return _bufferwidth; } protected set { _bufferwidth = (short)(value < 0 ? 0 : value); } }
        public int X { get { return _x; } set { _x = (short)(value < 0 ? 0 : value); updateBufferPos(); } }
        public int Y { get { return _y; } set { _y = (short)(value < 0 ? 0 : value); updateBufferPos(); } }
        public int FPS { get { return (int)_value; } }
        public List<ConsoleFrame> ChildFrames = new List<ConsoleFrame>();
        #endregion public variables

        public ConsoleFrame() : this(0, 0, 80, 25) {
        }

        // TODO 5: create a check to insure the handles are good to go
        public ConsoleFrame(int X, int Y, int Width, int Height) {
            // grabs the handle for the console window
            _hConsoleOut = APICall.CreateFile("CONOUT$", 0x40000000 | 0x80000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            _hConsoleIn = APICall.CreateFile("CONIN$", 0x80000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;

            _buffer = new CharInfo[this.Width * this.Height];

            _rect = new SMALL_RECT() {
                Left = (short)this.X,
                Top = (short)this.Y,
                Right = (short)(this.Width + this.X),
                Bottom = (short)(this.Height + this.Y)
            };
        }

        protected void getInput() {
            uint read = 0;
            uint readEvents = 0;

            if (APICall.GetNumberOfConsoleInputEvents(_hConsoleIn, out read) && read > 0) {
                INPUT_RECORD[] eventBuffer = new INPUT_RECORD[read];

                APICall.ReadConsoleInput(_hConsoleIn, eventBuffer, read, out readEvents);

                for (int i = 0; i < readEvents; i++) {
                    ControlKeyState conState = eventBuffer[i].KeyEvent.dwControlKeyState;
                    conState &= ~(
                        ControlKeyState.NumLockOn |
                        ControlKeyState.ScrollLockOn |
                        ControlKeyState.CapsLockOn);

                    switch (eventBuffer[i].EventType) {
                        case EventType.KEY_EVENT:
                            if (eventBuffer[i].KeyEvent.bKeyDown) {
                                Key_Pressed?.Invoke(eventBuffer[i].KeyEvent.wVirtualKeyCode, conState);
                            } else {
                                Key_Released?.Invoke(eventBuffer[i].KeyEvent.wVirtualKeyCode, conState);
                            }
                            break;

                        case EventType.MOUSE_EVENT:
                            if (eventBuffer[i].MouseEvent.dwEventFlags == MouseEventType.MOUSE_MOVED) {
                                Mouse_Moved?.Invoke(
                                    eventBuffer[i].MouseEvent.dwMousePosition.X,
                                    eventBuffer[i].MouseEvent.dwMousePosition.Y);
                            }

                            if (eventBuffer[i].MouseEvent.dwEventFlags == MouseEventType.MOUSE_WHEELED) {
                                MouseWheeled dir;

                                if (eventBuffer[i].MouseEvent.dwButtonState > 0)
                                    dir = MouseWheeled.Up;
                                else
                                    dir = MouseWheeled.Down;

                                Mouse_Wheeled?.Invoke(
                                    dir);
                            }

                            if (eventBuffer[i].MouseEvent.dwButtonState > 0) {
                                if (eventBuffer[i].MouseEvent.dwEventFlags == MouseEventType.DOUBLE_CLICK) {
                                    MouseButton_DoubleClicked?.Invoke(
                                        eventBuffer[i].MouseEvent.dwMousePosition.X,
                                        eventBuffer[i].MouseEvent.dwMousePosition.Y,
                                        eventBuffer[i].MouseEvent.dwButtonState);
                                } else {
                                    MouseButton_Clicked?.Invoke(
                                        eventBuffer[i].MouseEvent.dwMousePosition.X,
                                        eventBuffer[i].MouseEvent.dwMousePosition.Y,
                                        eventBuffer[i].MouseEvent.dwButtonState);
                                }
                            }

                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Sets the title of the console window.
        /// </summary>
        public void SetConsoleTitle(string Title) {
            APICall.SetConsoleTitle(Title);
        }

        /// <summary>
        /// Clears the buffer frame.
        /// </summary>
        public void Clear() {
            _buffer = new CharInfo[_bufferwidth * _bufferheight];
        }

        /// <summary>
        /// Resizes the buffer frame.
        /// </summary>
        /// <param name="Width">Width of buffer frame.</param>
        /// <param name="Height">Height of buffer frame.</param>
        public void ResizeBuffer(int Width, int Height) {
            this.Width = Width;
            this.Height = Height;

            _buffer = new CharInfo[_bufferwidth * _bufferheight];

            updateBufferPos();
        }

        /// <summary>
        /// Copies the source buffer frame to the destination buffer frame.
        /// </summary>
        /// <param name="src">Buffer frame to be copied.</param>
        /// <param name="dest">Buffer frame that will be copied over by the source buffer frame.</param>
        public static void CopyBuffer(ConsoleFrame src, ConsoleFrame dest) {
            if (src == null || dest == null) return;
            if (src._buffer == null || dest._buffer == null) return;

            CharInfo charinfo;

            int x, y = 0;
            for (int i = 0; i < src._buffer.Length; i++) {
                x = i % src._bufferwidth;
                y = i / src._bufferwidth;

                if (x + src.X + 1 > dest._bufferwidth || y + src.Y + 1 > dest._bufferheight)
                    continue;

                charinfo = src._buffer[x + src._bufferwidth * y];

                dest._buffer[(x + src.X) + dest._bufferwidth * (y + src.Y)] = charinfo;
            }
        }

        /// <summary>
        /// Reads the first character from the input.
        /// </summary>
        /// <returns>Returns the first character of the string inputed.</returns>
        public string Read() {
            StringBuilder sb = new StringBuilder();
            uint read = 0;

            if (APICall.ReadConsole(_hConsoleIn, sb, 1, out read, IntPtr.Zero)) {
                return sb.ToString(0, (int)read);
            }

            return string.Empty;
        }

        /// <summary>
        /// Reads input until ended.
        /// </summary>
        /// <returns>Returns input as a string.</returns>
        public string ReadLine() {
            const int maxCount = 256;
            StringBuilder sb = new StringBuilder(maxCount);
            uint read = 0;

            if (APICall.ReadConsole(_hConsoleIn, sb, maxCount, out read, IntPtr.Zero)) {
                return sb.ToString(0, (int)read - 1);
            }

            return string.Empty;
        }

        /// <summary>
        /// Returns a char based on the key pressed provided
        /// </summary>
        /// <returns>Char</returns>
        public char ReadKey() {
            return (char)APICall._getch();
        }

        /// <summary>
        /// Returns read key as a Virtual Key
        /// </summary>
        /// <returns>Virtual Key</returns>
        public VirtualKeys ReadAsVirtualKey() {
            int a = APICall._getch();

            if (a == 0 || a == 0xE0) {
                int b = APICall._getch();

                b = (int)APICall.MapVirtualKey((uint)b, 1);

                return (VirtualKeys)b;
            }

            StringBuilder sb = new StringBuilder();
            APICall.GetKeyboardLayoutName(sb);
            IntPtr ptr = APICall.LoadKeyboardLayout(sb.ToString(), 1);

            char c = (char)APICall.VkKeyScanEx((char)a, ptr);

            APICall.UnloadKeyboardLayout(ptr);

            return (VirtualKeys)c;
        }

        /// <summary>
        /// Runs getInput(), Update(), and Render() methods in a loop.
        /// </summary>
        public void Run() {
            // set the variables for fps count
            _sample = TimeSpan.FromSeconds(1);
            _value = 0;
            _frames = 0;
            _sw = Stopwatch.StartNew();

            _running = true;

            while (_running) {
                getFPS();
                getInput();
                Update?.Invoke();
                Render?.Invoke();
                RenderChildren();
                WriteBuffer();

                _frames++;

                System.Threading.Thread.Sleep(10);
            }

            Dispose();
        }

        private void getFPS() {
            if (_sw.Elapsed > _sample) {
                _value = (float)(_frames / _sw.Elapsed.TotalSeconds);
                _sw.Reset();
                _sw.Start();
                _frames = 0;
            }
        }

        /// <summary>
        /// Sets the buffer frame's position.
        /// </summary>
        /// <param name="X">X position</param>
        /// <param name="Y">Y position</param>
        public void SetBufferPosition(int X, int Y) {
            this.X = X;
            this.Y = Y;
        }

        /// <summary>
        /// Sets the console cursor position relative to the buffer frame.
        /// </summary>
        /// <param name="X">X position</param>
        /// <param name="Y">Y position</param>
        public void SetCursorPosition(int X, int Y) {
            _cursorPosition = new Coord((short)(X + this.X), (short)(Y + this.Y));

            if (!APICall.SetConsoleCursorPosition(_hConsoleOut, _cursorPosition)) {
                Write(0, 0, "Failed to update cursor position.", ConsoleColor.Red);
                WriteBuffer();
            }
        }

        public bool SetCursorVisibility(int Size, bool Visible) {
            CONSOLE_CURSOR_INFO cci = new CONSOLE_CURSOR_INFO {
                dwSize = (uint)Size,
                bVisible = Visible
            };

            return APICall.SetConsoleCursorInfo(_hConsoleOut, ref cci);
        }

        /// <summary>
        /// Stops the loop.
        /// </summary>
        public void Stop() {
            _running = false;
        }

        /// <summary>
        /// "Writes" the string to the buffer frame.  DrawBuffer must be called in order
        /// for the string to display.
        /// </summary>
        /// <param name="X">X position</param>
        /// <param name="Y">Y position</param>
        /// <param name="Text">The string to be written to the buffer frame.</param>
        /// <param name="ForegroundColor">The foreground color of the string.</param>
        /// <param name="BackgroundColor">The background color of the string.</param>
        /// <param name="UpdateCursorPosition">Will update the location of the console cursor relative to the position of the frame it's in.
        /// Setting this to 'true' can cause performance issues.</param>
        public void Write(int X, int Y, string Text,
            ConsoleColor ForegroundColor = ConsoleColor.Gray,
            ConsoleColor BackgroundColor = ConsoleColor.Black,
            bool UpdateCursorPosition = false) {
            if (_buffer.Length <= 0) return;
            if (Text.Length > _buffer.Length) Text = Text.Substring(0, _buffer.Length - 1);

            int x = 0, y = 0;
            for (int i = 0; i < Text.Length; ++i) {
                switch (Text[i]) {
                    // newline
                    case '\n': x = 0; y++; break;

                    // carriage return
                    case '\r': x = 0; break;

                    // tab
                    // TODO 6: Implement Tabstop
                    case '\t': x += 5; break;

                    default:
                        int j = (y + Y) * _bufferwidth + (x + X);

                        if (j < 0) break;

                        if (j < _buffer.Length) {
                            if ((x + X + 1) > _bufferwidth || (y + Y + 1) > _bufferheight ||
                                x + X - 0 < 0 || y + Y - 0 < 0)
                                break;

                            _buffer[j].Attributes = (short)((short)ForegroundColor | (short)((short)BackgroundColor * 0x0010));
                            _buffer[j].Char.AsciiChar = (byte)Text[i];

                            x++;
                        }

                        break;
                }
            }

            if (UpdateCursorPosition)
                SetCursorPosition(x + X, y + Y);
        }

        /// <summary>
        /// Draws the frame to the console window.  This is only needed if you aren't using Run() to
        /// automatically call the WriteBuffer() method.
        /// </summary>
        public void WriteBuffer() {
            // Copies the child frames to the parent frame for "rendering"
            //RenderChildren();

            // if the handle is valid, then go ahead and write to the console
            if (_hConsoleOut != null && !_hConsoleOut.IsInvalid) {
                bool b = APICall.WriteConsoleOutput(_hConsoleOut, _buffer,
                    new Coord() { X = _bufferwidth, Y = _bufferheight },
                    new Coord() { X = 0, Y = 0 },
                    ref _rect);
            }
        }

        private bool getCursorVisibility() {
            CONSOLE_CURSOR_INFO cci = new CONSOLE_CURSOR_INFO();

            bool result = APICall.GetConsoleCursorInfo(_hConsoleOut, out cci);

            return cci.bVisible;
        }

        public void RenderChildren() {
            foreach (ConsoleFrame cf in ChildFrames) {
                CopyBuffer(cf, this);

                cf.Clear();
            }
        }

        private void updateBufferPos() {
            _rect = new SMALL_RECT() {
                Left = (short)X,
                Top = (short)Y,
                Right = (short)(_bufferwidth + X),
                Bottom = (short)(_bufferheight + Y)
            };
        }

        public void Dispose() {
            Dispose(true);

            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    if (_hConsoleOut != null) { _hConsoleOut.Close(); _hConsoleOut.Dispose(); _hConsoleOut = null; }
                    if (_hConsoleIn != null) { _hConsoleIn.Close(); _hConsoleIn.Dispose(); _hConsoleIn = null; }
                    if (_buffer != null) { _buffer = null; }
                    if (ChildFrames != null) { ChildFrames.Clear(); ChildFrames = null; }
                }

                _disposed = true;
            }
        }
    }
}