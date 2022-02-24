using System;
using System.Collections.Generic;
using System.Text;

namespace SolverApp.Models
{
    public static class Solver
    {
        static public List<Solution> Solve(Map map)
        {
            Solution solution1 = new Solution();
            solution1.moves.Add(new Move(EColor.Blue, EMoveDirection.Up));
            solution1.moves.Add(new Move(EColor.Blue, EMoveDirection.Down));
            solution1.moves.Add(new Move(EColor.Blue, EMoveDirection.Left));
            Solution solution2 = new Solution();
            solution2.moves.Add(new Move(EColor.Red, EMoveDirection.Up));
            solution2.moves.Add(new Move(EColor.Red, EMoveDirection.Left));
            solution2.moves.Add(new Move(EColor.Blue, EMoveDirection.Up));
            solution2.moves.Add(new Move(EColor.Green, EMoveDirection.Right));
            solution2.moves.Add(new Move(EColor.Green, EMoveDirection.Up));
            return new List<Solution> {solution1, solution2};
        }
    }
}
