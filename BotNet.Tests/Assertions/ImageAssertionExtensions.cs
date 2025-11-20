using System;
using Shouldly;

namespace BotNet.Tests.Assertions {
	public static class ImageAssertionExtensions {
		/// <summary>
		/// Asserts that two byte arrays contain the same image data by comparing their lengths and bytes.
		/// Short-circuits on the first difference found.
		/// </summary>
		public static void ShouldContainSameImageAs(this byte[] actual, byte[] expected) {
			if (actual == null) {
				throw new ShouldAssertException("Actual image data should not be null");
			}

			if (expected == null) {
				throw new ShouldAssertException("Expected image data should not be null");
			}

			// Short circuit if lengths differ
			if (actual.Length != expected.Length) {
				throw new ShouldAssertException(
					$"Images should contain the same data but lengths differ\n" +
					$"Expected length: {expected.Length} bytes\n" +
					$"Actual length:   {actual.Length} bytes"
				);
			}

			// Compare byte by byte, short circuit on first difference
			for (int i = 0; i < actual.Length; i++) {
				if (actual[i] != expected[i]) {
					throw new ShouldAssertException(
						$"Images should contain the same data but differ at index {i}\n" +
						$"Expected value: {expected[i]}\n" +
						$"Actual value:   {actual[i]}"
					);
				}
			}
		}

		/// <summary>
		/// Asserts that two byte arrays contain different image data by comparing their lengths and bytes.
		/// Short-circuits on the first difference found.
		/// </summary>
		public static void ShouldNotContainSameImageAs(this byte[] actual, byte[] expected) {
			if (actual == null) {
				throw new ShouldAssertException("Actual image data should not be null");
			}

			if (expected == null) {
				throw new ShouldAssertException("Expected image data should not be null");
			}

			// Short circuit if lengths differ - images are different
			if (actual.Length != expected.Length) {
				return; // Images are different, assertion passes
			}

			// Compare byte by byte, short circuit on first difference
			for (int i = 0; i < actual.Length; i++) {
				if (actual[i] != expected[i]) {
					return; // Images are different, assertion passes
				}
			}

			// If we got here, all bytes match - images are the same
			throw new ShouldAssertException(
				$"Images should contain different data but all bytes match\n" +
				$"Length: {actual.Length} bytes"
			);
		}
	}
}
