using ConsoleFrameBuffer.Test.Utility;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ConsoleFrameBuffer.Test.Mapping {

    internal class CaveMap : Map {
        public Point UpFloorPosition;
        public Point DownFloorPosition;

        private string[] _stoneTiles = new string[]
        {
                ",", ".", ";"
        };

        public override void Generate(int Width = 80, int Height = 25) {
            this.Width = Width;
            this.Height = Height;

            Tiles = new Tile[Width + 1, Height + 1];

            FillFromPoint(new Point(Width / 2, Height / 2));

            int distance;

            while (true) {
                UpFloorPosition = new Point(Program.RandomNumber.Next(0, Width), Program.RandomNumber.Next(0, Height));

                if (Tiles[UpFloorPosition.X, UpFloorPosition.Y].Walkable)
                    break;
            }

            while (true) {
                DownFloorPosition = new Point(Program.RandomNumber.Next(0, Width), Program.RandomNumber.Next(0, Height));
                distance = (int)Math.Sqrt((Math.Pow(DownFloorPosition.X - UpFloorPosition.X, 2) + Math.Pow(DownFloorPosition.Y - UpFloorPosition.Y, 2)));

                if (distance > 25 && IsWalkable(DownFloorPosition.X, DownFloorPosition.Y)) break;
            }

            Tiles[UpFloorPosition.X, UpFloorPosition.Y].ID = "<";
            Tiles[UpFloorPosition.X, UpFloorPosition.Y].ForegroundColor = ConsoleColor.Cyan;
            Tiles[DownFloorPosition.X, DownFloorPosition.Y].ID = ">";
            Tiles[DownFloorPosition.X, DownFloorPosition.Y].ForegroundColor = ConsoleColor.Red;
        }

        private void diggerMode() {
            for (int x = 0; x < Tiles.GetUpperBound(0); x++)
                for (int y = 0; y < Tiles.GetUpperBound(1); y++) {
                    Tiles[x, y] = new Tile(((char)219).ToString(),
                                            ConsoleColor.Gray,
                                            ConsoleColor.Black,
                                            false, true);
                }

            int floorCount = 0;

            Point Digger = new Point(Width / 2, Height / 2);
            Tiles[Digger.X, Digger.Y] = new Tile(
                _stoneTiles[Program.RandomNumber.Next(0, _stoneTiles.Length)],
                ConsoleColor.Gray,
                ConsoleColor.Black,
                true);

            UpFloorPosition = new Point(Digger.X, Digger.Y);

            while (floorCount < (Width * Height) / 50) {
                int dir = Program.RandomNumber.Next(0, 4);

                if (dir == 0 && !IsOutOfBounds(Digger.X, Digger.Y - 2)) {
                    if (!IsWalkable(Digger.X, Digger.Y - 1)) floorCount++;

                    Tiles[Digger.X, Digger.Y - 1] = new Tile(
                        _stoneTiles[Program.RandomNumber.Next(0, _stoneTiles.Length)],
                        ConsoleColor.Gray,
                        ConsoleColor.Black,
                        true);

                    Digger.Y--;
                } else if (dir == 1 && !IsOutOfBounds(Digger.X + 3, Digger.Y)) {
                    if (!IsWalkable(Digger.X + 1, Digger.Y)) floorCount++;

                    Tiles[Digger.X + 1, Digger.Y] = new Tile(
                        _stoneTiles[Program.RandomNumber.Next(0, _stoneTiles.Length)],
                        ConsoleColor.Gray,
                        ConsoleColor.Black,
                        true);

                    Digger.X++;
                } else if (dir == 2 && !IsOutOfBounds(Digger.X, Digger.Y + 3)) {
                    if (!IsWalkable(Digger.X, Digger.Y + 1)) floorCount++;

                    Tiles[Digger.X, Digger.Y + 1] = new Tile(
                        _stoneTiles[Program.RandomNumber.Next(0, _stoneTiles.Length)],
                        ConsoleColor.Gray,
                        ConsoleColor.Black,
                        true);

                    Digger.Y++;
                } else if (dir == 3 && !IsOutOfBounds(Digger.X - 2, Digger.Y)) {
                    if (!IsWalkable(Digger.X - 1, Digger.Y)) floorCount++;

                    Tiles[Digger.X - 1, Digger.Y] = new Tile(
                        _stoneTiles[Program.RandomNumber.Next(0, _stoneTiles.Length)],
                        ConsoleColor.Gray,
                        ConsoleColor.Black,
                        true);

                    Digger.X--;
                }
            }

            // Clean the walls up
            CleanMap(((char)219).ToString(), _stoneTiles[Program.RandomNumber.Next(0, _stoneTiles.Length)], 10);

            int distance;

            while (true) {
                DownFloorPosition = new Point(Program.RandomNumber.Next(0, Width), Program.RandomNumber.Next(0, Height));
                distance = (int)Math.Sqrt((Math.Pow(DownFloorPosition.X - UpFloorPosition.X, 2) + Math.Pow(DownFloorPosition.Y - UpFloorPosition.Y, 2)));

                if (distance > 25 && IsWalkable(DownFloorPosition.X, DownFloorPosition.Y)) break;
            }

            Tiles[UpFloorPosition.X, UpFloorPosition.Y].ID = "<";
            Tiles[UpFloorPosition.X, UpFloorPosition.Y].ForegroundColor = ConsoleColor.Cyan;
            Tiles[DownFloorPosition.X, DownFloorPosition.Y].ID = ">";
            Tiles[DownFloorPosition.X, DownFloorPosition.Y].ForegroundColor = ConsoleColor.Red;
        }
    }
}