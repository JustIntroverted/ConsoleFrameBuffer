using ConsoleFrameBuffer.API;
using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace ConsoleFrameBuffer {

    #region Structs/Enums

    public enum ConsoleModes : uint {
        ENABLE_PROCESSED_INPUT = 0x0001,
        ENABLE_LINE_INPUT = 0x0002,
        ENABLE_ECHO_INPUT = 0x0004,
        ENABLE_WINDOW_INPUT = 0x0008,
        ENABLE_MOUSE_INPUT = 0x0010,
        ENABLE_INSERT_MODE = 0x0020,
        ENABLE_QUICK_EDIT_MODE = 0x0040,
        ENABLE_EXTENDED_FLAGS = 0x0080,
        ENABLE_AUTO_POSITION = 0x0100,
        ENABLE_PROCESSED_OUTPUT = 0x0001,
        ENABLE_WRAP_AT_EOL_OUTPUT = 0x0002,
    }

    public enum ControlKeyState {
        RightAltPressed = 0x0001,
        LeftAltPressed = 0x0002,
        RightCtrlPressed = 0x0004,
        LeftCtrlPressed = 0x0008,
        ShiftPressed = 0x0010,
        NumLockOn = 0x0020,
        ScrollLockOn = 0x0040,
        CapsLockOn = 0x0080,
        EnhancedKey = 0x0100
    }

    public enum EventType : ushort {
        FOCUS_EVENT = 0x0010,
        KEY_EVENT = 0x0001,
        MENU_EVENT = 0x0008,
        MOUSE_EVENT = 0x0002,
        WINDOW_BUFFER_SIZE_EVENT = 0x0004
    }

    public enum MouseEventType : uint {
        MOUSE_MOVED = 0x0001,
        DOUBLE_CLICK = 0x0002,
        MOUSE_WHEELED = 0x0004  // doesnt seem to work unfortunately
    }

    public enum VirtualKeys
        : ushort {

        /// <summary></summary>
        LeftButton = 0x01,

        /// <summary></summary>
        RightButton = 0x02,

        /// <summary></summary>
        Cancel = 0x03,

        /// <summary></summary>
        MiddleButton = 0x04,

        /// <summary></summary>
        ExtraButton1 = 0x05,

        /// <summary></summary>
        ExtraButton2 = 0x06,

        /// <summary></summary>
        Back = 0x08,

        /// <summary></summary>
        Tab = 0x09,

        /// <summary></summary>
        Clear = 0x0C,

        /// <summary></summary>
        Return = 0x0D,

        /// <summary></summary>
        Shift = 0x10,

        /// <summary></summary>
        Control = 0x11,

        /// <summary></summary>
        Menu = 0x12,

        /// <summary></summary>
        Pause = 0x13,

        /// <summary></summary>
        CapsLock = 0x14,

        /// <summary></summary>
        Kana = 0x15,

        /// <summary></summary>
        Hangeul = 0x15,

        /// <summary></summary>
        Hangul = 0x15,

        /// <summary></summary>
        Junja = 0x17,

        /// <summary></summary>
        Final = 0x18,

        /// <summary></summary>
        Hanja = 0x19,

        /// <summary></summary>
        Kanji = 0x19,

        /// <summary></summary>
        Escape = 0x1B,

        /// <summary></summary>
        Convert = 0x1C,

        /// <summary></summary>
        NonConvert = 0x1D,

        /// <summary></summary>
        Accept = 0x1E,

        /// <summary></summary>
        ModeChange = 0x1F,

        /// <summary></summary>
        Space = 0x20,

        /// <summary></summary>
        Prior = 0x21,

        /// <summary></summary>
        Next = 0x22,

        /// <summary></summary>
        End = 0x23,

        /// <summary></summary>
        Home = 0x24,

        /// <summary></summary>
        Left = 0x25,

        /// <summary></summary>
        Up = 0x26,

        /// <summary></summary>
        Right = 0x27,

        /// <summary></summary>
        Down = 0x28,

        /// <summary></summary>
        Select = 0x29,

        /// <summary></summary>
        Print = 0x2A,

        /// <summary></summary>
        Execute = 0x2B,

        /// <summary></summary>
        Snapshot = 0x2C,

        /// <summary></summary>
        Insert = 0x2D,

        /// <summary></summary>
        Delete = 0x2E,

        /// <summary></summary>
        Help = 0x2F,

        /// <summary></summary>
        N0 = 0x30,

        /// <summary></summary>
        N1 = 0x31,

        /// <summary></summary>
        N2 = 0x32,

        /// <summary></summary>
        N3 = 0x33,

        /// <summary></summary>
        N4 = 0x34,

        /// <summary></summary>
        N5 = 0x35,

        /// <summary></summary>
        N6 = 0x36,

        /// <summary></summary>
        N7 = 0x37,

        /// <summary></summary>
        N8 = 0x38,

        /// <summary></summary>
        N9 = 0x39,

        /// <summary></summary>
        A = 0x41,

        /// <summary></summary>
        B = 0x42,

        /// <summary></summary>
        C = 0x43,

        /// <summary></summary>
        D = 0x44,

        /// <summary></summary>
        E = 0x45,

        /// <summary></summary>
        F = 0x46,

        /// <summary></summary>
        G = 0x47,

        /// <summary></summary>
        H = 0x48,

        /// <summary></summary>
        I = 0x49,

        /// <summary></summary>
        J = 0x4A,

        /// <summary></summary>
        K = 0x4B,

        /// <summary></summary>
        L = 0x4C,

        /// <summary></summary>
        M = 0x4D,

        /// <summary></summary>
        N = 0x4E,

        /// <summary></summary>
        O = 0x4F,

        /// <summary></summary>
        P = 0x50,

        /// <summary></summary>
        Q = 0x51,

        /// <summary></summary>
        R = 0x52,

        /// <summary></summary>
        S = 0x53,

        /// <summary></summary>
        T = 0x54,

        /// <summary></summary>
        U = 0x55,

        /// <summary></summary>
        V = 0x56,

        /// <summary></summary>
        W = 0x57,

        /// <summary></summary>
        X = 0x58,

        /// <summary></summary>
        Y = 0x59,

        /// <summary></summary>
        Z = 0x5A,

        /// <summary></summary>
        LeftWindows = 0x5B,

        /// <summary></summary>
        RightWindows = 0x5C,

        /// <summary></summary>
        Application = 0x5D,

        /// <summary></summary>
        Sleep = 0x5F,

        /// <summary></summary>
        Numpad0 = 0x60,

        /// <summary></summary>
        Numpad1 = 0x61,

        /// <summary></summary>
        Numpad2 = 0x62,

        /// <summary></summary>
        Numpad3 = 0x63,

        /// <summary></summary>
        Numpad4 = 0x64,

        /// <summary></summary>
        Numpad5 = 0x65,

        /// <summary></summary>
        Numpad6 = 0x66,

        /// <summary></summary>
        Numpad7 = 0x67,

        /// <summary></summary>
        Numpad8 = 0x68,

        /// <summary></summary>
        Numpad9 = 0x69,

        /// <summary></summary>
        Multiply = 0x6A,

        /// <summary></summary>
        Add = 0x6B,

        /// <summary></summary>
        Separator = 0x6C,

        /// <summary></summary>
        Subtract = 0x6D,

        /// <summary></summary>
        Decimal = 0x6E,

        /// <summary></summary>
        Divide = 0x6F,

        /// <summary></summary>
        F1 = 0x70,

        /// <summary></summary>
        F2 = 0x71,

        /// <summary></summary>
        F3 = 0x72,

        /// <summary></summary>
        F4 = 0x73,

        /// <summary></summary>
        F5 = 0x74,

        /// <summary></summary>
        F6 = 0x75,

        /// <summary></summary>
        F7 = 0x76,

        /// <summary></summary>
        F8 = 0x77,

        /// <summary></summary>
        F9 = 0x78,

        /// <summary></summary>
        F10 = 0x79,

        /// <summary></summary>
        F11 = 0x7A,

        /// <summary></summary>
        F12 = 0x7B,

        /// <summary></summary>
        F13 = 0x7C,

        /// <summary></summary>
        F14 = 0x7D,

        /// <summary></summary>
        F15 = 0x7E,

        /// <summary></summary>
        F16 = 0x7F,

        /// <summary></summary>
        F17 = 0x80,

        /// <summary></summary>
        F18 = 0x81,

        /// <summary></summary>
        F19 = 0x82,

        /// <summary></summary>
        F20 = 0x83,

        /// <summary></summary>
        F21 = 0x84,

        /// <summary></summary>
        F22 = 0x85,

        /// <summary></summary>
        F23 = 0x86,

        /// <summary></summary>
        F24 = 0x87,

        /// <summary></summary>
        NumLock = 0x90,

        /// <summary></summary>
        ScrollLock = 0x91,

        /// <summary></summary>
        NEC_Equal = 0x92,

        /// <summary></summary>
        Fujitsu_Jisho = 0x92,

        /// <summary></summary>
        Fujitsu_Masshou = 0x93,

        /// <summary></summary>
        Fujitsu_Touroku = 0x94,

        /// <summary></summary>
        Fujitsu_Loya = 0x95,

        /// <summary></summary>
        Fujitsu_Roya = 0x96,

        /// <summary></summary>
        LeftShift = 0xA0,

        /// <summary></summary>
        RightShift = 0xA1,

        /// <summary></summary>
        LeftControl = 0xA2,

        /// <summary></summary>
        RightControl = 0xA3,

        /// <summary></summary>
        LeftMenu = 0xA4,

        /// <summary></summary>
        RightMenu = 0xA5,

        /// <summary></summary>
        BrowserBack = 0xA6,

        /// <summary></summary>
        BrowserForward = 0xA7,

        /// <summary></summary>
        BrowserRefresh = 0xA8,

        /// <summary></summary>
        BrowserStop = 0xA9,

        /// <summary></summary>
        BrowserSearch = 0xAA,

        /// <summary></summary>
        BrowserFavorites = 0xAB,

        /// <summary></summary>
        BrowserHome = 0xAC,

        /// <summary></summary>
        VolumeMute = 0xAD,

        /// <summary></summary>
        VolumeDown = 0xAE,

        /// <summary></summary>
        VolumeUp = 0xAF,

        /// <summary></summary>
        MediaNextTrack = 0xB0,

        /// <summary></summary>
        MediaPrevTrack = 0xB1,

        /// <summary></summary>
        MediaStop = 0xB2,

        /// <summary></summary>
        MediaPlayPause = 0xB3,

        /// <summary></summary>
        LaunchMail = 0xB4,

        /// <summary></summary>
        LaunchMediaSelect = 0xB5,

        /// <summary></summary>
        LaunchApplication1 = 0xB6,

        /// <summary></summary>
        LaunchApplication2 = 0xB7,

        /// <summary></summary>
        OEM1 = 0xBA,

        /// <summary></summary>
        OEMPlus = 0xBB,

        /// <summary></summary>
        OEMComma = 0xBC,

        /// <summary></summary>
        OEMMinus = 0xBD,

        /// <summary></summary>
        OEMPeriod = 0xBE,

        /// <summary></summary>
        OEM2 = 0xBF,

        /// <summary></summary>
        OEM3 = 0xC0,

        /// <summary></summary>
        OEM4 = 0xDB,

        /// <summary></summary>
        OEM5 = 0xDC,

        /// <summary></summary>
        OEM6 = 0xDD,

        /// <summary></summary>
        OEM7 = 0xDE,

        /// <summary></summary>
        OEM8 = 0xDF,

        /// <summary></summary>
        OEMAX = 0xE1,

        /// <summary></summary>
        OEM102 = 0xE2,

        /// <summary></summary>
        ICOHelp = 0xE3,

        /// <summary></summary>
        ICO00 = 0xE4,

        /// <summary></summary>
        ProcessKey = 0xE5,

        /// <summary></summary>
        ICOClear = 0xE6,

        /// <summary></summary>
        Packet = 0xE7,

        /// <summary></summary>
        OEMReset = 0xE9,

        /// <summary></summary>
        OEMJump = 0xEA,

        /// <summary></summary>
        OEMPA1 = 0xEB,

        /// <summary></summary>
        OEMPA2 = 0xEC,

        /// <summary></summary>
        OEMPA3 = 0xED,

        /// <summary></summary>
        OEMWSCtrl = 0xEE,

        /// <summary></summary>
        OEMCUSel = 0xEF,

        /// <summary></summary>
        OEMATTN = 0xF0,

        /// <summary></summary>
        OEMFinish = 0xF1,

        /// <summary></summary>
        OEMCopy = 0xF2,

        /// <summary></summary>
        OEMAuto = 0xF3,

        /// <summary></summary>
        OEMENLW = 0xF4,

        /// <summary></summary>
        OEMBackTab = 0xF5,

        /// <summary></summary>
        ATTN = 0xF6,

        /// <summary></summary>
        CRSel = 0xF7,

        /// <summary></summary>
        EXSel = 0xF8,

        /// <summary></summary>
        EREOF = 0xF9,

        /// <summary></summary>
        Play = 0xFA,

        /// <summary></summary>
        Zoom = 0xFB,

        /// <summary></summary>
        Noname = 0xFC,

        /// <summary></summary>
        PA1 = 0xFD,

        /// <summary></summary>
        OEMClear = 0xFE
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct CharInfo {

        [FieldOffset(0)]
        public CharUnion Char;

        [FieldOffset(2)]
        public short Attributes;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct CharUnion {

        [FieldOffset(0)]
        public char UnicodeChar;

        [FieldOffset(0)]
        public byte AsciiChar;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_CURSOR_INFO {
        public uint dwSize;
        public bool bVisible;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CONSOLE_READCONSOLE_CONTROL {
        public ulong nLength;
        public ulong nInitialChars;
        public ulong dwCtrlWakeupMask;
        public ulong dwControlKeyState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Coord {
        public short X;
        public short Y;

        public Coord(short X, short Y) {
            this.X = X;
            this.Y = Y;
        }
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct FOCUS_EVENT_RECORD {
        public uint bSetFocus;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct INPUT_RECORD {

        [FieldOffset(0)]
        public EventType EventType;

        [FieldOffset(4)]
        public KEY_EVENT_RECORD KeyEvent;

        [FieldOffset(4)]
        public MOUSE_EVENT_RECORD MouseEvent;

        [FieldOffset(4)]
        public WINDOW_BUFFER_SIZE_RECORD WindowBufferSizeEvent;

        [FieldOffset(4)]
        public MENU_EVENT_RECORD MenuEvent;

        [FieldOffset(4)]
        public FOCUS_EVENT_RECORD FocusEvent;
    };

    [StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode)]
    public struct KEY_EVENT_RECORD {

        [FieldOffset(0), MarshalAs(UnmanagedType.Bool)]
        public bool bKeyDown;

        [FieldOffset(4), MarshalAs(UnmanagedType.U2)]
        public ushort wRepeatCount;

        [FieldOffset(6), MarshalAs(UnmanagedType.U2)]
        public VirtualKeys wVirtualKeyCode;

        [FieldOffset(8), MarshalAs(UnmanagedType.U2)]
        public ushort wVirtualScanCode;

        [FieldOffset(10)]
        public char UnicodeChar;

        [FieldOffset(12), MarshalAs(UnmanagedType.U4)]
        public ControlKeyState dwControlKeyState;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct MENU_EVENT_RECORD {
        public uint dwCommandId;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct MOUSE_EVENT_RECORD {

        [FieldOffset(0)]
        public Coord dwMousePosition;

        [FieldOffset(4)]
        public VirtualKeys dwButtonState;

        [FieldOffset(8)]
        public ControlKeyState dwControlKeyState;

        [FieldOffset(12)]
        public MouseEventType dwEventFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct SMALL_RECT {
        public short Left;
        public short Top;
        public short Right;
        public short Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct WINDOW_BUFFER_SIZE_RECORD {
        public Coord dwSize;

        public WINDOW_BUFFER_SIZE_RECORD(short x, short y) {
            dwSize = new Coord(x, y);
        }
    }

    #endregion Structs/Enums

    public class RootFrameBuffer : IDisposable {
        private CharInfo[] _buffer;
        private short _bufferheight;
        private short _bufferwidth;
        private Coord _cursorPosition;
        private bool _disposed = false;
        private SafeFileHandle _hConsoleIn;
        private SafeFileHandle _hConsoleOut;
        private SMALL_RECT _rect;
        private bool _running = false;
        private int _tabStop;
        private short _x;

        private short _y;

        public RootFrameBuffer() : this(0, 0, 80, 25) {
        }

        // TODO 5: create a method to insure the handles are good to go
        public RootFrameBuffer(int X, int Y, int Width, int Height) {
            // grabs the handle for the console window
            _hConsoleOut = APICall.CreateFile("CONOUT$", 0x40000000 | 0x80000000, 2, IntPtr.Zero, FileMode.Open, 0, IntPtr.Zero);
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

        public delegate void KeyPressedDelegate(VirtualKeys KeyPressed, ControlKeyState KeyModifiers);

        public delegate void KeyReleasedDelegate(VirtualKeys KeyReleased, ControlKeyState KeyModifiers);

        public delegate void MouseButtonDoubleClickedDelegate(int X, int Y, VirtualKeys ButtonState);

        public delegate void MouseButtonClickedDelegate(int X, int Y, VirtualKeys ButtonState);

        public delegate void MouseMovedDelegate(int X, int Y);

        public delegate void RenderDelegate();

        public delegate void UpdateDelegate();

        public event KeyPressedDelegate Key_Pressed;

        public event KeyReleasedDelegate Key_Released;

        public event MouseButtonDoubleClickedDelegate MouseButton_DoubleClicked;

        public event MouseButtonClickedDelegate MouseButton_Clicked;

        public event MouseMovedDelegate Mouse_Moved;

        public event RenderDelegate Render;

        public event UpdateDelegate Update;

        public bool CursorVisible { get { return getCursorVisibility(); } }
        public int CursorX { get; set; }
        public int CursorY { get; set; }
        public int Height { get { return _bufferheight; } protected set { _bufferheight = (short)(value < 0 ? 0 : value); } }
        public int Width { get { return _bufferwidth; } protected set { _bufferwidth = (short)(value < 0 ? 0 : value); } }
        public int X { get { return _x; } set { _x = (short)(value < 0 ? 0 : value); updateBufferPos(); } }
        public int Y { get { return _y; } set { _y = (short)(value < 0 ? 0 : value); updateBufferPos(); } }

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
        /// Clears the buffer frame.
        /// </summary>
        public void Clear() {
            _buffer = new CharInfo[_bufferwidth * _bufferheight];
        }

        public void Dispose() {
            Dispose(true);

            GC.SuppressFinalize(this);
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
            StringBuilder sb = new StringBuilder();
            uint read = 0;

            if (APICall.ReadConsole(_hConsoleIn, sb, 256, out read, IntPtr.Zero)) {
                return sb.ToString(0, (int)read - 1);
            }

            return string.Empty;
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
        /// Runs the Render and Update methods in a loop.
        /// </summary>
        public void Run() {
            if (Update == null || Render == null) {
                return;
            }

            _running = true;

            while (_running) {
                getInput();
                Update();
                Render();

                Thread.Sleep(0);
            }

            Dispose();
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
                    case '\r': x = 0; y++; break;

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

        protected virtual void Dispose(bool disposing) {
            if (!_disposed) {
                if (disposing) {
                    if (_hConsoleOut != null) { _hConsoleOut.Close(); _hConsoleOut.Dispose(); _hConsoleOut = null; }
                    if (_hConsoleIn != null) { _hConsoleIn.Close(); _hConsoleIn.Dispose(); _hConsoleIn = null; }
                }

                _disposed = true;
            }
        }

        private bool getCursorVisibility() {
            CONSOLE_CURSOR_INFO cci = new CONSOLE_CURSOR_INFO();

            bool result = APICall.GetConsoleCursorInfo(_hConsoleOut, out cci);

            return cci.bVisible;
        }

        private void getInput() {
            // make sure we don't capture input if they don't use the built in events
            if (Key_Pressed == null && Key_Released == null &&
                Mouse_Moved == null && MouseButton_Clicked == null &&
                MouseButton_DoubleClicked == null) {
                return;
            }

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
                                if (Key_Pressed != null) {
                                    Key_Pressed(eventBuffer[i].KeyEvent.wVirtualKeyCode, conState);
                                }
                            } else {
                                if (Key_Released != null) {
                                    Key_Released(eventBuffer[i].KeyEvent.wVirtualKeyCode, conState);
                                }
                            }
                            break;

                        case EventType.MOUSE_EVENT:
                            if (eventBuffer[i].MouseEvent.dwEventFlags == MouseEventType.MOUSE_MOVED) {
                                if (Mouse_Moved != null) {
                                    Mouse_Moved(eventBuffer[i].MouseEvent.dwMousePosition.X, eventBuffer[i].MouseEvent.dwMousePosition.Y);
                                }
                            }

                            if (eventBuffer[i].MouseEvent.dwButtonState > 0) {
                                if (eventBuffer[i].MouseEvent.dwEventFlags == MouseEventType.DOUBLE_CLICK) {
                                    if (MouseButton_DoubleClicked != null) {
                                        MouseButton_DoubleClicked(
                                            eventBuffer[i].MouseEvent.dwMousePosition.X,
                                            eventBuffer[i].MouseEvent.dwMousePosition.Y,
                                            eventBuffer[i].MouseEvent.dwButtonState);
                                    }
                                } else {
                                    if (MouseButton_Clicked != null) {
                                        MouseButton_Clicked(
                                            eventBuffer[i].MouseEvent.dwMousePosition.X,
                                            eventBuffer[i].MouseEvent.dwMousePosition.Y,
                                            eventBuffer[i].MouseEvent.dwButtonState);
                                    }
                                }
                            }
                            break;
                    }
                }
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
    }
}