// نمونه‌ی ساده استفاده – می‌توانید در یک صحنه‌ی تست قرار دهید
using System.Collections.Generic;
using UnityEngine;

namespace AHL.AnalyticsLite
{
    public class DemoAnalyticsStarter : MonoBehaviour
    {
        [Tooltip("اگر خالی باشد، از Resources/AnalyticsLiteConfig.asset استفاده می‌شود")]
        public AnalyticsLiteConfig config;

        void Start()
        {
            if (!Analytics.Initialized)
            {
                if (config != null) Analytics.Initialize(config);
                // اگر opt-in فعال است، برای تست:
                Analytics.SetConsent(true);
            }

            Analytics.SetUserId("player_1234");
            Analytics.SetSuperProperty("build", Application.version);
            Analytics.Log("game_start");

            // مثال با پارامترها
            Analytics.Log("level_start", new Dictionary<string, object> {
                {"level", 1},
                {"difficulty", "normal"}
            });
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                Analytics.Log("tap", new Dictionary<string, object> { { "button", "space" } });
            }
        }
    }
}
