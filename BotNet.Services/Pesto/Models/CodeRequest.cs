namespace BotNet.Services.Pesto.Models; 

public sealed record CodeRequest(Language Language,
                                 string Code,
                                 int CompileTimeout = 10_000,
                                 int RunTimeout = 10_000,
                                 int MemoryLimit = 200_000_000,
                                 string Version = "latest");
