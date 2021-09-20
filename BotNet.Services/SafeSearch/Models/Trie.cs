using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace BotNet.Services.SafeSearch.Models {
	public class Trie<TValue> where TValue : class {
		public TrieNode<TValue> RootNode { get; private set; } = new TrieNode<TValue>(string.Empty.ToCharArray(), null);

		public void Traverse(
			ReadOnlyMemory<char> key,
			out TrieTraverseStatus status,
			out ReadOnlyMemory<char> keyPrefix,
			out ReadOnlyMemory<char> matchingKey,
			out ReadOnlyMemory<char> remainingNodeKey,
			out ReadOnlyMemory<char> remainingKey,
			out TrieNode<TValue> lastNode) {
			TrieNode<TValue> currentNode = RootNode;
			string prefix = string.Empty;
			do {
				currentNode.Traverse(key, out status, out matchingKey, out remainingNodeKey, out key, out currentNode);
				if (status == TrieTraverseStatus.Traversing) {
					prefix += new string(matchingKey.ToArray());
				}
			} while (status == TrieTraverseStatus.Traversing);
			keyPrefix = prefix.ToCharArray();
			remainingKey = key;
			lastNode = currentNode;
		}

		public void Add(string key, TValue value) {
			Traverse(
				key.Trim().ToLowerInvariant().ToCharArray(),
				out TrieTraverseStatus status,
				out _,
				out ReadOnlyMemory<char> matchingKey,
				out ReadOnlyMemory<char> remainingNodeKey,
				out ReadOnlyMemory<char> remainingKey,
				out TrieNode<TValue> lastNode);
			switch (status) {
				case TrieTraverseStatus.FullMatch:
					break;
				case TrieTraverseStatus.PartialMatch: {
						TrieNode<TValue> newNode = new(
							key: remainingNodeKey,
							value: lastNode.Value,
							nextNodes: lastNode.NextNodes
						);
						lastNode.Key = matchingKey;
						lastNode.NextNodes = new Dictionary<ReadOnlyMemory<char>, TrieNode<TValue>>(TrieNode<TValue>.KEY_COMPARER) { { remainingNodeKey.Slice(0, 1), newNode } };
						lastNode.Value = value;
					}
					break;
				case TrieTraverseStatus.Extraneous: {
						TrieNode<TValue> newNode = new(
							key: remainingKey,
							value: value
						);
						lastNode.NextNodes.Add(remainingKey.Slice(0, 1), newNode);
					}
					break;
				case TrieTraverseStatus.Mismatch: {
						TrieNode<TValue> newNode1 = new(
							key: remainingNodeKey,
							value: lastNode.Value,
							nextNodes: lastNode.NextNodes
						);
						TrieNode<TValue> newNode2 = new(
							key: remainingKey,
							value: value
						);
						lastNode.Key = matchingKey;
						lastNode.NextNodes = new Dictionary<ReadOnlyMemory<char>, TrieNode<TValue>>(TrieNode<TValue>.KEY_COMPARER) {
							{ remainingNodeKey.Slice(0, 1), newNode1 },
							{ remainingKey.Slice(0, 1), newNode2 }
						};
						lastNode.Value = null;
					}
					break;
				default:
				case TrieTraverseStatus.Traversing:
					throw new InvalidOperationException("Trie traversal state is invalid.");
			}
		}

		public void Clear() {
			RootNode.NextNodes.Clear();
		}

		public TValue GetValue(string key) {
			Traverse(
				key.Trim().ToLowerInvariant().ToCharArray(),
				out TrieTraverseStatus status,
				out _,
				out _,
				out _,
				out _,
				out TrieNode<TValue> lastNode);
			if (status == TrieTraverseStatus.FullMatch) {
				return lastNode.Value ?? throw new KeyNotFoundException($"Entry with key '{key}' was not found in the trie.");
			} else {
				throw new KeyNotFoundException($"Entry with key '{key}' was not found in the trie.");
			}
		}

		public bool TryGetValue(string key, [NotNullWhen(true)] out TValue? value) {
			Traverse(
				key.Trim().ToLowerInvariant().ToCharArray(),
				out TrieTraverseStatus status,
				out _,
				out _,
				out _,
				out _,
				out TrieNode<TValue> lastNode);
			if (status == TrieTraverseStatus.FullMatch && lastNode.Value is not null) {
				value = lastNode.Value;
				return true;
			} else {
				value = null;
				return false;
			}
		}

		public bool ContainsKey(string key) {
			Traverse(
				key.Trim().ToLowerInvariant().ToCharArray(),
				out TrieTraverseStatus status,
				out _,
				out _,
				out _,
				out _,
				out TrieNode<TValue> lastNode);
			return status == TrieTraverseStatus.FullMatch && lastNode.Value is not null;
		}

		public bool ContainsKeyWhichIsTheBeginningOf(string s) {
			Traverse(
				s.Trim().ToLowerInvariant().ToCharArray(),
				out TrieTraverseStatus status,
				out _,
				out _,
				out _,
				out _,
				out TrieNode<TValue> lastNode);
			return status is TrieTraverseStatus.FullMatch or TrieTraverseStatus.Extraneous && lastNode.Value is not null;
		}

		public ISet<TValue> FindValuesBeginningWith(string key) {
			Traverse(
				key.Trim().ToLowerInvariant().ToCharArray(),
				out TrieTraverseStatus status,
				out _,
				out _,
				out _,
				out _,
				out TrieNode<TValue> lastNode);
			HashSet<TValue> values = new();
			if (status is TrieTraverseStatus.FullMatch or TrieTraverseStatus.PartialMatch) {
				PopulateTrailingValues(ref values, lastNode);
			}
			return values;
		}

		private static void PopulateTrailingValues(ref HashSet<TValue> values, TrieNode<TValue> startNode) {
			if (startNode.Value != null) {
				values.Add(startNode.Value);
			}
			foreach (TrieNode<TValue> nextNode in startNode.NextNodes.Values) {
				PopulateTrailingValues(ref values, nextNode);
			}
		}
	}
}
