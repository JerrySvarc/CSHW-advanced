using System;
using System.Drawing;
using GamePhysics;

namespace JumpingPlatformGame {

	struct WorldPoint
    {
		public Meters X;
		public Meters Y;
	}

	struct Heading
    {
		public Meters LowerBound;
		public Meters UpperBound;
		public Speed Speed;
    }
	
	abstract class Entity {
		public virtual Color Color => Color.Black;
		public WorldPoint Location;
		public Heading Horizontal;
		public Heading Vertical;

		public abstract void Update(Seconds deltaSeconds);
    }

	class MovableEntity : Entity {

		public override void Update(Seconds deltaSeconds)
		{
			Meters newLocationX = (Location.X.Value + (deltaSeconds.Value * Horizontal.Speed.Value)).Meters();

            if (newLocationX.Value < Horizontal.LowerBound.Value)
            {
				Location.X.Value = Horizontal.LowerBound.Value;
				Horizontal.Speed.Value *= -1;
			}
			else if (newLocationX.Value > Horizontal.UpperBound.Value)
            {
				Location.X.Value = Horizontal.UpperBound.Value;
				Horizontal.Speed.Value *= -1;
			}
            else
            {
				Location.X = newLocationX;
            }
		}
	}

	class MovableJumpingEntity : MovableEntity {


		public override void Update(Seconds deltaSeconds)
		{
			Meters newLocationX = (Location.X.Value + (deltaSeconds.Value * Horizontal.Speed.Value)).Meters();

			if (newLocationX.Value < Horizontal.LowerBound.Value)
			{
				Location.X.Value = Horizontal.LowerBound.Value;
				Horizontal.Speed.Value *= -1;
			}
			else if (newLocationX.Value > Horizontal.UpperBound.Value)
			{
				Location.X.Value = Horizontal.UpperBound.Value;
				Horizontal.Speed.Value *= -1;
			}
			else
			{
				Location.X = newLocationX;
			}

			Meters newLocationY = (Location.Y.Value + (deltaSeconds.Value * Vertical.Speed.Value)).Meters();
			
			if (newLocationY.Value < Vertical.LowerBound.Value)
			{
				Location.Y.Value = Vertical.LowerBound.Value;
			}
			else if (newLocationY.Value > Vertical.UpperBound.Value)
			{
				Location.Y.Value = Vertical.UpperBound.Value;
				Vertical.Speed.Value *= -1;
			}
			else
			{
				Location.Y = newLocationY;
			}
		}
	}

	class Joe : MovableEntity {
		public override string ToString() => "Joe";
		public override Color Color => Color.Blue;
	}

	class Jack : MovableEntity {
		public override string ToString() => "Jack";
		public override Color Color => Color.LightBlue;
	}

	class Jane : MovableJumpingEntity {
		public override string ToString() => "Jane";
		public override Color Color => Color.Red;
	}

	class Jill : MovableJumpingEntity {
		public override string ToString() => "Jill";
		public override Color Color => Color.Pink;
	}

}