using System;
using UnityEngine;

namespace Twode.Pong
{
    public sealed class Ball : PongObject
    {
        // 0 = left, 1 = right
        public event Action<int> OnGoalTouched;

        [SerializeField, Range(5f, 85f)] private float _maxBounceAngle = 60f;
        [SerializeField] private float _additionalSpeedAfterCollision = 0.2f;
        
        private Vector2 _velocity;
        private float _actualSpeed;

        private void Start()
        {
            Reset();
        }

        // 0 (left side won) = direction right, 1 (right side won) = direction left
        public void Launch(int direction)
        {
            UpdateVelocity(_actualSpeed * (Mathf.Sign(direction) * Vector2.one.normalized));
        }

        public void Reset()
        {
            UpdateVelocity(Vector2.zero);
            transform.position = Vector3.zero;
            _actualSpeed = baseSpeed;
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if(other is null) return;

            if(other.CompareTag("Goal"))
            {
                OnGoalTouched?.Invoke(transform.position.x < 0 ? 0 : 1);
            }
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if(other?.collider is null || other.contacts is null) return;
            
            if(other.collider.CompareTag("Paddle"))
            {
                _actualSpeed += _additionalSpeedAfterCollision;
                
                Bounds paddleBounds = other.collider.bounds;
                Vector2 contactPoint = other.contacts[0].point;
                
                float paddleYCenter = paddleBounds.center.y;
                float relativeIntersectY = contactPoint.y - paddleYCenter;
                float normalizedRelativeIntersectionY = relativeIntersectY / paddleBounds.extents.y; 
                float bounceAngle = normalizedRelativeIntersectionY * _maxBounceAngle * Mathf.Deg2Rad;
                
                Vector2 newDirection = new Vector2(Mathf.Sign(_velocity.x) * -1, Mathf.Sin(bounceAngle)).normalized;
                UpdateVelocity(newDirection * _actualSpeed);
            }
        }

        protected override void OnFreeze()
        {
            rb.linearVelocity = Vector2.zero;
        }

        protected override void OnUnfreeze()
        {
            rb.linearVelocity = _velocity;
        }

        private void UpdateVelocity(Vector2 velocity)
        {
            _velocity = velocity;
            rb.linearVelocity = _velocity;
        }
    }
}