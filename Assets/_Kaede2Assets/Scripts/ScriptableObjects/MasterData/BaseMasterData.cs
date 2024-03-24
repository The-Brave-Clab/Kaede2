using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.ScriptableObjects
{
    public abstract class BaseMasterData<T> : ScriptableObject where T : BaseMasterData<T>
    {
        private static T instance = null;
        public static T Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = Resources.Load<T>($"master_data/{typeof(T).Name}");
                return instance;
            }
        }
    }
}