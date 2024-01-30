using SolverApp.Models;
using System;

namespace CsConsoleSolver
{
    class Program
    {
        private class ConsoleLogger : ILogger
        {
            public override void Clear()
            {
                Console.WriteLine("-------------------------------");
            }

            public override void Log(string content)
            {
                Console.WriteLine(content);
            }
        }

        private static void PrintMove(Move m)
        {
            switch (m.color)
            {
                case EColor.Red:
                    Console.Write("Red ");
                    break;
                case EColor.Green:
                    Console.Write("Green ");
                    break;
                case EColor.Blue:
                    Console.Write("Blue ");
                    break;
                case EColor.Yellow:
                    Console.Write("Yellow ");
                    break;
                case EColor.Gray:
                default:
                    Console.Write("Gray ");
                    break;
            }
            switch (m.direction)
            {
                case EMoveDirection.Up:
                    Console.Write("Up");
                    break;
                case EMoveDirection.Right:
                    Console.Write("Right");
                    break;
                case EMoveDirection.Down:
                    Console.Write("Down");
                    break;
                case EMoveDirection.Left:
                default:
                    Console.Write("Left");
                    break;
            }
            Console.Write(Environment.NewLine);
        }

        private static void PrintSolution(Solution s)
        {
            Console.WriteLine("New Solution in " + s.moves.Count + " moves :");
            foreach (var move in s.moves)
            {
                PrintMove(move);
            }
        }

        static void Main(string[] args)
        {
            var map = new Map("C:\\Github\\GalacticRunSolver\\Maps\\TestMap_5Robots_13Moves.txt");
            var logger = new ConsoleLogger();

            var fakeBW = new System.ComponentModel.BackgroundWorker();

            var solutions = Solver.Solve(map,logger, ref fakeBW);
            foreach (var s in solutions)
            {
                PrintSolution(s);
            }
        }
    }
}
