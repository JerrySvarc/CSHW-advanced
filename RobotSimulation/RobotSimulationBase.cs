//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved. 
// 
//  File: RoomPoint.cs
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AntisocialRobots
{
    public abstract class RobotSimulationBase
    {

        #region Constructors

        public RobotSimulationBase(int size)
        {
            ROOM_SIZE = size;

            _roomCells = new Robot[ROOM_SIZE, ROOM_SIZE];
            _lastRoom = new Robot[ROOM_SIZE, ROOM_SIZE];
            _robots = new List<Robot>();
            _movableRobots = new List<Robot>();
        }

        #endregion

        public List<Robot> Robots { get { return _robots; } }

        /// <summary>
        /// The width and height of the "room" (which is square) in "robot coordinates".  In
        /// other words, this many robots can sit side by side against one wall.
        /// </summary>
        protected int ROOM_SIZE { get; private set; }

        /// <summary>
        /// A map of the "room": item [x,y] (in "robot coordinates"; see RoomPoint) is the
        /// Robot that is at that spot in the room, or null if that spot is empty.
        /// </summary>
        protected Robot[,] _roomCells;
        protected Robot[,] _lastRoom;

        /// <summary>The list of all robots.</summary>
        protected List<Robot> _robots;
        /// <summary>The list of movable robots.</summary>
        protected List<Robot> _movableRobots;

        #region Bug Diagnosis/Fix

        public bool FoundBug { get; private set; }
        public Robot Robot1 { get; private set; }
        public Robot Robot2 { get; private set; }
        public RoomPoint ConflictLocation { get; private set; }

        /// <summary>
        /// Checks the board state for inconsistent robot locations.
        /// e.g. Checks to see if any two robots share the same location.
        /// </summary>
        /// <returns>true if a bug was found; otherwise, false.</returns>
        public bool CheckInvariant()
        {
            foreach (var robot in _robots)
            {
                var robotInCell = _roomCells[robot.Location.X, robot.Location.Y];
                if (robotInCell != robot)
                {
                    // stop the simulation
                    // store the bug configuration
                    this.ConflictLocation = robot.Location;
                    // remember the two robots
                    this.Robot1 = robot;
                    this.Robot2 = robotInCell;
                    this.FoundBug = true;
                    break;
                }
            }
            if (!FoundBug)
            {
                // make a copy of last room
                for (int i = 0; i < ROOM_SIZE; i++)
                    for (int j = 0; j < ROOM_SIZE; j++)
                        _lastRoom[i, j] = _roomCells[i, j];
            }
            return FoundBug;
        }

        #endregion

        /// <summary>
        /// Restores the robot locations to previous state based on lastRoom.
        /// </summary>
        public virtual void RestoreRoom()
        {
            for (int i = 0; i < ROOM_SIZE; i++)
            {
                for (int j = 0; j < ROOM_SIZE; j++)
                {
                    // restore the room
                    _roomCells[i, j] = _lastRoom[i, j];
                    // restore the robot locations
                    Robot robot2 = _lastRoom[i, j];
                    if (robot2 != null)
                        robot2.Location = new RoomPoint(i, j);
                }
            }
        }

        /// <summary>Creates a robot, which is placed at a given location with the room.</summary>
        public Robot CreateRobot(int x, int y, bool isMovable = true)
        {
            return CreateRobot(new RoomPoint(x, y), isMovable);
        }

        /// <summary>Creates a robot, which is placed at a given location with the room.</summary>
        /// <param name="pt">Where the robot should be placed, in RoomPoint coordinates.</param>
        public virtual Robot CreateRobot(RoomPoint pt, bool isMovable = true)
        {
            // Do nothing if there's already a robot here
            if (_roomCells[pt.X, pt.Y] != null) return null;

            // Create the new robot
            Robot robot = new Robot(isMovable) {
                Id = _robots.Count,
                Location = pt,
            };

            // Add the robot to our data structures
            _robots.Add(robot);
            if (isMovable)
                _movableRobots.Add(robot);
            _roomCells[pt.X, pt.Y] = robot;

            return robot;
        }

        public int FrameIdx { get; set; }

        public void PerformFrameUpdate()
        {
            OnPerformFrameUpdate(FrameIdx);

            FrameIdx++;
        }

        /// <summary>
        /// When implemented in a derived class, this method shall perform
        /// the logic for updating the simulation forward one frame.
        /// </summary>
        protected abstract void OnPerformFrameUpdate(int frameIdx);

        // The following methods were refactored away to allow our tests to exclude them
        // from preemptions since they use all local data.

        protected static void AggregateEffectiveDirection(RoomPoint ptR, RoomPoint ptS, ref double vectorX, ref double vectorY)
        {
            double inverseSquareDistance = 1.0 / RoomPoint.Square(ptR.DistanceTo(ptS));
            double angle = ptR.AngleTo(ptS);
            vectorX -= inverseSquareDistance * Math.Cos(angle);
            vectorY -= inverseSquareDistance * Math.Sin(angle);
        }

        protected static RoomPoint ComputeNewLocation(RoomPoint prevLoc, double vectorX, double vectorY, int roomSize)
        {
            double degrees = Math.Atan2(vectorY, vectorX) * 180 / Math.PI;
            degrees += 22.5;
            while (degrees < 0) degrees += 360;
            while (degrees >= 360) degrees -= 360;

            int direction = (int)(degrees * 8 / 360);

            // update the robot's (local) location
            if ((direction == 7) || (direction == 0) || (direction == 1))
                prevLoc.X = Math.Min(prevLoc.X + 1, roomSize - 1);
            else if ((direction == 3) || (direction == 4) || (direction == 5))
                prevLoc.X = Math.Max(prevLoc.X - 1, 0);

            if ((direction == 1) || (direction == 2) || (direction == 3))
                prevLoc.Y = Math.Min(prevLoc.Y + 1, roomSize - 1);
            else if ((direction == 5) || (direction == 6) || (direction == 7))
                prevLoc.Y = Math.Max(prevLoc.Y - 1, 0);
            return prevLoc;
        }

    }
}
