using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using BotNet.Commands;

namespace BotNet.Bot {
	internal static class CommandTypeExtractor {
		public static string GetSenderType(ICommand command) {
			// Get all properties named "Sender" from the hierarchy
			// and select the most derived one to avoid AmbiguousMatchException
			PropertyInfo? senderProperty = command.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.Name == "Sender")
				.OrderByDescending(p => p.DeclaringType, Comparer<Type?>.Create((x, y) => {
					if (x == y) return 0;
					if (x == null) return -1;
					if (y == null) return 1;
					return x.IsSubclassOf(y) ? 1 : -1;
				}))
				.FirstOrDefault();
			
			if (senderProperty == null) return "Unknown";
			
			object? sender = senderProperty.GetValue(command);
			return sender?.GetType().Name ?? "Unknown";
		}

		public static string GetChatType(ICommand command) {
			// Get all properties named "Chat" from the hierarchy
			// and select the most derived one to avoid AmbiguousMatchException
			PropertyInfo? chatProperty = command.GetType()
				.GetProperties(BindingFlags.Public | BindingFlags.Instance)
				.Where(p => p.Name == "Chat")
				.OrderByDescending(p => p.DeclaringType, Comparer<Type?>.Create((x, y) => {
					if (x == y) return 0;
					if (x == null) return -1;
					if (y == null) return 1;
					return x.IsSubclassOf(y) ? 1 : -1;
				}))
				.FirstOrDefault();
			
			if (chatProperty == null) return "Unknown";
			
			object? chat = chatProperty.GetValue(command);
			return chat?.GetType().Name ?? "Unknown";
		}
	}
}
