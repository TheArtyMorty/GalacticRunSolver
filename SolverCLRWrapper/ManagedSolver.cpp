#include "ManagedSolver.h"
#include "ManagedTypeConversionUtilities.h"

namespace CLI
{


// ----------- Logger -------------------------
LoggerWrapper::LoggerWrapper(IManagedLogger^ managedLogger)
{
	m_managedLogger = managedLogger;
}

void LoggerWrapper::Log(std::string input)
{
	m_managedLogger->Log(gcnew String(input.c_str()));
}




// ------------  Map  -------------------
ManagedMap::ManagedMap(String^ filepath) : 
	pMyMap(new RobotSolver::Map(string_to_char_array(filepath)))
{
}

RobotSolver::Map* ManagedMap::GetMap()
{
	return pMyMap;
}

ManagedMap::~ManagedMap()
{
	if (pMyMap != nullptr)
	{
		delete pMyMap;
	}
}
ManagedMap::!ManagedMap()
{
	if (pMyMap != nullptr)
	{
		delete pMyMap;
	}
}





// ------------- Move ----------------
ManagedMove::ManagedMove(ERobotColor color, EMoveDirection direction)
{
	Color = color;
	Move = direction;
}

ManagedMove::ManagedMove(RobotSolver::Move move)
{
	Color = (CLI::ERobotColor)move.robot();
	Move = (CLI::EMoveDirection)move.direction();
}




// ------------- Solution ----------------
ManagedSolution::ManagedSolution()
{
	Moves = gcnew System::Collections::Generic::List<ManagedMove^>();
}





// ----------- Solver --------------------
ManagedSolver::ManagedSolver(int maxMovesSafety, IManagedLogger^ managedLogger):
	pMyLogger(new LoggerWrapper(managedLogger)),
	pMySolver(new RobotSolver::Solver(maxMovesSafety, pMyLogger))
{
}

ManagedSolver::~ManagedSolver()
{
	if (pMyLogger != nullptr)
	{
		delete pMyLogger;
	}
	if (pMySolver != nullptr)
	{
		delete pMySolver;
	}
}
ManagedSolver::!ManagedSolver()
{
	if (pMyLogger != nullptr)
	{
		delete pMyLogger;
	}
	if (pMySolver != nullptr)
	{
		delete pMySolver;
	}
}
   
// --------------- Solver ----------------------
Collections::Generic::List<ManagedSolution^>^ ManagedSolver::GetAllSolutions(ManagedMap^ map)
{
	auto solutions = gcnew System::Collections::Generic::List<ManagedSolution^>();
	if (auto result = pMySolver->Solve(*map->GetMap()); !result.empty())
	{
		for (auto sol : result)
		{
			auto solution = gcnew ManagedSolution();
			for (const auto& move : sol.GetMoves())
			{
				solution->Moves->Add(gcnew ManagedMove(move));
			}
			solutions->Add(solution);
		}
	}
	return solutions;
}

}