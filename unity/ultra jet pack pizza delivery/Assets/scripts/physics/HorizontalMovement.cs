using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace Assets.scripts.physics
{
    public class HorizontalMovement : MonoBehaviour
    {

        public float maxSpeed;
        public float accelerationDuration;
        public float decelerationDuration;
        public float frictionAmout;

        public void execute(float xInput, Rigidbody2D rb, bool isGroundedBool)
        {
            float xSpeed = rb.velocity.x;

            // Manage acceleration
            float targetSpeed = xInput * maxSpeed;
            float duration = targetSpeed == 0 ? decelerationDuration : accelerationDuration;
            Vector2 xAcceleration = Vector2.right * getAccelerationForce(xSpeed, targetSpeed, duration);
            rb.AddForce(xAcceleration);

            // Add friction when stopping
            if (MathF.Abs(xInput) == 0 && isGroundedBool && MathF.Abs(xSpeed) > 0)
            {
                float amout = MathF.Min(MathF.Abs(xSpeed), frictionAmout);
                amout *= - Math.Sign(xSpeed);
                rb.AddForce(amout * Vector2.right, ForceMode2D.Impulse);
            }
        }

        private float getAccelerationForce(float currentSpeed, float targetSpeed, float accelerationDuration)
        {
            return (targetSpeed - currentSpeed) / accelerationDuration;
        }
    }
}
