using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using MalApi;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace AnimDL.WinUI.Dialogs.ViewModels;

public class UpdateAnimeStatusViewModel : ReactiveObject
{
    public UpdateAnimeStatusViewModel()
    {
        this.WhenAnyValue(x => x.Anime)
            .WhereNotNull()
            .Select(x => x.UserStatus)
            .WhereNotNull()
            .Subscribe(x =>
            {
                Status = x.Status;
                EpisodesWatched = x.WatchedEpisodes;
                Score = x.Score;
                StartDate = x.StartDate == new DateTime() ? null : new DateTimeOffset(x.StartDate);
                EndDate = x.FinishDate == new DateTime() ? null : new DateTimeOffset(x.FinishDate);
            });
    }


    [Reactive] public Anime Anime { get; set; }
    [Reactive] public AnimeStatus Status { get; set; }
    [Reactive] public double EpisodesWatched { get; set; }
    [Reactive] public Score Score { get; set; }
    [Reactive] public DateTimeOffset? StartDate { get; set; }
    [Reactive] public DateTimeOffset? EndDate { get; set; }
    [Reactive] public string Tags { get; set; }
    [Reactive] public Priority? Priority { get; set; }
    [Reactive] public int? RewatchCount { get; set; }
    [Reactive] public RewatchValue? RewatchValue { get; set; }
    public double TotalEpisodes => Anime?.TotalEpisodes ?? double.MaxValue;     
}