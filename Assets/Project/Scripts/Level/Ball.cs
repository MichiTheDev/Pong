using System;
using UnityEngine;

namespace Twode.Pong
{
    [RequireComponent(typeof(BoxCollider2D))]
    public sealed class Ball : PongObject
    {
        // -1 = left, 1 = right
        public event Action<int> OnGoalTouched;

        public Vector2 Velocity => rb.linearVelocity;
        
        [SerializeField, Range(5f, 85f)] private float _maxBounceAngle = 60f;
        [SerializeField] private float _speedPerCollision = 0.25f;

        private Vector2 _velocity;
        private BoxCollider2D _collider;

        protected override void Awake()
        {
            base.Awake();
            _collider = GetComponent<BoxCollider2D>();
        }

        public void Launch(int direction)
        {
            SetVelocity(Speed * (Mathf.Sign(direction) * Vector2.one.normalized).normalized);
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
                Bounds paddleBounds = other.collider.bounds;
                Vector2 contactPoint = other.contacts[0].point;

                Bounds ballBounds = _collider!.bounds;
                
                bool hitAbove = contactPoint.y - ballBounds.extents.y > paddleBounds.max.y;
                bool hitBelow = contactPoint.y + ballBounds.extents.y < paddleBounds.min.y;
                if (hitAbove || hitBelow)
                {
                    OnGoalTouched?.Invoke(transform.position.x < 0 ? -1 : 1);
                    return;
                }

                Speed += _speedPerCollision;
                
                float paddleYCenter = paddleBounds.center.y;
                float relativeIntersectY = contactPoint.y - paddleYCenter;
                float normalizedRelativeIntersectionY = relativeIntersectY / paddleBounds.extents.y; 
                float bounceAngle = normalizedRelativeIntersectionY * _maxBounceAngle * Mathf.Deg2Rad;
                
                Vector2 newDirection = new Vector2(Mathf.Cos(bounceAngle) * Mathf.Sign(-_velocity.x), Mathf.Sin(bounceAngle));
                SetVelocity(Speed * newDirection.normalized);
            }
            else if(other.collider.CompareTag("Border"))
            {
                SetVelocity(new Vector2(_velocity.x, -_velocity.y));
            }
        }

        private void SetVelocity(Vector2 velocity)
        {
            _velocity = velocity;
            rb.linearVelocity = _velocity;
        }

        protected override void OnFreeze()
        {
            rb.linearVelocity = Vector2.zero;
        }

        protected override void OnUnfreeze()
        {
            rb.linearVelocity = _velocity;
        }
    }
}