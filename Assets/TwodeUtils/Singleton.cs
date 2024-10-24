using UnityEngine;

namespace TwodeUtils
{
    public abstract class Singleton<T> : MonoBehaviour where T : MonoBehaviour
    {
        public static T Instance { get; private set; }

        protected virtual void Awake()
        {
            if(Instance is not null)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this as T;
        }
    }
}