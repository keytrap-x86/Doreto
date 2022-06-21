using Doreto.Core.Models;

namespace Doreto.Core.Services.Storages;

public class DatacenterStorage
{
    public IReadOnlyCollection<DofusServer> DofusServers { get; }
    public DatacenterStorage()
    {
        DofusServers = new List<DofusServer>
        {
            new DofusServer("1", "Auth", "172.65.206.193", ServerTypes.AuthServer),
            new DofusServer("601", "172.65.214.172", "Eratz", ServerTypes.GameServer),
            new DofusServer("602", "172.65.242.55", "Henual", ServerTypes.GameServer),
            new DofusServer("612", "172.65.226.26", "Boune", ServerTypes.GameServer),
            new DofusServer("613", "172.65.230.220", "Crail", ServerTypes.GameServer),
            new DofusServer("614", "172.65.204.203", "Galgarion", ServerTypes.GameServer),
            new DofusServer("0", "192.168.1.84" ,"Dev", ServerTypes.GameServer | ServerTypes.AuthServer)
        };
    }

    /// <summary>
    ///     Path to the DLL that will control the Dofus client
    /// </summary>
    public static readonly string DoretroInteropPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Doretro.Interop.dll");
}
