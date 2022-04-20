using UnityEngine;

namespace AsteroidGame.Combat
{
    public interface ITriggerable
    {
        void TriggerAmmo(Vector3 position, Quaternion rotation);
    }
}