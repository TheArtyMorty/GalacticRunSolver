#include "Map.h"
#include <fstream>
#include <sstream>
#include <queue>
#include <iostream>

namespace RobotSolver
{
std::vector<Robot> Map::GetRobots() const
{
    return m_robots;
}

int Map::GetMapSize() const
{
    return m_size;
}

int Map::GetHeuristicOfCell(int x, int y) const
{
    return m_heuristic[x][y];
}

static std::pair<unsigned char, unsigned char> NextCell(unsigned char x, unsigned char y, EMoveDirection direction)
{
    switch (direction)
    {
    case EMoveDirection::Down:
        return {x,y+1};
    case EMoveDirection::Up:
        return {x,y-1};
    case EMoveDirection::Left:
        return {x-1,y};
    case EMoveDirection::Right:
    default:
        return {x+1,y};
    }
}

void Map::Initialize()
{
    //Initialize accessibles
    m_accessiblesCells.clear();
    for (unsigned char i = 0; i < m_size; i++)
    {
        auto tempi = std::vector<std::vector<std::vector<Cell>>>{};
        for (unsigned char j = 0; j < m_size; j++)
        {
            auto tempj = std::vector<std::vector<Cell>>{};
            for (auto move : { EMoveDirection::Up, EMoveDirection::Right, EMoveDirection::Down, EMoveDirection::Left })
            {
                auto accessibles = std::vector<Cell>{};
                auto current = std::pair<unsigned char, unsigned char>{ i,j };
                while (IsMoveValid(current.first, current.second, move))
                {
                    auto next = NextCell(current.first, current.second, move);
                    accessibles.push_back(Cell{ EWallOrientation::None, next.first, next.second });
                    current = next;
                }
                tempj.push_back(accessibles);
            }
            tempi.push_back(tempj);
        }
        m_accessiblesCells.push_back(tempi);
    }
    //Initialize heuristic
    int initValue = m_size * m_size;
    m_heuristic = std::vector<std::vector<int>>(m_size,std::vector<int>(m_size, initValue));
    m_heuristic[m_target.x][m_target.y] = 0;

    std::queue<std::pair<int, int>> previousCells;
    previousCells.push({m_target.x, m_target.y});

    while (!previousCells.empty())
    {
        auto cell = previousCells.front();
        previousCells.pop();
        auto nextCellHValue = m_heuristic[cell.first][cell.second] + 1;

        for (auto move : { EMoveDirection::Up, EMoveDirection::Right, EMoveDirection::Down, EMoveDirection::Left })
        {
            auto current = cell;
            auto next = NextCell(current.first, current.second, move);
            while (IsMoveValid(current.first, current.second, move) &&
                   m_heuristic[next.first][next.second] >= nextCellHValue)
            {
                m_heuristic[next.first][next.second] = nextCellHValue;
                previousCells.push(next);
                current = next;
                next = NextCell(current.first, current.second, move);
            }
        }
    }
}

std::vector<Cell> Map::GetAccessibleCellsFrom(int x, int y, EMoveDirection direction) const
{
    return m_accessiblesCells[x][y][(unsigned int)direction];
}

bool Map::IsMoveValid(int x, int y, EMoveDirection direction) const
{
    if ((x == 0 && direction == EMoveDirection::Left) || 
        (x == m_size-1 && direction == EMoveDirection::Right) ||
        (y == 0 && direction == EMoveDirection::Up) ||
        (y == m_size-1 && direction == EMoveDirection::Down))
    {
        return false;
    }
    const auto wallxy = m_cells[x][y];
    if ((direction == EMoveDirection::Down && (wallxy == EWallOrientation::BottomLeft || wallxy == EWallOrientation::BottomRight)) ||
        (direction == EMoveDirection::Up && (wallxy == EWallOrientation::TopLeft || wallxy == EWallOrientation::TopRight)) ||
        (direction == EMoveDirection::Left && (wallxy == EWallOrientation::BottomLeft || wallxy == EWallOrientation::TopLeft)) ||
        (direction == EMoveDirection::Right && (wallxy == EWallOrientation::TopRight || wallxy == EWallOrientation::BottomRight)))
    {
        return false;
    }
    EWallOrientation wall;
    switch (direction)
    {
    case EMoveDirection::Right:
        wall = m_cells[x + 1][y];
        if (wall == EWallOrientation::BottomLeft || wall == EWallOrientation::TopLeft) return false;
        break;
    case EMoveDirection::Left:
        wall = m_cells[x - 1][y];
        if (wall == EWallOrientation::BottomRight || wall == EWallOrientation::TopRight) return false;
        break;
    case EMoveDirection::Down:
        wall = m_cells[x][y+1];
        if (wall == EWallOrientation::TopLeft || wall == EWallOrientation::TopRight) return false;
        break;
    case EMoveDirection::Up:
        wall = m_cells[x][y-1];
        if (wall == EWallOrientation::BottomLeft || wall == EWallOrientation::BottomRight) return false;
        break;
    }
    return true;
}

Target Map::GetTarget() const
{
    return m_target;
}

static EWallOrientation GetWallOrientation(const std::string& orient)
{
    if (orient == "TL") return EWallOrientation::TopLeft;
    if (orient == "TR") return EWallOrientation::TopRight;
    if (orient == "BL") return EWallOrientation::BottomLeft;
    if (orient == "BR") return EWallOrientation::BottomRight;
    return EWallOrientation::None;
}

Map::Map(const std::string& filepath)
{
    std::ifstream file(filepath);
    file >> m_size;
    std::string line;
    getline(file, line);
    m_cells = std::vector<std::vector<EWallOrientation>>(m_size, std::vector<EWallOrientation>(m_size, EWallOrientation::None));
    for (unsigned char i = 0; i < m_size; i++)
    {
        getline(file, line);
        std::istringstream lineAsStream(line);
        std::string cell;
        
        for (unsigned char j = 0; j < m_size; j++)
        {
            lineAsStream >> cell;
            if (cell != "..")
            {
                this->m_walls.push_back({ GetWallOrientation(cell), j,i });
            }
            m_cells[j][i] = GetWallOrientation(cell);
        }
    }
    while (getline(file, line))
    { 
        if (line == "Robots")
        {
            int robotCount;
            file >> robotCount;
            for (int i = 0; i < robotCount; i++)
            {
                int x, y;
                file >> x >> y;
                this->m_robots.push_back({ static_cast<ERobotColor>(i) , static_cast<unsigned char>(x),  static_cast<unsigned char>(y) });
            }
        }
        else if (line == "Target")
        {
            int x, y, id;
            file >> x >> y >> id;
            this->m_target = { static_cast<ERobotColor>(id) ,  static_cast<unsigned char>(x),  static_cast<unsigned char>(y) };
        }
    }
    Initialize();
}


Move::Move(ERobotColor color, EMoveDirection direction)
{
    move = (int)color;
    move = move << 2;
    move += (int)direction;
}

ERobotColor Move::robot() const
{
    auto color = move >> 2;
    return (ERobotColor)color;
}

EMoveDirection Move::direction() const
{
    auto shifted = move >> 2;
    auto direction = move - (shifted << 2);
    return (EMoveDirection)direction;
}


}