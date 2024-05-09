using System;
using Amazon;

namespace Kaede2
{
    public static class AWS
    {
        public static RegionEndpoint DefaultRegion => RegionEndpoint.APNortheast1;
        public static string AddressableBucket => "kaede2-addressables";
        public static string PublishBucket => "kaede2-publish";
        public static string TranslationBucket => "yuyuyui-scenario-translation";
        public static string EditorProfileName => "github_actions";
        private static Guid CognitoIdentityPoolGuid => new Guid(0x1b74aaab, 0x7cb6, 0x4073, 0x80, 0x8b, 0x90, 0x7b, 0x16, 0x40, 0x17, 0x8c);
        public static string CognitoIdentityPoolId => $"{DefaultRegion.SystemName}:{CognitoIdentityPoolGuid}";
        // use dualstack accelerated endpoint
        public static string DefaultAddressableLoadUrl => GetUrl(AddressableBucket, null, DefaultRegion, true, true);

        public static string GetUrl(string bucket, string key, RegionEndpoint region, bool useTransferAcceleration = false, bool useDualstackEndpoint = false)
        {
            string baseUrl;
            if (useTransferAcceleration && useDualstackEndpoint)
            {
                baseUrl = $"https://{bucket}.s3-accelerate.dualstack.amazonaws.com";
            }
            else if (useTransferAcceleration)
            {
                baseUrl = $"https://{bucket}.s3-accelerate.amazonaws.com";
            }
            else if (useDualstackEndpoint)
            {
                baseUrl = $"https://{bucket}.s3.dualstack.{region.SystemName}.amazonaws.com";
            }
            else
            {
                baseUrl = $"https://{bucket}.s3.{region.SystemName}.amazonaws.com";
            }

            return string.IsNullOrEmpty(key) ? baseUrl : $"{baseUrl}/{key}";
        }
    }
}
