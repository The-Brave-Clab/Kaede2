// if baseUrl is a variable and a string, buildUrl will be "<baseUrl>/Build"
// otherwise it will just be "Build"
// baseUrl is provided in the outer scope in production environments
// if baseUrl is not provided, it will be set to the current directory for easier local development
function buildUnityConfig(_baseUrl) {
  let baseUrl = typeof _baseUrl === "string" ? baseUrl : "";
  let buildUrl = baseUrl + (baseUrl.length > 0 ? "/" : "") + "Build";
  let streamingAssetsUrl = baseUrl + (baseUrl.length > 0 ? "/" : "") + "StreamingAssets";
  let loaderUrl = buildUrl + "/{{{ LOADER_FILENAME }}}";
  let unityConfig = {
    dataUrl: buildUrl + "/{{{ DATA_FILENAME }}}",
    frameworkUrl: buildUrl + "/{{{ FRAMEWORK_FILENAME }}}",
#if USE_WASM
    codeUrl: buildUrl + "/{{{ CODE_FILENAME }}}",
#endif
#if MEMORY_FILENAME
    memoryUrl: buildUrl + "/{{{ MEMORY_FILENAME }}}",
#endif
#if SYMBOLS_FILENAME
    symbolsUrl: buildUrl + "/{{{ SYMBOLS_FILENAME }}}",
#endif
    streamingAssetsUrl: streamingAssetsUrl,
    companyName: "{{{ COMPANY_NAME }}}",
    productName: "{{{ PRODUCT_NAME }}}",
    productVersion: "{{{ PRODUCT_VERSION }}}",
  };

  return { loaderUrl, unityConfig };
}
