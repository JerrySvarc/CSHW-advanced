//--------------------------------------------------------------------------
// 
//  Copyright (c) Microsoft Corporation.  All rights reserved.
//  Updated by Pavel Jezek, Charles University in Prague 
// 
//  File: RoomSimulation.cs
//
//--------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace AntisocialRobots
{
    public class RobotSimulation : RobotSimulationBase
    {
		public enum FrameUpdateMode {
			Sequential = 0,
			Parallel
		}

		public FrameUpdateMode UpdateMode { get; set; }

		public RobotSimulation(int size)
			: base(size) {
		}

		protected override void OnPerformFrameUpdate(int frameIdx) {
			switch (UpdateMode) {
				case FrameUpdateMode.Sequential:
					FrameUpdate_Sequential();
					break;
				case FrameUpdateMode.Parallel:
					FrameUpdate_Parallel(SimulateOneStep_NoLocks);
					break;
			}
		}

		private void FrameUpdate_Sequential() {
			Action<Robot> SimulateOneStep = SimulateOneStep_NoLocks;
			foreach (Robot robot in _movableRobots)
				SimulateOneStep(robot);
		}

		private void FrameUpdate_Parallel(Action<Robot> SimulateOneStep) {
			int threadCount = Environment.ProcessorCount;

			if (_movableRobots.Count < threadCount) {
				FrameUpdate_Sequential();
				return;
			}

			int robotsPerThread = _movableRobots.Count / threadCount;
			int robotsUnassigned = _movableRobots.Count % threadCount;

			Thread[] threads = new Thread[threadCount];
			for (int i = 0; i < threads.Length; i++) {
				int from = i * robotsPerThread;
				int to = from + robotsPerThread - 1;
				if (i == threads.Length - 1) {
					to += robotsUnassigned;
				}
				threads[i] = new Thread(() => {
					for (int ri = from; ri <= to; ri++) {
						SimulateOneStep(_movableRobots[ri]);
					}
				});
				threads[i].Start();
			}

			foreach (var t in threads) {
				t.Join();
			}
		}

		Object CellLock = new Object();
		Object RobotLock = new Object();
		/// <summary>Performs one step of the simulation for one robot.</summary>
		/// <param name="r">The robot to perform the simulation step for.</param>
		private void SimulateOneStep_NoLocks(Robot r)
        {
			
            if (!r.IsMovable)
                return;

            RoomPoint origLoc = r.Location; 
            RoomPoint newLoc = DetermineNewLocation(r);

			if (!newLoc.Equals(origLoc))
			{
                lock (CellLock) // jen jedno vlakno muze v danou chvili menit pozici robota na mapě
                {
					if (_roomCells[newLoc.X, newLoc.Y] == null) //potencionalni data race - jine vlakno muze po vykonani tohoto checku hodnotu zmenit
					{
						_roomCells[origLoc.X, origLoc.Y] = null; //potencionalni data race - zapisovani, kam se nejaky jiny robot uz mohl zapsat nebo tuto pozici čte
						_roomCells[newLoc.X, newLoc.Y] = r; //potencionalni data race - nejaky jiny robot uz se mohl na tuto lokaci dostat pred timto statementem
                        lock (RobotLock)
                        {
							r.Location = newLoc; //potencionalni data race - zapisujeme, ale mezitim mohou cist jini roboti starou lokaci
						}
					}
				}
			}
        }

        /// <summary>
        /// Computes the new desired location for the robot.
        /// This version doesn't do any thread synchronization
        /// operations, and is therefore NOT thread-safe.
        /// </summary>
        /// <param name="r"></param>
        /// <returns></returns>
        private RoomPoint DetermineNewLocation(Robot r)
        {
            RoomPoint ptR = r.Location; // zde je location immutable
            double vectorX = 0, vectorY = 0;

            lock (RobotLock) // Cist location ostatnich robotu lze pouze tehdy, pokud jine vlakno nemeni pozici nejakeho z robotů
            {
				foreach (Robot s in _robots)
				{
					if (r == s) continue;
					RoomPoint ptS = s.Location; //potencionalni data race - cteme location, kterou muze jine vlakno kdykoliv zmenit

					double inverseSquareDistance = 1.0 / RoomPoint.Square(ptR.DistanceTo(ptS));
					double angle = ptR.AngleTo(ptS);
					vectorX -= inverseSquareDistance * Math.Cos(angle);
					vectorY -= inverseSquareDistance * Math.Sin(angle);
				}
			}
            return ComputeNewLocation(ptR, vectorX, vectorY, ROOM_SIZE);
        }

    }
}
