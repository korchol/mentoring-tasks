﻿
namespace GameOfLife
{
    class Cell
    {
        public int PositionX { get; set; }
        public int PositionY { get; set; }
        public int Age { get; set; }
        public bool IsAlive { get; set; }

        public Cell(int row, int column, int age, bool alive)
        {
            PositionX = row * 5;
            PositionY = column * 5;
            Age = age;
            IsAlive = alive;
        }

        public void Reset()
        {
            Age = 0;
            IsAlive = false;
        }
    }
}