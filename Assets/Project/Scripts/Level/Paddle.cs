using UnityEngine;

namespace Twode.Pong
{
    public sealed class Paddle : PongObject
    {
        public void Move(float direction)
        {
            float velocity = Speed * direction;
            if(Mathf.Abs(velocity) >= Speed) velocity = Speed * Mathf.Sign(direction);

            rb.linearVelocityY = velocity;
        }
    }
}