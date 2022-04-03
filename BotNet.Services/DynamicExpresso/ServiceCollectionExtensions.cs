using System.Linq;
using DynamicExpresso;
using Microsoft.Extensions.DependencyInjection;

namespace BotNet.Services.DynamicExpresso {
	public static class ServiceCollectionExtensions {
		public static IServiceCollection AddCSharpEvaluator(this IServiceCollection services) {
			services.AddSingleton(new Interpreter(options: InterpreterOptions.Default | InterpreterOptions.LambdaExpressions)
				.Reference(
					from type in new[] {
						typeof(System.Array),
						typeof(System.ArraySegment<>),
						typeof(System.BitConverter),
						typeof(System.Buffer),
						typeof(System.Convert),
						typeof(System.Converter<,>),
						typeof(System.DateOnly),
						typeof(System.DateTimeKind),
						typeof(System.DayOfWeek),
						typeof(System.DateTimeOffset),
						typeof(System.Exception),
						typeof(System.Half),
						typeof(System.HashCode),
						typeof(System.Index),
						typeof(System.IntPtr),
						typeof(System.MathF),
						typeof(System.Memory<>),
						typeof(System.Nullable),
						typeof(System.Nullable<>),
						typeof(System.OperatingSystem),
						typeof(System.PlatformID),
						typeof(System.Random),
						typeof(System.Range),
						typeof(System.ReadOnlyMemory<>),
						typeof(System.ReadOnlySpan<>),
						typeof(System.SequencePosition),
						typeof(System.Span<>),
						typeof(System.StringComparer),
						typeof(System.StringComparison),
						typeof(System.StringSplitOptions),
						typeof(System.TimeOnly),
						typeof(System.TimeZoneInfo),
						typeof(System.Tuple),
						typeof(System.Tuple<>),
						typeof(System.TypeCode),
						typeof(System.Uri),
						typeof(System.UriBuilder),
						typeof(System.UriComponents),
						typeof(System.UriCreationOptions),
						typeof(System.UriFormat),
						typeof(System.UriHostNameType),
						typeof(System.UriKind),
						typeof(System.UriParser),
						typeof(System.UriPartial),
						typeof(System.UriTypeConverter),
						typeof(System.ValueTuple),
						typeof(System.ValueTuple<>),
						typeof(System.ValueType),
						typeof(System.Version),
						typeof(System.Collections.Generic.Dictionary<,>),
						typeof(System.Collections.Generic.HashSet<>),
						typeof(System.Collections.Generic.List<>),
						typeof(System.Collections.Generic.PriorityQueue<,>),
						typeof(System.Collections.Generic.Queue<>),
						typeof(System.Collections.Generic.SortedDictionary<,>),
						typeof(System.Collections.Generic.SortedList<,>),
						typeof(System.Collections.Generic.SortedSet<>),
						typeof(System.Collections.Generic.Stack<>),
						typeof(System.Text.Encodings.Web.HtmlEncoder),
						typeof(System.Text.Encodings.Web.JavaScriptEncoder),
						typeof(System.Text.Encodings.Web.TextEncoder),
						typeof(System.Text.Encodings.Web.TextEncoderSettings),
						typeof(System.Text.Encodings.Web.UrlEncoder),
						typeof(System.Text.Json.JsonSerializer),
						typeof(System.Text.Json.JsonSerializerOptions),
						typeof(System.Text.RegularExpressions.Regex),
						typeof(System.Text.RegularExpressions.RegexOptions),
					}
					select new ReferenceType(type)
				)
			);
			services.AddTransient<CSharpEvaluator>();
			return services;
		}
	}
}
