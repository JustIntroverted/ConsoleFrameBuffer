/*
    I'll add some comments into the test project at some point.  For now,
    follow the instructions in the README.md file to see how to set a project
    up using ConsoleFrameBuffer.
*/

using System;
using ConsoleFrameBuffer.Test.Frames;

namespace ConsoleFrameBuffer.Test {

    internal class Program {
        public static Random RandomNumber = new Random();

        private static void Main(string[] args) {
            RootFrame rootframe = new RootFrame();
        }
    }
}