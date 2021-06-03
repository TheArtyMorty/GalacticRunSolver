//#include "SolverLibrary.h"
//#include <algorithm>
//#include <map>
//#include <chrono>
//#include <iterator>
//#include <cmath>
//
//
//namespace RobotSolver
//{
//
//	static Robot& GetRobotFromState(std::vector<Robot>& robots, const Move& move)
//	{
//		for (auto& robot : robots)
//		{
//			if (robot.color == move.robot()) return robot;
//		}
//		throw std::exception("No roboto with this coloro foundo...");
//	}
//
//
//#pragma pack(1)
//	struct State
//	{
//	public:
//		std::vector<Robot>	Robots;
//		Moves	Moves;
//
//
//	private:
//		bool CanMove(const Map* map, int posX, int posY, EMoveDirection direction)
//		{
//			auto incrX = (direction == EMoveDirection::Left ? -1 : (direction == EMoveDirection::Right ? 1 : 0));
//			auto incrY = (direction == EMoveDirection::Up ? -1 : (direction == EMoveDirection::Down ? 1 : 0));
//			auto finalX = posX + incrX;
//			auto finalY = posY + incrY;
//			for (const auto& robot : Robots)
//			{
//				if (robot.x == finalX && robot.y == finalY) return false;
//			}
//			return map->IsMoveValid(posX, posY, direction);
//		}
//
//	public:
//		State(RobotSolver::Moves moves, const Map* map)
//		{
//			Robots = map->GetRobots();
//			Moves = moves;
//			for (const auto& move : moves.GetMoves())
//			{
//				SimulateState(move, map);
//			}
//		}
//
//		bool SimulateState(const Move& move, const Map* map)
//		{
//			auto& robot = GetRobotFromState(Robots, move);
//			auto posX = robot.x;
//			auto posY = robot.y;
//			const auto incrX = (move.direction() == EMoveDirection::Left ? -1 : (move.direction() == EMoveDirection::Right ? 1 : 0));
//			const auto incrY = (move.direction() == EMoveDirection::Up ? -1 : (move.direction() == EMoveDirection::Down ? 1 : 0));
//			//Try moving robot
//			int moveCount = 0;
//			while (CanMove(map, posX, posY, move.direction()))
//			{
//				robot.x += incrX;
//				robot.y += incrY;
//				posX = robot.x;
//				posY = robot.y;
//				moveCount++;
//			}
//			return moveCount > 0;
//		}
//
//
//		int ToKey(int mapSize) const
//		{
//			int result(0);
//			int offset = std::log2(mapSize);
//			for (const auto& robot : Robots)
//			{
//				result = result << (2 * offset);
//				result += (robot.x << offset) + robot.y;
//			}
//			return result;
//		}
//	};
//
//
//
//
//
//	static bool WasStateAlreadyDone(const std::vector<int>& positionsAlreadyDone, const State& state)
//	{
//		auto it = std::find(positionsAlreadyDone.begin(), positionsAlreadyDone.end(), state.ToKey(mapSize));
//		return it != positionsAlreadyDone.end();
//	}
//
//	static void CreateAllStatesFromState(std::vector<Moves>& states, const Moves& moves, const Map* map, std::vector<int>& statesAlreadyDone)
//	{
//		auto state = State(moves, map);
//		for (const auto& robot : state.Robots)
//		{
//			//if (robot.color == ERobotColor::Yellow) continue;
//			for (const auto& move : { EMoveDirection::Up, EMoveDirection::Down, EMoveDirection::Left, EMoveDirection::Right })
//			{
//				State newState = state;
//				newState.Moves.Add({ robot.color, move });
//				auto canMove = newState.SimulateState({ robot.color, move }, map);
//				if (canMove)
//				{
//					if (!WasStateAlreadyDone(statesAlreadyDone, newState))
//					{
//						states.push_back(newState.Moves);
//						statesAlreadyDone.push_back(newState.ToKey(mapSize));
//					}
//				}
//			}
//		}
//	}
//
//	bool IsSolutionValid(const Moves& moves, const Map* map)
//	{
//		auto state = State{ moves, map };
//		for (const auto& robot : state.Robots)
//		{
//			if (robot.color == map->GetTarget().targetColor)
//			{
//				return robot.x == map->GetTarget().x && robot.y == map->GetTarget().y;
//			}
//		}
//		return false;
//	}
//
//	static std::vector<Solution> GetAllValidSolutions(std::vector<Moves>& states, const Map* map)
//	{
//		auto firstNonValid = std::partition(states.begin(), states.end(), [&map](const auto& state) {return IsSolutionValid(state, map); });
//		std::vector<Solution> result{ states.begin(), firstNonValid };
//		return result;
//	}
//
//	static void Log(ILogger* pLogger, std::string tolog)
//	{
//		if (pLogger != nullptr)
//		{
//			pLogger->Log(tolog);
//		}
//	}
//
//	static void LogTime(ILogger* pLogger, std::chrono::steady_clock::time_point start)
//	{
//		if (pLogger != nullptr)
//		{
//			auto stop = std::chrono::high_resolution_clock::now();
//			auto duration = std::chrono::duration_cast<std::chrono::milliseconds>(stop - start).count();
//			pLogger->Log("Time spent : " + std::to_string(duration) + " ms");
//		}
//	}
//
//	std::string ColorFromMove(Move move)
//	{
//		switch (move.robot())
//		{
//		case ERobotColor::Blue:
//			return "Blue";
//		case ERobotColor::Red:
//			return "Red";
//		case ERobotColor::Green:
//			return "Green";
//		case ERobotColor::Yellow:
//			return "Yellow";
//		default:
//			return "No Color ?!?";
//		}
//	}
//
//	std::string DirectionFromMove(Move move)
//	{
//		switch (move.direction())
//		{
//		case EMoveDirection::Up:
//			return "Up";
//		case EMoveDirection::Down:
//			return "Down";
//		case EMoveDirection::Left:
//			return "Left";
//		case EMoveDirection::Right:
//			return "Right";
//		default:
//			return "Not valid move ?!?";
//		}
//	}
//
//
//	static void LogAllStates(std::vector<State> const& states, ILogger* logger)
//	{
//		for (const auto& state : states)
//		{
//			std::string solution;
//			for (const auto& move : state.Moves.GetMoves())
//			{
//				solution += ColorFromMove(move) + "-" + DirectionFromMove(move) + "__";
//			}
//			auto robots = state.Robots;
//			Log(logger, solution);
//		}
//	}
//
//	std::vector<Solution> Solver::Solve(const Map* map)
//	{
//		auto initialStart = std::chrono::high_resolution_clock::now();
//		mapSize = map->GetMapSize();
//
//		Moves initialState{};
//
//		std::vector<Moves> CurrentStates = { initialState };
//		auto endCurrentStates = CurrentStates.end();
//
//		std::vector<Moves> tempStates;
//
//		std::vector<int> statesAlreadyDone{ State(initialState, map).ToKey(mapSize) };
//
//		for (int i = 0; i < m_solverSafety; i++)
//		{
//			auto loopStart = std::chrono::high_resolution_clock::now();
//			//Verify solutions
//			{
//				auto validSolutions = GetAllValidSolutions(CurrentStates, map);
//				if (validSolutions.size() > 0)
//				{
//					Log(m_logger, std::to_string(validSolutions.size()) + " solutions in " + std::to_string(i) + " moves have been found.");
//					LogTime(m_logger, initialStart);
//					return validSolutions;
//				}
//			}
//			//Create new states
//			Log(m_logger, "Considering move " + std::to_string(i + 1) + " :");
//			for (const auto& state : CurrentStates)
//			{
//				CreateAllStatesFromState(tempStates, state, map, statesAlreadyDone);
//			}
//			std::swap(CurrentStates, tempStates);
//			tempStates.clear();
//			Log(m_logger, std::to_string(CurrentStates.size()) + " states calculated after move " + std::to_string(i + 1) + ".");
//			LogTime(m_logger, loopStart);
//		}
//
//		//Verify solutions
//		{
//			auto validSolutions = GetAllValidSolutions(CurrentStates, map);
//			if (validSolutions.size() > 0)
//			{
//				Log(m_logger, std::to_string(validSolutions.size()) + " solutions in " + std::to_string(m_solverSafety) + " moves have been found.");
//				LogTime(m_logger, initialStart);
//				return validSolutions;
//			}
//		}
//
//		Log(m_logger, "No solution has been found.");
//		LogTime(m_logger, initialStart);
//
//		return {};
//	}
//
//	std::vector<Solution> Solver::Solve(std::string mapPath)
//	{
//		const auto theMap = Map(mapPath);
//		return Solve(&theMap);
//	}
//
//
//
//
//
//	Moves::Moves()
//	{
//	}
//
//	void Moves::Add(Move move)
//	{
//		moves.push_back(move);
//	}
//
//	std::vector<Move> Moves::GetMoves() const
//	{
//		return moves;
//	}
//
//	//Moves::Moves()
//	//{
//	//	moves = 0;
//	//}
//	//
//	//void Moves::Add(Move move)
//	//{
//	//	moves = moves << 2;
//	//	moves += (int)move.robot();
//	//	moves = moves << 2;
//	//	moves += (int)move.direction();
//	//}
//	//
//	//std::vector<Move> Moves::GetMoves() const
//	//{
//	//	std::vector<Move> result{};
//	//	auto tempMoves = moves;
//	//	while (tempMoves > 0)
//	//	{
//	//		auto shifted = tempMoves >> 2;
//	//		auto direction = tempMoves - (shifted << 2);
//	//		tempMoves = shifted;
//	//		shifted = tempMoves >> 2;
//	//		auto color = tempMoves - (shifted << 2);
//	//		tempMoves = shifted;
//	//		auto move = Move{ (ERobotColor)color, (EMoveDirection)direction};
//	//		result.insert(result.begin(), move);
//	//	}
//	//	return result;
//	//}
//
//}