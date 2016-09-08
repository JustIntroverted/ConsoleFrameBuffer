using System;
using System.Collections.Generic;
using System.Linq;
using ConsoleFrameBuffer.Test.Utility;

namespace ConsoleFrameBuffer.Test.Mapping {

    public abstract class Map {
        public int Width { get; protected set; }
        public int Height { get; protected set; }

        public Tile[,] Tiles { get; protected set; }

        public abstract void Generate(int Width = 80, int Height = 25);

        public void ComputeFOV(int X, int Y) {
            ComputeFOV(new Point(X, Y));
        }

        public void ComputeFOV(Point point) {
            ResetMapVisibility(point);

            for (int i = 0; i < 100; i++) {
                float x, y;

                x = (float)Math.Cos((float)i);
                y = (float)Math.Sin((float)i);

                float ox, oy;
                int VIEW_RADIUS = 5;

                ox = point.X + .5f;
                oy = point.Y + .5f;

                for (int j = 0; j < VIEW_RADIUS; j++) {
                    ox += x;
                    oy += y;

                    if (!IsOutOfBounds((int)ox, (int)oy)) {
                        if ((int)ox != point.X || (int)oy != point.Y) {
                            Tiles[(int)ox, (int)oy].IsVisible = true;
                            Tiles[(int)ox, (int)oy].IsExplored = true;

                            if (Tiles[(int)ox, (int)oy].Wall)
                                break;
                        }
                    }
                }
            }
        }

        // TODO 9: ResetMapVisibility - eh, kind of reusing code, but whatevs
        private void ResetMapVisibility(Point point) {
            for (int i = 0; i < 100; i++) {
                float x, y;

                x = (float)Math.Cos((float)i);
                y = (float)Math.Sin((float)i);

                float ox, oy;
                int VIEW_RADIUS = 50;

                ox = point.X + .5f;
                oy = point.Y + .5f;

                for (int j = 0; j < VIEW_RADIUS; j++) {
                    ox += x;
                    oy += y;

                    if (!IsOutOfBounds((int)ox, (int)oy)) {
                        if ((int)ox != point.X || (int)oy != point.Y) {
                            Tiles[(int)ox, (int)oy].IsVisible = false;
                        }
                    }
                }
            }
        }

        private void seedMap() {
            byte rndpercent = 0;

            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    rndpercent = (byte)Program.RandomNumber.Next(0, 100);

                    if (rndpercent < 50) {
                        Tiles[x, y] = new Tile(
                            ".",
                            ConsoleColor.Gray,
                            ConsoleColor.Black,
                            true, false);
                    } else {
                        Tiles[x, y] = new Tile(
                            ((char)219).ToString(),
                            ConsoleColor.Gray,
                            ConsoleColor.Black,
                            false, true);
                    }
                }
            }

            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    Tiles[x, 0] = new Tile(
                            ((char)219).ToString(),
                            ConsoleColor.Gray,
                            ConsoleColor.Black,
                            false, true);           // top
                    Tiles[Width - 1, y] = new Tile(
                            ((char)219).ToString(),
                            ConsoleColor.Gray,
                            ConsoleColor.Black,
                            false, true);   // right
                    Tiles[0, y] = new Tile(
                            ((char)219).ToString(),
                            ConsoleColor.Gray,
                            ConsoleColor.Black,
                            false, true);           // left
                    Tiles[x, Height - 1] = new Tile(
                            ((char)219).ToString(),
                            ConsoleColor.Gray,
                            ConsoleColor.Black,
                            false, true);  // bottom
                }
            }
        }

        public void FillFromPoint(Point point) {
            restart:
            seedMap();

            List<Point> Filled = new List<Point>();
            Point curPoint = point;
            List<Point> Neighbors = getArea(4, 1, curPoint);

            Filled.Add(curPoint);

            while (Neighbors.Count > 0) {
                curPoint = Neighbors[0];

                List<Point> newNeighbors = getArea(4, 1, curPoint);

                foreach (Point p in newNeighbors) {
                    if (!IsOutOfBounds(p.X, p.Y) && Tiles[p.X, p.Y].Walkable &&
                        !Neighbors.Any(xy => xy.X == p.X && xy.Y == p.Y) &&
                        !Filled.Any(xy => xy.X == p.X && xy.Y == p.Y)) {
                        Neighbors.Add(p);
                    }
                }

                if (!Filled.Any(xy => xy.X == curPoint.X && xy.Y == curPoint.Y)) Filled.Add(curPoint);
                Neighbors.Remove(curPoint);
            }

            int minCellCount = 500;

            if (Filled.Count < minCellCount) {
                goto restart;
            }

            for (int x = 0; x < Width; x++) {
                for (int y = 0; y < Height; y++) {
                    if (!containsPoint(Filled, new Point(x, y))) {
                        Tiles[x, y] = new Tile(
                            ((char)219).ToString(),
                            ConsoleColor.Gray,
                            ConsoleColor.Black,
                            false, true);
                    }
                }
            }

            CleanMap(((char)219).ToString(), ".", 20);
        }

        private bool containsPoint(List<Point> list, Point point) {
            for (int i = 0; i < list.Count; i++) {
                if (list[i].X == point.X && list[i].Y == point.Y)
                    return true;
            }

            return false;
        }

        public void CleanMap(string id, string toid, int loops) {
            for (int j = 0; j < loops; j++) {
                for (int x = 1; x < Tiles.GetUpperBound(0) - 1; x++) {
                    for (int y = 1; y < Tiles.GetUpperBound(1) - 1; y++) {
                        if (Tiles[x, y].ID == id && getNeighbors(x, y, id) < 3) {
                            Tiles[x, y] = new Tile(toid,
                                                    ConsoleColor.Gray,
                                                    ConsoleColor.Black,
                                                    true, false);
                        }
                    }
                }
            }
        }

        private int getNeighbors(int x, int y, string id) {
            int count = 0;

            List<Point> list = getArea(9, 1, new Point(x, y));

            foreach (Point p in list) {
                if (!IsOutOfBounds(p.X, p.Y)) {
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
                double angle = slice * i;
                double newX = (center.X + radius * Math.Cos(angle));
                double newY = (center.Y + radius * Math.Sin(angle));

                list.Add(new Point((int)(float)Math.Round((float)newX, MidpointRounding.AwayFromZero),
                    (int)(float)Math.Round((float)newY, MidpointRounding.AwayFromZero)));
            }

            return list;
        }

        public bool IsWalkable(int x, int y) {
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