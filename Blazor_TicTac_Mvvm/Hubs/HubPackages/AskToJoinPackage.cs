namespace Blazor_TicTac_Mvvm.Hubs.HubPackages
{
    public record struct AskToJoinPackage(
       Guid MatchId,
       string MatchName,
       string HostName,
       string HostConnId,
       string AskerName,
       string AskerConnId)
    { }

    public record struct AnswerToJoinRequestPackage(
       Guid MatchId,
       string MatchName,
       string HostName,
       string HostConnId,
       string AskerName,
       string AskerConnId,
       bool hostIsFirstTurn)
    { }
}
