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
                default:
                    Console.Write("Yellow ");
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
            var map = new Map("C:\\Users\\lucm\\source\\repos\\GalacticRunSolver\\Maps\\HardMap1_14moves.txt");
            var logger = new ConsoleLogger();

            var solutions = Solver.Solve(map,logger);
            foreach (var s in solutions)
            {
                PrintSolution(s);
            }
        }
    }
}
