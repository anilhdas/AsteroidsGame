using System;
using UnityEngine;

using AsteroidGameUtils;

namespace AsteroidGame.AI
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class Asteroid : MonoBehaviour, IPoolable, ILoopable
    {
        public static Action<Asteroid> AsteroidCollided;

        public Rigidbody2D body { get ; set ; }

        public AsteroidSize Size { get; set; }

        float _movementSpeed;
        CircleCollider2D _circleCollider2D;
        SpriteRenderer _spriteRenderer;

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            _spriteRenderer = GetComponent<SpriteRenderer>();
        }

        void OnEnable()
        {
            GameManager.Instance.RegisterLoopable(this);
        }

        void OnDisable()
        {
            GameManager.Instance.UnregisterLoopable(this);
        }

        public void Initialize(AsteroidSize size, Sprite sprite, float speed)
        {
            Size = size;

            _spriteRenderer.sprite = sprite;
            _movementSpeed = speed;
            body.AddRelativeForce(transform.up * _movementSpeed, ForceMode2D.Impulse);

            // Resize collider with respect to the sprite size
            if (null == _circleCollider2D)
                _circleCollider2D = gameObject.AddComponent<CircleCollider2D>();
        }

        void OnCollisionEnter2D()
        {
            AsteroidCollided?.Invoke(this);
        }

        #region Pooling

        public bool IsPooled { get; set; }
        
        public void OnPoolAcquire() { }

        public void OnPoolRelease()
        {
            _movementSpeed = 0f;
            body.velocity = Vector2.zero;
            _spriteRenderer.sprite = null;

            Destroy(_circleCollider2D);
            _circleCollider2D = null;
        }

        #endregion
    }
}