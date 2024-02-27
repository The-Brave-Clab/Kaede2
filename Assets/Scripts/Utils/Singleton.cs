using UnityEngine;

namespace Kaede2.Utils
{
    public class Singleton<T> : MonoBehaviour where T : Singleton<T>
    {
        private static T instance;

        public static T Instance
        {
            get
            {
                if (instance != null) return instance;

                GameObject go = new GameObject(typeof(T).Name);
                instance = go.AddComponent<T>();
                return instance;
            }
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
        }
    }
}