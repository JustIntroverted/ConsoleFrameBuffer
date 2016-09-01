using System;

namespace ConsoleFrameBuffer.Test.Mapping {

    public class Tile {
        public string ID { get; set; }
        public ConsoleColor ForegroundColor { get; set; }
        public ConsoleColor BackgroundColor { get; set; }
        public bool Walkable { get; set; }
        public bool Wall { get; set; }
        public bool IsExplored { get; set; }

        public Tile() : this(string.Empty) {
        }

        public Tile(string ID, ConsoleColor ForegroundColor = ConsoleColor.Gray,
            ConsoleColor BackgroundColor = ConsoleColor.Black, bool Walkable = true, bool Wall = false) {
            this.ID = ID;
            this.ForegroundColor = ForegroundColor;
            this.BackgroundColor = BackgroundColor;
            this.Walkable = Walkable;
            this.Wall = Wall;
            this.IsExplored = false;
        }
    }
}