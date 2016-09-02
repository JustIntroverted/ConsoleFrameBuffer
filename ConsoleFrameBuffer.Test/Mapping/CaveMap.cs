using ConsoleFrameBuffer.Test.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleFrameBuffer.Test.Mapping {

    internal class CaveMap : Map {
        public Point StartPos;
        public Point DownFloor;

        public override void Generate(int Width = 80, int Height = 25) {
            this.Width = Width;
            this.Height = Height;

            string[] stoneTiles = new string[]
            {
                ",", "."
            };

            this.Tiles = new Tile[Width + 1, Height + 1];

            for (int x = 0; x < this.Tiles.GetUpperBound(0); x++)
                for (int y = 0; y < this.Tiles.GetUpperBound(1); y++) {
                    this.Tiles[x, y] = new Tile(((char)219).ToString(),
                                            ConsoleColor.Gray,
                                            ConsoleColor.Black,
                                            false, true);
                }

            int floorCount = 0;

            Point Digger = new Point(Width / 2, Height / 2);
            this.Tiles[Digger.X, Digger.Y] = new Tile(
                stoneTiles[Program.RandomNumber.Next(0, stoneTiles.Length)],
                ConsoleColor.Gray,
                ConsoleColor.Black,
                true);

            StartPos = new Point(Digger.X, Digger.Y);

            while (floorCount < (Width * Height) / 50) {
                Console.SetCursorPosition(0, 0);
                Console.Write(floorCount);
                int dir = Program.RandomNumber.Next(0, 4);

                if (dir == 0 && !IsOutOfBounds(Digger.X, Digger.Y - 2)) {
                    if (!IsFloor(Digger.X, Digger.Y - 1)) floorCount++;

                    this.Tiles[Digger.X, Digger.Y - 1] = new Tile(
                        stoneTiles[Program.RandomNumber.Next(0, stoneTiles.Length)],
                        ConsoleColor.Gray,
                        ConsoleColor.Black,
                        true);

                    Digger.Y--;
                } else if (dir == 1 && !IsOutOfBounds(Digger.X + 3, Digger.Y)) {
                    if (!IsFloor(Digger.X + 1, Digger.Y)) floorCount++;

                    this.Tiles[Digger.X + 1, Digger.Y] = new Tile(
                        stoneTiles[Program.RandomNumber.Next(0, stoneTiles.Length)],
                        ConsoleColor.Gray,
                        ConsoleColor.Black,
                        true);

                    Digger.X++;
                } else if (dir == 2 && !IsOutOfBounds(Digger.X, Digger.Y + 3)) {
                    if (!IsFloor(Digger.X, Digger.Y + 1)) floorCount++;

                    this.Tiles[Digger.X, Digger.Y + 1] = new Tile(
                        stoneTiles[Program.RandomNumber.Next(0, stoneTiles.Length)],
                        ConsoleColor.Gray,
                        ConsoleColor.Black,
                        true);

                    Digger.Y++;
                } else if (dir == 3 && !IsOutOfBounds(Digger.X - 2, Digger.Y)) {
                    if (!IsFloor(Digger.X - 1, Digger.Y)) floorCount++;

                    this.Tiles[Digger.X - 1, Digger.Y] = new Tile(
                        stoneTiles[Program.RandomNumber.Next(0, stoneTiles.Length)],
                        ConsoleColor.Gray,
                        ConsoleColor.Black,
                        true);

                    Digger.X--;
                }
            }

            // Clean the walls up
            CleanMap(((char)219).ToString(), stoneTiles[Program.RandomNumber.Next(0, stoneTiles.Length)], 10);

            int distance;

            while (true) {
                DownFloor = new Point(Program.RandomNumber.Next(0, Width), Program.RandomNumber.Next(0, Height));
                distance = (int)Math.Sqrt((Math.Pow(DownFloor.X - StartPos.X, 2) + Math.Pow(DownFloor.Y - StartPos.Y, 2)));

                if (distance > 40 && IsFloor(DownFloor.X, DownFloor.Y)) break;
            }

            this.Tiles[StartPos.X, StartPos.Y].ID = "^";
            this.Tiles[StartPos.X, StartPos.Y].ForegroundColor = ConsoleColor.Cyan;
            this.Tiles[DownFloor.X, DownFloor.Y].ID = ">";
            this.Tiles[DownFloor.X, DownFloor.Y].ForegroundColor = ConsoleColor.Red;
        }
    }
}