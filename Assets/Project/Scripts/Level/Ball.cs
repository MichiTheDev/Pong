using System;
using UnityEngine;

namespace Twode.Pong
{
    public sealed class Ball : PongObject
    {
        // -1 = left, 1 = right
        public event Action<int> OnGoalTouched;

        [SerializeField, Range(5f, 85f)] private float _maxBounceAngle = 60f;
        [SerializeField] private float _additionalSpeedAfterCollision = 0.2f;

        // -1 (left side won) = direction right, 1 (right side won) = direction left
        public void Launch(int direction)
        {
            Move(Mathf.Sign(direction) * Vector2.one.normalized);
        }
        
        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other is null) return;

            if(other.CompareTag("Goal"))
            {
                OnGoalTouched?.Invoke(transform.position.x < 0 ? -1 : 1);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if(other?.collider is null || other.contacts is null) return;
            
            if(other.collider.CompareTag("Paddle"))
            {
                Speed += _additionalSpeedAfterCollision;
                
                Bounds paddleBounds = other.collider.bounds;
                Vector2 contactPoint = other.contacts[0].point;
                
                float paddleYCenter = paddleBounds.center.y;
                float relativeIntersectY = contactPoint.y - paddleYCenter;
                float normalizedRelativeIntersectionY = relativeIntersectY / paddleBounds.extents.y; 
                float bounceAngle = normalizedRelativeIntersectionY * _maxBounceAngle * Mathf.Deg2Rad;
                
                Vector2 newDirection = new Vector2(Mathf.Sign(Velocity.x) * -1, Mathf.Sin(bounceAngle));
                Move(newDirection);
            }
        }
    }
}