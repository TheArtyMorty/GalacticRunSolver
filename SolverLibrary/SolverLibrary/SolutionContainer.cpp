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
		m_solutions = std::vector<Moves>();
		m_positions = std::vector<int>(maxMovesInSolution, 0);
	}

	void CSolutions::Add(Moves newSolution, int h)
	{
		m_solutions.insert(m_solutions.begin() + m_positions[h], newSolution);
		for (int i = h; i < maxMovesInSolution; i++)
		{
			m_positions[i]++;
		}
	}

	Moves CSolutions::PopFirst()
	{
		Moves result = m_solutions.front();
		m_solutions.erase(m_solutions.begin());
		for (int i = 0; i < maxMovesInSolution; i++)
		{
			if (m_positions[i] > 0)
			{
				m_positions[i]--;
			}
		}
		return result;
	}

	bool CSolutions::Empty() const
	{
		return m_solutions.empty();
	}
}