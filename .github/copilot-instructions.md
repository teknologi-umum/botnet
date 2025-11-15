# BotNet - Telegram Bot Architecture Guide

## Project Overview
BotNet is a .NET 10.0 Telegram bot with a modular, command-based architecture built on MediatR. The bot supports slash commands, AI interactions, code execution, image generation, and various integrations.

## Architecture Pattern: Command-Handler with Queue

### Command Flow
1. **Telegram Update → UpdateHandler** - Receives webhook/polling updates in `BotNet/Bot/UpdateHandler.cs`
2. **Update → Command** - Converts `Update` to `ICommand` (e.g., `SlashCommand`, `AskCommand`)
3. **Command → Queue** - Commands dispatched via `ICommandQueue` to decouple HTTP response from processing
4. **Queue → Handler** - `CommandConsumer` processes commands sequentially using MediatR
5. **Handler → Response** - `ICommandHandler<T>` implementations execute business logic

### Why the Queue Pattern?

**Critical**: The queue prevents infinite fan-out when handlers dispatch new commands.

Without the queue, handlers could directly call MediatR, creating recursive command chains:
- `MessageUpdate` handler dispatches `SlashCommand`
- `SlashCommand` handler dispatches `AskCommand`
- Each dispatch = immediate execution = nested async contexts
- **Risk**: Infinite loops, stack overflow, uncontrolled recursion

**The queue enforces**:
1. **Sequential Processing**: One command completes before the next starts
2. **Bounded Execution**: Handlers can't trigger cascading immediate execution
3. **Webhook Timeout Protection**: HTTP response returns immediately; processing happens async
4. **Single Point of Control**: All commands flow through `CommandConsumer` (see `BotNet/Bot/CommandConsumer.cs`)

**Pattern in practice**:
```csharp
// ❌ NEVER do this in a handler (infinite recursion risk):
await mediator.Send(new SomeCommand(...));

// ✅ ALWAYS dispatch to queue instead:
await commandQueue.DispatchAsync(SomeCommand.FromSlashCommand(command));
```

