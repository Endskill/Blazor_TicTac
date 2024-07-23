namespace Blazor_TicTac_Mvvm.Hubs.HubPackages
{
    public record struct HostMatchPackage(
         string MatchName,
         string HostName,
         int HostId,
         string ConnId,
         bool CreatedMatch)
    { }
}
