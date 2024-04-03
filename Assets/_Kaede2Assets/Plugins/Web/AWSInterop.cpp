#include <emscripten.h>

extern "C"
{
    void InitializeAWS(const char* cognitoIdentityPoolId, const char* regionSystemName);
    const char* GetPreSignedURL(const char* bucketName, const char* key, int expireInMinutes);
}

EM_JS(void, InitializeAWS_JS, (const char* cognitoIdentityPoolId, const char* regionSystemName),
{
    const credentials = AWS.fromCognitoIdentityPool({
        client: new AWS.CognitoIdentityClient({ region: UTF8ToString(regionSystemName) }),
        identityPoolId: UTF8ToString(cognitoIdentityPoolId),
    });

    this.s3Client = new AWS.S3Client({
        region: UTF8ToString(regionSystemName),
        credentials: credentials,
        useAccelerateEndpoint: true,
        useDualstackEndpoint: true,
    });
});

EM_ASYNC_JS(const char*, GetPreSignedURL_JS, (const char* bucketName, const char* key, int expireInMinutes),
{
    if (!this.s3Client) {
        return null;
    }

    const command = new AWS.GetObjectCommand({
        Bucket: UTF8ToString(bucketName),
        Key: UTF8ToString(key),
    });

    const signedUrl = await AWS.getSignedUrl(s3Client, command, { expiresIn: expireInMinutes * 60 });

    var bufferSize = lengthBytesUTF8(signedUrl) + 1;
    var buffer = _malloc(bufferSize);
    stringToUTF8(signedUrl, buffer, bufferSize);
    return buffer;
});

EMSCRIPTEN_KEEPALIVE
void InitializeAWS(const char* cognitoIdentityPoolId, const char* regionSystemName)
{
    InitializeAWS_JS(cognitoIdentityPoolId, regionSystemName);
}

EMSCRIPTEN_KEEPALIVE
const char* GetPreSignedURL(const char* bucketName, const char* key, int expireInMinutes)
{
    return GetPreSignedURL_JS(bucketName, key, expireInMinutes);
}
