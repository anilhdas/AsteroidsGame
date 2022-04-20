using System;
using UnityEngine;

namespace AsteroidGame.Combat
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class Missile : MonoBehaviour, IFireable, ILoopable
    {
        public float AmmoLife => GameConstants.MISSILE_LIFE_IN_SECONDS;
        public float AmmoSpeed => GameConstants.MISSILE_SPEED;
        public Action<IFireable> AmmoHit { get; set; }

        public Rigidbody2D body { get; set; }

        void Awake()
        {
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

        void FixedUpdate()
        {
            body.AddForce(transform.up * AmmoSpeed, ForceMode2D.Force);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            AmmoHit?.Invoke(this);
        }

        public void InitializeAmmo() 
        {
            body.AddForce(transform.up * AmmoSpeed, ForceMode2D.Impulse);
        }

        public void ResetAmmo()
        {
            body.velocity = Vector2.zero;
        }

        #region Pooling

        public bool IsPooled { get; set; }
        
        public void OnPoolAcquire()
        {
            InitializeAmmo();
        }

        public void OnPoolRelease()
        {
            ResetAmmo();
        }

        #endregion
    }
}