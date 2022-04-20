using UnityEngine.InputSystem;

namespace AsteroidGame
{
    public enum SteerState
    {
        None = 0,
        SteeredLeft = 1,
        SteeredRight = -1
    }

    public interface ISteerable
    {
        SteerState currentSteerState { get; set; }

        public void OnSteerInput(InputAction.CallbackContext context);
    }
}
