using AsteroidGameUtils;
using System;

namespace AsteroidGame.Combat
{
    public interface IFireable : IPoolable
    {
        public Action<IFireable> AmmoHit{ get; set; }

        float AmmoLife { get; }
        float AmmoSpeed { get; }

        void InitializeAmmo();

        void ResetAmmo();
    }
}