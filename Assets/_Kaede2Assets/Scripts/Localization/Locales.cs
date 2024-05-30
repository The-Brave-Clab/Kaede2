using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using UnityEngine;

namespace Kaede2.Localization
{
    [Serializable]
    public class SerializableCultureInfo : ISerializationCallbackReceiver, IEquatable<CultureInfo>
    {
        private CultureInfo cultureInfo;

        [SerializeField]
        private string cultureName;

        public SerializableCultureInfo()
        {
            cultureInfo = CultureInfo.InvariantCulture;
            cultureName = cultureInfo.Name;
        }

        public void OnBeforeSerialize()
        {
            cultureInfo ??= CultureInfo.InvariantCulture;
            cultureName = cultureInfo.Name;
        }

        public void OnAfterDeserialize()
        {
            try
            {
                cultureInfo = new CultureInfo(cultureName);
            }
            catch (Exception)
            {
                cultureInfo = CultureInfo.InvariantCulture;
            }
        }

        public static implicit operator CultureInfo(SerializableCultureInfo serializableCultureInfo)
        {
            return serializableCultureInfo.cultureInfo;
        }

        public static implicit operator SerializableCultureInfo(CultureInfo cultureInfo)
        {
            return new SerializableCultureInfo { cultureInfo = cultureInfo };
        }

        protected bool Equals(SerializableCultureInfo other)
        {
            return Equals(cultureInfo, other.cultureInfo);
        }

        public bool Equals(CultureInfo other)
        {
            return Equals(cultureInfo, other);
        }

        public override bool Equals(object obj)
        {
            if (obj is SerializableCultureInfo serializableCultureInfo)
            {
                return Equals(serializableCultureInfo);
            }

            if (obj is CultureInfo info)
            {
                return Equals(info);
            }

            return false;
        }

        public override int GetHashCode()
        {
            return cultureInfo == null ? CultureInfo.InvariantCulture.GetHashCode() : cultureInfo.GetHashCode();
        }
    }

    [CreateAssetMenu(menuName = "Kaede2/Localization/Locales")]
    public class Locales : ScriptableObject
    {
        [SerializeField]
        private List<SerializableCultureInfo> supportedCultures;

        public IReadOnlyList<CultureInfo> All => supportedCultures.Select(x => (CultureInfo)x).ToList();

        public void Add(CultureInfo cultureInfo)
        {
            supportedCultures.Add(cultureInfo);
        }

        public void Remove(CultureInfo cultureInfo)
        {
            supportedCultures.Remove(cultureInfo);
        }

#if UNITY_EDITOR
        private static Locales loadedInstance;
        public static Locales Load()
        {
            if (loadedInstance != null) return loadedInstance;

            string[] guids = UnityEditor.AssetDatabase.FindAssets($"t:{nameof(Locales)}");
            if (guids.Length == 0) return null;

            string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
            loadedInstance = UnityEditor.AssetDatabase.LoadAssetAtPath<Locales>(path);
            return loadedInstance;
        }
#endif
    }
}