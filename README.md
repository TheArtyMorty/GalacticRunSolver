# GalacticRunSolver

My attempt at solving galactic run (ricochet robots online) maps.
- Project is almosted finished (can we ever be?) :)

## Presentation

Galactic Run is an online adaptation of the boardgame "Ricochet Robots" (Rasende Roboter).
It provides cool functionnalities and an online versus mode : https://galactic.run/

We have been playing this cool online game with friends, and have been struggling several times to get the perfect solutions.
I decided to start this solver as a personal project (better solvers already exists) to improve my coding skills.

So far, the UI looks like this : 
![Banner](https://github.com/TheArtyMorty/GalacticRunSolver/blob/main/Documentation/ReadMe_Global.png)

You have the board on the left, which is customizable :
- you can move robots/target with drag and drops
- you can change a wall type by Left/Right clicking (loop through all wall types : Top Left, Top Right, Bottom Right, Bottom Left, Nothing)
- you can change the target color by double-clicking 

You also have options to save and load your maps (as txt files) on the top right side.

Then, whenever you want, you can solve the map.
This will log the process, and when the solutions have been found, they are displayed on the bottom right corner.
You can play solutions graphically (play button) to see the robots movements.

Update :
- You can now get the map from an URL (works with puzzles)
- You can also recognize the map from a screen section (works with versus game mode)
- Implemented a "bot" that can recognize the map, solve and play the solution for you. 
-- Obviously, this wasn't the point of my project, but it's very fun to see the bot solve a 11 moves map in less than a second.

## Solver

First, I would like to state that my goal is to be able to solve a 20 move map (considered very hard) in a reasonable time (less than 1 minute).

I previously use the Dijkstra algorythm (or in my own terms, the brut force algorythm) to calculate all possible moves from each state, and then proceed with those.
This was working fine with easy maps (7 moves and less), but the calculation time was exponentially related to the difficulty of the map.

I then switched to an A-Star algorythm to calculate the solutions.
Basically, this means that I calculate the most probable to succeed solutions first, and delay the unlikely ones (based on an heuristic function).
It should be much faster, and without having made any real comparisons with my previous algorythm, it seems that it is. 
The current solver works fine with any map with a solution in 10 moves or less.
However the number of states to calculate still grow exponentially, and I am not able to solve in reasonable time maps of 14 moves.

Update : After performance investigation, I was able to go much faster :
The 14 moves map now solves in 7 seconds. This could (and will need to) be improved, but that's an already very acceptable speed.

Update 2: After some investigation, with the A-Star algorythm, the 19 moves map is solved in about 1 minute. 
This was my initial goal so I'm pretty happy with the result.

## Projects

The main solution I use is "WPF_GalacticRunSolver\WPF_GalacticRunSolver.sln"
It contains the following projects 

SolverLibrary :
- Contains the actual solver that is used to calculate the solutions. 

ConsoleSolver : 
- A solver that only use the console for solving and showing the solutions.
- I only use it internally to test my solver (faster than with the UI).

RobotSolverTests :
- A unit test project with several predifined maps to avoid regressions in the solver.

SolverCLRWrapper :
- The c++/cli project that interface between the c# UI and the c++ solver.

WPF_GalacticRunSolver :
- The c# WPF UI for the solver. 

## Backlog

-Bot 
-- Improve the map recognition from the screen area (still fails sometimes)
-- Improve the area selection to be more user friendly.

Any proposal is welcomed :).
