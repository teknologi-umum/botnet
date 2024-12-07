using System;

namespace BotNet.Services.GoogleSheets {
	[AttributeUsage(validOn: AttributeTargets.Property)]
	public sealed class FromColumnAttribute(
		string column
	) : Attribute {
		public string Column { get; } = column;
	}
}
