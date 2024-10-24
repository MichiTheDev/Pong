using UnityEngine;
using UnityEngine.InputSystem;

namespace Twode.Pong
{
    public sealed class Paddle : PongObject
    {
        private float _direction;
        
        private void FixedUpdate()
        {
            if(Freezed)
            {
                rb.linearVelocityY = 0f;
                return;
            }
            
            rb.linearVelocityY = speed * _direction;
        }
        
        public void SetMovementInputActions(in InputAction inputAction)
        {
            if(inputAction is null)
            {
                Debug.LogWarning("For some reason the given inputAction is null :(");
                return;
            }
            
            inputAction.performed += MovementInput;   
            inputAction.canceled += MovementInput;   
        }

        public void EnableAI()
        {
            
        }
        
        private void MovementInput(InputAction.CallbackContext context)
        {
            _direction = context.ReadValue<float>();
        }
    }
}