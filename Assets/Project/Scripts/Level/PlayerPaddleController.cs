using UnityEngine;
using UnityEngine.InputSystem;

namespace Twode.Pong
{
    public sealed class PlayerPaddleController : MonoBehaviour, IPaddleController
    {
        public Paddle Paddle { get; set; }
        
        private InputAction _movementInputAction;
        
        public void SetInputEnabled(bool inputEnabled)
        {
            if(_movementInputAction is null) return;
            
            if(inputEnabled) _movementInputAction.Enable();
            else _movementInputAction.Disable();
        }
        
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
        
        private void MovementInput(InputAction.CallbackContext context)
        {
            Paddle?.Move(context.ReadValue<float>());
        }
    }
}