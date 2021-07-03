#pragma once
#include "../SolverLibrary/SolverLibrary/SolverLibrary.h"
#include <cliext/list>
#include <vcclr.h>

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



    class LoggerWrapper final : public RobotSolver::ILogger
    {
    public:
        LoggerWrapper(IManagedLogger^ managedLogger);
        ~LoggerWrapper();
        void Log(std::string input) final;
    private:
        gcroot<IManagedLogger^> m_managedLogger;
    };



    public ref class  ManagedMap
    {
    public:
        ManagedMap(String^ filepath);
        RobotSolver::Map* GetMap();
        ~ManagedMap();
        !ManagedMap();

    private:
        RobotSolver::Map* pMyMap;
    };




    public ref class ManagedSolver
    {
    public:
        ManagedSolver(int maxMovesSafety, IManagedLogger^ managedLogger);
        Collections::Generic::List<ManagedSolution^>^ GetAllSolutions(ManagedMap^ map);
        ~ManagedSolver();
        !ManagedSolver();

    private:
        LoggerWrapper* pMyLogger;
        RobotSolver::Solver* pMySolver;
    };


}