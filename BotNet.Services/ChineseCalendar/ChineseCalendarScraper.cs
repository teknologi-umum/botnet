using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using AngleSharp;
using AngleSharp.Dom;
using AngleSharp.Html.Dom;

namespace BotNet.Services.ChineseCalendar {
	public class ChineseCalendarScraper(HttpClient httpClient) {
		private const string UrlTemplate = "https://www.chinesecalendaronline.com/{0}/{1}/{2}.htm";

		public async Task<(
			string Clash,
			string Evil,
			string GodOfJoy,
			string GodOfHappiness,
			string GodOfWealth,
			string[] AuspiciousActivities,
			string[] InauspiciousActivities
		)> GetYellowCalendarAsync(DateOnly date, CancellationToken cancellationToken) {
			string url = string.Format(UrlTemplate, date.Year, date.Month, date.Day);
			using HttpRequestMessage httpRequest = new(HttpMethod.Get, url);
			using HttpResponseMessage httpResponse = await httpClient.SendAsync(httpRequest, cancellationToken);
			httpResponse.EnsureSuccessStatusCode();

			string html = await httpResponse.Content.ReadAsStringAsync(cancellationToken);

			IBrowsingContext browsingContext = BrowsingContext.New(Configuration.Default);
			IDocument document = await browsingContext.OpenAsync(req => req.Content(html), cancellationToken);
			IHtmlSpanElement? clashSpan = document.QuerySelector<IHtmlSpanElement>("div.cal-lunar-box > span:nth-child(2)");
			IHtmlSpanElement? evilSpan = document.QuerySelector<IHtmlSpanElement>("div.cal-lunar-box > span:nth-child(3)");
			IHtmlSpanElement? godOfJoySpan = document.QuerySelector<IHtmlSpanElement>("fieldset.cal-lunar-box > span:nth-child(2)");
			IHtmlSpanElement? godOfHappinessSpan = document.QuerySelector<IHtmlSpanElement>("fieldset.cal-lunar-box > span:nth-child(4)");
			IHtmlSpanElement? godOfWealthSpan = document.QuerySelector<IHtmlSpanElement>("fieldset.cal-lunar-box > span:nth-child(6)");
			IEnumerable<IHtmlListItemElement> auspiciousActivityElements = document.QuerySelectorAll<IHtmlListItemElement>(".order-1 ul.cal-event li");
			IEnumerable<IHtmlListItemElement> inauspiciousActivityElements = document.QuerySelectorAll<IHtmlListItemElement>(".order-3 ul.cal-event li");

			if (clashSpan is null || evilSpan is null || godOfJoySpan is null || godOfHappinessSpan is null || godOfWealthSpan is null) {
				throw new InvalidOperationException("ChineseCalendarOnline.com returned an unexpected response.");
			}

			return (
				Clash: clashSpan.InnerHtml,
				Evil: evilSpan.InnerHtml,
				GodOfJoy: godOfJoySpan.InnerHtml,
				GodOfHappiness: godOfHappinessSpan.InnerHtml,
				GodOfWealth: godOfWealthSpan.InnerHtml,
				AuspiciousActivities: auspiciousActivityElements
					.Select(element => element.InnerHtml)
					.ToArray(),
				InauspiciousActivities: inauspiciousActivityElements
					.Select(element => element.InnerHtml)
					.ToArray()
			);
		}
	}
}
