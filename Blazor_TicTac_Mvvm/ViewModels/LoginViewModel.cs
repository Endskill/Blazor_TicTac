using Blazor_TicTac_Mvvm.Base;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components;
using Blazor_TicTac_Mvvm.Services;
using Blazor_TicTac_Mvvm.Authentication;
using MudBlazor;

namespace Blazor_TicTac_Mvvm.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private readonly NavigationManager _navManager;
        private readonly DatabaseService _dbService;
        private readonly AuthProvider _authProvider;
        private readonly IDialogService _dialogService;
        private string _username;
        private bool _usernameDoesNotExist;

        [CascadingParameter]
        protected Task<AuthenticationState> AuthenticationStateTask { get; set; }

        public LoginViewModel(NavigationManager navManager, DatabaseService dbService, AuthProvider authProvider,
            IDialogService dialogService)
        {
            _navManager = navManager;
            _dbService = dbService;
            _authProvider = authProvider;
            _dialogService = dialogService;
        }

        public string Username
        {
            get => _username; set
            {
                if (SetProperty(ref _username, value))
                {
                    //Chaging the username removes the create user button.
                    UsernameDoesNotExist = false;
                    OnPropertyChanged(nameof(UsernameEmptyOrNull));
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating if <see cref="Username"/> does not exist in the database.
        /// </summary>
        public bool UsernameDoesNotExist
        {
            get => _usernameDoesNotExist;
            set => SetProperty(ref _usernameDoesNotExist, value);
        }

        public bool UsernameEmptyOrNull => string.IsNullOrEmpty(Username);

        public async Task LoginAsync()
        {
            //The login button is deactivated if there is not valid input.
            UsernameDoesNotExist = !(await _authProvider.TryLoginAsync(Username));
            if (UsernameDoesNotExist)
            {
                var test = await _dialogService.ShowMessageBox("User does not exist!", "Please create user before signing in.");
                return;
            }

            _navManager.NavigateTo("/lobbyselector", true);
        }

        public async Task CreateUserAsync()
        {
            //TODO Nutzer fragen, ob man den Nutzer mit diesem Username wirklich erstellen will.
            await _dbService.CreateUser(Username);
            await LoginAsync();
        }

        //TODO Remove Debug functions.
        internal Task Debug_RestartDatabaseAsync()
        {
            return _dbService.Debug_RestartDatabaseAsync();
        }

        internal Task Debug_Login1()
        {
            Username = "tobi";
            return LoginAsync();
        }

        internal Task Debug_Login2()
        {
            Username = "tobi2";
            return LoginAsync();
        }
    }
}