### Key Components
- **Commands** (`BotNet.Commands/`): Immutable records implementing `ICommand` (MediatR's `IRequest`)
- **Handlers** (`BotNet.CommandHandlers/`): Implement `ICommandHandler<TCommand>` via MediatR
- **Services** (`BotNet.Services/`): Business logic, external API clients, utilities
- **Queue**: In-memory unbounded channel for async processing (`CommandQueue.cs`)

## Adding New Bot Commands

### 1. Create Command Record
```csharp
// BotNet.Commands/YourFeature/YourCommand.cs
public sealed record YourCommand : ICommand {
    public string SomeData { get; }
    public SlashCommand Command { get; }
    
    private YourCommand(string someData, SlashCommand command) {
        SomeData = someData;
        Command = command;
    }
    
    public static YourCommand FromSlashCommand(SlashCommand command) {
        // Validation and parsing
        return new(someData: command.Text, command: command);
    }
}
```

### 2. Implement Handler
```csharp
// BotNet.CommandHandlers/YourFeature/YourCommandHandler.cs
public sealed class YourCommandHandler(
    ITelegramBotClient telegramBotClient
    // Inject required services
) : ICommandHandler<YourCommand> {
    public async Task Handle(YourCommand command, CancellationToken cancellationToken) {
        // Implementation
    }
}
```

### 3. Register in SlashCommandHandler
Add case in `BotNet.CommandHandlers/BotUpdate/Message/SlashCommandHandler.cs`:
```csharp
case "/yourcommand":
    await commandQueue.DispatchAsync(YourCommand.FromSlashCommand(command));
    break;
```

## Service Registration Pattern

Services use extension methods in `ServiceCollectionExtensions.cs`:
```csharp
public static IServiceCollection AddYourService(this IServiceCollection services) {
    services.AddTransient<YourService>();
    return services;
}
```

Register in `BotNet/Program.cs`:
```csharp
builder.Services.AddYourService();
```

## Configuration & Secrets

- **Development**: Use User Secrets (right-click project → Manage User Secrets)
- **Production**: Environment variables or file-based config (`/run/secrets/`)
- **Schema**: Options classes bound via `builder.Configuration.GetSection("YourOptions")`

Required secrets:
```json
{
  "BotOptions:AccessToken": "bot_token",
  "HostingOptions:UseLongPolling": true
}
```

## Project Structure

- **BotNet** - ASP.NET Core host, webhook endpoints, DI registration
- **BotNet.Commands** - Command DTOs (pure models, minimal dependencies)
- **BotNet.CommandHandlers** - MediatR request handlers (orchestration layer)
- **BotNet.Services** - Reusable services (HTTP clients, scrapers, renderers)
- **BotNet.Tests** - xUnit tests with FluentAssertions and Moq
- **pehape** - Multi-language PHP function ports (separate project)

## Testing

Run tests: `dotnet test BotNet.Tests/BotNet.Tests.csproj`

Use xUnit with:
- `Shouldly` for readable assertions
- `Moq` for mocking dependencies
- `[Theory]` + `[InlineData]` for parameterized tests

## Build & Run

**Local Development:**
```powershell
dotnet run --project BotNet/BotNet.csproj
```

**Docker:**
```powershell
docker build -t botnet .
docker run -p 80:80 --env-file .env botnet
```

**Deployment**: Fly.io via `fly.toml` configuration

## Rate Limiting & Privilege System

### Purpose
- **Prevent spam abuse** in public group chats
- **Reduce API costs** for Gemini and OpenAI token consumption
- **Prioritize privileged users** with better models and higher limits

### Rate Limiting
Rate limits are enforced in handlers before processing expensive operations:

```csharp
// BotNet.CommandHandlers/AI/RateLimit/AiRateLimiters.cs
internal static readonly RateLimiter GroupChatRateLimiter = 
    RateLimiter.PerUserPerChat(4, TimeSpan.FromMinutes(60));
```

**In handlers**:
```csharp
try {
    AiRateLimiters.GroupChatRateLimiter.ValidateActionRate(
        chatId: command.Chat.Id,
        userId: command.Sender.Id
    );
} catch (RateLimitExceededException exc) {
    await telegramBotClient.SendMessage(
        chatId: command.Chat.Id,
        text: $"<code>Coba lagi {exc.Cooldown}</code>",
        parseMode: ParseMode.Html
    );
    return;
}
```

### Privilege System
Configured via `CommandPrioritizationOptions` in `appsettings.json`:
```json
{
  "CommandPrioritizationOptions": {
    "HomeGroupChatIds": ["chat_id_1", "chat_id_2"],
    "VIPUserIds": ["user_id_1", "user_id_2"]
  }
}
```

**Sender Types**:
- `HumanSender` - Regular users (GPT-3.5, rate limited)
- `VipSender` - Privileged users (GPT-4, relaxed limits)

**Chat Types**:
- `PrivateChat` - 1-on-1 conversations
- `GroupChat` - Public groups (strict rate limits)
- `HomeGroupChat` - Trusted groups (GPT-4 access, relaxed limits)

**Model Selection Pattern**:
```csharp
model: command switch {
    ({ Sender: VipSender } or { Chat: HomeGroupChat }) => "gpt-4-1106-preview",
    _ => "gpt-3.5-turbo"
}
```

**Usage**: Pattern matching on `command.Sender` and `command.Chat` types determines privileges at runtime.

## Key Conventions

1. **Nullable Reference Types**: Enabled project-wide via `Directory.Build.props`
2. **Records over Classes**: Use `record` for immutable commands and DTOs
3. **Dependency Injection**: Constructor injection everywhere, avoid service locator
4. **Sequential Processing**: Commands execute one at a time (no concurrency in queue)
5. **Fire-and-Forget**: Long operations use `Task.Run()` to avoid webhook timeouts
6. **Rate Limiting**: Always check limits before expensive operations (AI, image gen, etc.)

## Common Patterns

### Working with Telegram API
- Always set `cancellationToken` on Telegram API calls
- Use `ParseMode.MarkdownV2` with `MarkdownV2Sanitizer.Sanitize()`
- Handle `UsageException` for command validation errors

### AI Integration Pattern
See `BotNet.CommandHandlers/AI/OpenAI/AskCommandHandler.cs`:
- Rate limit checks before processing
- Thread context from `ITelegramMessageCache`
- Model selection based on user privilege (`VipSender`, `HomeGroupChat`)
- Streaming responses for long generations

### Service Client Pattern
Each external API gets:
- Dedicated client class (e.g., `OpenAiClient`)
- Options class for configuration
- Extension method for DI registration
- Scoped/Transient lifetime based on state

## Debugging

- Development: Use VS/Rider debugger with `UseLongPolling: true`
- Production: Sentry integration via `SENTRY_DSN` environment variable
- Logs: `ILogger<T>` injected into services

## Resources

- Telegram Bot API: https://core.telegram.org/bots/api
- MediatR: https://github.com/jbogard/MediatR
- Project repo: teknologi-umum/botnet
