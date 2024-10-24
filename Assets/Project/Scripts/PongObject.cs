using JetBrains.Annotations;
using UnityEngine;

namespace Twode.Pong
{
    [RequireComponent(typeof(Rigidbody2D))]
    public abstract class PongObject : MonoBehaviour
    {
        public bool Freezed { get; private set; }

        [SerializeField] protected float speed;

        [NotNull] protected Rigidbody2D rb { get; private set; } = null!;

        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>()!;
        }

        public void Freeze()
        {
            Freezed = true;
        }

        public void Unfreeze()
        {
            Freezed = false;
        }
        
        protected virtual void OnValidate()
        {
            rb = GetComponent<Rigidbody2D>()!;
            rb.gravityScale = 0f;
            rb.freezeRotation = true;
            rb.interpolation = RigidbodyInterpolation2D.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        }
        
        protected virtual void OnFreeze() {}
        protected virtual void OnUnfreeze() {}
    }
}