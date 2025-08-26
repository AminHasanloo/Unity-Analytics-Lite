// Copyright (c) 2025 Amin Hasanloo

using System;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

namespace AHL.AnalyticsLite
{
    internal class HttpTransport
    {
        readonly string _endpoint;
        readonly string _appKey;
        readonly bool _verbose;

        public HttpTransport(string endpoint, string appKey, bool verbose)
        {
            _endpoint = endpoint?.Trim();
            _appKey = appKey ?? "";
            _verbose = verbose;
        }

        public IEnumerator PostJson(string payload, string sessionId, Action<bool, long, string> onComplete)
        {
            if (string.IsNullOrEmpty(_endpoint))
            {
                onComplete?.Invoke(false, 0, "Empty endpoint");
                yield break;
            }

            byte[] body = Encoding.UTF8.GetBytes(payload);
            using (var req = new UnityWebRequest(_endpoint, UnityWebRequest.kHttpVerbPOST))
            {
                req.uploadHandler = new UploadHandlerRaw(body);
                req.downloadHandler = new DownloadHandlerBuffer();
                req.SetRequestHeader("Content-Type", "application/json; charset=utf-8");
                if (!string.IsNullOrEmpty(_appKey))
                    req.SetRequestHeader("X-App-Key", _appKey);
                if (!string.IsNullOrEmpty(sessionId))
                    req.SetRequestHeader("X-Session-Id", sessionId);
                req.SetRequestHeader("X-SDK", "analytics-lite-unity/0.1.0");

#if UNITY_2020_1_OR_NEWER
                yield return req.SendWebRequest();
                bool ok = req.result == UnityWebRequest.Result.Success && req.responseCode >= 200 && req.responseCode < 300;
#else
                yield return req.SendWebRequest();
                bool ok = !req.isNetworkError && !req.isHttpError && req.responseCode >= 200 && req.responseCode < 300;
#endif
                if (_verbose)
                {
                    Debug.Log($"[AnalyticsLite] POST {_endpoint} -> {(ok ? "OK" : "FAIL")} {req.responseCode} {req.error}\n{(ok ? "" : req.downloadHandler?.text)}");
                }
                onComplete?.Invoke(ok, req.responseCode, ok ? null : (req.error ?? req.downloadHandler?.text));
            }
        }
    }
}
