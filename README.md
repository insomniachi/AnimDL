# AnimDL
This project is a C# port of [**justfoolingaround/animdl**](https://github.com/justfoolingaround/animdl)

## Installation
1.  dotNET CLI (Global)
    ```
    dotnet tool install --global AnimDL
    ```
2. dotNET CLI (Local)
    ```
    dotnet new tool-manifest # if you are setting up this repo
    dotnet tool install --local AnimDL
    ```
3. Cake
    ```
    #tool dotnet:?package=AnimDL
    ```
4. NUKE
    ```
    nuke :add-package AnimDL
    ```
## Usage
```
Usage:
  AnimDL [command] [options]

Options:
  --version       Show version information
  -?, -h, --help  Show help and usage information

Commands:
  config          configure options for application
  grab <Title>    grabs stream links
  search <Title>  search anime on provider
  stream <Title>  stream anime
```

The `stream` option is disabled automatically if the project cannot find any of the supported streamers.

## `stream` / `grab`

These commands are the main set of command in the project. All of them scrape the target site, the only difference is how it is used.

- The `stream` option tosses the stream url to a player so that you can seamlessly binge your anime.
    - Streaming supports Discord Rich Presence with [`discord-rpc-csharp`](https://github.com/Lachee/discord-rpc-csharp).
    ```
    Description:
        stream anime

    Usage:
        AnimDL stream [<Title>] [options]

    Arguments:
        <Title>  Title of anime to search

    Options:
        -p, --provider <AnimeOut|AnimePahe|AnimixPlay|GogoAnime|Tenshi|Zoro>  provider name [default: AnimixPlay]
        -r, --range <range>                                                   range of episodes [default: 0..^0]
        --player <Vlc>                                                        media player to stream. [default: Vlc]
        -?, -h, --help                                                        Show help and usage information
    ```
- The `grab` option simply streams the stream url to the stdout stream.
    - This is useful for external usage and testing.
    ```
    Description:
        grabs stream links

    Usage:
        AnimDL grab [<Title>] [options]

    Arguments:
        <Title>  Title of anime to search

    Options:
        -p, --provider <AnimeOut|AnimePahe|AnimixPlay|GogoAnime|Tenshi|Zoro>  provider name [default: AnimixPlay]
        -r, --range <range>                                                   range of episodes [default: 0..^0]
        -?, -h, --help                                                        Show help and usage information
        ```

```sh
animdl stream "One Piece" 
```
<p align="center">
<sub>
Providers can be specified by using provider prefix, <code>stream "One Piece" -p gogoanime</code>, will use the 9Anime provider.
</sub></p>

## `-r` / `--range` argument
This argument is shared by **stream** and **grab**, it can be used to hand over custom ranges for selecting episodes.<br/>
uses c# range and indices syntax.
```
2..5    // episodes from [2-5]
5..     // episodes from 5 till end.
5       // 5th episode
^3      // 3rd episode from end
^3..    // last 3 episodes
```
if you don't provide a value for this argument, all available episodes will be taken.

# Configuration
use **animdl config** edit the configuration file in your default editor for `.json` files
```
{
  "DefaultProvider": "AnimixPlay",
  "DefaultMediaPlayer": "VLC",
  "UseRichPresense": false,
  "MediaPlayers": {
    "VLC": {
      "Executable": "%ProgramFiles%\\VideoLAN\\VLC\\vlc.exe"
    }
  }
}
```
