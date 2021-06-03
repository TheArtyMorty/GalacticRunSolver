#include "ManagedSolver.h"
#include "ManagedTypeConversionUtilities.h"
#include <vcclr.h>

namespace CLI
{
class LoggerWrapper final : public RobotSolver::ILogger
{
public:
	LoggerWrapper(IManagedLogger^ managedLogger);
	void Log(std::string input) final;
private:
	gcroot<IManagedLogger^> m_managedLogger;
};

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
	ManagedObject(new RobotSolver::Map(string_to_char_array(filepath)))
{

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
	ManagedObject(new RobotSolver::Solver(maxMovesSafety, pMyLogger))
{
}

ManagedSolver::~ManagedSolver()
{
	if (pMyLogger != nullptr)
	{
		delete pMyLogger;
	}
}
ManagedSolver::!ManagedSolver()
{
	if (pMyLogger != nullptr)
	{
		delete pMyLogger;
	}
}
   
// --------------- Solver ----------------------
ManagedSolution^ ManagedSolver::Solve(ManagedMap^ map)
{
	if (auto result = m_Instance->Solve(*map->GetInstance()); !result.empty())
	{
		auto firstSolution = result.front();
		auto solution = gcnew ManagedSolution();
		for (const auto& move : firstSolution.GetMoves())
		{
			solution->Moves->Add(gcnew ManagedMove(move));
		}
		return solution;
	}
	return gcnew ManagedSolution();
}

Collections::Generic::List<ManagedSolution^>^ ManagedSolver::GetAllSolutions(ManagedMap^ map)
{
	auto solutions = gcnew System::Collections::Generic::List<ManagedSolution^>();
	if (auto result = m_Instance->Solve(*map->GetInstance()); !result.empty())
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