using System.Runtime.CompilerServices;
using UnityEngine;
using UnityEngine.UI;

using AsteroidGame.Combat;
using AsteroidGameUtils;

using Random = UnityEngine.Random;
using System.Collections.Generic;

namespace AsteroidGame
{
    public enum GameState
    {
        GameStarted,
        GameOver
    }

    public static class GameBounds
    {
        static Bounds _gameBounds;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void SetBounds(Bounds gameBounds)
        {
            _gameBounds = gameBounds;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 WithinBounds(Vector2 point)
        {
            if (_gameBounds.Contains(point))
                return point;

            Vector2 relativePos = point - (Vector2)_gameBounds.center;

            if (Mathf.Abs(relativePos.x) > _gameBounds.extents.x)
                relativePos.x *= -1;

            if (Mathf.Abs(relativePos.y) > _gameBounds.extents.y)
                relativePos.y *= -1;

            return relativePos + (Vector2)_gameBounds.center;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 GetRandomPointInBounds()
        {
            return new Vector2(
                Random.Range(_gameBounds.min.x, _gameBounds.max.x),
                Random.Range(_gameBounds.min.y, _gameBounds.max.y)
            );
        }
    }

    public class HUD
    {
        readonly Text _scoreText, _healthText;
        int _score, _health;

        public HUD(Text scoreText, Text healthText)
        {
            _scoreText = scoreText;
            _healthText = healthText;

            Reset();
        }

        public void Reset()
        {
            _score = 0;
            _health = GameConstants.MAX_PLAYER_HEALTH;

            UpdateHUD();
        }

        public void UpdateScore()
        {
            _score++;
            UpdateHUD();
        }

        public void UpdateHealth(int health)
        {
            _health = health;
            UpdateHUD();
        }

        void UpdateHUD()
        {
            _scoreText.text = _score.ToString();
            _healthText.text = _health.ToString();
        }
    }

    public class GameManager : Singleton<GameManager>
    {
        static readonly HashSet<ILoopable> _loopables = new HashSet<ILoopable>();

        public delegate void GameStateChange(GameState state);
        public GameStateChange GameStateChanged;

        public HUD gameHUD;

        public GameSettings gameSettings;
        public GameContent gameContent;
        public PlayerShip player;

        [SerializeField]
        Text _scoreText, _healthText;

        [SerializeField]
        GameObject menuPanel;

        protected override void Awake()
        {
            base.Awake();

            if (null == gameSettings)
                Debug.LogWarning($"{nameof(gameSettings)} field in {gameObject.name} hasn't been initialized");

            if (null == gameContent)
                Debug.LogWarning($"{nameof(gameContent)} field in {gameObject.name} hasn't been initialized");

            if (null == player)
                Debug.LogWarning($"{nameof(player)} field in {gameObject.name} hasn't been initialized");

            if (null == menuPanel)
                Debug.LogWarning($"{nameof(menuPanel)} field in {gameObject.name} hasn't been initialized");

            if (null == _scoreText)
                Debug.LogWarning($"{nameof(_scoreText)} field in {gameObject.name} hasn't been initialized");

            if (null == _healthText)
                Debug.LogWarning($"{nameof(_healthText)} field in {gameObject.name} hasn't been initialized");

            GameBounds.SetBounds(gameSettings.GameBounds);
            InitializePlayer();
            InitializeGameHUD();
        }

        void OnEnable()
        {
            player.playerDead += HandlePlayerDead;
        }

        void FixedUpdate()
        {
            UpdateLoopables();
        }

        void OnDisable()
        {
            player.playerDead -= HandlePlayerDead;
        }

        void UpdateLoopables()
        {
            HashSet<ILoopable>.Enumerator loopableEnumerator = _loopables.GetEnumerator();

            while (loopableEnumerator.MoveNext())
            {
                Rigidbody2D body = loopableEnumerator.Current.body;
                body.position = GameBounds.WithinBounds(body.position);
            }
        }

        public void RegisterLoopable(ILoopable instance)
        {
            _loopables.Add(instance);
        }

        public void UnregisterLoopable(ILoopable instance)
        {
            _loopables.Remove(instance);
        }

        public void UnregisterAllLoopables()
        {
            _loopables.Clear();
        }

        void HandlePlayerDead()
        {
            GameStateChanged?.Invoke(GameState.GameOver);
            menuPanel.SetActive(true);
            TimeUtil.SetTimeScale(0, true);
        }

        public void HandleGameStart()
        {
            TimeUtil.SetTimeScale(1, true);
            menuPanel.SetActive(false);
            gameHUD.Reset();

            GameStateChanged?.Invoke(GameState.GameStarted);
        }

        void InitializePlayer()
        {
            // Todo Use factory pattern
            var primaryWeapon = new Weapon<Bullet>(
                gameContent.bulletPrefab,
                GameConstants.PLAYER_AMMO_COLOR,
                GameConstants.PLAYER_AMMO_LAYER,
                GameConstants.WEAPON_POOL_SIZE,
                GameConstants.BULLET_POOL_PARENT_NAME);

            var bonusWeapon = new Weapon<Missile>(
                gameContent.missilePrefab,
                GameConstants.PLAYER_AMMO_COLOR,
                GameConstants.PLAYER_AMMO_LAYER,
                GameConstants.WEAPON_POOL_SIZE,
                GameConstants.MISSSILE_POOL_PARENT_NAME);

            player.Initialize(primaryWeapon, bonusWeapon);
        }

        void InitializeGameHUD()
        {
            gameHUD = new HUD(_scoreText, _healthText);
        }


    }
}
