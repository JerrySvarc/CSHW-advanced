using System.Drawing;

namespace JumpingPlatformGame {
	class Entity {
		public virtual Color Color => Color.Black;
	}

	//TODO: implement movable entity
	class MovableEntity : Entity {





	}
	//TODO: implement MovableJumpingEntity
	class MovableJumpingEntity : MovableEntity {


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