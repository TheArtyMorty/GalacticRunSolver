﻿using System;
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

		bool IsEmpty(Position p)
		{
			return !Robots.Any(r => r._Position == p);
		}

		Case GetEndPosition(Map map, Robot robot, EMoveDirection direction)
		{
			var current = new Position(robot._Position);
			var next = map.NextCell(current.X, current.Y, direction);
			while (map.IsMoveValid(map._Cases[current.Y][current.X], direction, true) && IsEmpty(new Position(next.Item1, next.Item2)))
            {
				current.X = next.Item1;
				current.Y = next.Item2;
				next = map.NextCell(current.X, current.Y, direction);
			}
			return map._Cases[current.Y][current.X];
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

		public void SimulateState(Move move, Map map)
		{
			var robot = Robots.Find(r => r._Color == move.color);
			var endCell = GetEndPosition(map, robot, move.direction);
			robot._Position = endCell._Position;
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
					var newState = new State(state.Moves, map);
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
