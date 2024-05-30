using System;
using System.Collections.Generic;
using System.Globalization;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.Localization
{
    [Serializable]
    public abstract class LocalizedItemBase<T>
    {
        [SerializeField]
        protected SerializableDictionary<SerializableCultureInfo, T> localizedValues = new();

        public T Get(CultureInfo cultureInfo)
        {
            localizedValues ??= new();
            return localizedValues.GetValueOrDefault(cultureInfo);
        }

        public void Set(CultureInfo cultureInfo, T value)
        {
            localizedValues ??= new();
            localizedValues[cultureInfo] = value;
        }
    }

    [Serializable]
    public class LocalizedString : LocalizedItemBase<string>
    {
    }

    [Serializable]
    public class LocalizedAsset<T> : LocalizedItemBase<T> where T : UnityEngine.Object
    {
    }
}