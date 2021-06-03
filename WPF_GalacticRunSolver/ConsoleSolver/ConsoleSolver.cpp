// ConsoleSolver.cpp : This file contains the 'main' function. Program execution begins and ends there.
//
#pragma once

#include <iostream>
#include "../../SolverLibrary/SolverLibrary/SolverLibrary.h"
#include "../../SolverLibrary/SolverLibrary/Map.h";
#include <string>

static std::string StringColorFromRobotColor(RobotSolver::ERobotColor color)
{
    switch (color)
    {
    case RobotSolver::ERobotColor::Blue:
        return "Blue";
    case RobotSolver::ERobotColor::Red:
        return "Red";
    case RobotSolver::ERobotColor::Green:
        return "Green";
    case RobotSolver::ERobotColor::Yellow:
        return "Yellow";
    default:
        return "No Color ?!?";
    }
}

static std::string DirectionFromMove(RobotSolver::EMoveDirection move)
{
    switch (move)
    {
    case RobotSolver::EMoveDirection::Up:
        return "Up";
    case RobotSolver::EMoveDirection::Down:
        return "Down";
    case RobotSolver::EMoveDirection::Left:
        return "Left";
    case RobotSolver::EMoveDirection::Right:
        return "Right";
    default:
        return "Not valid move ?!?";
    }
}

static void OutputSolution(const RobotSolver::Moves& solution)
{
    std::cout << "Solution : " << std::endl;
    auto moves = solution.GetMoves();
    for (const auto& move : moves)
    {
        std::cout << StringColorFromRobotColor(move.robot()) << " " << DirectionFromMove(move.direction()) << std::endl;
    }
    std::cout << std::endl;
}

class MyConsoleLogger final : public RobotSolver::ILogger
{
public:
    void Log(std::string input) final
    {
        std::cout << input << std::endl;
    }
};


//Ez map 3 moves = TestMap.txt

//HardMap 14 moves : HardMap1_14moves.txt


int main()
{
    auto map = RobotSolver::Map("C://Users//lucm//source//repos//RobotSolver//Maps//HardMap1_14moves.txt");
    auto myLogger = MyConsoleLogger{};
    auto theSolver = RobotSolver::Solver{15, &myLogger };
    auto solutions = theSolver.Solve(map);

    if (!solutions.empty())
    {
        for (const auto& solution : solutions)
        {
            OutputSolution(solution);
        }
    }
    else
    {
        std::cout << "No solution has been found..." << std::endl;
    }
}