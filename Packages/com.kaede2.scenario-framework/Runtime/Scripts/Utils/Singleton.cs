using UnityEngine;

namespace Kaede2.Scenario.Framework.Utils
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static bool shuttingDown = false;
        private static T instance;

        public static T Instance
        {
            get
            {
                if (shuttingDown) return null;
                if (instance != null) return instance;

                GameObject go = new GameObject(typeof(T).Name);
                instance = go.AddComponent<T>();
                return instance;
            }
        }

        public static T EnsureInstance()
        {
            return Instance;
        }

        protected virtual void Awake()
        {
            if (instance == null)
            {
                instance = (T) this;
            }
            else if (instance != this)
            {
                Destroy(gameObject);
            }

            shuttingDown = false;
        }

        protected virtual void OnEnable()
        {
            shuttingDown = false;
        }

        protected virtual void OnApplicationQuit()
        {
            shuttingDown = true;
        }
    }
}