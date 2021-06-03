#include "SolverLibrary.h"
#include <algorithm>
#include <map>
#include <chrono>
#include <iterator>
#include <cmath>


namespace RobotSolver
{

static Robot& GetRobotFromState(std::vector<Robot>& robots, const Move& move)
{
	for (auto& robot : robots)
	{
		if (robot.color == move.robot()) return robot;
	}
	throw std::exception("No roboto with this coloro foundo...");
}


#pragma pack(1)
struct State
{
public:
	std::vector<Robot>	Robots;
	Moves	Moves;
	

private:
	Cell GetEndPosition(const Map& map, Robot robot, EMoveDirection direction)
	{
		auto accessiblesCells = map.GetAccessibleCellsFrom(robot.x, robot.y, direction);
		if (accessiblesCells.empty()) return Cell{ EWallOrientation::None, robot.x, robot.y };
		auto filteredRobots = std::vector<Robot>{};
		for (const auto& r : Robots)
		{
			if (robot.color == r.color) continue;
			if ((r.y == robot.y && (direction == EMoveDirection::Left || direction == EMoveDirection::Right)) ||
				(r.x == robot.x && (direction == EMoveDirection::Up || direction == EMoveDirection::Down)))
			{
				filteredRobots.push_back(r);
			}
		}
		auto lastCell = Cell{ EWallOrientation::None, robot.x, robot.y };
		for (const auto& cell : accessiblesCells)
		{
			for (const auto& r : filteredRobots)
			{
				if (cell.x == r.x && cell.y == r.y)
				{
					return lastCell;
				}
			}
			lastCell = cell;
		}
		return accessiblesCells.back();
	}

public:
	State(RobotSolver::Moves moves, const Map& map)
	{
		Robots = map.GetRobots();
		Moves = moves;
		for (const auto& move : moves.GetMoves())
		{
			SimulateState(move, map);
		}
	}

	void SimulateState(const Move& move, const Map& map)
	{
		auto& robot = GetRobotFromState(Robots, move);
		auto endCell = GetEndPosition(map, robot, move.direction());
		robot.x = endCell.x;
		robot.y = endCell.y;
	}


	int ToKey(int mapSize) const
	{
		int result(0);
		int offset = std::log2(mapSize);
		for (const auto& robot : Robots)
		{
			result = result << (2 * offset);
			result += (robot.x << offset) + robot.y;
		}
		return result;
	}

