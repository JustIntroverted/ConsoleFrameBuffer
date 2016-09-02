using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;

namespace ConsoleFrameBuffer {

    public class FrameBuffer {

        #region Structs

        [StructLayout(LayoutKind.Sequential)]
        private struct Coord {
            public short X;
            public short Y;

            public Coord(short X, short Y) {
                this.X = X;
                this.Y = Y;
            }
        };

        [StructLayout(LayoutKind.Explicit)]
        private struct CharUnion {

            [FieldOffset(0)]
            public char UnicodeChar;

            [FieldOffset(0)]
            public byte AsciiChar;
        }

        [StructLayout(LayoutKind.Explicit)]
        private struct CharInfo {

            [FieldOffset(0)]
            public CharUnion Char;

            [FieldOffset(2)]
            public short Attributes;
        }

        [StructLayout(LayoutKind.Sequential)]
        private struct SmallRect {
            public short Left;
            public short Top;
            public short Right;
            public short Bottom;
        }

        #endregion Structs

        #region DLL Imports

        [DllImport("Kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        private static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WriteConsoleOutput(
          SafeFileHandle hConsoleOutput,
          CharInfo[] lpBuffer,
          Coord dwBufferSize,
          Coord dwBufferCoord,
          ref SmallRect lpWriteRegion);

        #endregion DLL Imports

        public int X { get { return _x; } set { _x = (short)(value < 0 ? 0 : value); updateBufferPos(); } }
        public int Y { get { return _y; } set { _y = (short)(value < 0 ? 0 : value); updateBufferPos(); } }
        public int Width { get { return _bufferwidth; } }
        public int Height { get { return _bufferheight; } }
        private short _x;
        private short _y;
        private int _bufferwidth { get; set; }
        private int _bufferheight { get; set; }

        private SafeFileHandle _h;
        private CharInfo[] _buffer;
        private SmallRect _rect;

        public FrameBuffer() : this(0, 0, 80, 25) {
        }

        public FrameBuffer(int X, int Y, int Width, int Height) {
            // grabs the handle for the console window
            _h = CreateFile("CONOUT$", 0x40000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);

            this.X = X;
            this.Y = Y;
            _bufferwidth = Width;
            _bufferheight = Height;

            _buffer = new CharInfo[_bufferwidth * _bufferheight];

            _rect = new SmallRect() {
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
        public void Write(int X, int Y, string Text,
            ConsoleColor ForegroundColor = ConsoleColor.Gray,
            ConsoleColor BackgroundColor = ConsoleColor.Black) {
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
        }

        /// <summary>
        /// Draws the buffer frame to the console window.
        /// </summary>
        public void DrawBuffer() {
            // if the handle is valid, then go ahead and write to the console
            if (!_h.IsInvalid) {
                bool b = WriteConsoleOutput(_h, _buffer,
                    new Coord() { X = (short)(_bufferwidth), Y = (short)(_bufferheight) },
                    new Coord() { X = 0, Y = 0 },
                    ref _rect);
            }
        }

        /// <summary>
        /// Copies the source buffer frame to the destination buffer frame.
        /// </summary>
        /// <param name="src">Buffer frame to be copied.</param>
        /// <param name="dest">Buffer frame that will be copied over by the source buffer frame.</param>
        public static void CopyBuffer(FrameBuffer src, FrameBuffer dest) {
            for (int i = 0; i < src._buffer.Length; i++)
                if (src._buffer[i].Char.AsciiChar > 0)
                    dest._buffer[i + src._bufferwidth * src.Y] = src._buffer[i];
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
            _rect = new SmallRect() {
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
            _bufferwidth = Width;
            _bufferheight = Height;

            _buffer = new CharInfo[_bufferwidth * _bufferheight];
        }

        /// <summary>
        /// Clears the buffer frame.
        /// </summary>
        public void Clear() {
            _buffer = new CharInfo[_bufferwidth * _bufferheight];
        }
    }
}