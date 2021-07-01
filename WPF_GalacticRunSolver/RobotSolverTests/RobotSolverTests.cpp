#include "pch.h"
#include "CppUnitTest.h"

#include "../../SolverLibrary/SolverLibrary/SolverLibrary.h"

using namespace Microsoft::VisualStudio::CppUnitTestFramework;
using namespace RobotSolver;

static const std::string mapsFolder = "C://Users//lucm//source//repos//GalacticRunSolver//Maps//";

static constexpr auto BlueRobot = ERobotColor::Blue;
static constexpr auto GreenRobot = ERobotColor::Green;
static constexpr auto RedRobot = ERobotColor::Red;
static constexpr auto YellowRobot = ERobotColor::Yellow;

static constexpr auto Up = EMoveDirection::Up;
static constexpr auto Down = EMoveDirection::Down;
static constexpr auto Left = EMoveDirection::Left;
static constexpr auto Right = EMoveDirection::Right;

namespace RobotSolverTests
{
	TEST_CLASS(RobotSolverTests)
	{
	public:

		class MyUnitTestsLogger final : public RobotSolver::ILogger
		{
		public:
			void Log(std::string input) final
			{
				//Do nothing
			}
		};

		static void CompareSolutions(const std::vector<Move>& moves, const std::vector<Move>& expectedMoves)
		{
			Assert::AreEqual(moves.size(), expectedMoves.size());
			for(size_t i = 0; i < moves.size(); i++)
			{
				auto actualMove = moves[i];
				auto expectedMove = expectedMoves[i];
				Assert::AreEqual((int)actualMove.direction(), (int)expectedMove.direction());
				Assert::AreEqual((int)actualMove.robot(), (int)expectedMove.robot());
			}
		}

		static std::vector<Solution> SolveMap(const std::string& path)
		{
			auto map = Map(path);
			auto myLogger = MyUnitTestsLogger{};
			auto theSolver = Solver{ 20, &myLogger };
			return theSolver.Solve(map);
		}

		TEST_METHOD(SolverLibraryTests_3_moves_blue_only)
		{
			auto solutions = SolveMap(mapsFolder + "TestMap.txt");

			Assert::IsTrue(!solutions.empty());
			Assert::AreEqual((int)solutions.size(), 1);

			auto moves = solutions.front().GetMoves();
			auto expectedMoves = std::vector<Move>{ 
				Move(BlueRobot, Up),
				Move(BlueRobot, Left), 
				Move(BlueRobot, Down)};

			CompareSolutions(moves, expectedMoves);
		}

		TEST_METHOD(SolverLibraryTests_6_moves_red_only)
		{
			auto solutions = SolveMap(mapsFolder + "TestMap2_6moves_redOnly.txt");

			Assert::IsTrue(!solutions.empty());
			Assert::AreEqual((int)solutions.size(), 1);

			auto moves = solutions.front().GetMoves();
			auto expectedMoves = std::vector<Move>{
				Move(GreenRobot, Down),
				Move(GreenRobot, Left),
				Move(GreenRobot, Down),
				Move(GreenRobot, Left),
				Move(GreenRobot, Up),
				Move(GreenRobot, Left) };

			CompareSolutions(moves, expectedMoves);
		}

		TEST_METHOD(SolverLibraryTests_7_moves_red_yellow)
		{
			auto solutions = SolveMap(mapsFolder + "TestMap2.txt");

			Assert::IsTrue(!solutions.empty());
			Assert::AreEqual((int)solutions.size(), 1);

			auto moves = solutions.front().GetMoves();
			auto expectedMoves = std::vector<Move>{
				Move(GreenRobot, Down),
				Move(RedRobot, Down),
				Move(GreenRobot, Left),
				Move(GreenRobot, Down),
				Move(GreenRobot, Left),
				Move(GreenRobot, Up),
				Move(GreenRobot, Left) };

			CompareSolutions(moves, expectedMoves);
		}

		TEST_METHOD(SolverLibraryTests_7_moves_three_robots)
		{
			auto solutions = SolveMap(mapsFolder + "Map_6.txt");

			Assert::IsTrue(!solutions.empty());
			Assert::AreEqual((int)solutions.size(), 1);

			auto moves = solutions.front().GetMoves();
			auto expectedMoves = std::vector<Move>{
				Move(BlueRobot, Up),
				Move(RedRobot, Left),
				Move(BlueRobot, Left),
				Move(BlueRobot, Down),
				Move(YellowRobot, Right),
				Move(BlueRobot, Right),
				Move(BlueRobot, Down) };

			CompareSolutions(moves, expectedMoves);
		}

		TEST_METHOD(SolverLibraryTests_2_solutions)
		{
			auto solutions = SolveMap(mapsFolder + "Map_5.txt");

			Assert::IsTrue(!solutions.empty());
			Assert::AreEqual((int)solutions.size(), 2);

			auto moves = solutions.back().GetMoves();
			auto expectedMoves = std::vector<Move>{
				Move(YellowRobot, Right),
				Move(BlueRobot, Down),
				Move(GreenRobot, Down),
				Move(RedRobot, Down),
				Move(YellowRobot, Left),
				Move(RedRobot, Up),
				Move(GreenRobot, Up) };

			CompareSolutions(moves, expectedMoves);
		}

		TEST_METHOD(SolverLibraryTests_3_solutions)
		{
			auto solutions = SolveMap(mapsFolder + "TestMap4_6moves3solutions.txt");

			Assert::IsTrue(!solutions.empty());
			Assert::AreEqual((int)solutions.size(), 3);

			auto moves = solutions.back().GetMoves();
			auto expectedMoves = std::vector<Move>{
				Move(BlueRobot, Up),
				Move(BlueRobot, Left),
				Move(BlueRobot, Up),
				Move(RedRobot, Left),
				Move(RedRobot, Down),
				Move(RedRobot, Right) };

			CompareSolutions(moves, expectedMoves);
		}

		TEST_METHOD(SolverLibraryTests_10_Moves)
		{
			auto solutions = SolveMap(mapsFolder + "TestMap5_10moves4solutions.txt");

			Assert::IsTrue(!solutions.empty());
			Assert::AreEqual((int)solutions.size(), 4);

			auto moves = solutions.front().GetMoves();
			auto expectedMoves = std::vector<Move>{
				Move(GreenRobot, Right),
				Move(GreenRobot, Down),
				Move(GreenRobot, Left),
				Move(GreenRobot, Down),
				Move(GreenRobot, Right),
				Move(GreenRobot, Down),
				Move(GreenRobot, Left),
				Move(GreenRobot, Down),
				Move(GreenRobot, Right),
				Move(GreenRobot, Up) };

			CompareSolutions(moves, expectedMoves);
		}

		TEST_METHOD(SolverLibraryTests_11_Moves)
		{
			auto solutions = SolveMap(mapsFolder + "TestMap6_11moves.txt");

			Assert::IsTrue(!solutions.empty());
			Assert::AreEqual((int)solutions.size(), 2);

			auto moves = solutions.front().GetMoves();
			auto expectedMoves = std::vector<Move>{
				Move(YellowRobot, Down),
				Move(YellowRobot, Right),
				Move(YellowRobot, Down),
				Move(RedRobot, Right),
				Move(GreenRobot, Right),
				Move(RedRobot, Up),
				Move(RedRobot, Left),
				Move(YellowRobot, Right),
				Move(RedRobot, Down),
				Move(YellowRobot, Left),
				Move(YellowRobot, Down) };

			CompareSolutions(moves, expectedMoves);
		}

		TEST_METHOD(SolverLibraryTests_Hard_14_Moves)
		{
			auto solutions = SolveMap(mapsFolder + "HardMap1_14moves.txt");

			Assert::IsTrue(!solutions.empty());
			Assert::AreEqual((int)solutions.size(), 1);

			auto moves = solutions.front().GetMoves();
			auto expectedMoves = std::vector<Move>{
				Move(RedRobot, Up),
				Move(RedRobot, Left),
				Move(RedRobot, Down),
				Move(GreenRobot, Left),
				Move(RedRobot, Up),
				Move(RedRobot, Left),
				Move(BlueRobot, Left),
				Move(BlueRobot, Up),
				Move(BlueRobot, Left),
				Move(RedRobot, Down),
				Move(RedRobot, Right),
				Move(RedRobot, Up),
				Move(RedRobot, Left),
				Move(RedRobot, Down) };

			CompareSolutions(moves, expectedMoves);
		}
	};
}
