using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BotNet.Services.SafeSearch.Models;
using FluentAssertions;
using Xunit;

namespace BotNet.Tests.Services.SafeSearch {
	public class TrieTests {
		[Fact]
		public void CanAddOneEntryAndTraverse() {
			Trie<object> trie = new();
			object apple = new();
			trie.Add("apple", apple);

			trie.Traverse(
				"apple".ToCharArray(),
				out TrieTraverseStatus status,
				out System.ReadOnlyMemory<char> keyprefix,
				out System.ReadOnlyMemory<char> matchingKey,
				out System.ReadOnlyMemory<char> remainingNodeKey,
				out System.ReadOnlyMemory<char> remainingKey,
				out TrieNode<object>? lastNode);

			status.Should().Be(TrieTraverseStatus.FullMatch);
			keyprefix.ToString().Should().Be("");
			matchingKey.ToString().Should().Be("apple");
			remainingNodeKey.ToString().Should().Be("");
			remainingKey.ToString().Should().Be("");
			lastNode.Should().NotBeNull();
			lastNode.Key.ToString().Should().Be("apple");

			trie.Traverse(
				"app".ToCharArray(),
				out status,
				out keyprefix,
				out matchingKey,
				out remainingNodeKey,
				out remainingKey,
				out lastNode);

			status.Should().Be(TrieTraverseStatus.PartialMatch);
			keyprefix.ToString().Should().Be("");
			matchingKey.ToString().Should().Be("app");
			remainingNodeKey.ToString().Should().Be("le");
			remainingKey.ToString().Should().Be("");
			lastNode.Should().NotBeNull();
			lastNode.Key.ToString().Should().Be("apple");

			trie.Traverse(
				"apples".ToCharArray(),
				out status,
				out keyprefix,
				out matchingKey,
				out remainingNodeKey,
				out remainingKey,
				out lastNode);

			status.Should().Be(TrieTraverseStatus.Extraneous);
			keyprefix.ToString().Should().Be("");
			matchingKey.ToString().Should().Be("apple");
			remainingNodeKey.ToString().Should().Be("");
			remainingKey.ToString().Should().Be("s");
			lastNode.Should().NotBeNull();
			lastNode.Key.ToString().Should().Be("apple");
		}

		[Fact]
		public void CanAddOneEntryAndLookup() {
			Trie<object> trie = new();
			object apple = new();
			trie.Add("apple", apple);

			object? value = trie.GetValue("apple");
			value.Should().Be(apple);

			new Action([ExcludeFromCodeCoverage] () => _ = trie.GetValue("app")).Should().Throw<KeyNotFoundException>();
			new Action([ExcludeFromCodeCoverage] () => _ = trie.GetValue("apples")).Should().Throw<KeyNotFoundException>();
			new Action([ExcludeFromCodeCoverage] () => _ = trie.GetValue("banana")).Should().Throw<KeyNotFoundException>();
			new Action([ExcludeFromCodeCoverage] () => _ = trie.GetValue("")).Should().Throw<KeyNotFoundException>();

			trie.TryGetValue("apple", out value).Should().BeTrue();
			value.Should().Be(apple);

			trie.TryGetValue("app", out value).Should().BeFalse();
			value.Should().BeNull();

			trie.TryGetValue("apples", out value).Should().BeFalse();
			value.Should().BeNull();

			trie.TryGetValue("banana", out value).Should().BeFalse();
			value.Should().BeNull();

			trie.TryGetValue("", out value).Should().BeFalse();
			value.Should().BeNull();

			trie.ContainsKey("apple").Should().BeTrue();
			trie.ContainsKey("app").Should().BeFalse();
			trie.ContainsKey("apples").Should().BeFalse();
			trie.ContainsKey("banana").Should().BeFalse();
			trie.ContainsKey("").Should().BeFalse();

			trie.ContainsKeyWhichIsTheBeginningOf("apple").Should().BeTrue();
			trie.ContainsKeyWhichIsTheBeginningOf("app").Should().BeFalse();
			trie.ContainsKeyWhichIsTheBeginningOf("apples").Should().BeTrue();
			trie.ContainsKeyWhichIsTheBeginningOf("banana").Should().BeFalse();
			trie.ContainsKeyWhichIsTheBeginningOf("").Should().BeFalse();
		}
	}
}
