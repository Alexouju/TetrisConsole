using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

// play to understand https://www.playemulator.io/nes-online/classic-tetris/ (in Edge)
// https://tetris.fandom.com/wiki/Orientation to check rotating positions
// make sure the external console is chosen in project properties
// run without debugging to fix console window size error

class Tetris
{
    static int boardWidth = 10;
    static int boardHeight = 20;
    static bool[,] board = new bool[boardHeight, boardWidth];
    static bool[,] currentPiece;
    static int currentX, currentY;

    static int score = 0;
    static Random random = new Random();

    static void Main(string[] args)
    {
        Thread inputThread = new Thread(InputSystem);
        inputThread.Start();

        ResetPiece();

        while (true)
        {
            if (!MovePiece(0, 1))
            {
                MergePieceToBoard();
                ClearFullLines();
                ResetPiece();
                if (!PieceCanFit())
                {
                    Console.Clear();
                    Console.WriteLine("Game Over!");
                    Console.WriteLine($"Score: {score}");
                    break;
                }
            }

            RenderBoardAndBorders();
            Thread.Sleep(500);
        }
    }

    static void InputSystem()
    {
        while (true)
        {
            if (Console.KeyAvailable)
            {
                switch (Console.ReadKey(true).Key)
                {
                    case ConsoleKey.UpArrow:
                        RotatePiece();
                        break;
                    case ConsoleKey.LeftArrow:
                        MovePiece(-1, 0);
                        break;
                    case ConsoleKey.RightArrow:
                        MovePiece(1, 0);
                        break;
                    case ConsoleKey.DownArrow:
                        if (!MovePiece(0, 1))
                        {
                            MergePieceToBoard();
                            ClearFullLines();
                            ResetPiece();
                        }
                        break;
                }
            }
        }
    }

    static void RenderBoardAndBorders()
    {
        Console.Clear();
        DrawBorder();

        for (int y = 0; y < boardHeight; y++)
        {
            Console.Write("|");
            for (int x = 0; x < boardWidth; x++)
            {
                if (board[y, x] || (y >= currentY && y < currentY + currentPiece.GetLength(0) &&
                                    x >= currentX && x < currentX + currentPiece.GetLength(1) &&
                                    currentPiece[y - currentY, x - currentX]))
                {
                    Console.Write("#");
                }
                else
                {
                    Console.Write(" ");
                }
            }
            Console.WriteLine("|");
        }

        DrawBorder();
        Console.WriteLine($"Score: {score}");
    }

    static void DrawBorder()
    {
        Console.WriteLine("+" + new string('-', boardWidth) + "+");
    }

    static void ResetPiece()
    {
        currentPiece = GetRandomPiece();
        currentX = boardWidth / 2 - 2;
        currentY = 0;
    }

    static bool[,] GetRandomPiece()
    {
        List<bool[,]> pieces = new List<bool[,]> //from top to down, from left to right, I J L O S T Z
        {
            new bool[,] {
                {false, false, false, false},
                {true, true, true, true},
                {false, false, false, false},
                {false, false, false, false}},
            new bool[,] {
                {true, false, false, false},
                {true, true, true, false},
                {false, false, false, false},
                {false, false, false, false}},
            new bool[,] {
                {false, false, true, false},
                {true, true, true, false},
                {false, false, false, false},
                {false, false, false, false}},
            new bool[,] {
                {false, false, false, false},
                {false, true, true, false},
                {false, true, true, false},
                {false, false, false, false}},
            new bool[,] {
                {false, true, true, false},
                {true, true, false, false},
                {false, false, false, false},
                {false, false, false, false}},
            new bool[,] {
                {false, true, false, false},
                {true, true, true, false},
                {false, false, false, false},
                {false, false, false, false}},
            new bool[,] {
                {true, true, false, false},
                {false, true, true, false},
                {false, false, false, false},
                {false, false, false, false}
            }
        };
        return pieces[random.Next(pieces.Count)];
    }

    static bool MovePiece(int dx, int dy)
    {
        if (CanPlaceTetromino(currentPiece, currentX + dx, currentY + dy))
        {
            currentX += dx;
            currentY += dy;
            return true;
        }
        return false;
    }

    static void RotatePiece()
    {
        var newPiece = RotateMatrixClockwise(currentPiece);
        if (CanPlaceTetromino(newPiece, currentX, currentY))
        {
            currentPiece = newPiece;
        }
    }

    static bool[,] RotateMatrixClockwise(bool[,] matrix)
    {
        var newMatrix = new bool[matrix.GetLength(1), matrix.GetLength(0)];
        for (int y = 0; y < matrix.GetLength(0); y++)
        {
            for (int x = 0; x < matrix.GetLength(1); x++)
            {
                newMatrix[x, matrix.GetLength(0) - y - 1] = matrix[y, x];
            }
        }
        return newMatrix;
    }

    static void MergePieceToBoard()
    {
        for (int y = 0; y < currentPiece.GetLength(0); y++)
        {
            for (int x = 0; x < currentPiece.GetLength(1); x++)
            {
                if (currentPiece[y, x])
                {
                    int boardY = currentY + y;
                    int boardX = currentX + x;

                    if (boardY >= 0 && boardY < boardHeight)
                    {
                        board[boardY, boardX] = true;
                    }
                }
            }
        }

    }

    static void ClearFullLines()
    {
        for (int y = 0; y < boardHeight; y++)
        {
            bool lineComplete = true;
            for (int x = 0; x < boardWidth; x++)
            {
                if (!board[y, x])
                {
                    lineComplete = false;
                    break;
                }
            }

            if (lineComplete)
            {
                score += 100;
                for (int yy = y; yy > 0; yy--)
                {
                    for (int x = 0; x < boardWidth; x++)
                    {
                        board[yy, x] = board[yy - 1, x];
                    }
                }
            }
        }
    }

    static bool CanPlaceTetromino(bool[,] tetromino, int startX, int startY)
    {
        for (int y = 0; y < tetromino.GetLength(0); y++)
        {
            for (int x = 0; x < tetromino.GetLength(1); x++)
            {
                if (tetromino[y, x])
                {
                    int boardX = startX + x;
                    int boardY = startY + y;
                    if (boardX < 0 || boardX >= boardWidth || boardY >= boardHeight)
                    {
                        return false;
                    }

                    if (boardY >= 0 && board[boardY, boardX])
                    {
                        return false;
                    }
                }
            }
        }
        Debug.WriteLine($"CanPlaceTetromino({startX}, {startY})");
        return true;
    }

    static bool PieceCanFit()
    {
        return CanPlaceTetromino(currentPiece, currentX, currentY);
    }
}