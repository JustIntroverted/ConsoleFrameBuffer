using ConsoleFrameBuffer.Test.Utility;
using System;
using System.Collections.Generic;

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
                int VIEW_RADIUS = 10;

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

        // TODO: ResetMapVisibility - eh, kind of reusing code, but whatevs
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

        public void CleanMap(string id, string toid, int loops) {
            for (int j = 0; j < loops; j++) {
                for (int x = 1; x < Tiles.GetUpperBound(0) - 1; x++) {
                    for (int y = 1; y < Tiles.GetUpperBound(1) - 1; y++) {
                        if (Tiles[x, y].ID == id && getNeighbors(x, y, id) < 3) {
                            Tiles[x, y] = new Tile(toid,
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
                double angle = slice * (double)i;
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