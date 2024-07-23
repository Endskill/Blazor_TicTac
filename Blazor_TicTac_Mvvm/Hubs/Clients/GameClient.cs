using Blazor_TicTac_Mvvm.Data.GameData;
using Blazor_TicTac_Mvvm.Hubs.HubPackages;
using Blazor_TicTac_Mvvm.Hubs.Interfaces;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.SignalR.Client;
using MudBlazor;

namespace Blazor_TicTac_Mvvm.Hubs.Clients
{
    public class GameClient : IGameClient
    {
        private readonly HubConnection _connection;
        private readonly IDialogService _dialogService;

        public GameClient(NavigationManager navMan, IDialogService dialogService)
        {
            _dialogService = dialogService;

            _connection = new HubConnectionBuilder()
                .WithUrl(navMan.ToAbsoluteUri("/gaminghub"))
                .Build();

            _connection.On<SetFieldPackage>(nameof(ReceiveSetFieldAsync), ReceiveSetFieldAsync);
            _connection.On<SetFieldPackage>(nameof(ReceiveIllegalFieldSetSync), ReceiveIllegalFieldSetSync);
            _connection.On<TicTacState>(nameof(ReceiveWinnerAsync), ReceiveWinnerAsync);
        }

        public EventCallback<SetFieldPackage> SetField { get; set; }
        public EventCallback IllegalSetField { get; set; }
        public EventCallback GameWon { get; set; }

        public async Task SetupGameAsync(Guid matchId)
        {
            await _connection.StartAsync();
            await _connection.SendAsync(nameof(GamingHub.JoinGroupAsync), matchId);
        }

        public Task SetFieldAsync(SetFieldPackage package)
        {
            return _connection.SendAsync(nameof(GamingHub.SetFieldAsync), package);
        }

        public Task ReceiveSetFieldAsync(SetFieldPackage package)
        {
            return SetField.InvokeAsync(package);
        }

        public Task ReceiveIllegalFieldSetSync(SetFieldPackage package)
        {
            return IllegalSetField.InvokeAsync();
        }

        public Task ReceiveWinnerAsync(TicTacState winner)
        {
            return GameWon.InvokeAsync(winner);
        }
    }
}
