using System;
using System.Collections.Specialized;
using System.Linq;
using System.Net.Http;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BotNet.Services.Preview {
	public class YoutubePreview : IDisposable {
		private readonly HttpClientHandler _httpClientHandler;
		private readonly HttpClient _httpClient;
		private readonly string _userAgent;
		private bool _disposedValue;

		public YoutubePreview() {
			_httpClientHandler = new() {
				AllowAutoRedirect = false
			};
			_userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/106.0.0.0 Safari/537.36";
			_httpClient = new(_httpClientHandler);
			_httpClient.DefaultRequestHeaders.UserAgent.ParseAdd(_userAgent);
		}

		public static Uri? ValidateYoutubeLink(string message) {
			return Regex.Matches(message, @"(https?://)?(www.)?youtube.com/watch\?v=[a-zA-Z0-9_-]+")
				.Select(match => new Uri(match.Value))
				.FirstOrDefault();
		}


		/// <summary>
		/// Grid Preview, or in terms of youtube is called storyboard.
		/// Generally youtube will fetch image when the progress bar is hovered. But fortunately,
		/// youtube response gave us clue.
		/// We can get the image from JSON inside their javascript. We only need this link
		/// eg: https://i.ytimg.com/sb/<id>/storyboard3_L$L/$N.jpg
		/// 
		/// We only need to change the id, $L and $N.
		/// $L and $N is the grid length, 2 is recommended, so it will be
		/// eg: https://i.ytimg.com/sb/<id>/storyboard3_L2/M2.jpg
		/// </summary>
		/// <param name="youtubeLink"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		/// <exception cref="InvalidOperationException"></exception>
		public async Task<Uri> YoutubeStoryBoardAsync(Uri youtubeLink, CancellationToken cancellationToken) {
			using HttpResponseMessage response = await _httpClient.GetAsync(youtubeLink.ToString(), cancellationToken);
			response.EnsureSuccessStatusCode();
			string responseBody = await response.Content.ReadAsStringAsync();

			string jsPattern = @"[^<]*ytInitialPlayerResponse\s*=\s*({.*?});[^<]*<\/script>";
			Match match = Regex.Match(responseBody, jsPattern, RegexOptions.Singleline);

			if (!match.Success) {
				throw new InvalidOperationException("Failed to get preview image");
			}

			string jsonData = match.Groups[1].Value.Trim();
			if (string.IsNullOrWhiteSpace(jsonData)) {
				throw new InvalidOperationException("Failed to get JSON data");
			}

			JsonNode? jsonObject = JsonNode.Parse(jsonData);
			if (string.IsNullOrWhiteSpace(jsonData)) {
				throw new InvalidOperationException("Failed parse JSON");
			}

			JsonNode? storyBoards = jsonObject?["storyboards"]?["playerStoryboardSpecRenderer"]?["spec"];
			if (storyBoards == null) {
				throw new InvalidOperationException("Failed to get storyboards link");
			}

			string storyBoardsLink = storyBoards.ToString();

			Uri uri = new(storyBoardsLink);

			// The "spec" key from storyboard is having a query string with "|" (pipe) delimiter.
			// Somehow the Uri class cannot read "|" (pipe) delimiter and make the query string chopped.
			string queryString = storyBoardsLink.TrimStart('?');
			queryString = queryString.Replace('|', '&');
			// Parse the query string manually
			NameValueCollection queryParams = HttpUtility.ParseQueryString(queryString);

			// We need to take last dynamically generated "sigh" key from the querystring
			string sighQueryKey = storyBoardsLink.Split(@"rs$").Last();

			// We need to take last dynamically generated "sqp" key from the querystring
			string sqpQueryKey = storyBoardsLink.Split('|').First();

			Uri sqp = new(sqpQueryKey);

			// Currently only L2 and M2 combination.
			// The other combination L1 - LN and M1 - MN is need different "sigh" query string
			string path = uri.AbsolutePath.Replace("$L", "2").Replace("$N", "M2");

			// Rebuilt the Uri 
			Uri storyboardYoutube = new(uri.Scheme + "://" + uri.Host + path + sqp.Query + "&sigh=rs%24" + sighQueryKey);

			return storyboardYoutube;

		}

		protected virtual void Dispose(bool disposing) {
			if (!_disposedValue) {
				if (disposing) {
					// dispose managed state (managed objects)
					_httpClient.Dispose();
					_httpClientHandler.Dispose();
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
