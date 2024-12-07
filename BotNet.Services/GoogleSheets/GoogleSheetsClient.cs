using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Google.Apis.Sheets.v4;
using Google.Apis.Sheets.v4.Data;

namespace BotNet.Services.GoogleSheets {
	public sealed class GoogleSheetsClient(
		SheetsService sheetsService
	) {
		public async Task<ImmutableList<T>> GetDataAsync<T>(string spreadsheetId, string range, string firstColumn, CancellationToken cancellationToken) {
			int firstColumnIndex = GetColumnIndex(firstColumn);

			// Fetch data
			SpreadsheetsResource.ValuesResource.GetRequest getRequest = sheetsService.Spreadsheets.Values.Get(
				spreadsheetId: spreadsheetId,
				range: range
			);
			ValueRange response = await getRequest.ExecuteAsync(cancellationToken);

			// Get type info
			ConstructorInfo constructor = typeof(T).GetConstructors().Single();
			PropertyInfo[] properties = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);

			// Map data
			ImmutableList<T>.Builder builder = ImmutableList.CreateBuilder<T>();
			foreach (IList<object> row in response.Values) {
				if (row.Count < properties.Length) continue;

				object?[] parameters = new object?[properties.Length];
				for (int i = 0; i < properties.Length; i++) {
					PropertyInfo property = properties[i];

					FromColumnAttribute fromColumn = property.GetCustomAttribute<FromColumnAttribute>()
						?? throw new InvalidProgramException("Property not decorated with [FromColumn]");

					int columnIndex = GetColumnIndex(fromColumn.Column) - firstColumnIndex;
					if (columnIndex >= row.Count) {
						parameters[i] = null;
						continue;
					}

					if (row[columnIndex] is not string value) {
						parameters[i] = null;
						continue;
					}

					if (property.PropertyType == typeof(string)) {
						parameters[i] = value;
					} else if (property.PropertyType == typeof(decimal)) {
						if (decimal.TryParse(value, out decimal decimalValue)) {
							parameters[i] = decimalValue;
						} else {
							parameters[i] = 0m;
						}
					} else if (property.PropertyType == typeof(decimal?)) {
						if (decimal.TryParse(value, out decimal decimalValue)) {
							parameters[i] = decimalValue;
						} else {
							parameters[i] = null;
						}
					} else if (property.PropertyType == typeof(int)) {
						if (int.TryParse(value, out int intValue)) {
							parameters[i] = intValue;
						} else {
							parameters[i] = 0;
						}
					} else if (property.PropertyType == typeof(int?)) {
						if (int.TryParse(value, out int intValue)) {
							parameters[i] = intValue;
						} else {
							parameters[i] = null;
						}
					} else if (property.PropertyType == typeof(double)) {
						if (double.TryParse(value, out double doubleValue)) {
							parameters[i] = doubleValue;
						} else {
							parameters[i] = 0.0;
						}
					} else if (property.PropertyType == typeof(double?)) {
						if (double.TryParse(value, out double doubleValue)) {
							parameters[i] = doubleValue;
						} else {
							parameters[i] = null;
						}
					} else {
						parameters[i] = Convert.ChangeType(value, property.PropertyType);
					}
				}

				builder.Add((T)constructor.Invoke(parameters));
			}

			return builder.ToImmutable();
		}

		private static int GetColumnIndex(string columnName) {
			int index = 0;
			foreach (char c in columnName) {
				index *= 26;
				index += (c - 'A' + 1);
			}
			return index - 1;
		}
	}
}
