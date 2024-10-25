using JetBrains.Annotations;
using UnityEngine;

namespace Twode.Pong
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class PongObject : MonoBehaviour
    {
        public Vector2 Velocity { get; private set; }
        public bool Freezed { get; private set; }
        public float Speed { get; protected set; }

        [SerializeField] private float _baseSpeed = 5f;
        
        [NotNull] private Rigidbody2D _rb = null!;
        private Vector2 _velocity;
        private Vector3 _startPosition;

        protected virtual void Awake()
        {
            OnValidate();
            _startPosition = transform.position;
            Speed = _baseSpeed;
        }
        
        public void Move(Vector2 direction)
        {
            if(Freezed) return;
            
            Velocity = Speed * direction.normalized;
            _rb.linearVelocity = Velocity;
        }

        public void Freeze()
        {
            Freezed = true;
            _rb.linearVelocity = Vector2.zero;
        }

        public void Unfreeze()
        {
            Freezed = false;
            _rb.linearVelocity = Velocity;
        }

        public void ResetObject()
        {
            transform.position = _startPosition;
            Speed = _baseSpeed;
            _rb.linearVelocity = Vector2.zero;
        }
        
        protected virtual void OnValidate()
        {
            _rb = GetComponent<Rigidbody2D>()!;
            _rb.gravityScale = 0f;
            _rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            _rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            _rb.freezeRotation = true;
        }
    }
}