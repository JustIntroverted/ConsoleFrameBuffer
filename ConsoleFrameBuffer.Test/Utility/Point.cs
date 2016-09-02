namespace ConsoleFrameBuffer.Test.Utility {

    public class Point {
        public int X { get; set; }
        public int Y { get; set; }

        public Point() : this(0, 0) {
        }

        public Point(int X, int Y) {
            this.X = X;
            this.Y = Y;
        }

        public static Point operator +(Point one, Point two) {
            return new Point(one.X + two.X, one.Y + two.Y);
        }
    }
}