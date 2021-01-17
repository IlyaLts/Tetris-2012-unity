/*
===============================================================================
    Copyright (C) 2020 Ilya Lyakhovets
    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU General Public License as published by
    the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.
    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU General Public License for more details.
    You should have received a copy of the GNU General Public License
    along with this program.  If not, see <http://www.gnu.org/licenses/>.
===============================================================================
*/

using UnityEngine;
using UnityEngine.UI;
using System;

namespace IlyaLts.Tetris
{
    public struct Block
    {
        public const int width = 32;
        public const int height = 32;

        public enum Color
        {
            Cyen,
            Blue,
            Orange,
            Yellow,
            Green,
            Purple,
            Red
        };

        public bool filled;
        public Color clr;
        public GameObject obj;
    }

    public class Figure
    {
        public static readonly int[, , ,] figures = new int [NumOfFigures, numOfRotations, height, height]
        {
            // 1
            {
                {
                    {0, 0, 0, 0},
                    {1, 1, 1, 1},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {0, 1, 0, 0},
                    {0, 1, 0, 0},
                    {0, 1, 0, 0},
                    {0, 1, 0, 0},
                },
                {
                    {0, 0, 0, 0},
                    {1, 1, 1, 1},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {0, 1, 0, 0},
                    {0, 1, 0, 0},
                    {0, 1, 0, 0},
                    {0, 1, 0, 0},
                },
            },
            // 2
            {
                {
                    {1, 1, 1, 0},
                    {0, 0, 1, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {1, 1, 0, 0},
                    {1, 0, 0, 0},
                    {1, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {1, 0, 0, 0},
                    {1, 1, 1, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {0, 1, 0, 0},
                    {0, 1, 0, 0},
                    {1, 1, 0, 0},
                    {0, 0, 0, 0},
                },
            },
            // 3
            {
                {
                    {1, 1, 1, 0},
                    {1, 0, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {1, 0, 0, 0},
                    {1, 0, 0, 0},
                    {1, 1, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {0, 0, 1, 0},
                    {1, 1, 1, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {1, 1, 0, 0},
                    {0, 1, 0, 0},
                    {0, 1, 0, 0},
                    {0, 0, 0, 0},
                },
            },
            // 4
            {
                {
                    {1, 1, 0, 0},
                    {1, 1, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {1, 1, 0, 0},
                    {1, 1, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {1, 1, 0, 0},
                    {1, 1, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {1, 1, 0, 0},
                    {1, 1, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
            },
            // 5
            {
                {
                    {0, 1, 1, 0},
                    {1, 1, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {1, 0, 0, 0},
                    {1, 1, 0, 0},
                    {0, 1, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {0, 1, 1, 0},
                    {1, 1, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {1, 0, 0, 0},
                    {1, 1, 0, 0},
                    {0, 1, 0, 0},
                    {0, 0, 0, 0},
                },
            },
            // 6
            {
                {
                    {1, 1, 1, 0},
                    {0, 1, 0, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {0, 1, 0, 0},
                    {0, 1, 1, 0},
                    {0, 1, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {0, 1, 0, 0},
                    {1, 1, 1, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {0, 1, 0, 0},
                    {1, 1, 0, 0},
                    {0, 1, 0, 0},
                    {0, 0, 0, 0},
                },
            },
            // 7
            {
                {
                    {1, 1, 0, 0},
                    {0, 1, 1, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {0, 1, 0, 0},
                    {1, 1, 0, 0},
                    {1, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {1, 1, 0, 0},
                    {0, 1, 1, 0},
                    {0, 0, 0, 0},
                    {0, 0, 0, 0},
                },
                {
                    {0, 1, 0, 0},
                    {1, 1, 0, 0},
                    {1, 0, 0, 0},
                    {0, 0, 0, 0},
                }
            }
        };

        public const int NumOfFigures = 7;
        public const int numOfRotations = 4;
        public const int width = 4;
        public const int height = 4;

        public int x = 0;
        public int y = 0;
        public int rot = 0;
        public int num = 0;
        public int numNext = 99999;
        public Block[,] blocks = new Block[width, height];

        public void New(int x, int y)
        {
            this.x = x;
            this.y = y;
            rot = 0;

            if (numNext > NumOfFigures)
                num = UnityEngine.Random.Range(0, NumOfFigures);
            else
                num = numNext;

            numNext = UnityEngine.Random.Range(0, NumOfFigures);

            for (int i = 0; i < width; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    blocks[i, j].filled = Convert.ToBoolean(figures[num, rot, j, i]);
                    blocks[i, j].clr = (Block.Color) num;
                    blocks[i, j].obj = null;
                }
            }
        }

        public void Rotate()
        {
            rot++;
            if (rot == numOfRotations) rot = 0;

            for (int i = 0; i < height; i++)
            {
                for (int j = 0; j < height; j++)
                {
                    blocks[i, j].filled = Convert.ToBoolean(figures[num, rot, j, i]);
                    blocks[i, j].clr = (Block.Color) num;
                }
            }
        }

        public void Move(int x, int y)
        {
            this.x += x;
            this.y -= y;

            for (int i = 0; i < Figure.width; i++)
            {
                for (int j = 0; j < Figure.height; j++)
                {   
                    if (blocks[i, j].obj)
                    {
                        Transform temp = blocks[i, j].obj.GetComponent<Transform>();
                        temp.position = new Vector3(temp.position.x + Block.width * x, temp.position.y + Block.height * y);
                    }
                }
            }
        }
    }
}
