using Microsoft.AspNetCore.Components;
using System.ComponentModel;

namespace Blazor_TicTac_Mvvm.Base
{
    public interface IViewModelBase : INotifyPropertyChanged
    {
        EventCallback<Func<Task>> RunOnUiThread { internal get; set; }

        Task OnInitializedAsync();
    }
}
