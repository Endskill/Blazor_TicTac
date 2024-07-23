namespace Blazor_TicTac_Mvvm.Hubs.HubPackages
{
    public record struct PingPackage
        (string Sender,
        string Receiver)
    { }
}
