// Copyright (c) 2025 Amin Hasanloo

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace AHL.AnalyticsLite
{
    public static class Analytics
    {
        static bool _initialized;
        static AnalyticsLiteConfig _config;
        static FileEventQueue _queue;
        static HttpTransport _transport;
        static string _queuePath;
        static string _sessionId;
        static string _deviceId;
        static string _userId;
        static bool _hasConsent;
        static bool _sending;
        static Dictionary<string, object> _superProps = new Dictionary<string, object>();
        static Core _core;

        class Core : MonoBehaviour
        {
            public void Init()
            {
                DontDestroyOnLoad(gameObject);
                StartCoroutine(FlushLoop());
            }

            IEnumerator FlushLoop()
            {
                while (true)
                {
                    yield return new WaitForSeconds(_config.flushIntervalSeconds);
                    Analytics.Flush();
                }
            }

            void OnApplicationPause(bool paused)
            {
                if (paused) Analytics.Flush();
            }

            void OnApplicationQuit()
            {
                Analytics.Flush(); // ممکن است تمام نشود، به README مراجعه کنید
            }
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void AutoInit()
        {
            // اگر Asset در Resources وجود دارد، به صورت خودکار مقداردهی می‌شود
            var cfg = Resources.Load<AnalyticsLiteConfig>("AnalyticsLiteConfig");
            if (cfg != null && !_initialized) Initialize(cfg);
        }

        public static bool Initialized => _initialized;
        public static string SessionId => _sessionId;

        public static void Initialize(AnalyticsLiteConfig config)
        {
            if (_initialized) return;
            _config = config != null ? config : ScriptableObject.CreateInstance<AnalyticsLiteConfig>();
            _deviceId = SystemInfo.deviceUniqueIdentifier; // توجه به نکات پرایوسی در README
            _sessionId = Guid.NewGuid().ToString("N");
            _hasConsent = !_config.optInRequired; // اگر opt-in اجباری است، تا زمان SetConsent(true) ارسال نمی‌شود

            _queuePath = Path.Combine(Application.persistentDataPath, _config.queueFileName);
            _queue = new FileEventQueue(_queuePath, _config.maxEventsPerFile);
            _transport = new HttpTransport(_config.apiBaseUrl, _config.appKey, _config.verboseLogging);

            var go = new GameObject("AnalyticsLite(Core)") { hideFlags = HideFlags.HideAndDontSave };
            _core = go.AddComponent<Core>();
            _core.Init();

            if (_config.verboseLogging)
            {
                Debug.Log($"[AnalyticsLite] Initialized. persistentDataPath={Application.persistentDataPath}");
            }

            _initialized = true;
        }

        public static void SetConsent(bool consent)
        {
            _hasConsent = consent;
            if (_config.verboseLogging)
                Debug.Log($"[AnalyticsLite] Consent set to: {consent}");
        }

        public static void SetUserId(string userId)
        {
            _userId = userId;
            if (_config.verboseLogging)
                Debug.Log($"[AnalyticsLite] UserId set: {_userId}");
        }

        public static void SetSuperProperty(string key, object value)
        {
            if (string.IsNullOrEmpty(key)) return;
            _superProps[key] = value;
        }

        public static void ClearSuperProperties()
        {
            _superProps.Clear();
        }

        public static void Log(string eventName, IDictionary<string, object> parameters = null)
        {
            if (!_initialized)
            {
                Debug.LogWarning("[AnalyticsLite] Not initialized. Create a Config asset in Resources or call Initialize() manually.");
                return;
            }

            var p = new Dictionary<string, object>();
            // super props (کاربر مقدم است: params روی سوپرپراپرتی override می‌شود)
            foreach (var kv in _superProps)
                p[kv.Key] = kv.Value;

            if (parameters != null)
            {
                foreach (var kv in parameters)
                    p[kv.Key] = kv.Value;
            }

            var ev = new Dictionary<string, object>
            {
                {"name", eventName},
                {"ts", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()},
                {"params", p}
            };

            string line = SimpleJson.Serialize(ev);
            _queue.Enqueue(line);

            if (_config.verboseLogging)
                Debug.Log($"[AnalyticsLite] Enqueued event: {eventName}");
        }

        public static void Flush()
        {
            if (!_initialized) return;
            if (_sending) return;
            if (!_config.sendInEditor && Application.isEditor) return;
            if (_config.optInRequired && !_hasConsent) return;

            var batch = _queue.PeekBatch(_config.batchSize);
            if (batch == null || batch.Count == 0) return;

            _sending = true;

            // Envelope
            var meta = new Dictionary<string, object>
            {
                {"appKey", _config.appKey},
                {"sessionId", _sessionId},
                {"userId", string.IsNullOrEmpty(_userId) ? null : _userId},
                {"deviceId", _deviceId},
                {"platform", Application.platform.ToString()},
                {"appVersion", Application.version},
                {"unity", Application.unityVersion},
                {"locale", Application.systemLanguage.ToString()}
            };

            string metaJson = SimpleJson.Serialize(meta);
            string eventsArray = "[" + string.Join(",", batch) + "]";
            string payload = "{\"meta\":" + metaJson + ",\"events\":" + eventsArray + "}";

            _core.StartCoroutine(_transport.PostJson(payload, _sessionId, (ok, code, err) =>
            {
                if (ok)
                {
                    _queue.RemoveBatch(batch.Count);
                }
                else
                {
                    if (_config.verboseLogging)
                        Debug.LogWarning($"[AnalyticsLite] Send failed ({code}): {err}");
                }
                _sending = false;
            }));
        }
    }
}
