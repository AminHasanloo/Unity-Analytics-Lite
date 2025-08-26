// Copyright (c) 2025 Amin Hasanloo

using UnityEngine;

namespace AHL.AnalyticsLite
{
    [CreateAssetMenu(fileName = "AnalyticsLiteConfig", menuName = "Analytics Lite/Config", order = 1)]
    public class AnalyticsLiteConfig : ScriptableObject
    {
        [Header("Collector")]
        [Tooltip("آدرس سرویس دریافت رویداد. مثال: https://your-collector.example.com/ingest")]
        public string apiBaseUrl = "https://your-collector.example.com/ingest";

        [Tooltip("کلید اپلیکیشن برای احراز هویت ساده سمت سرور")]
        public string appKey = "dev-app-key";

        [Header("Sending")]
        [Range(1, 1000)] public int batchSize = 20;
        [Range(1f, 120f)] public float flushIntervalSeconds = 10f;
        public bool sendInEditor = false;

        [Header("Privacy")]
        [Tooltip("اگر فعال باشد، قبل از ارسال باید صراحتاً رضایت کاربر گرفته شود")]
        public bool optInRequired = false;

        [Header("Diagnostics")]
        public bool verboseLogging = true;

        [Header("Storage")]
        [Tooltip("نام فایل صف داخل persistentDataPath")]
        public string queueFileName = "analytics-queue.jsonl";
        [Tooltip("حداکثر تعداد خطوط قبل از چرخش فایل (اختیاری)")]
        public int maxEventsPerFile = 10000;
    }
}
