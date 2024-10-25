using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.Serialization;

namespace Twode.Pong
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class PongObject : MonoBehaviour
    {
        public bool Freezed { get; private set; }
        public float Speed { get; protected set; }

        [SerializeField] private float _baseSpeed = 5f;
        
        [NotNull] protected Rigidbody2D rb = null!;
        private Vector3 _startPosition;

        protected virtual void Awake()
        {
            OnValidate();
            _startPosition = transform.position;
            Speed = _baseSpeed;
        }

        public void Freeze()
        {
            Freezed = true;
            OnFreeze();
        }

        public void Unfreeze()
        {
            Freezed = false;
            OnUnfreeze();
        }

        public void ResetObject()
        {
            transform.position = _startPosition;
            Speed = _baseSpeed;
            rb.linearVelocity = Vector2.zero;
        }
        
        protected virtual void OnValidate()
        {
            rb = GetComponent<Rigidbody2D>()!;
            rb.gravityScale = 0f;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
            rb.freezeRotation = true;
        }
        
        protected virtual void OnFreeze() {}
        protected virtual void OnUnfreeze() {}
    }
}