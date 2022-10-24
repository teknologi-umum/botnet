# BOTNET
Telegram Bot written in .NET

## Build and Run
1. Open `BotNet.sln` in Visual Studio
2. Right click `BotNet` project in Solution Explorer, select `Manage User Secrets`
3. In the opened `secrets.json`, add your bot token to following properties:

```json
{
  "BotOptions:AccessToken": "yourtoken",
  "GoogleMapOptions:ApiKey": "yourApiKey",
  "HostingOptions:UseLongPolling": true
}
```

4. Run the project by pressing F5