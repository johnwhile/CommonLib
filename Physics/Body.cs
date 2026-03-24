using System;
using System.Collections.Generic;
using System.Text;

using Common.Maths;

namespace Common.Physics
{
    public class RigidBody2D
    {
        static Vector2f Gravity = new Vector2f(0, -10);

        Matrix3x3f transform;

        public Vector2f Position
        {
            get { return transform.Position;}
            set { transform.Position = value; }
        }


        public float Mass;
        public Vector2f Velocity;
        public Vector2f Acceleration;

        public RigidBody2D(Vector2f Position, Vector2f Velocity)
        {
            this.Position = Position;
            this.Velocity = Velocity;
        }


        public void IntegrateStep(float dt)
        {
            Position += Velocity * dt + (Acceleration + Gravity) * (dt * dt);
        }
    }
}
