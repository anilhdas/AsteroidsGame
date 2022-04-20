//#define DEBUG_VERBOSE

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using AsteroidGame.Combat;

namespace AsteroidGame
{
    [RequireComponent(typeof(Rigidbody2D))]
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerShip : MonoBehaviour, ILoopable, IThrustable, ISteerable
    {
        public Action playerCollided;
        public Action playerDead;

        public SteerState currentSteerState { get; set; }
        public ThrustState currentThrustState { get; set; }

        public Rigidbody2D body { get ; set ; }

        ITriggerable _primaryWeapon, _bonusWeapon;

        int _healthPoints;
        bool _isBonusWeaponActive;

        // Consts
        const float LINEAR_SPEED = 0.2f;
        const float ANGULAR_SPEED = 0.05f;
        
        
        #region Unity events

        void Awake()
        {
            body = GetComponent<Rigidbody2D>();
            GameManager.Instance.GameStateChanged += HandleGameStateChanged;
            enabled = false;
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
            // Update linear momentum
            if (ThrustState.None != currentThrustState)
                body.AddForce(transform.up * LINEAR_SPEED * (int)currentThrustState, ForceMode2D.Impulse);

            // Update angular momentum
            if (SteerState.None != currentSteerState)
                body.AddTorque(ANGULAR_SPEED * (int)currentSteerState, ForceMode2D.Impulse);
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            playerCollided?.Invoke();

            if (0 >= --_healthPoints)
            {
                playerDead?.Invoke();
                enabled = false;
            }

            GameManager.Instance.gameHUD.UpdateHealth(_healthPoints);
        }

        void OnTriggerEnter2D(Collider2D collider)
        {
            // Todo Check if collider is bonus weapon
            _isBonusWeaponActive = true;
            Invoke(nameof(ResetWeapon), GameConstants.BONUS_WEAPON_TIME_IN_SECONDS);
        }

        void OnDestroy()
        {
            GameManager.Instance.GameStateChanged -= HandleGameStateChanged;
        }

        #endregion // Unity Events

        public void Initialize(ITriggerable primaryWeapon, ITriggerable bonusWeapon)
        {
            _primaryWeapon = primaryWeapon;
            _bonusWeapon = bonusWeapon;
        }

        void HandleGameStateChanged(GameState state)
        {
            switch(state)
            {
                case GameState.GameStarted:
                    enabled = true;

                    _healthPoints = GameConstants.MAX_PLAYER_HEALTH;
                    currentSteerState = SteerState.None;
                    currentThrustState = ThrustState.None;

                    ResetWeapon();

                    break;
                case GameState.GameOver:
                    enabled = false;

                    transform.position = Vector2.zero;
                    transform.rotation = Quaternion.identity;
                    body.velocity = Vector2.zero;
                    body.angularVelocity = 0;
                    break;
            }
        }

        void FireWeapon()
        {
            if (_isBonusWeaponActive)
                _bonusWeapon.TriggerAmmo(transform.position, transform.rotation);
            else
                _primaryWeapon.TriggerAmmo(transform.position, transform.rotation);
        }

        void ResetWeapon()
        {
            _isBonusWeaponActive = false;
        }

        #region User Input

        public void OnFireInput(InputAction.CallbackContext context)
        {
            if (enabled)
            {
                if (context.performed)
                    FireWeapon();
            }
        }
        public void OnThrustInput(InputAction.CallbackContext context)
        {
            if (enabled)
            {
                if (context.started)
                {
                    ThrustState thrustState = context.ReadValue<float>() < 0 ? ThrustState.ThrustedBackward : ThrustState.ThrustedForward;
                    currentThrustState = thrustState;
                }
                else if (context.performed || context.canceled)
                    currentThrustState = ThrustState.None;
            }
        }

        public void OnSteerInput(InputAction.CallbackContext context)
        {
            if (enabled)
            {
                if (context.started)
                {
                    SteerState steerState = context.ReadValue<float>() < 0 ? SteerState.SteeredLeft : SteerState.SteeredRight;
                    currentSteerState = steerState;
                }
                else if (context.performed || context.canceled)
                    currentSteerState = SteerState.None;
            }
        }

        #endregion
    }
}