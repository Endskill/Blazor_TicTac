using Blazor_TicTac_Mvvm.Data.GameData;
using Blazor_TicTac_Mvvm.Hubs.HubPackages;

namespace Blazor_TicTac_Mvvm.Hubs.Interfaces
{
    public interface IGameClient
    {
        Task ReceiveSetFieldAsync(SetFieldPackage package);
        Task ReceiveIllegalFieldSetSync(SetFieldPackage package);
        Task ReceiveWinnerAsync(TicTacState winner);
    }
}
