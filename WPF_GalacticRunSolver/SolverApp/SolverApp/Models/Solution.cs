using SolverApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;

namespace SolverApp.Models
{
    public class Move
    {
        public Move(EColor c, EMoveDirection d)
        {
            color = c;
            direction = d;
        }

        public EColor color;
        public EMoveDirection direction;
    }
    public class Solution
    {
        public List<Move> moves = new List<Move> {};
    }
}
