﻿using AnimDL.WinUI.Core.Helpers;

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
        <video id = ""my_video_1"" class=""video-js vjs-fluid vjs-default-skin vjs-fill"" controls autoplay>
            <source src = ""{0}"" type=""application/x-mpegURL"">
        </video>

        <script>
            var player = videojs('my_video_1');
            player.ready(function () {{
                var obj = new Object();
                obj.MessageType = ""Ready"";
                window.chrome.webview.postMessage(obj);
                this.on('timeupdate', function () {{
                   var obj = new Object();
                   obj.MessageType = ""TimeUpdate"";
                   obj.Content  = this.currentTime().toString();
                   window.chrome.webview.postMessage(obj);
                }})
                this.on('durationchange', function () {{
                   var obj = new Object();
                   obj.MessageType = ""DurationUpdate"";
                   obj.Content  = this.duration().toString();
                   window.chrome.webview.postMessage(obj);
                }})
                this.on('ended', function () {{
                   var obj = new Object();
                   obj.MessageType = ""Ended"";
                   window.chrome.webview.postMessage(obj);
                }})
              }});
        </script>
    </body>
</html>";

    public static string GetPlayerHtml(string url)
    {
        return string.Format(PlayerFormat, url);
    }
}
