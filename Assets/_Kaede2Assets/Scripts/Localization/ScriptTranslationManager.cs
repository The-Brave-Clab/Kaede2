using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using Amazon.S3;
using Amazon.S3.Model;
using Kaede2.Scenario.Framework.Utils;
using Kaede2.Utils;
using UnityEngine;

namespace Kaede2.Localization
{
    public class ScriptTranslationManager
    {
        private static IAmazonS3 s3Client;

        private static Dictionary<CultureInfo, (bool succeeded, List<string> result)> translatedScenarios;

        public enum LoadStatus
        {
            Success,
            Warning,
            Failure,
            Loading
        }

        public static IEnumerator LoadTranslations()
        {
            translatedScenarios ??= new();
            s3Client ??= new AmazonS3Client();

            CoroutineGroup group = new();

            foreach (var cultureInfo in LocalizationManager.AllLocales)
            {
                if (cultureInfo.Name == "ja") continue;
                // if we already have the translations for this language, we skip it
                if (translatedScenarios.ContainsKey(cultureInfo) && translatedScenarios[cultureInfo].succeeded) continue;

                group.Add(ListTranslationsCoroutine(AWS.TranslationBucket, cultureInfo, list =>
                {
                    // in the scenario of multiple load requests are made, we need to check current results
                    // for succeeded loads, we update the list of translated scenarios
                    translatedScenarios[cultureInfo] = (true, ProcessKeys(list));
                    typeof(ScriptTranslationManager).Log($"Found {translatedScenarios[cultureInfo].result.Count} translations for {cultureInfo.Name}");
                }, () =>
                {
                    // for failed loads, we only mark the language as failed when it's not already marked
                    if (translatedScenarios.ContainsKey(cultureInfo) && translatedScenarios[cultureInfo].succeeded)
                        return;
                    typeof(ScriptTranslationManager).LogError($"Failed to load translations for {cultureInfo.Name}");
                    translatedScenarios[cultureInfo] = (false, null);
                }));
            }

            yield return group.WaitForAll();

            typeof(ScriptTranslationManager).Log("Translations loaded");
        }

        public static LoadStatus GetTranslationStatus(string scenarioName, CultureInfo language)
        {
            if (language.Name == LocalizationManager.AllLocales.First().Name)
                return LoadStatus.Success;

            if (translatedScenarios == null)
                return LoadStatus.Failure;

            if (!translatedScenarios.ContainsKey(language))
                return LoadStatus.Loading;

            if (!translatedScenarios[language].succeeded || translatedScenarios[language].result == null)
                return LoadStatus.Warning;

            if (!translatedScenarios[language].result.Contains(scenarioName))
                return LoadStatus.Failure;

            return LoadStatus.Success;
        }

        private static List<string> ProcessKeys(List<string> allKeys)
        {
            List<string> keys = new();
            if (allKeys == null) return keys;
            foreach (var key in allKeys)
            {
                string[] split = key.Split('/');
                if (split.Length != 3) continue;
                if (!string.Equals($"{split[1]}.json", split[2], StringComparison.CurrentCultureIgnoreCase)) continue;
                keys.Add(split[1]);
            }

            return keys;
        }

        // use this to convert a multi-threading task into a main thread coroutine
        // so that callbacks are executed on the main thread, preventing the need for using locks
        private static IEnumerator ListTranslationsCoroutine(string bucketName, CultureInfo language, Action<List<string>> onFinished, Action onError)
        {
            bool finished = false;
            bool error = false;
            List<string> result = new();

            Task.Run(async () =>
            {
                await ListTranslations(bucketName, language, list =>
                    {
                        result = list;
                        error = false;
                    },
                    () => error = true);
                finished = true;
            });

            while (!finished)
                yield return null;

            if (error)
                onError?.Invoke();
            else
                onFinished?.Invoke(result);
        }

        /// <summary>
        /// This method uses a paginator to retrieve the list of objects in an
        /// an Amazon S3 bucket.
        /// </summary>
        /// <param name="client">An Amazon S3 client object.</param>
        /// <param name="bucketName">The name of the S3 bucket whose objects
        /// you want to list.</param>
        public static async Task ListTranslations(string bucketName, CultureInfo language, Action<List<string>> onFinished, Action onError)
        {
            var listObjectsV2Paginator = s3Client.Paginators.ListObjectsV2(new ListObjectsV2Request
            {
                BucketName = AWS.TranslationBucket,
                Prefix = language.Name + "/"
            });

            List<string> keys = new();

            await foreach (var response in listObjectsV2Paginator.Responses)
            {
                if (response.HttpStatusCode != System.Net.HttpStatusCode.OK)
                {
                    onError?.Invoke();
                    return;
                }

                foreach (var entry in response.S3Objects)
                {
                    keys.Add(entry.Key);
                }
                onFinished?.Invoke(keys);
            }
        }
    }
}