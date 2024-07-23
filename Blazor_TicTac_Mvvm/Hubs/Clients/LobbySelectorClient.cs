using Blazor_TicTac_Mvvm.Hubs.HubPackages;
using Blazor_TicTac_Mvvm.Hubs.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Blazor_TicTac_Mvvm.Hubs.HubServices
{
    public class LobbySelectorClient : ILobbySelectorClient
    {
        private readonly HubConnection _connection;
        private readonly IDialogService _dialogService;

        public LobbySelectorClient(NavigationManager navMan, IDialogService dialogService)
        {
            _dialogService = dialogService;

            _connection = new HubConnectionBuilder()
                .WithUrl(navMan.ToAbsoluteUri("/lobbyselectorhub"))
                .Build();

            _connection.On<AskToJoinPackage>(nameof(ReceiveSomeoneAskedToJoinAsync), ReceiveSomeoneAskedToJoinAsync);
            _connection.On<AnswerToJoinRequestPackage>(nameof(ReceiveAnswerToJoinAsync), ReceiveAnswerToJoinAsync);
            _connection.On(nameof(ReceiveMatchClosedAsync), ReceiveMatchClosedAsync);
            _connection.On<HostMatchPackage>(nameof(ReceiveMatchOpenedAsync), ReceiveMatchOpenedAsync);
        }

        public EventCallback ServerListUpdated { get; set; }
        public EventCallback<AnswerToJoinRequestPackage> StartGame { get; set; }

        public Task StartClientAsync()
        {
            if (_connection.State != HubConnectionState.Connected)
                return _connection.StartAsync();

            return Task.CompletedTask;
        }

        #region Client=>Server Communication
        /// <inheritdoc/>
        public Task CreateNewMatchAsync(HostMatchPackage package)
        {
            package.ConnId = _connection.ConnectionId;
            return _connection.SendAsync(nameof(LobbySelectorHub.CreateNewMatchAsync), package);
        }

        /// <inheritdoc/>
        public Task StopSearchingMatch(HostMatchPackage package)
            => _connection.SendAsync(nameof(LobbySelectorHub.StopSearchingMatch), package);

        /// <inheritdoc/>
        public Task AskToJoinRoom(AskToJoinPackage package)
        {
            package.AskerConnId = _connection.ConnectionId;
            return _connection.SendAsync(nameof(LobbySelectorHub.AskToJoinRoom), package);
        }

        /// <inheritdoc/>
        public Task AnswerJoinRequest(AnswerToJoinRequestPackage package)
            => _connection.SendAsync(nameof(LobbySelectorHub.AnswerJoinRequest), package);

        #endregion

        #region Server=>Client Communication
        /// <inheritdoc/>
        public Task ReceiveAnswerToJoinAsync(AnswerToJoinRequestPackage package)
        {
            return StartGame.InvokeAsync(package);
        }

        /// <inheritdoc/>
        public Task ReceiveMatchClosedAsync()
        {
            //TODO Schauen was passiert, wenn der Server deine zurzeit gehostete Lobby schließt.
            //Sollte eigentlich nicht passieren. Vielleicht unter einen kleinen Timeout.
            return ServerListUpdated.InvokeAsync();
        }

        /// <inheritdoc/>
        public async Task ReceiveSomeoneAskedToJoinAsync(AskToJoinPackage package)
        {
            //TODO Überprüfen, ob der lokale Spieler nicht zufällig gerade sein Spiel schon geschlossen hat.
            //TODO Überprüfung, ob nicht zufälligerweise 2 Anfragen gleichzeitig kommen.
            //TODO Theoretisch schauen wie der Spielname ist. Was passiert, wenn multiple offene Suchanfragen???

            var rng = new Random(DateTime.Now.Millisecond);
            var answerPackage = new AnswerToJoinRequestPackage(
                Guid.NewGuid(),
                package.MatchName,
                package.HostName,
                package.HostConnId,
                package.AskerName,
                package.AskerConnId,
                Convert.ToBoolean(rng.Next(0, 1)));

            //Server creates the database entry of the running game for us.
            //We will call Startgame when we receive the answer.
            await _connection.SendAsync(nameof(LobbySelectorHub.AnswerJoinRequest), answerPackage);
        }

        /// <inheritdoc/>
        public Task ReceiveMatchOpenedAsync(HostMatchPackage package)
        {
            if (!package.CreatedMatch)
            {
                return _dialogService.ShowMessageBox("unable to create lobby.", $"A game with the name {package.MatchName} does already exist!");
            }

            return ServerListUpdated.InvokeAsync();
        }

        /// <inheritdoc/>
        public Task ReceivePingRequestAsync(PingPackage package)
        {
            return _connection.SendAsync(nameof(LobbySelectorHub.AnswerPingAsync), package);
        }

        #endregion
    }
}
