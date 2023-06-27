using System;
using System.Collections.Generic;
using System.Linq;

public class Point2D
{
    public int X { get; }
    public int Y { get; }
    public char Tag { get; }

    public Point2D(int x, int y, char tag)
    {
        X = x;
        Y = y;
        Tag = tag;
    }
}

public class Program
{
    public static void Main(string[] args)
    {
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Click Enter to generate a shape");

        while (Console.ReadLine() == "")
        {
            Console.Clear();
            int pointCount = new Random().Next(3, 20);
            List<Point2D> points = GenerateRandomPoints(pointCount);

            List<Point2D> convexHull = FindConvexHull(points);

            List<Point2D> detailedConvexHull = DoubleConvexHullDetail(convexHull);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nCoordinate Grid:");
            PlotPointsAndShapeOnGrid(detailedConvexHull);
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("\nClick Enter to generate a new shape");
        }
    }
    public static List<Point2D> GenerateRandomPoints(int count)
    {
        List<Point2D> points = new List<Point2D>();

        Random random = new Random();

        char tag = 'A';
        for (int i = 0; i < count; i++)
        {
            int x = random.Next(1, 21);
            int y = random.Next(1, 21);

            Point2D point = new Point2D(x, y, tag);
            points.Add(point);

            tag++;
        }

        return points;
    }

    public static List<Point2D> FindConvexHull(List<Point2D> points)
    {
        Point2D leftmostPoint = points.OrderBy(p => p.X).ThenBy(p => p.Y).First();

        List<Point2D> convexHull = new List<Point2D>();
        convexHull.Add(leftmostPoint);

        Point2D currentPoint = leftmostPoint;

        while (true)
        {
            Point2D nextPoint = points[0];

            foreach (Point2D point in points)
            {
                if (point == currentPoint)
                    continue;

                int orientation = GetOrientation(currentPoint, point, nextPoint);

                if (orientation == 1 || (orientation == 0 && GetDistance(currentPoint, point) > GetDistance(currentPoint, nextPoint)))
                {
                    nextPoint = point;
                }
            }

            if (nextPoint == leftmostPoint)
                break;

            convexHull.Add(nextPoint);
            currentPoint = nextPoint;
        }

        return convexHull;
    }

    public static List<Point2D> DoubleConvexHullDetail(List<Point2D> convexHull)
    {
        List<Point2D> detailedConvexHull = new List<Point2D>();

        for (int i = 0; i < convexHull.Count; i++)
        {
            detailedConvexHull.Add(convexHull[i]);

            int nextIndex = (i + 1) % convexHull.Count;
            Point2D currentPoint = convexHull[i];
            Point2D nextPoint = convexHull[nextIndex];

            int dx = nextPoint.X - currentPoint.X;
            int dy = nextPoint.Y - currentPoint.Y;

            // Increase the number of intermediate points
            int numIntermediatePoints = 100; // Adjust this value to control the level of detail

            detailedConvexHull.AddRange(GetIntermediatePoints(currentPoint, nextPoint, dx, dy, numIntermediatePoints));
        }

        return detailedConvexHull;
    }

    public static IEnumerable<Point2D> GetIntermediatePoints(Point2D startPoint, Point2D endPoint, int dx, int dy, int numIntermediatePoints)
    {
        int steps = numIntermediatePoints + 2; // Include the start and end points
        double xIncrement = (double)dx / steps;
        double yIncrement = (double)dy / steps;

        double x = startPoint.X;
        double y = startPoint.Y;

        for (int i = 0; i <= steps; i++)
        {
            int roundedX = (int)Math.Round(x);
            int roundedY = (int)Math.Round(y);

            yield return new Point2D(roundedX, roundedY, '#');

            x += xIncrement;
            y += yIncrement;
        }
    }



    public static int GetOrientation(Point2D p1, Point2D p2, Point2D p3)
    {
        int val = (p2.Y - p1.Y) * (p3.X - p2.X) - (p2.X - p1.X) * (p3.Y - p2.Y);

        if (val == 0)
            return 0;  // Collinear

        return (val > 0) ? 1 : 2; // Clockwise or Counterclockwise
    }

    public static double GetDistance(Point2D p1, Point2D p2)
    {
        int dx = p2.X - p1.X;
        int dy = p2.Y - p1.Y;

        return Math.Sqrt(dx * dx + dy * dy);
    }

    public static void PlotPointsAndShapeOnGrid(List<Point2D> points)
    {
        int maxX = points.Max(p => p.X);
        int maxY = points.Max(p => p.Y);

        double xScale = 1.0;
        double yScale = 1.0;

        int maxDiff = Math.Max(maxX, maxY);
        if (maxDiff > 20)
        {
            xScale = (double)maxX / 20;
            yScale = (double)maxY / 20;
        }

        // Create a 2D array to represent the grid
        char[,] grid = new char[21, 21];

        foreach (Point2D point in points)
        {
            int scaledX = (int)Math.Round(point.X / xScale);
            int scaledY = (int)Math.Round(point.Y / yScale);
            grid[scaledY, scaledX] = point.Tag;
        }

        // Connect the points to form a shape without intersecting lines
        for (int i = 0; i < points.Count; i++)
        {
            Point2D currentPoint = points[i];
            Point2D nextPoint = points[(i + 1) % points.Count]; // Wrap around to the first point for the last connection

            int scaledX1 = (int)Math.Round(currentPoint.X / xScale);
            int scaledY1 = (int)Math.Round(currentPoint.Y / yScale);
            int scaledX2 = (int)Math.Round(nextPoint.X / xScale);
            int scaledY2 = (int)Math.Round(nextPoint.Y / yScale);

            ConnectPointsOnGrid(grid, new Point2D(scaledX1, scaledY1, currentPoint.Tag), new Point2D(scaledX2, scaledY2, nextPoint.Tag));
        }

        // Print the grid with the shape
        for (int y = 20; y >= 0; y--)
        {
            for (int x = 0; x <= 20; x++)
            {
                Console.ForegroundColor = ConsoleColor.White;
                Console.Write(grid[y, x]);
            }
            Console.WriteLine();
        }
    }

    public static void ConnectPointsOnGrid(char[,] grid, Point2D point1, Point2D point2)
    {
        int x1 = point1.X;
        int y1 = point1.Y;
        int x2 = point2.X;
        int y2 = point2.Y;

        bool steep = Math.Abs(y2 - y1) > Math.Abs(x2 - x1);

        if (steep)
        {
            Swap(ref x1, ref y1);
            Swap(ref x2, ref y2);
        }

        if (x1 > x2)
        {
            Swap(ref x1, ref x2);
            Swap(ref y1, ref y2);
        }

        int dx = x2 - x1;
        int dy = Math.Abs(y2 - y1);
        int error = dx / 2;
        int yStep = y1 < y2 ? 1 : -1;
        int y = y1;

        for (int x = x1; x <= x2; x++)
        {
            if (steep)
                grid[x, y] = '#';
            else
                grid[y, x] = '#';

            error -= dy;
            if (error < 0)
            {
                y += yStep;
                error += dx;
            }
        }
    }

    public static void Swap(ref int a, ref int b)
    {
        int temp = a;
        a = b;
        b = temp;
    }

    public static char GetPointSymbol(int x, int y, List<Point2D> points)
    {
        foreach (Point2D point in points)
        {
            if (point.X == x && point.Y == y)
                return point.Tag;
        }

        return ' ';
    }
}
