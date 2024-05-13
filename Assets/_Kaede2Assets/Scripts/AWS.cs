using System;
using System.Collections.Generic;

namespace Kaede2
{
    public static class AWS
    {
        public static string DefaultRegion => "ap-northeast-1"; // Asia Pacific (Tokyo)
        private static string MainAddressableBucket => "kaede2-addressables";
        private static string ScenarioOnlyAddressableBucket => "kaede2-addressables-scenario-only";
#if SCENARIO_ONLY
        public static string AddressableBucket => ScenarioOnlyAddressableBucket;
#else
        public static string AddressableBucket => MainAddressableBucket;
#endif
        public static string PublishBucket => "kaede2-publish";
        public static string TranslationBucket => "yuyuyui-scenario-translation";
        public static string EditorProfileName => "github_actions";
        private static Guid CognitoIdentityPoolGuid => new Guid(0x1b74aaab, 0x7cb6, 0x4073, 0x80, 0x8b, 0x90, 0x7b, 0x16, 0x40, 0x17, 0x8c);
        public static string CognitoIdentityPoolId => $"{DefaultRegion}:{CognitoIdentityPoolGuid}";
        // use dualstack accelerated endpoint
        public static string DefaultAddressableLoadUrl => GetUrl(AddressableBucket, null, DefaultRegion, true, true, true);

        private static Dictionary<string, string> CdnMap = new()
        {
            { MainAddressableBucket, "d3b1h60nzo7az4" },
            { ScenarioOnlyAddressableBucket, "d278luw8xyi1q" },
            { TranslationBucket, "d1igmkcvf9ttyi" },
            { PublishBucket, "d3w0m29iaipqyp" },
        };

        public static string GetUrl(string bucket, string key, string region, bool useTransferAcceleration = false, bool useDualstackEndpoint = false, bool useCdn = false)
        {
            string GetBaseUrl()
            {
                if (useCdn && CdnMap.TryGetValue(bucket, out var cdn))
                {
                    return $"https://{cdn}.cloudfront.net";
                }

                if (useTransferAcceleration && useDualstackEndpoint)
                {
                    return $"https://{bucket}.s3-accelerate.dualstack.amazonaws.com";
                }

                if (useTransferAcceleration)
                {
                    return $"https://{bucket}.s3-accelerate.amazonaws.com";
                }

                if (useDualstackEndpoint)
                {
                    return $"https://{bucket}.s3.dualstack.{region}.amazonaws.com";
                }

                return $"https://{bucket}.s3.{region}.amazonaws.com";
            }

            return string.IsNullOrEmpty(key) ? GetBaseUrl() : $"{GetBaseUrl()}/{key}";
        }
    }
}
