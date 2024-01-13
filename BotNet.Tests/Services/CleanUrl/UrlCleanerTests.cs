using Xunit;
using BotNet.Services.UrlCleaner;

namespace BotNet.Tests.Services.CleanUrl {
	public class UrlCleanerTests {

		[Theory]
		[InlineData("https://nasional.kompas.com/read/2024/01/10/17560541/jokowi-belum-ucapkan-selamat-ultah-ke-pdi-p-ganjar-lupa-kali?utm_source=Telegram&utm_medium=Referral&utm_campaign=Top_Desktop", "https://nasional.kompas.com/read/2024/01/10/17560541/jokowi-belum-ucapkan-selamat-ultah-ke-pdi-p-ganjar-lupa-kali")]
		[InlineData("https://www.reddit.com/r/indonesia/comments/10nc28j/kerugian_udah_tembus_7m_gara2_bug_promo/?$deep_link=true&correlation_id=b1d34957-35e3-4ce1-9120-eb111509ae81&post_fullname=t3_10nc28j&post_index=1&ref=email_digest&ref_campaign=email_digest&ref_source=email&utm_content=post_title&utm_medium=Email%20Amazon%20SES&$3p=e_as&_branch_match_id=696254937267305114&_branch_referrer=H4sIAAAAAAAAA22QXWrDMBCET%2BO%2B2Yksp0kKoRRKr7GspY2jRH9IK9Ljd920fSpIMHyj3Rl0Yc71ZbMpZK3jAXMevIu3jc6v3TjpfCLA%2BiQyFbe4iB5a8afLOtXpt278kHO%2F34efeZOCgCLXRZsiVYeihQaKXEWqbTTj4SrqRqUtDiM0ixdgCnOrsA%2BwYMER5rZALimkNUJLymSJMqzdOv3OpVE3PptUCnlklyI4K3xWVk%2FH3b7XO9L9ZEj1RzVue5qVUrvtEemgZC6nynBu3kcMtK7T8NfrYUp7%2BhRnfV3oLIoCOg%2FWLVT5AcFgyOiW%2BL9bUyuGfj2BjQOYFFl%2BQuh3DDv29AW7S%2FV8gwEAAA%3D%3D", "https://www.reddit.com/r/indonesia/comments/10nc28j/kerugian_udah_tembus_7m_gara2_bug_promo/?$deep_link=true&$3p=e_as")]
		[InlineData("https://www.kaorinusantara.or.id/newsline/194064/kak-seto-beneran-jadi-seto-kaiba-di-google?fbclid=IwAR2TTZgHLAAYJtZj_L5MKRGrHzrCa04_y8SMwYG-cteuyL6A5u1VVDjqh_c", "https://www.kaorinusantara.or.id/newsline/194064/kak-seto-beneran-jadi-seto-kaiba-di-google")]
		[InlineData("https://www.facebook.com/groups/informatika.cringeposting/permalink/1110311033679168/?ref=share&mibextid=Cw5JYn", "https://www.facebook.com/groups/informatika.cringeposting/permalink/1110311033679168")]
		[InlineData("https://www.instagram.com/reel/CvOeEfJhG0f/?igshid=NTc4MTIwNjQ2YQ%3D%3D", "https://www.instagram.com/reel/CvOeEfJhG0f")]
		[InlineData("https://twitter.com/petergyang/status/1573489316147306496?ref_src=twsrc%5Etfw%7Ctwcamp%5Etweetembed%7Ctwterm%5E1573489316147306496%7Ctwgr%5E9bfbec9d831b2a896ffc769afc3b65024c52850b%7Ctwcon%5Es1_&ref_url=https%3A%2F%2Fgames.ensipedia.id%2Fnews%2Fcerdas-mahasiswa-ini-manfaatkan-ai-untuk-mengerjakan-tugas-kuliah-dan-dapat-nilai-a%2F", "https://twitter.com/petergyang/status/1573489316147306496")]
		public void CleanUrl_ShouldRemoveQueryParametersBasedOnRules(string url, string result) {
			string cleanedUrl = UrlCleaner.Clean(new System.Uri(url)).ToString();
			Assert.Equal(result, cleanedUrl);
		}
	}
}