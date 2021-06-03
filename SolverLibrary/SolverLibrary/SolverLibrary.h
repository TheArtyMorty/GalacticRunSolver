#pragma once
#include "Map.h"
#include "SolutionContainer.h"

namespace RobotSolver
{
class ILogger
{
public:
	virtual void Log(std::string input) = 0;
};

using Solution = Moves;

class Solver
{
public:
	Solver(int maxMovesSafety, ILogger* logger) : m_solverSafety(maxMovesSafety), m_logger(logger) {}
	std::vector<Solution> Solve(const Map& map);
	std::vector<Solution> Solve(std::string mapPath);
private:
	ILogger* m_logger;
	int			m_solverSafety;
};

std::string ColorFromMove(Move move);
std::string DirectionFromMove(Move move);
}

