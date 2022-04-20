using UnityEngine;

namespace AsteroidGame
{
    public interface ILoopable
    {
        Rigidbody2D body{ get; set; }
    }
}
