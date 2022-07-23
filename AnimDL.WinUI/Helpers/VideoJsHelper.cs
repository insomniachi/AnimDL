﻿using ColorCode.Compilation.Languages;
using Microsoft.VisualBasic.Devices;
using System.Reactive.Joins;

namespace AnimDL.WinUI.Helpers;

public class VideoJsHelper
{
    const string PlayerFormat = @"<!DOCTYPE html>
<html>
    <head>
        <meta charset=utf-8 />
        <title>Your title</title>
        <link href=""https://unpkg.com/video.js/dist/video-js.css"" rel=""stylesheet"">
        <script src=""https://unpkg.com/video.js/dist/video.js""></script >
        <script src=""https://unpkg.com/videojs-contrib-hls/dist/videojs-contrib-hls.js""></script>
    </head>

    <body>
        <video id = ""my_video_1"" class=""video-js vjs-fluid vjs-default-skin"" controls width=""100%"" height=""100%"" autoplay>
            <source src = ""{0}"" type=""application/x-mpegURL"">
        </video>

        <script>
            var player = videojs('my_video_1');
            player.play();
        </script>
    </body>
</html>";

    public static string GetPlayerHtml(string url)
    {
        return string.Format(PlayerFormat, url);
    }
}