	int Heuristic(const Map& map) const
	{
		for (const auto& r : Robots)
		{
			if (map.GetTarget().targetColor == r.color)
			{
				return map.GetHeuristicOfCell(r.x, r.y) + Moves.GetMoves().size();
			}
		}
		return 99999;
	}
};

static bool WasStateAlreadyDone(const std::vector<int>& positionsAlreadyDone, const State& state)
{
	auto it = std::find(positionsAlreadyDone.begin(), positionsAlreadyDone.end(), state.ToKey(mapSize));
	return it != positionsAlreadyDone.end();
}

static std::vector<State> CreateAllStatesFromState(const State& state, const Map& map, std::vector<int>& statesAlreadyDone)
{
	std::vector<State> states = std::vector<State>();
	for (const auto& robot : state.Robots)
	{
		//if (robot.color == ERobotColor::Yellow) continue;
		for (const auto& move : { EMoveDirection::Up, EMoveDirection::Down, EMoveDirection::Left, EMoveDirection::Right })
		{
			State newState = state;
			newState.Moves.Add({ robot.color, move });
			newState.SimulateState({ robot.color, move }, map);

			if (!WasStateAlreadyDone(statesAlreadyDone, newState))
			{
				states.push_back(newState);
				statesAlreadyDone.push_back(newState.ToKey(mapSize));
			}
		}
	}
	return states;
}

bool IsSolutionValid(const State& state, const Map& map)
{
	for (const auto& robot : state.Robots)
	{
		if (robot.color == map.GetTarget().targetColor)
		{
			return robot.x == map.GetTarget().x && robot.y == map.GetTarget().y;
		}
	}
	return false;
}

static void Log(ILogger* pLogger, std::string tolog)
{
	if (pLogger != nullptr)
	{
		pLogger->Log(tolog);
	}
}

static void LogTime(ILogger* pLogger, std::chrono::steady_clock::time_point start)
{
	if (pLogger != nullptr)
	{
		auto stop = std::chrono::high_resolution_clock::now();
		auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(stop - start).count();
		if (duration > 1000)
		{
			duration = std::chrono::duration_cast<std::chrono::seconds>(stop - start).count();
			pLogger->Log("Time spent : " + std::to_string(duration) + " s");
		}
		else
		{
			pLogger->Log("Time spent : " + std::to_string(duration) + " ms");
		}
	}
}

std::string ColorFromMove(Move move)
{
	switch (move.robot())
	{
	case ERobotColor::Blue:
		return "Blue";
	case ERobotColor::Red:
		return "Red";
	case ERobotColor::Green:
		return "Green";
	case ERobotColor::Yellow:
		return "Yellow";
	default:
		return "No Color ?!?";
	}
}

std::string DirectionFromMove(Move move)
{
	switch (move.direction())
	{
	case EMoveDirection::Up:
		return "Up";
	case EMoveDirection::Down:
		return "Down";
	case EMoveDirection::Left:
		return "Left";
	case EMoveDirection::Right:
		return "Right";
	default:
		return "Not valid move ?!?";
	}
}


static void LogAllStates(std::vector<State> const& states, ILogger* logger)
{
	for (const auto& state : states)
	{
		std::string solution;
		for (const auto& move : state.Moves.GetMoves())
		{
			solution += ColorFromMove(move) + "-" + DirectionFromMove(move) + "__";
		}
		auto robots = state.Robots;
		Log(logger, solution);
	}
}

std::vector<Solution> Solver::Solve(const Map& map)
{
	auto initialStart = std::chrono::high_resolution_clock::now();
	mapSize = map.GetMapSize();

	Moves initialMoves{};
	State initialState(initialMoves, map);
	
	if (initialState.Heuristic(map) >= 30)
	{
		Log(m_logger, "No valid solution exist...");
		return {};
	}

	if (initialState.Heuristic(map) == 0)
	{
		Log(m_logger, "Robot is already on target...");
		return { initialMoves };
	}

	CSolutions CurrentStates;
	CurrentStates.Add(initialMoves, initialState.Heuristic(map));

	std::vector<int> statesAlreadyDone{ initialState.ToKey(mapSize) };

	std::vector<Solution> solutions = std::vector<Solution>{};
	int bestSolution = -1;

	int currentH = -1;

	while (!CurrentStates.Empty())
	{
		auto state = State(CurrentStates.PopFirst(), map);

		auto stateH = state.Heuristic(map);

		if (!solutions.empty() && state.Heuristic(map) > bestSolution)
		{
			break;
		}

		if (currentH < stateH)
		{
			currentH = stateH;
			Log(m_logger, "Considering moves with H : " + std::to_string(currentH) + "...");
			LogTime(m_logger, initialStart);
		}

		auto states = CreateAllStatesFromState(state, map, statesAlreadyDone);
		for (const auto& sta : states)
		{
			if (IsSolutionValid(sta, map))
			{
				Log(m_logger, "A valid solution in " + std::to_string(sta.Moves.GetMoves().size()) + " has been found.");
				if (bestSolution == -1) bestSolution = sta.Moves.GetMoves().size();
				solutions.push_back(sta.Moves);
			}
			else
			{
				CurrentStates.Add(sta.Moves, sta.Heuristic(map));
			}
		}
	}

	Log(m_logger, "All solutions have been found.");
	LogTime(m_logger, initialStart);
	return solutions;
}

std::vector<Solution> Solver::Solve(std::string mapPath)
{
	const auto theMap = Map(mapPath);
	return Solve(theMap);
}

}