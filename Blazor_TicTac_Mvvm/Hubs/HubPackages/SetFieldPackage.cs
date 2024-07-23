using Blazor_TicTac_Mvvm.Data.GameData;

namespace Blazor_TicTac_Mvvm.Hubs.HubPackages
{
    public record struct SetFieldPackage
        (Guid MatchId,
        int GameFieldIndex,
        int FieldIndex,
        TicTacState Player)
    { }
}
