using System.Drawing;
using MazeCreator.Core;

namespace MazeEngine
{
    public class MazeDrawer
    {
        enum position
        {
            wallFront,
            wallLeft,
            wallCorridorLeft,
            wallRight,
            wallCorridorRight,
            ceiling,
            ceilingCorridorLeft,
            ceilingCorridorRight,
            floor,
            floorCorridorLeft,
            floorCorridorRight,
        }

        enum objectType
        {
            regular,
            start,
            finish
        }

        public enum direction
        {
            up,
            down,
            left,
            right
        }

        private bool[,] maze;

        private int frameSize;
        private int hFrame;

        private int size;

        private int startX, startY;
        private int finishX, finishY;

        private bool drawerFlag;

        public Color emptyFrame { get; set; }
        public Color wallSurface { get; set; }
        public Color wallBorder { get; set; }
        public Color ceilingSurface { get; set; }
        public Color ceilingBorder { get; set; }
        public Color floorSurface { get; set; }
        public Color floorBorder { get; set; }
        public Color startSurface { get; set; }
        public Color startBorder { get; set; }
        public Color finishSurface { get; set; }
        public Color finishBorder { get; set; }

        public int X { get; private set; }
        public int Y { get; private set; }

        public direction dir { get; set; }

        public bool isOnFinish
        {
            get => X == finishX && Y == finishY;
        }

        public bool CheckUp { get => checkUp(X, Y); }
        public bool CheckDown { get => checkDown(X, Y); }
        public bool CheckLeft { get => checkLeft(X, Y); }
        public bool CheckRight { get => checkRight(X, Y); }

        public MazeDrawer(int frameSize, int mazeSize)
        {

            this.frameSize = frameSize;
            this.hFrame = frameSize / 2;

            this.size = mazeSize + 1;
            mazeSize /= 2;

            var m = Creator.GetCreator().Create(mazeSize, mazeSize);

            maze = new bool[size, size];

            for (int i = 0; i < mazeSize; i++)
                for (int j = 0; j < mazeSize; j++)
                {
                    maze[i * 2 + 1, j * 2 + 1] = true;

                    maze[i * 2 + 1, j * 2] = !m[i, j].HasLeftWall;

                    maze[i * 2, j * 2 + 1] = !m[i, j].HasTopWall;
                }

            startX = 1;
            startY = 1;

            finishX = this.size - 2;
            finishY = this.size - 2;

            X = startX;
            Y = startY;

            if (CheckDown)
                dir = direction.down;
            else if (CheckRight)
                dir = direction.right;
            else if (CheckLeft)
                dir = direction.left;
            else if (CheckUp)
                dir = direction.up;
        }

        public void TurnLeft() => dir = getLeftDirection();

        public void TurnRight() => dir = getRightDirection();

        public bool StepForward()
        {
            switch (dir)
            {
                case direction.up:
                    if (CheckUp)
                    {
                        X--;
                        return true;
                    }
                    break;

                case direction.left:
                    if (CheckLeft)
                    {
                        Y--;
                        return true;
                    }
                    break;

                case direction.down:
                    if (CheckDown)
                    {
                        X++;
                        return true;
                    }
                    break;

                case direction.right:
                    if (CheckRight)
                    {
                        Y++;
                        return true;
                    }
                    break;
            };

            return false;
        }

        public bool StepBackward()
        {
            switch (dir)
            {
                case direction.down:
                    if (CheckUp)
                    {
                        X--;
                        return true;
                    }
                    break;

                case direction.right:
                    if (CheckLeft)
                    {
                        Y--;
                        return true;
                    }
                    break;

                case direction.up:
                    if (CheckDown)
                    {
                        X++;
                        return true;
                    }
                    break;

                case direction.left:
                    if (CheckRight)
                    {
                        Y++;
                        return true;
                    }
                    break;
            };

            return false;
        }

        public Image GetImage(bool switchMode = false)
        {
            if (switchMode)
                drawerFlag = !drawerFlag;

            return drawerFlag ? DrawMap() : DrawFrame(); 
        }

