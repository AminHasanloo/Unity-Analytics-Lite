// Project Settings > Analytics Lite
#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

namespace AHL.AnalyticsLite.Editor
{
    public class AnalyticsLiteSettingsProvider : SettingsProvider
    {
        const string kPath = "Project/Analytics Lite";

        public AnalyticsLiteSettingsProvider(string path, SettingsScope scope = SettingsScope.Project) : base(path, scope) {}

        [SettingsProvider]
        public static SettingsProvider CreateProvider()
        {
            return new AnalyticsLiteSettingsProvider(kPath, SettingsScope.Project);
        }

        public override void OnGUI(string searchContext)
        {
            GUILayout.Label("Analytics Lite - تنظیمات", EditorStyles.boldLabel);
            var guids = AssetDatabase.FindAssets("t:AnalyticsLiteConfig");
            AnalyticsLiteConfig cfg = null;

            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                cfg = AssetDatabase.LoadAssetAtPath<AnalyticsLiteConfig>(path);
            }

            if (cfg == null)
            {
                EditorGUILayout.HelpBox("Config پیدا نشد. یک Asset بسازید.", MessageType.Warning);
                if (GUILayout.Button("ساخت Config در Resources"))
                {
                    ConfigAssetUtility.CreateConfigInResources();
                }
                return;
            }

            SerializedObject so = new SerializedObject(cfg);
            EditorGUILayout.PropertyField(so.FindProperty("apiBaseUrl"));
            EditorGUILayout.PropertyField(so.FindProperty("appKey"));
            EditorGUILayout.PropertyField(so.FindProperty("batchSize"));
            EditorGUILayout.PropertyField(so.FindProperty("flushIntervalSeconds"));
            EditorGUILayout.PropertyField(so.FindProperty("sendInEditor"));
            EditorGUILayout.PropertyField(so.FindProperty("optInRequired"));
            EditorGUILayout.PropertyField(so.FindProperty("verboseLogging"));
            EditorGUILayout.PropertyField(so.FindProperty("queueFileName"));
            EditorGUILayout.PropertyField(so.FindProperty("maxEventsPerFile"));

            so.ApplyModifiedProperties();

            EditorGUILayout.Space();
            if (GUILayout.Button("باز کردن پوشه persistentDataPath"))
            {
                EditorUtility.RevealInFinder(Application.persistentDataPath);
            }
        }
    }
}
#endif
