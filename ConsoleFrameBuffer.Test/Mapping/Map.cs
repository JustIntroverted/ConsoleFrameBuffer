using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleFrameBuffer.Test.Mapping {

    public class Point {
        public int X { get; set; }
        public int Y { get; set; }

        public Point() : this(0, 0) {
        }

        public Point(int X, int Y) {
            this.X = X;
            this.Y = Y;
        }
    }

    public struct Camera {
        public int X;
        public int Y;
        private Point currentPoint;

        public void FixCamera(Point p, int width, int height) {
            this.X = ((p.X + 1) - (width / 2) - 1);
            this.Y = ((p.Y - 0) - (height / 2) - 1);

            currentPoint = p;
        }

        public Point GetPoint() {
            return currentPoint;
        }
    }

    public abstract class Map {
        public int Width;
        public int Height;

        public Tile[,] Tiles;

        public virtual void Generate(int Width = 80, int Height = 25) {
        }

        public void CleanMap(string id, string toid, int loops) {
            for (int j = 0; j < loops; j++) {
                for (int x = 1; x < this.Tiles.GetUpperBound(0) - 1; x++) {
                    for (int y = 1; y < this.Tiles.GetUpperBound(1) - 1; y++) {
                        if (Tiles[x, y].ID == id && getNeighbors(x, y, id) < 5) {
                            this.Tiles[x, y] = new Tile(toid,
                                                    ConsoleColor.Gray,
                                                    ConsoleColor.Black,
                                                    true);
                        }
                    }
                }
            }
        }

        private int getNeighbors(int x, int y, string id) {
            int count = 0;

            List<Point> list = getArea(9, 1, new Point(x, y));

            foreach (Point p in list) {
                if (!(p.X < 0 || p.X > Tiles.GetUpperBound(0) - 1 || p.Y < 0 || p.Y > Tiles.GetUpperBound(1) - 1)) {
                    if (Tiles[p.X, p.Y].ID == id)
                        count++;
                }
            }

            return count;
        }

        private List<Point> getArea(int points, double radius, Point center) {
            List<Point> list = new List<Point>();
            double slice = 2 * Math.PI / points;

            for (int i = 0; i < points; i++) {
                double angle = slice * (double)i;
                double newX = (center.X + radius * Math.Cos(angle));
                double newY = (center.Y + radius * Math.Sin(angle));

                list.Add(new Point((int)(float)Math.Round((float)newX, MidpointRounding.AwayFromZero),
                    (int)(float)Math.Round((float)newY, MidpointRounding.AwayFromZero)));
            }

            return list;
        }

        public bool IsFloor(int x, int y) {
            if (Tiles[x, y].Walkable) return true;

            return false;
        }

        public bool IsOutOfBounds(int x, int y) {
            if (x < 0 || x > Width - 1) return true;
            if (y < 0 || y > Height - 1) return true;

            return false;
        }
    }
}