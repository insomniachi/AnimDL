using System.Threading.Tasks;
using MalApi;

namespace AnimDL.WinUI.Contracts.Services;

public interface IViewService
{
    Task UpdateAnimeStatus(Anime anime);
}
