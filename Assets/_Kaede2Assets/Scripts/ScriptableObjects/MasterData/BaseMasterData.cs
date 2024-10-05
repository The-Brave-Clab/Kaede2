using System.Collections.Generic;
using UnityEngine;

namespace Kaede2.ScriptableObjects
{
    public abstract class BaseMasterData<TSelf, TData> : ScriptableObject
        where TSelf : BaseMasterData<TSelf, TData>
        where TData : class, new()
    {
        private static TSelf instance = null;
        public static TSelf Instance
        {
            get
            {
                if (instance != null) return instance;
                instance = Resources.Load<TSelf>($"master_data/{typeof(TSelf).Name}");
                return instance;
            }
        }

        public abstract TData[] Data { get; }

        public interface IProvider
        {
            IEnumerable<TData> Provide();
        }
    }
}