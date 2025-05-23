using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace GameOfLife
{
    class Grid
    {

        private int SizeX;
        private int SizeY;
        private Cell[,] cells;
        private Cell[,] nextGenerationCells;
        private static Random rnd;
        private Canvas drawCanvas;
        private Ellipse[,] cellsVisuals;


        public Grid(Canvas c)
        {
            drawCanvas = c;
            rnd = new Random();
            SizeX = (int) (c.Width / 5);
            SizeY = (int)(c.Height / 5);
            cells = new Cell[SizeX, SizeY];
            nextGenerationCells = new Cell[SizeX, SizeY];
            cellsVisuals = new Ellipse[SizeX, SizeY];

            for (int i = 0; i < SizeX; i++)
                for (int j = 0; j < SizeY; j++)
                {
                    cells[i, j] = new Cell(i, j, 0, false);
                    nextGenerationCells[i, j] = new Cell(i, j, 0, false);
                }

            SetRandomPattern();
            InitCellsVisuals();
            UpdateGraphics();

        }


        public void Clear()
        {
            for (int i = 0; i < SizeX; i++)
                for (int j = 0; j < SizeY; j++)
                {
                    cells[i, j].Reset();
                    nextGenerationCells[i, j].Reset();
                    cellsVisuals[i, j].Fill = Brushes.Gray;
                }
        }


        void MouseMove(object sender, MouseEventArgs e)
        {
            var cellVisual = sender as Ellipse;

            int i = (int) cellVisual.Margin.Left / 5;
            int j = (int) cellVisual.Margin.Top / 5;


            if (e.LeftButton == MouseButtonState.Pressed)
            {
                if (!cells[i, j].IsAlive)
                {
                    cells[i, j].IsAlive = true;
                    cells[i, j].Age = 0;
                    cellVisual.Fill = Brushes.White;
                }
            }
        }

        public void UpdateGraphics()
        {
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    Brush newColor = cells[i, j].IsAlive
                        ? (cells[i, j].Age < 2 ? Brushes.White : Brushes.DarkGray)
                        : Brushes.Gray;

                    // update only if color changed
                    if (cellsVisuals[i, j].Fill != newColor)
                    {
                        cellsVisuals[i, j].Fill = newColor;
                    }
                }
            }
        }

        public void InitCellsVisuals()
        {
            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    cellsVisuals[i, j] = new Ellipse
                    {
                        Width = 5,
                        Height = 5,
                        Margin = new Thickness(cells[i, j].PositionX, cells[i, j].PositionY, 0, 0),
                        Fill = Brushes.Gray
                    };
                    drawCanvas.Children.Add(cellsVisuals[i, j]);
                }
            }

            // subscribe by canvas insted of every single ellipse
            drawCanvas.MouseMove += CanvasMouseMove;
            drawCanvas.MouseLeftButtonDown += CanvasMouseMove;
            UpdateGraphics();
        }

        private void CanvasMouseMove(object sender, MouseEventArgs e)
        {
            Point position = e.GetPosition(drawCanvas);
            int i = (int)(position.X / 5);
            int j = (int)(position.Y / 5);

            if (i >= 0 && i < SizeX && j >= 0 && j < SizeY)
            {
                if (e.LeftButton == MouseButtonState.Pressed && !cells[i, j].IsAlive)
                {
                    cells[i, j].IsAlive = true;
                    cells[i, j].Age = 0;
                    cellsVisuals[i, j].Fill = Brushes.White;
                }
            }
        }


        public static bool GetRandomBoolean()
        {
            return rnd.NextDouble() > 0.8;
        }

        public void SetRandomPattern()
        {
            for (int i = 0; i < SizeX; i++)
                for (int j = 0; j < SizeY; j++)
                    cells[i, j].IsAlive = GetRandomBoolean();
        }

        public void UpdateToNextGeneration()
        {
            // reassign instead of big iteration
            var temp = cells;
            cells = nextGenerationCells;
            nextGenerationCells = temp;

            UpdateGraphics();
        }


        public void Update()
        {
            bool alive = false;
            int age = 0;

            for (int i = 0; i < SizeX; i++)
            {
                for (int j = 0; j < SizeY; j++)
                {
                    CalculateNextGeneration(i, j, ref alive, ref age);
                    nextGenerationCells[i, j].IsAlive = alive;
                    nextGenerationCells[i, j].Age = age;
                }
            }
            UpdateToNextGeneration();
        }

        public void CalculateNextGeneration(int row, int column, ref bool isAlive, ref int age)
        {
            isAlive = cells[row, column].IsAlive;
            age = cells[row, column].Age;

            int count = CountNeighbors(row, column);

            // simple avoiding too mamy unneccessary ifs
            if (isAlive && count < 2)
            {
                isAlive = false;
                age = 0;
            }

            else if (isAlive && (count == 2 || count == 3))
            {
                cells[row, column].Age++;
                isAlive = true;
                age = cells[row, column].Age;
            }

            else if (isAlive && count > 3)
            {
                isAlive = false;
                age = 0;
            }

            else if (!isAlive && count == 3)
            {
                isAlive = true;
                age = 0;
            }
        }

        public int CountNeighbors(int i, int j)
        {
            int count = 0;

            if (i != SizeX - 1 && cells[i + 1, j].IsAlive) count++;
            if (i != SizeX - 1 && j != SizeY - 1 && cells[i + 1, j + 1].IsAlive) count++;
            if (j != SizeY - 1 && cells[i, j + 1].IsAlive) count++;
            if (i != 0 && j != SizeY - 1 && cells[i - 1, j + 1].IsAlive) count++;
            if (i != 0 && cells[i - 1, j].IsAlive) count++;
            if (i != 0 && j != 0 && cells[i - 1, j - 1].IsAlive) count++;
            if (j != 0 && cells[i, j - 1].IsAlive) count++;
            if (i != SizeX - 1 && j != 0 && cells[i + 1, j - 1].IsAlive) count++;

            return count;
        }
    }
}