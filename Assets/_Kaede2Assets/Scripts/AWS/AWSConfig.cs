using System;
using Amazon;

namespace Kaede2.AWS
{
    public static class AWSConfig
    {
        public static RegionEndpoint DefaultRegion => RegionEndpoint.APNortheast1;
        public static string AddressableBucket => "kaede2-addressables";
        public static string PublishBucket => "kaede2-publish";
        public static string TranslationBucket => "yuyuyui-scenario-translation";
        public static string EditorProfileName => "github_actions";
        private static Guid CognitoIdentityPoolGuid => new Guid(0x1b74aaab, 0x7cb6, 0x4073, 0x80, 0x8b, 0x90, 0x7b, 0x16, 0x40, 0x17, 0x8c);
        public static string CognitoIdentityPoolId => $"{DefaultRegion.SystemName}:{CognitoIdentityPoolGuid}";
    }
}
