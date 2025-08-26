# Unity Analytics Lite (Event Logger)

یک سیستم سبک ثبت رویداد برای بازی‌های یونیتی با ارسال دسته‌ای، صف فایل‌محور و تنظیمات ساده.  
**Developer:** Amin Hasanloo – MIT Licensed

## ویژگی‌ها
- API ساده: `Analytics.Log("event_name", params)`
- ارسال دسته‌ای (batch) در بازه‌ی زمانی قابل‌تنظیم
- ذخیره‌سازی آفلاین (JSONL) در `persistentDataPath`
- سشن، Device ID، نسخه اپ، پلتفرم و اینکه در یونیتی چه ورژنی هستید
- Super Properties (خصوصیات سراسری برای همه رویدادها)
- Settings در **Project Settings > Analytics Lite**
- Opt-in/Consent برای رعایت پرایوسی

## نصب

### روش ۱: UPM (پیشنهادی)
1. کل این پوشه را در ریشه پروژه در مسیر `Packages/com.aminhasanloo.analytics-lite` قرار دهید.
2. یونیتی را ریفرش کنید. پکیج در Package Manager شناسایی می‌شود.

### روش ۲: پوشه‌ی Assets
- محتویات `Runtime/` و `Editor/` را داخل `Assets/AnalyticsLite/` کپی کنید.

## راه‌اندازی سریع

1. از منو **Tools > Analytics Lite > Create Config (Resources)** یک تنظیمات بسازید  
   این فایل در `Assets/Resources/AnalyticsLiteConfig.asset` ایجاد می‌شود و پلاگین به صورت خودکار هنگام اجرا آن را لود می‌کند.
2. در **Project Settings > Analytics Lite** مقادیر زیر را تنظیم کنید:
   - **Collector URL** (مثال: `https://your-collector.example.com/ingest`)
   - **App Key**
   - **Batch Size**، **Flush Interval**
   - **Send In Editor** (در صورت نیاز)
   - **Opt-in Required** اگر می‌خواهید فقط با رضایت صریح ارسال شود.
3. (اختیاری) اسکریپت نمونه `DemoAnalyticsStarter.cs` را در یک صحنه قرار دهید یا از API زیر استفاده کنید.

## استفاده

```csharp
using AHL.AnalyticsLite;
using System.Collections.Generic;

// اگر Config در Resources است، AutoInit انجام می‌شود.
// در غیر این صورت می‌توانید دستی Initialize کنید:
// Analytics.Initialize(myConfigAsset);

Analytics.SetConsent(true);          // فقط اگر opt-in لازم است
Analytics.SetUserId("player_1234");  // اختیاری
Analytics.SetSuperProperty("build", Application.version);

// رویداد ساده
Analytics.Log("game_start");

// رویداد با پارامترها
Analytics.Log("level_start", new Dictionary<string, object> {
    {"level", 3},
    {"difficulty", "hard"}
});

// می‌توانید هر زمان بخواهید، Flush را صدا بزنید (غیرهمزمان)
Analytics.Flush();
```

### شکل Payload
هر ارسال شامل یک envelope با meta و لیستی از events است:
```json
{
  "meta": {
    "appKey": "YOUR_APP_KEY",
    "sessionId": "5f01c4f2d7e84d10b0b36a7c3b8b0e50",
    "userId": "player_1234",
    "deviceId": "XXXXXXXX",
    "platform": "Android",
    "appVersion": "1.0.0",
    "unity": "2022.3.12f1",
    "locale": "English"
  },
  "events": [
    {"name":"game_start","ts":1724635800000,"params":{}},
    {"name":"level_start","ts":1724635812345,"params":{"level":3,"difficulty":"hard"}}
  ]
}
```

> **توجه:** زمان (`ts`) برحسب **میلی‌ثانیه از یونیکس تایم** است.

## نکات اجرایی
- اگر `Opt-in Required` فعال باشد تا زمانی که `Analytics.SetConsent(true)` صدا نزنید، چیزی ارسال نمی‌شود (اما رویدادها در صف نوشته می‌شوند).
- در Editor، پیش‌فرض **ارسال نمی‌شود** مگر اینکه `Send In Editor` را فعال کنید.
- هنگام خروج از اپ تلاش به `Flush()` انجام می‌شود ولی تضمین ۱۰۰٪ برای تکمیل درخواست در همه پلتفرم‌ها وجود ندارد (محدودیت چرخه عمر اپ‌ها). پیشنهاد: در لحظات کم‌ریسک (مثلاً پایان مرحله) دستی `Flush()` را فراخوانی کنید.

## سرور گردآورنده (Collector)
یک نمونه‌ی بسیار ساده با Node.js/Express:

```js
// فقط برای توسعه/تست. به هیچ وجه در تولید بدون احراز هویت و نرخ‌دهی مناسب استفاده نکنید.
const express = require('express');
const app = express();
app.use(express.json({limit: '1mb'}));

app.post('/ingest', (req, res) => {
  const appKey = req.header('x-app-key');
  if (appKey !== process.env.APP_KEY) return res.status(401).send('bad app key');

  const payload = req.body; // { meta: {}, events: [] }
  console.log('Received batch:', JSON.stringify(payload));
  res.sendStatus(200);
});

app.listen(3000, () => console.log('Collector listening on :3000'));
```

## پرایوسی و رضایت
- اگر قوانین منطقه شما (مثلاً GDPR/CCPA) لازم می‌داند، `Opt-in Required` را روشن کنید و قبل از هر ارسال، از کاربر رضایت بگیرید.
- مقدار `SystemInfo.deviceUniqueIdentifier` بسته به پلتفرم معنای متفاوتی دارد؛ اگر نیاز دارید، می‌توانید آن را خاموش کنید یا شناسه‌ی خود را مدیریت کنید.
- داده‌ی شخصی ارسال نکنید مگر واقعاً ضروری باشد (مینیمم‌سازی داده).

## عیب‌یابی
- با فعال‌بودن `Verbose Logging` لاگ‌های جزئیات ارسال در Console دیده می‌شود.
- مسیر فایل صف: `Application.persistentDataPath/<queueFileName>`

## نسخه‌بندی
به CHANGELOG.md مراجعه کنید.

## لایسنس
MIT © 2025 Amin Hasanloo
