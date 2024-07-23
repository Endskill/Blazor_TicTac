using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.AspNetCore.Components;

namespace Blazor_TicTac_Mvvm.Base
{
    public partial class ViewModelBase : ObservableObject, IViewModelBase
    {
        public EventCallback<Func<Task>> RunOnUiThread { get; set; }

        public virtual Task OnInitializedAsync() => Task.CompletedTask;

        protected virtual void NotifyStateChanged() => OnPropertyChanged((string)null);

        /// <summary>
        /// Runs <paramref name="executable"/> on the ui thread.
        /// </summary>
        protected Task InvokeAsync(Func<Task> executable) => RunOnUiThread.InvokeAsync(executable);
    }
}
