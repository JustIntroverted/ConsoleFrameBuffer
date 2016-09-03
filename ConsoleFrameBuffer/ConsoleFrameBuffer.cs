using ConsoleFrameBuffer.API;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Text;

namespace ConsoleFrameBuffer {

    public class RootFrameBuffer : IDisposable {
        private bool _disposed = false;

        public int X { get { return _x; } set { _x = (short)(value < 0 ? 0 : value); updateBufferPos(); } }
        public int Y { get { return _y; } set { _y = (short)(value < 0 ? 0 : value); updateBufferPos(); } }
        public int CursorX { get; set; }
        public int CursorY { get; set; }
        public int Width { get { return _bufferwidth; } protected set { _bufferwidth = (short)(value < 0 ? 0 : value); } }
        public int Height { get { return _bufferheight; } protected set { _bufferheight = (short)(value < 0 ? 0 : value); } }
        private short _x;
        private short _y;
        private Coord _cursorPosition;
        private short _bufferwidth;
        private short _bufferheight;

        private SafeFileHandle _hConsoleOut;
        private SafeFileHandle _hConsoleIn;
        private CharInfo[] _buffer;
        private StringBuilder _read = new StringBuilder();
        private SMALL_RECT _rect;

        public RootFrameBuffer() : this(0, 0, 80, 25) {
        }

        public RootFrameBuffer(int X, int Y, int Width, int Height) {
            // grabs the handle for the console window
            _hConsoleOut = APICall.CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
            _hConsoleIn = APICall.CreateFile("CONIN$", 0x80000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            this.X = X;
            this.Y = Y;
            this.Width = Width;
            this.Height = Height;

            _buffer = new CharInfo[_bufferwidth * _bufferheight];

            _rect = new SMALL_RECT() {
                Left = (short)this.X,
                Top = (short)this.Y,
                Right = (short)(_bufferwidth + this.X),
                Bottom = (short)(_bufferheight + this.Y)
            };
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
        /// <param name="UpdateCursorPosition">If you choose to update the console cursor position
        /// with each update.  Setting this to 'true' will cause performance issues.</param>
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
                    case '\n': y++; break;

                    // carriage return
                    case '\r': x = 0; break;

                    // tab
                    case '\t': x += 5; break;

                    default:
                        int j = (y + Y) * _bufferwidth + (x + X);

                        if (j < 0) return;

                        if (j < _buffer.Length) {
                            _buffer[j].Attributes = (short)((short)ForegroundColor | (short)((short)BackgroundColor * 0x0010));
                            _buffer[j].Char.AsciiChar = (byte)Text[i];
                            x++;
                        }
                        break;
                }
            }

            if (UpdateCursorPosition)
                SetCursorPosition(x, y);
        }

        /// <summary>
        /// Reads the current keyboard input.
        /// </summary>
        /// <returns>Returns input as a string.</returns>
        public string ReadLine() {
            uint read = 0;

            if (APICall.ReadConsole(_hConsoleIn, _read, 256, out read, IntPtr.Zero)) {
                return _read.ToString(0, (int)read - 1);
            }

            return string.Empty;
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

        /// <summary>
        /// Draws the buffer frame to the console window.
        /// </summary>
        public void WriteBuffer() {
            // if the handle is valid, then go ahead and write to the console
            if (_hConsoleOut != null && !_hConsoleOut.IsInvalid) {
                bool b = APICall.WriteConsoleOutput(_hConsoleOut, _buffer,
                    new Coord() { X = _bufferwidth, Y = _bufferheight },
                    new Coord() { X = 0, Y = 0 },
                    ref _rect);
            }
        }

        /// <summary>
        /// Copies the source buffer frame to the destination buffer frame.
        /// </summary>
        /// <param name="src">Buffer frame to be copied.</param>
        /// <param name="dest">Buffer frame that will be copied over by the source buffer frame.</param>
        public static void CopyBuffer(RootFrameBuffer src, RootFrameBuffer dest) {
            if (src == null || dest == null) return;
            if (src._buffer == null || dest._buffer == null) return;

            for (int i = 0; i < src._buffer.Length; i++) {
                if (src._buffer[i].Char.AsciiChar > 0)
                    dest._buffer[i + src._bufferwidth * src.Y] = src._buffer[i];
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

        private void updateBufferPos() {
            _rect = new SMALL_RECT() {
                Left = (short)X,
                Top = (short)Y,
                Right = (short)(_bufferwidth + X),
                Bottom = (short)(_bufferheight + Y)
            };
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
        /// Clears the buffer frame.
        /// </summary>
        public void Clear() {
            _buffer = new CharInfo[_bufferwidth * _bufferheight];
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
                }

                _disposed = true;
            }
        }
    }
}