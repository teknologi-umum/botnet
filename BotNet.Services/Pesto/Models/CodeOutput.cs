using System.Text.Json.Serialization;

namespace BotNet.Services.Pesto.Models;

public record CodeOutput(
	[property: JsonPropertyName("stdout")] string Stdout,
	[property: JsonPropertyName("stderr")] string Stderr,
	[property: JsonPropertyName("output")] string Output,
	[property: JsonPropertyName("exitCode")] int ExitCode
);
