using Blazor_TicTac_Mvvm.Base;
using Blazor_TicTac_Mvvm.Components.Pages;
using Blazor_TicTac_Mvvm.Hubs.HubPackages;
using Blazor_TicTac_Mvvm.Hubs.HubServices;
using Blazor_TicTac_Mvvm.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Blazor_TicTac_Mvvm.Data.Database.Models;

namespace Blazor_TicTac_Mvvm.ViewModels
{
    public class LobbySelectorViewModel : ViewModelBase, IDisposable
    {
        private readonly LobbySelectorClient _client;
        private readonly LocalStorageService _localStorage;
        private readonly DatabaseService _databaseService;
        private readonly NavigationManager _navMan;
        private Timer _timer;
        private string _gameName = Guid.NewGuid().ToString().Substring(30);
        private List<SearchGameModel> _searchingGames;
        private List<SuperTicTacGameModel> _playedGames;

        public LobbySelectorViewModel(LobbySelectorClient client, LocalStorageService localStorage,
            DatabaseService databaseService, NavigationManager navMan)
        {
            _client = client;
            _localStorage = localStorage;
            _databaseService = databaseService;
            _navMan = navMan;

            _client.ServerListUpdated = new EventCallback(null, UpdateGameListAsync);
            _client.StartGame = new EventCallback<AnswerToJoinRequestPackage>(null, StartGameAsync);
        }

        public Task<AuthenticationState> AuthenticationStateTask { get; set; }

        public int LocalPlayerId { get; set; }

        /// <summary>
        /// Gets or sets the name of the game we may create.
        /// </summary>
        public string GameName
        {
            get => _gameName;
            set => SetProperty(ref _gameName, value);
        }

        public List<SearchGameModel> SearchingGames
        {
            get => _searchingGames;
            set => SetProperty(ref _searchingGames, value);
        }

        public List<SuperTicTacGameModel> PlayedGames
        {
            get => _playedGames;
            set => SetProperty(ref _playedGames, value);
        }

        public override async Task OnInitializedAsync()
        {
            await _client.StartClientAsync();
            //Server and client sided initialize. Server does not have the sessionstorage and returns null as user.
            var storageUser = await _localStorage.GetUserAsync();
            if (storageUser != null)
            {
                LocalPlayerId = storageUser.Value.Id;
                PlayedGames = await _databaseService.GetPlayedGamesAsync(LocalPlayerId);
            }

            await UpdateGameListAsync();

            await base.OnInitializedAsync();
        }

        public async Task CreateMatchAsync()
        {
            var user = await AuthenticationStateTask;

            var createMatchPackage = new HostMatchPackage();
            createMatchPackage.MatchName = GameName;
            createMatchPackage.HostName = user.User.Identity.Name;
            var id = user.User.Identities.First()
               .Claims.First(x => x.Type == System.Security.Claims.ClaimTypes.Sid).Value;
            createMatchPackage.HostId = Convert.ToInt32(id);

            await _client.CreateNewMatchAsync(createMatchPackage);

            _timer = new Timer(KeepAliveTimerElapsed, null, 15000, 15000);
        }

        public async Task JoinGameAsync(SearchGameModel gameToJoin)
        {
            var user = await AuthenticationStateTask;

            var package = new AskToJoinPackage(Guid.Empty,
                gameToJoin.GameName,
                gameToJoin.HostModel.Name,
                gameToJoin.ConnectionIdToMessage,
                user.User.Identity.Name,
                null);

            await _client.AskToJoinRoom(package);
        }

        public async Task UpdateGameListAsync()
        {
            SearchingGames = await _databaseService.GetOpenGames();
        }

        public void OpenHistoryGame(SuperTicTacGameModel historyGame)
        {

        }

        private async Task StartGameAsync(AnswerToJoinRequestPackage package)
        {
            var dic = new Dictionary<string, object>()
            {
                [nameof(SuperTicTacPage.MatchId)] = package.MatchId.ToString(),
            };
            var uri = _navMan.GetUriWithQueryParameters("/SuperTicTacToe", dic);

            _navMan.NavigateTo(uri, true);
        }

        private void KeepAliveTimerElapsed(object _)
        {
            var user = AuthenticationStateTask.Result;
            var id = user.User.Identities.First()
              .Claims.First(x => x.Type == System.Security.Claims.ClaimTypes.Sid).Value;
            var playerId = Convert.ToInt32(id);

            _databaseService.KeepSearchGameAlive(playerId);
        }

        public void Dispose()
        {
            //TODO Theoretisch im Dispose schon einen Lobby stop Search schicken.
            _timer?.Dispose();
        }
    }
}
