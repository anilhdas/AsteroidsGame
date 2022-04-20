using UnityEngine.InputSystem;

namespace AsteroidGame
{
    public enum ThrustState
    {
        None = 0,
        ThrustedForward = 1,
        ThrustedBackward = -1
    }

    public interface IThrustable
    {
        ThrustState currentThrustState { get; set; }
        void OnThrustInput(InputAction.CallbackContext context);
    }
}
