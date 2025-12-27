using SolverApp.Models;

namespace MauiSolverApp.Utilities
{
    static public class BoardUtilities
    {
        static public List<Tuple<int, int, EWallType>> GetWallsForQuadrant(string board, int editionIndex)
        {
            var wallsToSet = new List<Tuple<int, int, EWallType>>();
            switch (board)
            {
                case "A":
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 4, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 2, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 5, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 1, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 7, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case "B":
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 5, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 2, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 1, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 3, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 5, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 6, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case "C":
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 4, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 1, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 2, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 5, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 6, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case "D":
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 2, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 1, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 3, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 4, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 6, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case "E":
                case "F":
                case "G":
                case "H":
                default:
                    break;
            }
            return wallsToSet;
        }
    }
}
