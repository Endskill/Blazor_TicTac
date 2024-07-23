namespace Blazor_TicTac_Mvvm.Hubs.HubPackages
{
    /// <summary>
    /// Data packaged used, for client server communication.
    /// </summary>
    /// <param name="MatchName"></param>
    /// <param name="HostName"></param>
    /// <param name="HostId"></param>
    /// <param name="HostConnId"></param>
    public record struct PlayersInMatchPackage
        (string MatchName,
        string HostName,
        int HostId,
        string HostConnId)
    {
        public static implicit operator PlayersInMatchPackage(HostMatchPackage openData)
        {
            return new PlayersInMatchPackage(openData.MatchName, openData.HostName, openData.HostId, openData.ConnId);
        }
    }
}
