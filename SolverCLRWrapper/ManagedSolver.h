#pragma once
#include "ManagedObject.h"
#include "../SolverLibrary/SolverLibrary/SolverLibrary.h"
#include <cliext/list>

using namespace System;
namespace CLI
{

    public enum class ERobotColor
    {
        Red = 0,
        Green,
        Blue,
        Yellow
    };

    public enum class EMoveDirection
    {
        Up = 0,
        Right,
        Down,
        Left
    };

    public ref class ManagedMove
    {
    public:
        ManagedMove(ERobotColor color, EMoveDirection direction);
        ManagedMove(RobotSolver::Move move);
        EMoveDirection Move;
        ERobotColor Color;
    };

    public ref class ManagedSolution
    {
    public:
        ManagedSolution();
        Collections::Generic::List<ManagedMove^>^ Moves;
    };

public interface class IManagedLogger
{
public:
    void Log(String^ input);
};

class LoggerWrapper;

public ref class  ManagedMap : public ManagedObject<RobotSolver::Map>
{
public:
    ManagedMap(String^ filepath);
};

public ref class ManagedSolver : public ManagedObject<RobotSolver::Solver>
{
public:
    ManagedSolver(int maxMovesSafety, IManagedLogger^ managedLogger);
    ManagedSolution^ Solve(ManagedMap^ map);
    Collections::Generic::List<ManagedSolution^>^ GetAllSolutions(ManagedMap^ map);
    ~ManagedSolver();
    !ManagedSolver();

private:
    LoggerWrapper* pMyLogger;
};
}