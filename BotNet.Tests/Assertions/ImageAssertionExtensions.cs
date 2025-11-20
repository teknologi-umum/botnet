using System;
using System.Security.Cryptography;
using Shouldly;

namespace BotNet.Tests.Assertions {
	public static class ImageAssertionExtensions {
		/// <summary>
		/// Asserts that two byte arrays contain the same image data by comparing their SHA256 hashes.
		/// This is much faster than comparing every byte in the array.
		/// </summary>
		public static void ShouldContainSameImageAs(this byte[] actual, byte[] expected) {
			if (actual == null) {
				throw new ShouldAssertException("Actual image data should not be null");
			}

			if (expected == null) {
				throw new ShouldAssertException("Expected image data should not be null");
			}

			string actualHash = ComputeHash(actual);
			string expectedHash = ComputeHash(expected);

			if (actualHash != expectedHash) {
				throw new ShouldAssertException(
					$"Images should contain the same data\n" +
					$"Expected hash: {expectedHash}\n" +
					$"Actual hash:   {actualHash}\n" +
					$"Expected size: {expected.Length} bytes\n" +
					$"Actual size:   {actual.Length} bytes"
				);
			}
		}

		/// <summary>
		/// Asserts that two byte arrays contain different image data by comparing their SHA256 hashes.
		/// This is much faster than comparing every byte in the array.
		/// </summary>
		public static void ShouldNotContainSameImageAs(this byte[] actual, byte[] expected) {
			if (actual == null) {
				throw new ShouldAssertException("Actual image data should not be null");
			}

			if (expected == null) {
				throw new ShouldAssertException("Expected image data should not be null");
			}

			string actualHash = ComputeHash(actual);
			string expectedHash = ComputeHash(expected);

			if (actualHash == expectedHash) {
				throw new ShouldAssertException(
					$"Images should contain different data but hashes match\n" +
					$"Hash: {actualHash}\n" +
					$"Size: {actual.Length} bytes"
				);
			}
		}

		private static string ComputeHash(byte[] data) {
			byte[] hashBytes = SHA256.HashData(data);
			return Convert.ToHexString(hashBytes);
		}
	}
}
