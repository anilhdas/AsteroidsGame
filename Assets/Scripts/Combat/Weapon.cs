using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AsteroidGameUtils;

namespace AsteroidGame.Combat
{
    public class Weapon<TAmmo> : ITriggerable where TAmmo : MonoBehaviour, IFireable
    {
        static ObjectPool<TAmmo> _ammoPool;
        static Transform _ammoPoolParent;

        readonly Color _ammoColor;
        readonly int _ammoLayerID;

        public Weapon(GameObject ammoPrefab, Color ammoColor, int ammoLayerID, int preallocateCount, string parentObjName = "Ammo Pool")
        {
            if (null == _ammoPool)
            {
                _ammoPool = new ObjectPool<TAmmo>(ammoPrefab);
                _ammoPoolParent = _ammoPool.Preallocate(preallocateCount, parentObjName);
            }

            _ammoColor = ammoColor;
            _ammoLayerID = ammoLayerID;

            GameManager.Instance.GameStateChanged += HandleGameStateChanged;
        }

        ~Weapon()
        {
            GameManager.Instance.GameStateChanged -= HandleGameStateChanged;
        }

        public void TriggerAmmo(Vector3 position, Quaternion rotation)
        {
            TAmmo instance = _ammoPool.Acquire(position, rotation, out bool isNew);
            instance.transform.SetParent(_ammoPoolParent);

            instance.GetComponent<SpriteRenderer>().color = _ammoColor;
            instance.gameObject.layer = _ammoLayerID;

            instance.AmmoHit += HandleAmmoHit;

            // Todo Rework this
            instance.StartCoroutine(ReleaseAmmo(instance));
        }

        public IEnumerator ReleaseAmmo(TAmmo instance)
        {
            yield return new WaitForSeconds(instance.AmmoLife);

            // Ammo might have been released already during events (AmmoHit / GameOver)
            if (!instance.IsPooled)
            {
                ResetAmmoData(instance);
                _ammoPool.Release(instance);
            }
        }

        void ResetAmmoData(TAmmo instance)
        {
            instance.AmmoHit -= HandleAmmoHit;

            instance.GetComponent<SpriteRenderer>().color = GameConstants.DEFAULT_AMMO_COLOR;
            instance.gameObject.layer = GameConstants.NO_COLLISION_LAYER;
            instance.transform.position = Vector2.zero;
            instance.transform.rotation = Quaternion.identity;
        }

        void HandleAmmoHit(IFireable instance)
        {
            TAmmo ammoInstance = instance as TAmmo;

            if (ammoInstance.gameObject.layer == GameConstants.PLAYER_AMMO_LAYER)
                GameManager.Instance.gameHUD.UpdateScore();

            ResetAmmoData(ammoInstance);
            _ammoPool.Release(ammoInstance);
        }

        void HandleGameStateChanged(GameState state)
        {
            switch(state)
            {
                case GameState.GameStarted:
                    break;
                case GameState.GameOver:
                    _ammoPool.ReleaseAll(out Stack<TAmmo> releasedInstances);

                    Stack<TAmmo>.Enumerator stackEnumerator = releasedInstances.GetEnumerator();
                    while (stackEnumerator.MoveNext())
                        ResetAmmoData(stackEnumerator.Current);

                    releasedInstances.Clear();
                    break;
            }
        }
    }
}