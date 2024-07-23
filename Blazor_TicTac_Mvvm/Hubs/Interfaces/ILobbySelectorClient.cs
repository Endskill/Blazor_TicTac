using Blazor_TicTac_Mvvm.Hubs.HubPackages;

namespace Blazor_TicTac_Mvvm.Hubs.Interfaces
{
    public interface ILobbySelectorClient
    {
        /// <summary>
        /// The player in <paramref name="package"/> => <see cref="AskToJoinPackage.AskerConnId"/>
        /// tries to join the match <see cref="AskToJoinPackage.MatchId"/>.
        /// </summary>
        Task ReceiveSomeoneAskedToJoinAsync(AskToJoinPackage package);

        /// <summary>
        /// Runs when you've sent another player a join request and they answered it.
        /// </summary>
        Task ReceiveAnswerToJoinAsync(AnswerToJoinRequestPackage package);

        /// <summary>
        /// Runs when a different player closes the match searching.
        /// </summary>
        Task ReceiveMatchClosedAsync();

        /// <summary>
        /// Run when any player has created a match or your own match has failed to be created.
        /// </summary>
        Task ReceiveMatchOpenedAsync(HostMatchPackage package);

        /// <summary>
        /// The server periodically pings all clients that have a searching game open, to validate if the search request is still active.
        /// </summary>
        Task ReceivePingRequestAsync(PingPackage package);
    }
}
