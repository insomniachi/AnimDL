﻿namespace AnimDL.Core;

public class DefaultUrl
{
    public static string AnimePahe { get; set; } = "https://animepahe.com/";
    public static string AnimixPlay { get; set; } = "https://animixplay.to/";
    public static string Tenshi { get; set; } = "https://tenshi.moe/";
    public static string GogoAnime { get; set; } = "https://www1.gogoanime.bid/";
    public static string Zoro { get; set; } = "https://zoro.to/";
    public static string Yugen { get; set; } = "https://yugen.to/";
    public static string AllAnime { get; set; } = "https://allanime.site/";
    public static string Marin { get; set; } = "https://marin.moe/";
}

public class GlobalConfig
{
    public static string AudioType { get; set; } = "sub";
}

public class Headers
{
    public const string Referer = "referer";
    public const string Cookie = "cookie";
}

public static class Config
{
    public const string BaseUrl = "BaseUrl";
}