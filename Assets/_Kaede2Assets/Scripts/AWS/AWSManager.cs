using System;
using Amazon.CognitoIdentity;
using Amazon.S3;
using Amazon.S3.Model;
using Kaede2.Utils;
using UnityEngine.AddressableAssets;
using UnityEngine.Networking;

namespace Kaede2.AWS
{
    public static class AWSManager
    { 
        public static string DefaultAddressableLoadUrl => $"s3://{Config.AddressableBucket}";

        public static void Initialize()
        {
            InitializeAWS();
            typeof(AWSManager).Log("Initialized");

            Addressables.WebRequestOverride = EditWebRequestURL;
        }

        private static CognitoAWSCredentials credentials;
        private static AmazonS3Client s3Client;
        private static void InitializeAWS()
        {
            credentials = new CognitoAWSCredentials(Config.CognitoIdentityPoolId, Config.DefaultRegion);
            AmazonS3Config config = new AmazonS3Config
            {
                UseDualstackEndpoint = true,
                UseAccelerateEndpoint = true,
                RegionEndpoint = Config.DefaultRegion,
            };
            s3Client = new AmazonS3Client(credentials, config);
        }
        private static string GetPreSignedURL(string bucketName, string key, int expireInMinutes)
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