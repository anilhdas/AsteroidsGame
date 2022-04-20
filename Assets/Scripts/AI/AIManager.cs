using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;

using AsteroidGame.Combat;
using AsteroidGameUtils;

namespace AsteroidGame.AI
{
    public enum AsteroidSize
    {
        Small = 0,
        Large = 1
    }

    public class AIManager : Singleton<AIManager>
    {
        GameContent _gameContent;

        Transform _playerTransform;

        ObjectPool<Asteroid> _asteroidPool;
        Transform _asteroidPoolParent;

        AlienShip _alienShip;
        GameObject[] _paths;

        #region Unity Callbacks

        protected override void Awake()
        {
            base.Awake();

            _playerTransform = GameManager.Instance.player.transform;
            _gameContent = GameManager.Instance.gameContent;

            _asteroidPool = new ObjectPool<Asteroid>(_gameContent.genericAsteroidPrefab);
            _asteroidPoolParent = _asteroidPool.Preallocate(10, GameConstants.ASTEROID_POOL_PARENT_NAME);

            GameManager.Instance.GameStateChanged += HandleGameStateChanged;
            Asteroid.AsteroidCollided += HandleAsteroidCollided;

            SpawnAIPaths();
            SpawnAlien();

            enabled = false;
        }

        void OnDestroy()
        {
            GameManager.Instance.GameStateChanged -= HandleGameStateChanged;
            Asteroid.AsteroidCollided -= HandleAsteroidCollided;
            _alienShip.AlienDestroyed -= HandleAlienDestroyed;
        }

        #endregion // Unity Callbacks

        #region Event Handlers

        void HandleGameStateChanged(GameState state)
        {
            switch(state)
            {
                case GameState.GameStarted:
                    enabled = true;
                    RestartAlien();
                    SpawnAllAsteroids();
                    break;
                case GameState.GameOver:
                    enabled = false;
                    DisableAlien();
                    ReleaseAllAsteroids();
                    break;
            }
        }

        void HandleAsteroidCollided(Asteroid instance)
        {
            _asteroidPool.Release(instance);

            switch (instance.Size)
            {
                case AsteroidSize.Small:
                    // Spawn replacement asteroid occasionally to maintain the crowd
                    if (Time.frameCount % 3 == 0)
                        SpawnAsteroidRandom(AsteroidSize.Large);
                    break;
                case AsteroidSize.Large:
                    SpawnAsteroid(AsteroidSize.Small, instance.transform.position, GetRandomRotation(), GetRandomSpeed());
                    SpawnAsteroid(AsteroidSize.Small, instance.transform.position, GetRandomRotation(), GetRandomSpeed());
                    break;
            }
        }

        void HandleAlienDestroyed()
        {
            DisableAlien();
            Invoke(nameof(RestartAlien), GameConstants.AI_SPAWN_COOLDOWN);
        }

        #endregion

        void SpawnAllAsteroids()
        {
            int numberOfAsteroids = Random.Range(GameConstants.MIN_ASTEROIDS, GameConstants.MAX_ASTEROIDS);

            for (int i = 0; i < numberOfAsteroids; ++i)
            {
                SpawnAsteroidRandom((AsteroidSize)(1));
            }
        }

        void SpawnAIPaths()
        {
            GameObject[] pathPrefabs = _gameContent.aIPathPrefabs;

            _paths = new GameObject[pathPrefabs.Length];
            var pathParent = new GameObject(GameConstants.AI_PATH_PARENT_NAME).transform;

            for (int i = 0; i < _paths.Length; ++i)
            {
                _paths[i] = GameObject.Instantiate(pathPrefabs[i]);
                _paths[i].transform.SetParent(pathParent);
                _paths[i].SetActive(false);
            }
        }

