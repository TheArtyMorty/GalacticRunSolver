#pragma once
#include <vector>
#include "Map.h"

namespace RobotSolver
{
	struct Moves
	{
	private:
		std::vector<RobotSolver::Move> moves;

	public:
		Moves();
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
		std::vector<Moves> m_solutions;
		std::vector<int> m_positions;
	};
}