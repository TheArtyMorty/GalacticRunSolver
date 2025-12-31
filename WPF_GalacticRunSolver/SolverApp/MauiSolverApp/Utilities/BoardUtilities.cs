using SolverApp.Models;

namespace MauiSolverApp.Utilities
{
    static public class BoardUtilities
    {
        static public List<Tuple<int, int, EWallType>> GetWallsForQuadrant(int board, int editionIndex)
        {
            switch (editionIndex)
            {
                case 1: // Edition 2 (Blue)
                    return GetWallsForBlueEdition(board);
                case 0: // Edition 1 and 3 (Red)
                default:
                    return GetWallsForRedEdition(board);
            }
        }

        static private List<Tuple<int, int, EWallType>> GetWallsForRedEdition(int board)
        {
            var wallsToSet = new List<Tuple<int, int, EWallType>>();
            switch (board)
            {
                case 1:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 2, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 1, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 4, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 3, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 5, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 2:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 4, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 1, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 6, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 2, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 7, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 3:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 2, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 1, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 4, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 6, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 3, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 4:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 2, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 3, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 1, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 6, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 4, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 5:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 3, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 5, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 1, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 3, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 7, EWallType.BottomRight)); //Wirlwind
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 6, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 6:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 4, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 6, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 1, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 2, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 7, EWallType.BottomRight)); //Wirlwind
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 5, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 7:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 6, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 5, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 2, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 4, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 3, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 8:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 4, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 1, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 2, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 5, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 6, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 9:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 5, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 3, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 2, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 6, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 5, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 10:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 4, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 7, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 2, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 5, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 1, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 11:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 6, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 3, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 2, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 6, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 1, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 12:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 5, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 3, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 6, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 6, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 1, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 13:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 5, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 3, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 2, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 1, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 5, EWallType.BottomRight)); //Wirlwind
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 6, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 14:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 5, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 1, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 5, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 2, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 3, EWallType.BottomRight)); //Wirlwind
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 6, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 15:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 3, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 1, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 4, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 5, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 6, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 16:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 5, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 6, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 4, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 1, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 2, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                default:
                    break;
            }
            return wallsToSet;
        }

        static private List<Tuple<int, int ,EWallType>> GetWallsForBlueEdition(int board)
        {
            var wallsToSet = new List<Tuple<int, int, EWallType>>();
            switch (board)
            {
                case 1:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 4, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 2, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 5, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 1, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 7, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 2:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(5, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 5, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 2, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 1, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 3, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 5, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 6, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 3:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 4, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 1, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 2, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 5, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(4, 6, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 4:
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 0, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(0, 2, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(2, 1, EWallType.TopRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(6, 3, EWallType.BottomLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(1, 4, EWallType.TopLeft));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(3, 6, EWallType.BottomRight));
                    wallsToSet.Add(new Tuple<int, int, EWallType>(7, 7, EWallType.TopLeft));
                    break;
                case 5:
                case 6:
                case 7:
                case 8:
                default:
                    break;
            }
            return wallsToSet;
        }
    }
}
