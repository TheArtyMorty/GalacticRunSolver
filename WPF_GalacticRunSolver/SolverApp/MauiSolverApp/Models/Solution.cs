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

        public Move(Move m)
        {
            color = m.color;
            direction = m.direction;
        }

        public EColor color;
        public EMoveDirection direction;
    }
    public class Solution
    {
        public Solution()
        {
            moves = new List<Move> {};
        }
        public Solution(List<Move> m)
        {
            moves = m;
        }
        public List<Move> moves = new List<Move> {};
    }

    public class SolutionsByHeuristic
    {
        public Dictionary<int, Queue<Solution>> solutions = new Dictionary<int, Queue<Solution>>();

        public void Add(Solution s, int h)
        {
            if (solutions.ContainsKey(h))
            {
                solutions[h].Enqueue(s);
            }
            else
            {
                var temp = new Queue<Solution>();
                temp.Enqueue(s);
                solutions.Add(h, temp );
            }
        }

        public Solution PopFirst()
        {
            for (int i = 0; i < 30; i++)
            {
                if (solutions.ContainsKey(i) && solutions[i].Count > 0)
                {
                    return solutions[i].Dequeue();
                }
            }
            return new Solution();
        }

        internal bool Empty()
        {
            for (int i = 0; i < 30; i++)
            {
                if (solutions.ContainsKey(i))
                {
                    return false;
                }
            }
            return true;
        }
    }
}
