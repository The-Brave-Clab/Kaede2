using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2
{
    public class Supporters : ScriptableObject
    {
        private static string URL => AWS.GetUrl(AWS.PublishBucket, "supporters.txt", AWS.DefaultRegion, true, true, true);
        public static string LocalPath => Path.Combine(Application.persistentDataPath, "supporters.txt");

        private static Supporters _instance = null;

        public static Supporters GetInstance()
        {
            if (_instance != null) return _instance;
            _instance = CreateInstance<Supporters>();

            if (File.Exists(LocalPath))
                LoadFromDisk(_instance);
            CoroutineProxy.Start(LoadFromOnline(_instance));

            return _instance;
        }

        [Serializable]
        public struct SupporterInfo
        {
            public string name;
            public int supportCount;
        }

        [SerializeField]
        private List<SupporterInfo> supporters;

        private Dictionary<int, List<string>> supportersPrioritized;
        public IReadOnlyDictionary<int, List<string>> SupportersPrioritized => supportersPrioritized;

        private void Parse(string text)
        {
            var lines = text.Split('\n');
            supporters = new List<SupporterInfo>();
            foreach (var line in lines)
            {
                var args = line.Split('\t');
                if (args.Length < 2) continue;
                if (!int.TryParse(args[1], out var supportCount)) continue;
                supporters.Add(new SupporterInfo
                {
                    name = args[0],
                    supportCount = supportCount
                });
            }
        }

        private void Prioritize()
        {
            supportersPrioritized = new Dictionary<int, List<string>>();
            foreach (var supporter in supporters)
            {
                if (!supportersPrioritized.TryGetValue(supporter.supportCount, out var list))
                {
                    list = new List<string>();
                    supportersPrioritized[supporter.supportCount] = list;
                }

                list.Add(supporter.name);
            }
        }

        private void SaveToDisk()
        {
            var text = string.Join("\n", supporters.ConvertAll(s => $"{s.name}\t{s.supportCount}"));
            File.WriteAllText(LocalPath, text);
        }

        private static void LoadFromDisk(Supporters instance)
        {
            var text = File.ReadAllText(LocalPath);
            instance.Parse(text);
            instance.Prioritize();
        }

        private static IEnumerator LoadFromOnline(Supporters instance)
        {
            var request = UnityEngine.Networking.UnityWebRequest.Get(URL);
            yield return request.SendWebRequest();
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                instance.Parse(request.downloadHandler.text);
                instance.Prioritize();
                instance.SaveToDisk();
            }
        }

        public static IEnumerator DownloadAndSave()
        {
            var request = UnityEngine.Networking.UnityWebRequest.Get(URL);
            yield return request.SendWebRequest();
            if (request.result == UnityEngine.Networking.UnityWebRequest.Result.Success)
            {
                File.WriteAllText(LocalPath, request.downloadHandler.text);
                typeof(Supporters).Log("Downloaded supporters");
            }
            else
            {
                typeof(Supporters).LogWarning($"Failed to download supporters: {request.error}");
            }
        }
    }
}