        public Image DrawMap()
        {
            var map = new Bitmap(frameSize, frameSize);

            float s = frameSize / (float)size;

            using (var g = Graphics.FromImage(map))
            {
                g.Clear(emptyFrame);

                Pen pen;
                Brush brush;

                for (int i = 0; i < size; i++)
                    for (int j = 0; j < size; j++)
                    {
                        if (!maze[i, j])
                            (pen, brush) = getInstruments(position.wallFront);
                        else if (i == startX && j == startY)
                            (pen, brush) = getInstruments(position.wallFront, objectType.start);
                        else if (i == finishX && j == finishY)
                            (pen, brush) = getInstruments(position.wallFront, objectType.finish);
                        else
                            (pen, brush) = getInstruments(position.floor);

                        g.FillRectangle(brush, j * s, i * s, s, s);
                        g.DrawRectangle(pen, j * s, i * s, s, s);
                    }

                var points = dir switch
                {
                    direction.up => new PointF[]
                    {
                        new PointF(Y * s + s/2f, X * s),
                        new PointF(Y * s, X * s + s),
                        new PointF(Y * s + s, X * s + s),
                    },

                    direction.down => new PointF[]
                    {
                        new PointF(Y * s + s/2f, X * s + s),
                        new PointF(Y * s, X * s),
                        new PointF(Y * s + s, X * s),
                    },

                    direction.left => new PointF[]
                    {
                        new PointF(Y * s, X * s + s/2f),
                        new PointF(Y * s + s, X * s),
                        new PointF(Y * s + s, X * s + s),
                    },

                    direction.right => new PointF[]
                    {
                        new PointF(Y * s + s, X * s + s/2f),
                        new PointF(Y * s, X * s),
                        new PointF(Y * s, X * s + s),
                    },

                    _ => null
                };

                (pen, brush) = getInstruments(position.ceiling);

                g.FillPolygon(brush, points);
                g.DrawPolygon(pen, points);
            }

            return map;
        }

        public Image DrawFrame()
        {
            var frame = new Bitmap(frameSize, frameSize);

            var g = Graphics.FromImage(frame);

            g.Clear(emptyFrame);

            (int l, int x, int y) = getCorridorEnd();

            drawObject(ref g, l, position.wallFront);

            for (int i = l; i > 0; i--)
            {
                bool check, isStart, isFinish;

                (check, isStart, isFinish) = checkCorridor(x, y, true);

                if (check)
                {
                    drawObject(ref g, i, position.wallCorridorLeft);

                    objectType t;

                    if (isStart)
                        t = objectType.start;
                    else if (isFinish)
                        t = objectType.finish;
                    else
                        t = objectType.regular;

                    drawObject(ref g, i, position.ceilingCorridorLeft, t);
                    drawObject(ref g, i, position.floorCorridorLeft, t);
                }
                else
                    drawObject(ref g, i, position.wallLeft);

                (check, isStart, isFinish) = checkCorridor(x, y, false);

                if (check)
                {
                    drawObject(ref g, i, position.wallCorridorRight);

                    objectType t;

                    if (isStart)
                        t = objectType.start;
                    else if (isFinish)
                        t = objectType.finish;
                    else
                        t = objectType.regular;

                    drawObject(ref g, i, position.ceilingCorridorRight, t);
                    drawObject(ref g, i, position.floorCorridorRight, t);
                }
                else
                    drawObject(ref g, i, position.wallRight);

                if (x == finishX && y == finishY)
                {
                    drawObject(ref g, i, position.ceiling, objectType.finish);
                    drawObject(ref g, i, position.floor, objectType.finish);
                }
                else if (x == startX && y == startY)
                {
                    drawObject(ref g, i, position.ceiling, objectType.start);
                    drawObject(ref g, i, position.floor, objectType.start);
                }
                else
                {
                    drawObject(ref g, i, position.ceiling);
                    drawObject(ref g, i, position.floor);
                }

                switch (dir)
                {
                    case direction.up: ++x; break;
                    case direction.left: ++y; break;
                    case direction.down: --x; break;
                    case direction.right: --y; break;
                };
            }

            g.Dispose();

            return frame;
        }

