using System;
using System.Text.RegularExpressions;

namespace BotNet.Services.UrlCleaner {
  public partial class UrlCleaner {
    /// <summary>
    /// Cleans the specified URL by removing query parameters based on predefined rules.
    /// </summary>
    /// <param name="url">The URL to be cleaned.</param>
    /// <returns>A cleaned URI.</returns>
    public static Uri Clean(Uri url) {
      foreach (Rule rule in RuleData.Rules) {
        if (rule.Match.IsMatch(url.ToString())) {
          foreach (string r in rule.Rules) {
            url = new Uri(Regex.Replace(url.ToString(), $"[&?]({r})=?[^&]*", ""));
          }
        }
      }

      // Remove trailing '?' or '&' if present
      string cleanedUrl = url.ToString().TrimEnd('?', '&').TrimEnd('/');

      return new Uri(cleanedUrl);
    }
  }
}