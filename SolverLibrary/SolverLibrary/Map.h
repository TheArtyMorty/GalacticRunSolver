#pragma once

#include <vector>
#include <string>

namespace RobotSolver
{
static int mapSize = 16;

enum class EWallOrientation
{
	None = 0,
	TopLeft,
	TopRight,
	BottomLeft,
	BottomRight
};

struct Cell
{
public:
	EWallOrientation orientation;
	unsigned char x;
	unsigned char y;
};

enum class ERobotColor : char
{
	Red = 0,
	Green,
	Blue,
	Yellow
};

struct Robot
{
public:
	ERobotColor color;
	unsigned char x;
	unsigned char y;
};

struct Target
{
public:
	ERobotColor targetColor;
	unsigned char x;
	unsigned char y;
};

enum class EMoveDirection : char
{
	Up = 0,
	Right,
	Down,
	Left
};

struct Move
{
private:
	int move;

public:
	Move(ERobotColor color, EMoveDirection direction);
	ERobotColor		robot() const;
	EMoveDirection	direction() const;
};

class Map
{
public:
	Map(const std::string& filepath);
	std::vector<Robot> GetRobots() const;
	Target GetTarget() const;
	int GetMapSize() const;
	int GetHeuristicOfCell(int x, int y) const ;
	std::vector<Cell> GetAccessibleCellsFrom(int x, int y, EMoveDirection direction) const;

private:
	void Initialize();
	bool IsMoveValid(int x, int y, EMoveDirection direction) const;

private:
	int											m_size;
	std::vector<Cell>							m_walls;
	std::vector<Robot>							m_robots;
	std::vector<std::vector<EWallOrientation>>	m_cells;
	Target										m_target;
	std::vector<std::vector<int>>				m_heuristic;
	std::vector<std::vector<std::vector<std::vector<Cell>>>> m_accessiblesCells;
};
}