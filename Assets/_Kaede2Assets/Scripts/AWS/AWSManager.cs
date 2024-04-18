using System;
#if !UNITY_WEBGL || UNITY_EDITOR
using Amazon;
using Amazon.CognitoIdentity;
using Amazon.S3;
using Amazon.S3.Model;
#else
using System.Runtime.InteropServices;
#endif
using Kaede2.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Kaede2.AWS
{
    public static class AWSManager
    { 
        // I was going to use s3:// but addressables won't treat it as remote path
        public static string DefaultAddressableLoadUrl => $"https://{Config.AddressableBucket}";

        public static void Initialize()
        {
            InitializeAWS(Config.CognitoIdentityPoolId, Config.DefaultRegion.SystemName);
            typeof(AWSManager).Log("Initialized");

            Addressables.WebRequestOverride = EditWebRequestURL;
        }

#if !UNITY_WEBGL || UNITY_EDITOR
        private static CognitoAWSCredentials credentials;
        private static AmazonS3Client s3Client;
        private static void InitializeAWS(string cognitoIdentityPoolId, string regionSystemName)
        {
            credentials = new CognitoAWSCredentials(cognitoIdentityPoolId, RegionEndpoint.GetBySystemName(regionSystemName));
            AmazonS3Config config = new AmazonS3Config
            {
                UseDualstackEndpoint = true,
                UseAccelerateEndpoint = true,
                RegionEndpoint = RegionEndpoint.GetBySystemName(regionSystemName),
            };
            s3Client = new AmazonS3Client(credentials, config);
        }
        public static string GetPreSignedURL(string bucketName, string key, int expireInMinutes)
        {
            var request = new GetPreSignedUrlRequest
            {
                BucketName = bucketName,
                Key = key,
                Expires = DateTime.Now.AddMinutes(expireInMinutes),
                Protocol = Protocol.HTTPS,
            };

            return s3Client.GetPreSignedURL(request);
        }
#else
        [DllImport("__Internal")]
        public static extern string GetPreSignedURL(string bucketName, string key, int expireInMinutes);

        [DllImport("__Internal")]
        private static extern void InitializeAWS(string cognitoIdentityPoolId, string regionSystemName);
#endif

        private static void EditWebRequestURL(UnityWebRequest request)
        {
            // we set the runtime load path to [Kaede2.AWS.AWSManager.DefaultAddressableLoadUrl]/[BuildTarget]
            // which will become request.url
            // so by removing $"{DefaultAddressableLoadUrl}/" we can get the object key
            if (!request.url.StartsWith(DefaultAddressableLoadUrl)) return;

            var key = request.url.Replace($"{DefaultAddressableLoadUrl}/", "");
            var preSignedUrl = GetPreSignedURL(Config.AddressableBucket, key, 120);

            request.url = preSignedUrl;
        }
    }
}