        private void drawObject(ref Graphics g, int distance, position position, objectType type = objectType.regular)
        {
            (var pen, var brush) = getInstruments(position, type);

            var points = getPolygonCoordinates(position, distance);

            g.FillPolygon(brush, points);
            g.DrawPolygon(pen, points);
        }

        private (Pen, Brush) getInstruments(position position, objectType type = objectType.regular)
        {
            return (new Pen(type switch
            {
                objectType.start => startBorder,
                objectType.finish => finishBorder,
                _ => getColor(position, false)
            }, 2), new SolidBrush(type switch
            {
                objectType.start => startSurface,
                objectType.finish => finishSurface,
                _ => getColor(position, true)
            }));
        }

        private bool checkLeft(int x, int y) => maze[x, y - 1];

        private bool checkRight(int x, int y) => maze[x, y + 1];

        private bool checkUp(int x, int y) => maze[x - 1, y];

        private bool checkDown(int x, int y) => maze[x + 1, y];

        private direction getLeftDirection() => dir switch
        {
            direction.up => direction.left,
            direction.left => direction.down,
            direction.down => direction.right,
            direction.right => direction.up
        };

        private direction getRightDirection() => dir switch
        {
            direction.up => direction.right,
            direction.left => direction.up,
            direction.down => direction.left,
            direction.right => direction.down
        };

        private (int, int, int) getCorridorEnd()
        {
            int len = 1, x = X, y = Y;

            switch (dir)
            {
                case direction.up:
                    while (checkUp(x--, y))
                        len++;
                    return (len, ++x, y);

                case direction.left:
                    while (checkLeft(x, y--))
                        len++;
                    return (len, x, ++y);

                case direction.down:
                    while (checkDown(x++, y))
                        len++;
                    return (len, --x, y);

                case direction.right:
                    while (checkRight(x, y++))
                        len++;
                    return (len, x, --y);
            };

            return (len, x, y);
        }

        private (bool, bool, bool) checkCorridor(int x, int y, bool isLeft)
        {
            bool check = false;

            switch (isLeft ? getLeftDirection() : getRightDirection())
            {
                case direction.up:
                    check = checkUp(x, y);
                    x--;
                    break;

                case direction.down:
                    check = checkDown(x, y);
                    x++;
                    break;

                case direction.left:
                    check = checkLeft(x, y);
                    y--;
                    break;

                case direction.right:
                    check = checkRight(x, y);
                    y++;
                    break;
            }

            return (check, x == startX && y == startY, x == finishX && y == finishY);
        }

        private Color getColor(position p, bool isSurface)
        {
            switch (p)
            {
                case position.wallFront:
                case position.wallLeft:
                case position.wallCorridorLeft:
                case position.wallRight:
                case position.wallCorridorRight:
                    return isSurface ? wallSurface : wallBorder;

                case position.ceiling:
                case position.ceilingCorridorLeft:
                case position.ceilingCorridorRight:
                    return isSurface ? ceilingSurface : ceilingBorder;

                case position.floor:
                case position.floorCorridorLeft:
                case position.floorCorridorRight:
                    return isSurface ? floorSurface : floorBorder;

                default: return Color.Transparent;
            };
        }

