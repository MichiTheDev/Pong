using UnityEngine;

namespace Twode.Pong
{
    public sealed class AIPaddleController : MonoBehaviour, IPaddleController
    {
        private const float SLOW_DOWN_THRESHOLD = 0.2f;
        
        public Paddle Paddle { get; set; }

        private bool _canMove;
        private Transform _target;

        private void Update()
        {
            if(Paddle is null || !_canMove || Paddle.Freezed || _target is null) return;
            
            float distance = _target.transform.position.y - transform.position.y;
            float speedFactor = Mathf.Abs(distance) < SLOW_DOWN_THRESHOLD ? Mathf.Abs(distance / SLOW_DOWN_THRESHOLD) : 1f;
            Paddle.Move(Mathf.Sign(distance) * speedFactor);
        }

        public void SetInputEnabled(bool inputEnabled)
        {
            _canMove = inputEnabled;
        }

        public void SetTarget(Transform target)
        {
            _target = target;
        }
    }
}