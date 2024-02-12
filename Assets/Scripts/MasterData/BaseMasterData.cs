using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
// ReSharper disable IdentifierTypo InconsistentNaming

namespace Kaede2.MasterData
{
    [Serializable]
    public abstract class BaseMasterData
    {
        private static Dictionary<Type, BaseMasterData> _masterDataCache = new();

        public static IEnumerator Load<T>(Action<T> onComplete) where T : BaseMasterData
        {
            if (_masterDataCache.ContainsKey(typeof(T)))
            {
                onComplete?.Invoke((T)_masterDataCache[typeof(T)]);
                yield break;
            }

            var opHandle = Addressables.LoadAssetAsync<TextAsset>($"MasterData/{typeof(T).Name}.json");

            if (!opHandle.IsDone)
                yield return opHandle;

            if (opHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var text = opHandle.Result.text;
                var data = JsonUtility.FromJson<T>(text);
                _masterDataCache.Add(typeof(T), data);
                onComplete?.Invoke(data);
            }

            Addressables.Release(opHandle);
        }
    }

    public enum CharacterId
    {
        Unknown = 0,
        YuukiYuuna = 1,
        TougouMimori = 2,
        InubouzakiFuu = 3,
        InubouzakiItsuki = 4,
        MiyoshiKarin = 5,
        WashioSumi = 6,
        MinowaGin = 7,
        NogiSonoko_S = 8,
        NogiWakaba = 9,
        DoiTamako = 10,
        IyojimaAnzu = 11,
        KooriChikage = 12,
        TakashimaYuuna = 13,
        ShiratoriUtano = 14,
        NogiSonoko_M = 15,
        AkiharaSekka = 16,
        KohaguraNatsume = 17,
        UesatoHinata = 18,
        FujimoriMito = 19,
        AkamineYuuna = 20,
        KokudoAya = 21,
        KusunokiMebuki = 22,
        KagajouSuzume = 23,
        MirokuYumiko = 24,
        YamabushiShizuku = 25,
        YamabushiShizuku_I = 26,
        MirokuRenge = 27,
        KiryuShizuka = 28,
        // 29?
        AkiMasuzu = 30,
        HanamotoYoshika = 31,
        TenmaMisaki = 32,
        HokkedouHime = 33,
        FuyouYuuna = 34,
        YuzukiYuuna = 35,
    }
}