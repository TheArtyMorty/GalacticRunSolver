#include "SolutionContainer.h"

namespace RobotSolver
{
	Moves::Moves()
	{
	}

	void Moves::Add(RobotSolver::Move move)
	{
		moves.push_back(move);
	}

	std::vector<RobotSolver::Move> Moves::GetMoves() const
	{
		return moves;
	}




	static const int maxMovesInSolution = 30;

	CSolutions::CSolutions()
	{
		m_solutions = std::vector<std::queue<Moves>>(maxMovesInSolution, std::queue<Moves>());
	}

	void CSolutions::Add(Moves newSolution, int h)
	{
		m_solutions[h].push(newSolution);
	}

	Moves CSolutions::PopFirst()
	{
		Moves result{};
		for (int i = 0; i < maxMovesInSolution; i++)
		{
			if (!m_solutions[i].empty())
			{
				result = m_solutions[i].front();
				m_solutions[i].pop();
				return result;
			}
		}
		return result;
	}

	bool CSolutions::Empty() const
	{
		for (int i = 0; i < maxMovesInSolution; i++)
		{
			if (!m_solutions[i].empty())
			{
				return false;
			}
		}
		return true;
	}
}