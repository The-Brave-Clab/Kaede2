using System;
using System.Collections.Generic;
using Amazon;

namespace Kaede2
{
    public static class AWS
    {
        public static RegionEndpoint DefaultRegion => RegionEndpoint.APNortheast1;
#if SCENARIO_ONLY
        public static string AddressableBucket => "kaede2-addressables-scenario-only";
#else
        public static string AddressableBucket => "kaede2-addressables";
#endif
        public static string PublishBucket => "kaede2-publish";
        public static string TranslationBucket => "yuyuyui-scenario-translation";
        public static string EditorProfileName => "github_actions";
        private static Guid CognitoIdentityPoolGuid => new Guid(0x1b74aaab, 0x7cb6, 0x4073, 0x80, 0x8b, 0x90, 0x7b, 0x16, 0x40, 0x17, 0x8c);
        public static string CognitoIdentityPoolId => $"{DefaultRegion.SystemName}:{CognitoIdentityPoolGuid}";
        // use dualstack accelerated endpoint
        public static string DefaultAddressableLoadUrl => GetUrl(AddressableBucket, null, DefaultRegion, true, true, true);

        private static Dictionary<string, string> CdnMap = new()
        {
            { "kaede2-addressables", "d3b1h60nzo7az4" },
            { "kaede2-addressables-scenario-only", "d278luw8xyi1q" },
            { "yuyuyui-scenario-translation", "d1igmkcvf9ttyi" },
        };

        public static string GetUrl(string bucket, string key, RegionEndpoint region, bool useTransferAcceleration = false, bool useDualstackEndpoint = false, bool useCdn = false)
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
                    return $"https://{bucket}.s3.dualstack.{region.SystemName}.amazonaws.com";
                }

                return $"https://{bucket}.s3.{region.SystemName}.amazonaws.com";
            }

            return string.IsNullOrEmpty(key) ? GetBaseUrl() : $"{GetBaseUrl()}/{key}";
        }
    }
}
