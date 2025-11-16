using BotNet.Bot;
using BotNet.Commands;
using BotNet.Commands.BotUpdate.Message;
using BotNet.Commands.ChatAggregate;
using BotNet.Commands.SenderAggregate;
using Shouldly;
using Xunit;

namespace BotNet.Tests.Bot {
	public class CommandConsumerReflectionTests {
		[Fact]
		public void GetSenderType_WithHumanSender_ReturnsHumanSenderType() {
			// Arrange
			TestHumanMessageCommand command = new(
				sender: new VipSender(Id: 123, Name: "Test User")
			);

			// Act
			string senderType = CommandTypeExtractor.GetSenderType(command);

			// Assert
			senderType.ShouldBe("VipSender");
		}

		[Fact]
		public void GetSenderType_WithRegularHumanSender_ReturnsHumanSenderType() {
			// Arrange
			TestHumanMessageCommand command = new(
				sender: new HumanSender(Id: 123, Name: "Test User")
			);

			// Act
			string senderType = CommandTypeExtractor.GetSenderType(command);

			// Assert
			senderType.ShouldBe("HumanSender");
		}

		[Fact]
		public void GetSenderType_WithBotSender_ReturnsBotSenderType() {
			// Arrange
			TestBotMessageCommand command = new(
				sender: new BotSender(Id: 456, Name: "Bot User")
			);

			// Act
			string senderType = CommandTypeExtractor.GetSenderType(command);

			// Assert
			senderType.ShouldBe("BotSender");
		}

		[Fact]
		public void GetSenderType_WithNoSenderProperty_ReturnsUnknown() {
			// Arrange
			TestCommandWithoutSender command = new();

			// Act
			string senderType = CommandTypeExtractor.GetSenderType(command);

			// Assert
			senderType.ShouldBe("Unknown");
		}

		[Fact]
		public void GetChatType_WithGroupChat_ReturnsGroupChatType() {
			// Arrange
			TestHumanMessageCommandWithGroupChat command = new(
				sender: new HumanSender(Id: 123, Name: "Test User")
			);

			// Act
			string chatType = CommandTypeExtractor.GetChatType(command);

			// Assert
			chatType.ShouldBe("GroupChat");
		}

		[Fact]
		public void GetChatType_WithHomeGroupChat_ReturnsHomeGroupChatType() {
			// Arrange  
			TestHumanMessageCommandWithHomeGroup command = new(
				sender: new HumanSender(Id: 123, Name: "Test User")
			);

			// Act
			string chatType = CommandTypeExtractor.GetChatType(command);

			// Assert
			chatType.ShouldBe("HomeGroupChat");
		}

		[Fact]
		public void GetChatType_WithPrivateChat_ReturnsPrivateChatType() {
			// Arrange
			TestHumanMessageCommand command = new(
				sender: new HumanSender(Id: 123, Name: "Test User")
			);

			// Act
			string chatType = CommandTypeExtractor.GetChatType(command);

			// Assert
			chatType.ShouldBe("PrivateChat");
		}

		[Fact]
		public void GetChatType_WithNoChatProperty_ReturnsUnknown() {
			// Arrange
			TestCommandWithoutChat command = new();

			// Act
			string chatType = CommandTypeExtractor.GetChatType(command);

			// Assert
			chatType.ShouldBe("Unknown");
		}

		// Test helper classes
		private sealed record TestHumanMessageCommand : HumanMessageBase, ICommand {
			public TestHumanMessageCommand(
				HumanSender sender,
				ChatBase? chat = null
			) : base(
				messageId: new MessageId(1),
				chat: chat ?? CreatePrivateChat(sender.Id),
				sender: sender,
				text: "test",
				imageFileId: null,
				replyToMessage: null
			) { }

			private static ChatBase CreatePrivateChat(SenderId senderId) {
				Telegram.Bot.Types.Chat telegramChat = new() {
					Id = senderId,
					Type = Telegram.Bot.Types.Enums.ChatType.Private
				};
				return PrivateChat.FromTelegramChat(telegramChat);
			}
		}

		private sealed record TestHumanMessageCommandWithHomeGroup : HumanMessageBase, ICommand {
			public TestHumanMessageCommandWithHomeGroup(
				HumanSender sender
			) : base(
				messageId: new MessageId(1),
				chat: CreateHomeGroupChat(),
				sender: sender,
				text: "test",
				imageFileId: null,
				replyToMessage: null
			) { }

			private static HomeGroupChat CreateHomeGroupChat() {
				Telegram.Bot.Types.Chat telegramChat = new() {
					Id = 789,
					Title = "Home Group",
					Type = Telegram.Bot.Types.Enums.ChatType.Supergroup
				};
				return HomeGroupChat.FromTelegramChat(telegramChat);
			}
		}

		private sealed record TestHumanMessageCommandWithGroupChat : HumanMessageBase, ICommand {
			public TestHumanMessageCommandWithGroupChat(
				HumanSender sender
			) : base(
				messageId: new MessageId(1),
				chat: CreateGroupChat(),
				sender: sender,
				text: "test",
				imageFileId: null,
				replyToMessage: null
			) { }

			private static GroupChat CreateGroupChat() {
				Telegram.Bot.Types.Chat telegramChat = new() {
					Id = 789,
					Title = "Test Group",
					Type = Telegram.Bot.Types.Enums.ChatType.Group
				};
				return GroupChat.FromTelegramChat(telegramChat);
			}
		}

		private sealed record TestBotMessageCommand : BotMessageBase, ICommand {
			public TestBotMessageCommand(
				BotSender sender
			) : base(
				messageId: new MessageId(1),
				chat: CreatePrivateChat(sender.Id),
				sender: sender,
				text: "test",
				imageFileId: null,
				replyToMessage: null
			) { }

			private static ChatBase CreatePrivateChat(SenderId senderId) {
				Telegram.Bot.Types.Chat telegramChat = new() {
					Id = senderId,
					Type = Telegram.Bot.Types.Enums.ChatType.Private
				};
				return PrivateChat.FromTelegramChat(telegramChat);
			}
		}

		private sealed record TestCommandWithoutSender : ICommand;

		private sealed record TestCommandWithoutChat : ICommand;
	}
}
