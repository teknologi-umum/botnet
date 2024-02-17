using System;
using Microsoft.Data.Sqlite;

namespace BotNet.Services.Sqlite {
	public sealed class ScopedDatabase : IDisposable {
		private readonly SqliteConnection _connection;
		private bool _disposedValue;

		public ScopedDatabase() {
			_connection = new SqliteConnection("Data Source=:memory:");
			_connection.Open();
		}

		public int ExecuteNonQuery(string commandText) {
			using SqliteCommand command = _connection.CreateCommand();
			command.CommandText = commandText;
			return command.ExecuteNonQuery();
		}

		public int ExecuteNonQuery(string commandText, (string Name, object? Value)[] parameters) {
			using SqliteCommand command = _connection.CreateCommand();
			command.CommandText = commandText;
			foreach ((string name, object? value) in parameters) {
				command.Parameters.AddWithValue(name, value ?? DBNull.Value);
			}
			return command.ExecuteNonQuery();
		}

		public void ExecuteReader(string commandText, Action<SqliteDataReader> readAction) {
			using SqliteCommand command = _connection.CreateCommand();
			command.CommandText = commandText;
			using SqliteDataReader reader = command.ExecuteReader();
			readAction(reader);
		}

		public void ExecuteReader(string commandText, (string Name, object? Value)[] parameters, Action<SqliteDataReader> readAction) {
			using SqliteCommand command = _connection.CreateCommand();
			command.CommandText = commandText;
			foreach ((string name, object? value) in parameters) {
				command.Parameters.AddWithValue(name, value ?? DBNull.Value);
			}
			using SqliteDataReader reader = command.ExecuteReader();
			readAction(reader);
		}

		private void Dispose(bool disposing) {
			if (!_disposedValue) {
				if (disposing) {
					// dispose managed state (managed objects)
					_connection.Dispose();
				}

				_disposedValue = true;
			}
		}

		public void Dispose() {
			// Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}
	}
}