        void SpawnAlien()
        {
            var alien = GameObject.Instantiate(_gameContent.alienShipPrefab, GameBounds.GetRandomPointInBounds(), Quaternion.identity);
            _alienShip = alien.GetComponent<AlienShip>();

            Weapon<Bullet> primaryWeapon = new Weapon<Bullet>(
                _gameContent.bulletPrefab,
                GameConstants.ALIEN_AMMO_COLOR,
                GameConstants.ALIEN_AMMO_LAYER,
                GameConstants.WEAPON_POOL_SIZE,
                GameConstants.BULLET_POOL_PARENT_NAME);

            _alienShip.Initialize(primaryWeapon);
            _alienShip.AlienDestroyed += HandleAlienDestroyed;

            DisableAlien();
        }

        void RestartAlien()
        {
            // Enabled check is unavoidable as the method is being invoked after cool-down
            if (enabled)
            {
                AssignRandomPath();

                Rigidbody2D alienBody = _alienShip.GetComponent<Rigidbody2D>();
                alienBody.MovePosition(GameBounds.GetRandomPointInBounds());

                _alienShip.gameObject.SetActive(true);
            }
        }

        void DisableAlien()
        {
            _alienShip.gameObject.SetActive(false);
        }

        void ReleaseAllAsteroids()
        {
            _asteroidPool.ReleaseAll(out Stack<Asteroid> releasedInstances);

            Stack<Asteroid>.Enumerator stackEnumerator = releasedInstances.GetEnumerator();
            while (stackEnumerator.MoveNext())
                ResetAsteroidData(stackEnumerator.Current);

            releasedInstances.Clear();
        }

        void ResetAsteroidData(Asteroid instance)
        {
            instance.transform.SetPositionAndRotation(Vector2.zero, Quaternion.identity);
        }

        public float GetAngleTowardsPlayer(Vector2 position)
        {
            Vector2 alienToPlayerDirecion = (Vector2)_playerTransform.position - position;

            float dotValue = Vector2.Dot(Vector2.up, alienToPlayerDirecion);
            float angleInRadians = Mathf.Acos(dotValue / (Vector2.up.magnitude * alienToPlayerDirecion.magnitude));

            return angleInRadians * Mathf.Rad2Deg * -1;
        }

        GameObject SpawnAsteroidRandom(AsteroidSize size)
        {
            Vector2 position = GameBounds.GetRandomPointInBounds();
            Quaternion rotation = GetRandomRotation();
            float speed = GetRandomSpeed();

            return SpawnAsteroid(size, position, rotation, speed);
        }

        GameObject SpawnAsteroid(AsteroidSize size, Vector2 position, Quaternion rotation, float speed)
        {
            // Spawn away from Player
            if (Vector2.Distance(position, _playerTransform.position) < GameConstants.ASTEROID_FREE_ZONE)
            {
                Vector2 direction = (position - (Vector2)_playerTransform.position).normalized;
                position = direction * GameConstants.ASTEROID_FREE_ZONE;
            }

            Asteroid instance = _asteroidPool.Acquire(position, rotation, out bool isNew);
            instance.transform.SetParent(_asteroidPoolParent);

            Sprite sprite = GetSprite(size);

            instance.Initialize(size, sprite, speed);

            return instance.gameObject;
        }

        void AssignRandomPath()
        {
            _alienShip.SetPath(GetRandomPath());
        }


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        GameObject GetRandomPath()
        {
            return _paths[Random.Range(0, _paths.Length)];
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Sprite GetSprite(AsteroidSize size)
        {
            Sprite sprite = null;

            switch (size)
            {
                case AsteroidSize.Small:
                    sprite = _gameContent.smallAsteroidSprites[Random.Range(0, _gameContent.smallAsteroidSprites.Length)];
                    break;
                case AsteroidSize.Large:
                    sprite = _gameContent.largeAsteroidSprites[Random.Range(0, _gameContent.largeAsteroidSprites.Length)];
                    break;
            }

            return sprite;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Quaternion GetRandomRotation()
        {
            return Quaternion.Euler(0f, 0f, Random.Range(0, 360));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        float GetRandomSpeed()
        {
            return Random.Range(0.2f, 2f);
        }
    }
}