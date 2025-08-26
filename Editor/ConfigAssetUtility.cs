#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using System.IO;

namespace AHL.AnalyticsLite.Editor
{
    public static class ConfigAssetUtility
    {
        [MenuItem("Tools/Analytics Lite/Create Config (Resources)")]
        public static void CreateConfigInResources()
        {
            string resourcesDir = "Assets/Resources";
            if (!AssetDatabase.IsValidFolder(resourcesDir))
            {
                AssetDatabase.CreateFolder("Assets", "Resources");
            }

            string path = Path.Combine(resourcesDir, "AnalyticsLiteConfig.asset");
            var existing = AssetDatabase.LoadAssetAtPath<AnalyticsLiteConfig>(path);
            if (existing != null)
            {
                Selection.activeObject = existing;
                EditorUtility.DisplayDialog("Analytics Lite", "Config از قبل وجود دارد.", "باشه");
                return;
            }

            var cfg = ScriptableObject.CreateInstance<AnalyticsLiteConfig>();
            AssetDatabase.CreateAsset(cfg, path);
            AssetDatabase.SaveAssets();
            Selection.activeObject = cfg;
            EditorUtility.DisplayDialog("Analytics Lite", "Config ساخته شد: Resources/AnalyticsLiteConfig.asset", "باشه");
        }
    }
}
#endif
