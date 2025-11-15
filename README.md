# 🤖 BotNet

A feature-rich, production-grade Telegram bot built with .NET 10 — featuring AI conversations, code execution, image generation, weather forecasts, and more.

[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![License](https://img.shields.io/github/license/teknologi-umum/botnet)](LICENSE)

## ✨ Features

### 🧠 AI & Conversations
- **GPT-4 & Gemini Integration** - Chat with OpenAI GPT-4 or Google Gemini 2.0 Flash
- **Thread-Aware Conversations** - Maintains context across reply chains
- **Image Analysis** - GPT-4 Vision support for analyzing images
- **Smart Rate Limiting** - Prevents abuse while prioritizing VIP users

### 🎨 Creative Tools
- **AI Image Generation** - Create images with DALL-E 3 or Stability.ai SDXL
- **Meme Generator** - Generate custom memes
- **Color Cards** - Generate beautiful color palette cards

### 💻 Code Execution
Execute code in **20+ programming languages** directly in Telegram:
- C, C++, C#, F#, VB.NET, TypeScript, JavaScript
- Python, Ruby, PHP, Go, Rust, Java, Kotlin, Scala, Swift
- Clojure, Crystal, Dart, Elixir, Lua, Pascal, Julia
- CommonLisp, SQLite3

### 🌦️ Information Services
- **Weather Forecasts** - Current weather and forecasts
- **BMKG Updates** - Indonesian weather agency alerts
- **Google Maps** - Location search and maps
- **Primbon** - Indonesian horoscope and predictions

### 🔧 Available Commands

#### AI & Creative
- `/ask [question]` - Chat with AI (GPT-4 or Gemini), supports threaded conversations
- `/art [prompt]` - Generate AI images with DALL-E 3 or Stability.ai

#### Code & Data
- `/exec [code]` or `/python`, `/js`, `/cpp`, etc. - Execute code in 20+ languages

#### Information
- `/weather [location]` - Get current weather and forecasts
- `/map [query]` - Search locations on Google Maps
- `/bmkg` - Get Indonesian weather agency (BMKG) updates

#### Fun & Misc
- `/humor` - Get random programming jokes from r/ProgrammerHumor
- `/primbon [query]` - Indonesian horoscope and predictions
- `/khodam [name]` - Check your spiritual companion (Indonesian meme)

## 🏗️ Architecture

BotNet uses a sophisticated **Command-Handler-Queue** pattern built on MediatR:

```
Telegram Update → UpdateHandler → Command → Queue → Handler → Response
```

**Key Design Principles:**
- ✅ **Bounded Command Queue** - Prevents memory exhaustion (1000 capacity, drop oldest)
- ✅ **Rate Limiting** - Per-user, per-chat, and per-day limiters with automatic cleanup
- ✅ **Background Task Management** - Consistent exception handling and metrics
- ✅ **Prometheus Metrics** - Full observability for production monitoring
- ✅ **Memory Leak Prevention** - Comprehensive safeguards (see [MEMORY_LEAK_AUDIT.md](MEMORY_LEAK_AUDIT.md))

## 📊 Production-Ready

- **No Critical Memory Leaks** - Audited and verified (~500 KB - 4 MB overhead)
- **Comprehensive Metrics** - Command queue, rate limiters, caches, background tasks
- **Exception Handling** - Centralized error handling via `BackgroundTask.Run()`
- **Unit Tested** - 50+ tests with xUnit and Shouldly

## 🚀 Quick Start

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0)
- Telegram Bot Token ([create one via @BotFather](https://t.me/botfather))

### Build and Run
1. Clone the repository:
   ```bash
   git clone https://github.com/teknologi-umum/botnet.git
   cd botnet
   ```

2. Set up secrets:
   ```bash
   dotnet user-secrets init --project BotNet
   dotnet user-secrets set "BotOptions:AccessToken" "your-bot-token" --project BotNet
   dotnet user-secrets set "HostingOptions:UseLongPolling" "true" --project BotNet
   ```

3. Run the bot:
   ```bash
   dotnet run --project BotNet
   ```

### Docker Deployment
```bash
docker build -t botnet .
docker run -e BotOptions__AccessToken=your-token botnet
```

### Configuration

Required secrets (via User Secrets or environment variables):
```json
{
  "BotOptions:AccessToken": "your-telegram-bot-token",
  "HostingOptions:UseLongPolling": true,
  "OpenAiOptions:ApiKey": "your-openai-key",
  "GeminiOptions:ApiKey": "your-gemini-key",
  "StabilityOptions:ApiKey": "your-stability-key",
  "GoogleMapOptions:ApiKey": "your-google-maps-key",
  "WeatherOptions:ApiKey": "your-weather-api-key"
}
```

## 🛠️ Development

### Project Structure
```
BotNet/                   # ASP.NET Core host, webhooks, DI
BotNet.Commands/          # Command DTOs (immutable records)
BotNet.CommandHandlers/   # MediatR handlers (business logic)
BotNet.Services/          # Reusable services (AI, weather, etc.)
BotNet.Tests/            # xUnit tests
```

### Running Tests
```bash
dotnet test BotNet.Tests/BotNet.Tests.csproj
```

## 🤝 Contributing

We welcome contributions! Please:
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Follow the [coding conventions](.github/copilot-instructions.md#key-conventions):
   - ✅ Use explicit types (no `var`)
   - ✅ Use records for immutable DTOs
   - ✅ Constructor injection for dependencies
   - ✅ Sequential command processing via queue
4. Write tests for new features
5. Submit a pull request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

Built with ❤️ by [Teknologi Umum](https://github.com/teknologi-umum) community.

---

**Live Bot:** Try it at [@TeknumBot](https://t.me/teknumbot) (if deployed)