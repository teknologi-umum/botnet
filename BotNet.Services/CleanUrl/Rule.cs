using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace BotNet.Services.UrlCleaner {
  public class Rule {
    public required string Name { get; set; }
    public required Regex Match { get; set; }
    public required List<string> Rules { get; set; }
    public List<string>? Replace { get; set; }

  }

  public static class RuleData {
    /// <summary>
    /// Represents a list of rules for cleaning URLs.
    /// </summary>
    public static List<Rule> Rules = [
    new Rule
    {
      Name = "Global",
      Match = new Regex("./*"),
      Rules =
    [
            // https://en.wikipedia.org/wiki/UTM_parameters
            "utm_source", "utm_medium", "utm_term", "utm_campaign",
            "utm_content", "utm_name", "utm_cid", "utm_reader", "utm_viz_id",
            "utm_pubreferrer", "utm_swu", "utm_social-type", "utm_brand",
            "utm_team", "utm_feeditemid", "utm_id", "utm_marketing_tactic",
            "utm_creative_format", "utm_campaign_id", "utm_source_platform",
            "utm_timestamp", "utm_souce",
            // ITM parameters, a variant of UTM parameters
            "itm_source", "itm_medium", "itm_term", "itm_campaign", "itm_content",
            "itm_channel", "itm_source_s", "itm_medium_s", "itm_campaign_s",
            "itm_audience",
            // INT parameters, another variant of UTM
            "int_source", "int_cmp_name", "int_cmp_id", "int_cmp_creative",
            "int_medium", "int_campaign",
            // piwik
            "pk_campaign", "pk_cpn", "pk_source", "pk_medium",
            "pk_keyword", "pk_kwd", "pk_content", "pk_cid",
            "piwik_campaign", "piwik_cpn", "piwik_source", "piwik_medium",
            "piwik_keyword", "piwik_kwd", "piwik_content", "piwik_cid",
            // Google Ads
            "gclid", "ga_source", "ga_medium", "ga_term", "ga_content", "ga_campaign",
            "ga_place", "gclid", "gclsrc",
            // hhsa
            "hsa_cam", "hsa_grp", "hsa_mt", "hsa_src", "hsa_ad", "hsa_acc",
            "hsa_net", "hsa_kw", "hsa_tgt", "hsa_ver", "hsa_la", "hsa_ol",
            // Facebook
            "fbclid",
            // Olytics
            "oly_enc_id", "oly_anon_id",
            // Vero
            "vero_id", "vero_conv",
            // Drip
            "__s", 
            // HubSpot
            "_hsenc", "_hsmi", "__hssc", "__hstc", "__hsfp", "hsCtaTracking",
            // Marketo
            "mkt_tok",
            // Matomo
            "mtm_campaign", "mtm_keyword", "mtm_kwd", "mtm_source", "mtm_medium",
            "mtm_content", "mtm_cid", "mtm_group", "mtm_placement", 
            // Oracle Eloqua
            "elqTrackId", "elq", "elqaid", "elqat", "elqCampaignId", "elqTrack",
            // MailChimp
            "mc_cid", "mc_eid",
            // Other              
            "ncid", "cmpid", "mbid",
            // Reddit Ads
            "rdt_cid"
            ]
    },
    new Rule
    {
      Name = "audible.com",
      Match = new Regex("www.audible.com", RegexOptions.IgnoreCase),
      Rules = ["qid", "sr", "pf_rd_p", "pf_rd_r", "plink", "ref"]
    },
    new Rule
    {
      Name = "reddit.com",
      Match = new Regex(@".*\.reddit\.com", RegexOptions.IgnoreCase),
      Rules =
    [
    "ref_campaign", "ref_source", "tags", "keyword", "channel", "campaign",
        "user_agent", "domain", "base_url", "$android_deeplink_path",
        "$deeplink_path", "$og_redirect", "share_id", "correlation_id", "$deep_link", "post_index", "ref", "_branch_match_id", "post_fullname", "$3p", "_branch_referrer"
      ]
    },
    new Rule
    {
      Name = "facebook.com",
      Match = new Regex(@".*\.facebook\.com", RegexOptions.IgnoreCase),
      Rules =
    [
    "fbclid", "fb_ref", "fb_source", "referral_code", "referral_story_type", "tracking", "ref", "mibextid", "app", "_rdr", "m_entstream_source", "paipv", "locale", "eav"
      ],
    },
    new Rule
    {
      Name = "shopee.com",
      Match = new Regex(@"^(?:https?:\/\/)?(?:[^.]+\.)?shopee\.[a-z0-9]{0,3}", RegexOptions.IgnoreCase),
      Rules =
    [
    "af_siteid", "pid", "af_click_lookback", "af_viewthrough_lookback",
        "is_retargeting", "af_reengagement_window", "af_sub_siteid", "c"
      ]
    },
    new Rule
    {
      Name = "instagram.com",
      Match = new Regex(@"^(?:https?:\/\/)?(?:[^.]+\.)?instagram\.com", RegexOptions.IgnoreCase),
      Rules = ["igshid", "source"],
    },
    new Rule
    {
      Name = "twitter.com or x.com",
      Match = new Regex("(twitter.com|x.com)", RegexOptions.IgnoreCase),
      Rules = ["s", "src", "ref_url", "ref_src"]
    },
    new Rule
    {
      Name = "youtube.com",
      Match = new Regex(@".*\.youtube\.com", RegexOptions.IgnoreCase),
      Rules = ["gclid", "feature", "app", "src", "lId", "cId", "embeds_referring_euri"],
    },
    new Rule
    {
      Name = "discord.com",
      Match = new Regex(@".*\.discord\.com", RegexOptions.IgnoreCase),
      Rules = ["source"]
    },
    new Rule
    {
      Name = "medium.com",
      Match = new Regex(@"medium\.com", RegexOptions.IgnoreCase),
      Rules = ["source"]
    },
new Rule
    {
      Name = "apple.com",
      Match = new Regex(@".*\.apple\.com", RegexOptions.IgnoreCase),
      Rules = ["uo", "app", "at", "ct", "ls", "pt", "mt", "itsct", "itscg", "referrer", "src", "cid"]
    },
    new Rule
    {
      Name = "music.apple.com",
      Match = new Regex(@"music\.apple\.com", RegexOptions.IgnoreCase),
      Rules = ["i", "lId", "cId", "sr", "src"]
    },
    new Rule
    {
      Name = "play.google.com",
      Match = new Regex(@"play\.google\.com", RegexOptions.IgnoreCase),
      Rules = ["referrer", "pcampaignid"]
    },
    new Rule
    {
      Name = "bing.com",
      Match = new Regex(@"^www\.bing\.com", RegexOptions.IgnoreCase),
      Rules = [
        "qs", "form", "sp", "pq", "sc", "sk", "cvid", "FORM",
        "ck", "simid", "thid", "cdnurl", "pivotparams", "ghsh", "ghacc",
        "ccid", "", "ru"
      ]
    }
    ];
  }
}