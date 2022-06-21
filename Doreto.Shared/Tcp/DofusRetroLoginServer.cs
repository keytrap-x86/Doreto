
using System.Net.Sockets;
using System.Net;

using NetCoreServer;

namespace Doreto.Shared.Tcp;

class DofusRetroLoginServer : TcpServer
{
    public DofusRetroLoginServer(IPAddress address, int port) : base(address, port) { }

    protected override TcpSession CreateSession() { return new ClientSession(this); }

    protected override void OnError(SocketError error)
    {
        Console.WriteLine($"Chat TCP server caught an error with code {error}");
    }
}
