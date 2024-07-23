using Blazor_TicTac_Mvvm.Authentication;
using Blazor_TicTac_Mvvm.Components;
using Blazor_TicTac_Mvvm.Data.Database;
using Blazor_TicTac_Mvvm.Hubs;
using Blazor_TicTac_Mvvm.Hubs.Clients;
using Blazor_TicTac_Mvvm.Hubs.HubServices;
using Blazor_TicTac_Mvvm.Services;
using Blazor_TicTac_Mvvm.Services.HostedServices;
using Blazor_TicTac_Mvvm.ViewModels;
using Microsoft.AspNetCore.Components.Authorization;
using MudBlazor.Services;

namespace Blazor_TicTac_Mvvm;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        Register(builder.Services);
        
        // Add services to the container.
        builder.Services.AddRazorComponents()
            .AddInteractiveServerComponents();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            app.UseHsts();
        }

        app.UseHttpsRedirection();

        app.UseAuthentication();
        app.UseAuthorization();

        app.UseStaticFiles();
        app.UseAntiforgery();

        app.MapHub<GamingHub>("/gaminghub");
        app.MapHub<LobbySelectorHub>("/lobbyselectorhub");

        app.MapRazorComponents<App>()
            .AddInteractiveServerRenderMode();

        app.Run();
    }

    private static void Register(IServiceCollection coll)
    {
        coll.AddServerSideBlazor();
        coll.AddRazorComponents().AddInteractiveServerComponents().AddHubOptions(options =>
        {
            options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
        });
        coll.AddSignalR();
        coll.AddMudServices();

        coll.AddSingleton<LobbyCheckerService>();
        coll.AddHostedService(provider => provider.GetRequiredService<LobbyCheckerService>());
        //TODO Einen ASP.NET HostedService hinzufügen, der öfters mal die Datenbank überprüft ob 
        //suchende Spiele getimeouted sind. https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services?view=aspnetcore-8.0&tabs=visual-studio

        coll.AddScoped<LocalStorageService>();
        coll.AddScoped<AuthProvider>();
        coll.AddScoped<AuthenticationStateProvider>(sp => sp.GetRequiredService<AuthProvider>());

        coll.AddDbContextFactory<TicTacContext>();
        coll.AddScoped<DatabaseService>();

        coll.AddScoped<LoginViewModel>();
        coll.AddScoped<LobbySelectorViewModel>();
        coll.AddScoped<SuperTicTacViewModel>();

        coll.AddScoped<LobbySelectorClient>();
        coll.AddScoped<GameClient>();

        coll.AddAuthentication(y =>
        {
            y.DefaultScheme = "cookie";
            y.DefaultAuthenticateScheme = "cookie";
        }).AddCookie("cookie", x =>
        {
            x.LoginPath = "/login";
        });
    }
}