using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace ConsoleFrameBuffer.API {

    public static class APICall {

        #region DLL Imports

        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Auto)]
        public static extern SafeFileHandle CreateFile(
            string fileName,
            [MarshalAs(UnmanagedType.U4)] uint fileAccess,
            [MarshalAs(UnmanagedType.U4)] uint fileShare,
            IntPtr securityAttributes,
            [MarshalAs(UnmanagedType.U4)] FileMode creationDisposition,
            [MarshalAs(UnmanagedType.U4)] int flags,
            IntPtr template);

        [DllImport("kernel32.dll")]
        public static extern bool FlushConsoleInputBuffer(
            SafeFileHandle hConsoleInput);

        [DllImport("msvcrt.dll")]
        public static extern int _getch();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool GetConsoleCursorInfo(
            SafeFileHandle hConsoleOutput,
            out CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

        [DllImport("kernel32.dll")]
        public static extern bool GetConsoleMode(
            SafeFileHandle hConsoleHandle,
            out uint lpMode);

        [DllImport("kernel32.dll")]
        public static extern bool GetNumberOfConsoleInputEvents(
            SafeFileHandle hConsoleInput,
            out uint lpcNumberOfEvents);

        [DllImport("msvcrt.dll", CallingConvention = CallingConvention.Cdecl)]
        public static extern int _kbhit();

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool ReadConsole(
            SafeFileHandle hConsoleInput,
            [Out] StringBuilder lpBuffer,
            [MarshalAs(UnmanagedType.U4)] uint nNumberOfCharsToRead,
            [MarshalAs(UnmanagedType.U4)] out uint lpNumberOfCharsRead,
            IntPtr pInputControl);

        [DllImport("kernel32.dll", EntryPoint = "ReadConsoleInputW", CharSet = CharSet.Unicode)]
        public static extern bool ReadConsoleInput(
            SafeFileHandle hConsoleInput,
            [Out] INPUT_RECORD[] lpBuffer,
            uint nLength,
            out uint lpNumberOfEventsRead);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCursorInfo(
            SafeFileHandle hConsoleOutput,
            [In] ref CONSOLE_CURSOR_INFO lpConsoleCursorInfo);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetConsoleCursorPosition(
            SafeFileHandle hConsoleOutput,
            Coord dwCursorPosition);

        [DllImport("kernel32.dll")]
        public static extern bool SetConsoleMode(
            SafeFileHandle hConsoleHandle,
            uint dwMode);

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool WriteConsoleOutput(
            SafeFileHandle hConsoleOutput,
            CharInfo[] lpBuffer,
            Coord dwBufferSize,
            Coord dwBufferCoord,
            ref SMALL_RECT lpWriteRegion);

        #endregion DLL Imports
    }
}