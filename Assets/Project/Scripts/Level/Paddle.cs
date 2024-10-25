using UnityEngine;
using UnityEngine.InputSystem;

namespace Twode.Pong
{
    public sealed class Paddle : PongObject
    {
        private InputAction _movementInputAction;

        public void SetMovementInputActions(in InputAction inputAction)
        {
            if(inputAction is null)
            {
                Debug.LogWarning("For some reason the given inputAction is null :(");
                return;
            }

            _movementInputAction = inputAction;
            _movementInputAction.performed += MovementInput;   
            _movementInputAction.canceled += MovementInput;   
        }

        public void EnableAI()
        {
            
        }

        public void SetInputEnabled(bool inputEnabled)
        {
            if(_movementInputAction is null) return;
            
            if(inputEnabled) _movementInputAction.Enable();
            else _movementInputAction.Disable();
        }
        
        private void MovementInput(InputAction.CallbackContext context)
        {
            Move(new Vector2(0f, context.ReadValue<float>())); 
        }
    }
}