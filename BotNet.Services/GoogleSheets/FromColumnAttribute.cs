using System;

namespace BotNet.Services.GoogleSheets {
	[AttributeUsage(validOn: AttributeTargets.Property, AllowMultiple = false)]
	public sealed class FromColumnAttribute : Attribute {
		public string Column { get; }

		public FromColumnAttribute(string column) {
			Column = column;
		}
	}
}
