using System.Collections.Generic;
using UnityEngine;

namespace AsteroidGameUtils
{
    public interface IPoolable
    {
        bool IsPooled { get; set; }

        void OnPoolAcquire();
        void OnPoolRelease();
    }

    public class ObjectPool<TComponent>
        where TComponent : MonoBehaviour, IPoolable
    {
        readonly GameObject _prefab;
        readonly Stack<TComponent> _pool = new Stack<TComponent>();
        readonly HashSet<TComponent> _acquiredObjects = new HashSet<TComponent>();

        public ObjectPool(GameObject prefab)
        {
            _prefab = prefab;
        }

        /// <summary>
        /// Pre-allocate pool to reduce runtime instantiations
        /// </summary>
        public Transform Preallocate(int count, string parentObjName = "Object Pool")
        {
            Transform parent = new GameObject(parentObjName).transform;

            for (int i = 0; i < count; i++)
            {
                TComponent instance = ForceInstantiate(Vector3.zero, Quaternion.identity);
                instance.transform.SetParent(parent);
                Release(instance);
            }

            return parent;
        }

        /// <summary>
        /// Pulls an object from the pool if available, or instantiates a new one otherwise.
        /// </summary>
        public TComponent Acquire(Vector3 position, Quaternion rotation, out bool isNew)
        {
            TComponent instance = Acquire_Internal(position, rotation, out isNew);
            _acquiredObjects.Add(instance);

            instance.IsPooled = false;
            instance.OnPoolAcquire();

            return instance;
        }

        TComponent Acquire_Internal(Vector3 position, Quaternion rotation, out bool isNew)
        {
            if (null != _pool && _pool.Count > 0)
            {
                TComponent instance = _pool.Pop();
                instance.transform.SetPositionAndRotation(position, rotation);
                instance.gameObject.SetActive(true);
                isNew = false;

                return instance;
            }

            isNew = true;
            return ForceInstantiate(position, rotation);
        }

        /// <summary>
        /// Force instantiates a new object and returns it
        /// </summary>
        public TComponent ForceInstantiate(Vector3 position, Quaternion rotation)
        {
            return GameObject.Instantiate(_prefab, position, rotation).GetComponent<TComponent>();
        }

        /// <summary>
        /// Returns the object to the pool and notifies it.
        /// </summary>
        public void Release(TComponent instance)
        {
            instance.IsPooled = true;
            instance.gameObject.SetActive(false);
            _acquiredObjects.Remove(instance);
            _pool.Push(instance);
            instance.OnPoolRelease();
        }

        public void ReleaseAll(out Stack<TComponent> releasedInstances)
        {
            releasedInstances = new Stack<TComponent>(_acquiredObjects.Count);

            HashSet<TComponent>.Enumerator acquiredObjectsEnumerator = _acquiredObjects.GetEnumerator();

            while (acquiredObjectsEnumerator.MoveNext())
            {
                TComponent instance = acquiredObjectsEnumerator.Current;

                instance.IsPooled = true;
                instance.gameObject.SetActive(false);
                releasedInstances.Push(instance);
                _pool.Push(instance);
                instance.OnPoolRelease();
            }

            _acquiredObjects.Clear();
        }

        /// <summary>
        /// Clears all object in the pool and destroys them.
        /// </summary>
        public void Clear()
        {
            Stack<TComponent>.Enumerator poolEnumerator = _pool.GetEnumerator();
            while(poolEnumerator.MoveNext())
            {
                GameObject.Destroy(poolEnumerator.Current.gameObject);
            }

            _pool.Clear();
        }
    }
}
