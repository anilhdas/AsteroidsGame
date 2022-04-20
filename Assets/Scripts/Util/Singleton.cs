using UnityEngine;

namespace AsteroidGameUtils
{
    [DisallowMultipleComponent]
    public class Singleton<ScriptType> : MonoBehaviour where ScriptType : MonoBehaviour
    {
        public static ScriptType Instance { get; private set; }

        protected virtual void Awake()
        {
            if (null != Instance)
            {
                Debug.LogError($"Singleton {typeof(ScriptType).Name} already exist on GO {Instance.gameObject.name}. Destroying component on {this.gameObject}");
                Destroy(this);
                return;
            }

            Instance = this as ScriptType;
        }
    }
}