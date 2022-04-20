using System;
using System.Collections;
using UnityEngine;

using PathCreation;
using AsteroidGame.Combat;
using AsteroidGameUtils;

using Random = UnityEngine.Random;

namespace AsteroidGame.AI
{
    [RequireComponent(typeof(PathFollower))]
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class AlienShip : MonoBehaviour
    {
        public Action AlienDestroyed;

        [HideInInspector]
        Rigidbody2D _rigidbody2D;

        ITriggerable _primaryWeapon;

        GameObject _currentPath;
        PathFollower _follower;

        #region Unity Callbacks

        void Awake()
        {
            _rigidbody2D = GetComponent<Rigidbody2D>();
            _follower = GetComponent<PathFollower>();

            _rigidbody2D.freezeRotation = true;
        }

        void OnEnable()
        {
            StartCoroutine(FireWeapon());
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            AlienDestroyed?.Invoke();
            _rigidbody2D.velocity = Vector2.zero;
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }

        #endregion

        public void Initialize(ITriggerable primaryWeapon)
        {
            _primaryWeapon = primaryWeapon;
        }

        public void SetPath(GameObject path)
        {
            if (null != _currentPath)
                _currentPath.SetActive(false);

            _currentPath = path;
            _currentPath.SetActive(true);

            _follower.pathCreator = _currentPath.GetComponent<PathCreator>();
        }

        IEnumerator FireWeapon()
        {
            float coolDownTime = Random.Range(GameConstants.AI_FIRE_MIN_INTERVAL, GameConstants.AI_FIRE_MAX_INTERVAL);
            yield return TimeUtil.WaitForSeconds(coolDownTime);

            float zAngle = AIManager.Instance.GetAngleTowardsPlayer(transform.position);
            float inaccuracyDegrees = Random.Range(GameConstants.AI_MIN_INACCURACY_IN_DEGREES, GameConstants.AI_MAX_INACCURACY_IN_DEGREES);

            _primaryWeapon.TriggerAmmo(transform.position, Quaternion.Euler(0f, 0f, zAngle + inaccuracyDegrees));

            yield return FireWeapon();
        }
    }
}
