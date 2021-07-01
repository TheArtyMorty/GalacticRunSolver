#pragma once
#include <vector>
#include <queue>
#include "Map.h"

namespace RobotSolver
{
	struct Moves
	{
	private:
		std::vector<RobotSolver::Move> moves;

	public:
		void Add(RobotSolver::Move move);
		std::vector<RobotSolver::Move> GetMoves() const;
	};


	class CSolutions
	{
	public:
		CSolutions();
		void Add(Moves newSolution, int h);
		Moves PopFirst();
		bool Empty() const;

	private:
		std::vector<std::queue<Moves>> m_solutions;
	};
}