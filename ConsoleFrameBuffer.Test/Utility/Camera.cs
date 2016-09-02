namespace ConsoleFrameBuffer.Test.Utility {

    public class Camera {
        public int X { get; protected set; }
        public int Y { get; protected set; }
        private Point currentPoint;

        public void FixCamera(Point p, int width, int height) {
            X = ((p.X + 1) - (width / 2) - 1);
            Y = ((p.Y - 1) - (height / 2) - 1);

            currentPoint = p;
        }

        public Point GetPoint() {
            return currentPoint;
        }
    }
}