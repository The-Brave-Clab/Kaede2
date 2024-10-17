using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Kaede2.Utils;
using UnityEngine;
using UnityEngine.Networking;

namespace Kaede2.Localization
{
    public static class LocalizeScript
    {
        public enum TranslationStatus
        {
            Found,
            NetworkError,
            NotFound,
            Loading
        }

        public static IEnumerator DownloadTranslation(string scenarioName, string language, Action<TranslationStatus, string> onDownloaded, bool headOnly = false)
        {
            string key = $"{language}/{scenarioName}/{scenarioName}.json";
            var url = AWS.GetUrl(AWS.TranslationBucket, key, AWS.DefaultRegion, true, true, true);
            Uri uri = new Uri(url);
            var request = headOnly ? UnityWebRequest.Head(uri) : UnityWebRequest.Get(uri);
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                onDownloaded?.Invoke(TranslationStatus.Found, headOnly ? "" : request.downloadHandler.text);
            }
            else
            {
                // headOnly is meant for checking if the translation exists, so we don't log errors
                if (!headOnly)
                    typeof(LocalizeScript).LogWarning($"Failed to download translation: {request.error}");
                onDownloaded?.Invoke(
                    request.result == UnityWebRequest.Result.ProtocolError
                        ? TranslationStatus.NotFound
                        : TranslationStatus.NetworkError,
                    null);
            }
        }

        public static string ApplyTranslation(string script, string translationJson)
        {
            TranslationJson translation = JsonUtility.FromJson<TranslationJson>(translationJson);

            var scriptText = script.Replace("\r\n", "\n").Replace("\n\r", "\n");
            var scriptLines = scriptText.Split("\n");
            var scriptName = translation.name;

            var translatedScript = $"// Translated language: {translation.language}\n";

            int captionIndex = 0;
            int messageIndex = 0;
            
            foreach (var line in scriptLines)
            {
                string newLine = line;
                var args = line.Split("\t");
                if (args[0] == "caption")
                {
                    var lineId = $"{scriptName}_c_{captionIndex:00}";
                    ++captionIndex;
                    var captionLine = translation.lines.FirstOrDefault(l => l.id == lineId);
                    if (captionLine != null)
                    {
                        var translatedCaption = captionLine.text[0];

                        var captionArg = args[1];
                        var captionArgSplit = captionArg.Split(':');
                        captionArgSplit[1] = translatedCaption;
                        args[1] = string.Join(':', captionArgSplit);
                        newLine = string.Join('\t', args);
                    }
                }
                else if (args[0] == "mes" || args[0] == "mes_auto")
                {
                    var voiceName = args[2];

                    var lineId = $"{scriptName}_m_{voiceName}_{messageIndex:000}";
                    ++messageIndex;
                    var messageLine = translation.lines.FirstOrDefault(l => l.id == lineId);
                    if (messageLine != null)
                    {
                        var translatedName = messageLine.text[0];
                        var translatedMessage = messageLine.text[1].Replace("\n", "\\n");

                        var nameArg = args[1];
                        var nameArgSplit = nameArg.Split(':');
                        var nameIndex = nameArgSplit.Length > 1 ? 1 : 0;
                        nameArgSplit[nameIndex] = translatedName;
                        args[1] = string.Join(':', nameArgSplit);
                        args[3] = translatedMessage;
                        newLine = string.Join('\t', args);
                    }
                }

                translatedScript += newLine + "\n";
            }
            
            return translatedScript;
        }

        [Serializable]
        private class TranslationJson
        {
            public string name;
            public string language;
            public List<Line> lines;

            [Serializable]
            public class Line
            {
                public string type;
                public string id;
                public int index;
                public List<string> text;
            }
        }
    }
}