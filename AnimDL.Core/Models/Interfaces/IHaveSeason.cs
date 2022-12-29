namespace AnimDL.Core.Models.Interfaces;

public interface IHaveSeason : IHaveYear
{
    public string Season { get; set; }
}
