using Microsoft.AspNetCore.Components;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;

namespace Blazor_TicTac_Mvvm.Base
{
    public class MvvmComponentBase<TViewModel> : ComponentBase, IDisposable
        where TViewModel : IViewModelBase
    {
        [Inject]
        [NotNull]
        protected TViewModel ViewModel { get; init; }

        protected TViewModel Vm => ViewModel;

        protected override void OnInitialized()
        {
            ViewModel.PropertyChanged += ViewModelPropertyChanged;
            ViewModel.RunOnUiThread = new EventCallback<Func<Task>>(null, ViewModelInvokeAsync);
            base.OnInitialized();
        }

        protected override async Task OnInitializedAsync()
        {
            await ViewModel.OnInitializedAsync();
            await base.OnInitializedAsync();
        }

        public virtual void Dispose()
        {
            ViewModel.PropertyChanged -= ViewModelPropertyChanged;
        }

        private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
        {
            InvokeAsync(StateHasChanged);
        }

        private Task ViewModelInvokeAsync(Func<Task> execute)
        {
            return InvokeAsync(execute);
        }
    }
}