        private PointF[] getPolygonCoordinates(position p, int distance) =>
            p switch
            {
                position.wallFront => new PointF[]
                    {
                        new PointF(hFrame + hFrame/(distance+0.5f), hFrame + hFrame/(distance+0.5f)),
                        new PointF(hFrame + hFrame/(distance+0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame - hFrame/(distance+0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame - hFrame/(distance+0.5f), hFrame + hFrame/(distance+0.5f)),
                    },

                position.wallLeft => new PointF[]
                    {
                        new PointF(hFrame - hFrame/(distance+0.5f), hFrame + hFrame/(distance+0.5f)),
                        new PointF(hFrame - hFrame/(distance+0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame - hFrame/(distance-0.5f), hFrame - hFrame/(distance-0.5f)),
                        new PointF(hFrame - hFrame/(distance-0.5f), hFrame + hFrame/(distance-0.5f)),
                    },

                position.wallCorridorLeft => new PointF[]
                    {
                        new PointF(hFrame - hFrame/(distance+0.5f), hFrame + hFrame/(distance+0.5f)),
                        new PointF(hFrame - hFrame/(distance+0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame - hFrame/(distance-0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame - hFrame/(distance-0.5f), hFrame + hFrame/(distance+0.5f)),
                    },

                position.wallRight => new PointF[]
                    {
                        new PointF(hFrame + hFrame/(distance+0.5f), hFrame + hFrame/(distance+0.5f)),
                        new PointF(hFrame + hFrame/(distance+0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame + hFrame/(distance-0.5f), hFrame - hFrame/(distance-0.5f)),
                        new PointF(hFrame + hFrame/(distance-0.5f), hFrame + hFrame/(distance-0.5f)),
                    },

                position.wallCorridorRight => new PointF[]
                    {
                        new PointF(hFrame + hFrame/(distance+0.5f), hFrame + hFrame/(distance+0.5f)),
                        new PointF(hFrame + hFrame/(distance+0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame + hFrame/(distance-0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame + hFrame/(distance-0.5f), hFrame + hFrame/(distance+0.5f)),
                    },

                position.floor => new PointF[]
                    {
                        new PointF(hFrame + hFrame/(distance+0.5f), hFrame + hFrame/(distance+0.5f)),
                        new PointF(hFrame + hFrame/(distance-0.5f), hFrame + hFrame/(distance-0.5f)),
                        new PointF(hFrame - hFrame/(distance-0.5f), hFrame + hFrame/(distance-0.5f)),
                        new PointF(hFrame - hFrame/(distance+0.5f), hFrame + hFrame/(distance+0.5f)),
                    },

                position.floorCorridorLeft => new PointF[]
                    {
                        new PointF(hFrame - hFrame/(distance-0.5f), hFrame + hFrame/(distance+0.5f)),
                        new PointF(hFrame - hFrame/(distance+0.5f), hFrame + hFrame/(distance+0.5f)),
                        new PointF(hFrame - hFrame/(distance-0.5f), hFrame + hFrame/(distance-0.5f)),
                    },

                position.floorCorridorRight => new PointF[]
                    {
                        new PointF(hFrame + hFrame/(distance-0.5f), hFrame + hFrame/(distance+0.5f)),
                        new PointF(hFrame + hFrame/(distance+0.5f), hFrame + hFrame/(distance+0.5f)),
                        new PointF(hFrame + hFrame/(distance-0.5f), hFrame + hFrame/(distance-0.5f)),
                    },

                position.ceiling => new PointF[]
                    {
                        new PointF(hFrame + hFrame/(distance+0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame + hFrame/(distance-0.5f), hFrame - hFrame/(distance-0.5f)),
                        new PointF(hFrame - hFrame/(distance-0.5f), hFrame - hFrame/(distance-0.5f)),
                        new PointF(hFrame - hFrame/(distance+0.5f), hFrame - hFrame/(distance+0.5f)),
                    },

                position.ceilingCorridorLeft => new PointF[]
                    {
                        new PointF(hFrame - hFrame/(distance-0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame - hFrame/(distance+0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame - hFrame/(distance-0.5f), hFrame - hFrame/(distance-0.5f)),
                    },

                position.ceilingCorridorRight => new PointF[]
                    {
                        new PointF(hFrame + hFrame/(distance-0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame + hFrame/(distance+0.5f), hFrame - hFrame/(distance+0.5f)),
                        new PointF(hFrame + hFrame/(distance-0.5f), hFrame - hFrame/(distance-0.5f)),
                    },

                _ => null
            };
    }
}
