using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace SolverApp.Models
{


public abstract class ILogger
    {
        public abstract void Log(string content);
        public abstract void Clear();
    }

	public struct State
	{
		public List<Robot> Robots;
		public List<Move> Moves;

		Position GetEndPosition(Map map, Robot robot, EMoveDirection direction)
		{
			var pos = robot._Position;
			var max = map._AccessiblesCells[pos.Y][pos.X][direction];
            switch (direction)
            {
                case EMoveDirection.Up:
                {
					if (max == pos.Y) return map._Cases[pos.Y][pos.X]._Position;
					else
                    {
						foreach (var r in Robots)
                        {
							if (r._Color == robot._Color) continue; //do not consider moving robot
							if (r._Position.X != pos.X) continue; //not on same column
							if (r._Position.Y >= pos.Y) continue; //below moving robot
							max = Math.Max(r._Position.Y+1, max);
                        }
						return map._Cases[max][pos.X]._Position;
                    }
				}
                case EMoveDirection.Right:
				{
					if (max == pos.X) return map._Cases[pos.Y][pos.X]._Position;
					else
					{
						foreach (var r in Robots)
						{
							if (r._Color == robot._Color) continue; //do not consider moving robot
							if (r._Position.Y != pos.Y) continue; //not on same line
							if (r._Position.X <= pos.X) continue; //left of moving robot
							max = Math.Min(r._Position.X-1, max);
						}
						return map._Cases[pos.Y][max]._Position;
					}
				}
				case EMoveDirection.Down:
				{
					if (max == pos.Y) return map._Cases[pos.Y][pos.X]._Position;
					else
					{
						foreach (var r in Robots)
						{
							if (r._Color == robot._Color) continue; //do not consider moving robot
							if (r._Position.X != pos.X) continue; //not on same column
							if (r._Position.Y <= pos.Y) continue; //above moving robot
							max = Math.Min(r._Position.Y-1, max);
						}
						return map._Cases[max][pos.X]._Position;
					}
				}
				case EMoveDirection.Left:
                default:
				{
					if (max == pos.X) return map._Cases[pos.Y][pos.X]._Position;
					else
					{
						foreach (var r in Robots)
						{
							if (r._Color == robot._Color) continue; //do not consider moving robot
							if (r._Position.Y != pos.Y) continue; //not on same line
							if (r._Position.X >= pos.X) continue; //right of moving robot
							max = Math.Max(r._Position.X+1, max);
						}
						return map._Cases[pos.Y][max]._Position;
					}
				}
			}
        }

        private void Initialize(List<Move> moves, Map map)
        {
			foreach (var move in moves)
			{
				Moves.Add(new Move(move));
			}

			foreach (var move in Moves)
			{
				SimulateState(move, map);
			}
		}

		public State(Solution moves, Map map)
		{
			Robots = map.GetRobots();
			Moves = new List<Move>();
			Initialize(moves.moves, map);
		}

		public State(List<Move> moves, Map map)
		{
			Robots = map.GetRobots();
			Moves = new List<Move>();
			Initialize(moves, map);
		}

		public State(State other)
		{
			Robots = new List<Robot>();
			foreach (var robot in other.Robots)
			{
				Robots.Add(new Robot(robot));
			}
			Moves = new List<Move>();
			foreach (var move in other.Moves)
			{
				Moves.Add(new Move(move));
			}
		}

		public void SimulateState(Move move, Map map)
		{
			var robot = Robots.Find(r => r._Color == move.color);
			robot._Position = GetEndPosition(map, robot, move.direction);
		}

		public long ToKey(int mapSize)
		{
			long result = 0;
			int offset = (int)Math.Log(mapSize,2);
			foreach (var robot in Robots)
			{
				result = result << (2 * offset);
				result += (robot._Position.X << offset) + robot._Position.Y;
			}
			return result;
		}

		public int Heuristic(Map map)
		{
			return map.GetHeuristicOfPosition(Robots.Find(r => r._Color == map._Target._Color)._Position) + Moves.Count;
		}
    };


public static class Solver
    {
		static private void Log(ILogger logger, Stopwatch sw, string text)
        {
			logger.Log(sw.Elapsed.ToString("mm\\:ss\\.ff") + " - " + text);
		}

		static EMoveDirection[] moveDirections = { EMoveDirection.Up, EMoveDirection.Down, EMoveDirection.Left, EMoveDirection.Right};

		static List<State> CreateAllStatesFromState(State state, Map map, HashSet<long> statesAlreadyDone)
		{
			var states = new List<State>();
			foreach (var robot in state.Robots)
			{
				foreach (var direction in moveDirections)
				{
					var newState = new State(state);
					var move = new Move(robot._Color, direction);
					newState.Moves.Add(move);
					newState.SimulateState(move, map);

					if (!statesAlreadyDone.Contains(newState.ToKey(map._Size)))
					{
						statesAlreadyDone.Add(newState.ToKey(map._Size));
						states.Add(newState);
					}
				}
			}
			return states;
		}

		static public List<Solution> Solve(Map inputMap, ILogger logger)
        {
			//init
			var map = new Map(inputMap);
			map.InitializeHeuristic();
			Stopwatch stopWatch = new Stopwatch();
			logger.Clear();
			Log(logger, stopWatch, "Starting solving");
			stopWatch.Start();
			var solutions = new List<Solution>();

			//Initial State
			var initialMoves = new Solution{ };
			State initialState = new State(initialMoves, map);

			if (initialState.Heuristic(map) >= 30)
			{
				Log(logger, stopWatch, "No valid solution exist.");
				return solutions;
			}
			else if (initialState.Heuristic(map) == 0)
			{
				Log(logger, stopWatch, "Robot is already on target...");
				return solutions;
			}

			var CurrentStates = new SolutionsByHeuristic();
			CurrentStates.Add(initialMoves, initialState.Heuristic(map));

			HashSet<long> statesAlreadyDone = new HashSet<long> { initialState.ToKey(map._Size) };

			int bestSolution = -1;
			int currentH = -1;

			while (!CurrentStates.Empty())
			{
				var state = new State(CurrentStates.PopFirst(), map);
				var stateH = state.Heuristic(map);

				if (solutions.Count > 0 && stateH > bestSolution)
				{
					//No more solution can be found with same number of moves
					break;
				}

				if (currentH < stateH)
				{
					currentH = stateH;
					Log(logger, stopWatch, "Considering moves with H : " + currentH + "...");
				}

				var states = CreateAllStatesFromState(state, map, statesAlreadyDone);
				foreach (var sta in states)
				{
					if (sta.Robots.Find(r => r._Color == map._Target._Color)._Position == map._Target._Position)
					{
						Log(logger, stopWatch, "A valid solution in " + sta.Moves.Count() + " has been found.");
						if (bestSolution == -1) bestSolution = sta.Moves.Count();
						solutions.Add(new Solution(sta.Moves));
					}
					else
					{
						CurrentStates.Add(new Solution(sta.Moves), sta.Heuristic(map));
					}
				}
			}

			//return
			Log(logger, stopWatch, "Returning solutions (" + solutions.Count + ").");
            return solutions;
        }
    }
}
