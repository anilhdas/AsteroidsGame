using UnityEngine;

namespace AsteroidGame
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class PowerUp : MonoBehaviour, ILoopable
    {
        public Rigidbody2D body { get ; set; }

        void Awake()
        {
            enabled = false;
            body = GetComponent<Rigidbody2D>();
        }

        void OnEnable()
        {
            GameManager.Instance.RegisterLoopable(this);
        }

        void OnDisable()
        {
            GameManager.Instance.UnregisterLoopable(this);
        }
    }
}
