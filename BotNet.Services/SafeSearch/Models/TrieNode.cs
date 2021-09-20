using System;
using System.Collections.Generic;

namespace BotNet.Services.SafeSearch.Models {
	public class TrieNode<TValue> where TValue : class {
		public ReadOnlyMemory<char> Key;
		public TValue? Value;
		public IDictionary<ReadOnlyMemory<char>, TrieNode<TValue>> NextNodes;

		public static readonly KeyComparer KEY_COMPARER = new();

		public TrieNode(ReadOnlyMemory<char> key, TValue? value) {
			Key = key;
			Value = value;
			NextNodes = new Dictionary<ReadOnlyMemory<char>, TrieNode<TValue>>(KEY_COMPARER);
		}

		public TrieNode(ReadOnlyMemory<char> key, TValue? value, IDictionary<ReadOnlyMemory<char>, TrieNode<TValue>> nextNodes) {
			Key = key;
			Value = value;
			NextNodes = nextNodes;
		}

		public void Traverse(
			in ReadOnlyMemory<char> key,
			out TrieTraverseStatus status,
			out ReadOnlyMemory<char> matchingKey,
			out ReadOnlyMemory<char> remainingNodeKey,
			out ReadOnlyMemory<char> remainingKey,
			out TrieNode<TValue> currentNode) {
			MatchBeginning(in Key, in key, out matchingKey, out remainingNodeKey, out remainingKey);
			if (remainingNodeKey.Length == 0 && remainingKey.Length == 0) {
				status = TrieTraverseStatus.FullMatch;
				currentNode = this;
			} else if (remainingKey.Length == 0) {
				status = TrieTraverseStatus.PartialMatch;
				currentNode = this;
			} else if (remainingNodeKey.Length == 0) {
				if (NextNodes.TryGetValue(remainingKey.Slice(0, 1), out TrieNode<TValue>? nextNode)) {
					status = TrieTraverseStatus.Traversing;
					currentNode = nextNode;
				} else {
					status = TrieTraverseStatus.Extraneous;
					currentNode = this;
				}
			} else {
				status = TrieTraverseStatus.Mismatch;
				currentNode = this;
			}
		}

		private static void MatchBeginning(
			in ReadOnlyMemory<char> left,
			in ReadOnlyMemory<char> right,
			out ReadOnlyMemory<char> matching,
			out ReadOnlyMemory<char> remainingLeft,
			out ReadOnlyMemory<char> remainingRight) {
			int leftLen = left.Length;
			int rightLen = right.Length;
			int matchLen = 0;
			ReadOnlySpan<char> leftSpan = left.Span;
			ReadOnlySpan<char> rightSpan = right.Span;
			while (matchLen < leftLen && matchLen < rightLen) {
				if (leftSpan[matchLen] != rightSpan[matchLen]) {
					matching = left.Slice(0, matchLen);
					remainingLeft = left[matchLen..];
					remainingRight = right[matchLen..];
					return;
				} else {
					matchLen++;
				}
			}
			matching = left.Slice(0, matchLen);
			remainingLeft = left[matchLen..];
			remainingRight = right[matchLen..];
		}

		public class KeyComparer : IEqualityComparer<ReadOnlyMemory<char>> {
			public bool Equals(ReadOnlyMemory<char> x, ReadOnlyMemory<char> y) {
				if (x.Length != y.Length) {
					return false;
				} else {
					ReadOnlySpan<char> xSpan = x.Span;
					ReadOnlySpan<char> ySpan = y.Span;
					for (int i = x.Length - 1; i >= 0; i--) {
						if (xSpan[i] != ySpan[i]) return false;
					}
					return true;
				}
			}

			public int GetHashCode(ReadOnlyMemory<char> obj) {
				int hash = 585400441;
				ReadOnlySpan<char> span = obj.Span;
				for (int i = obj.Length - 1; i >= 0; i--) {
					hash = hash * -1521134295 + span[i].GetHashCode();
				}
				return hash;
			}
		}
	}
}
