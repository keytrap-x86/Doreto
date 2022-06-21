
using NetCoreServer;

namespace Doreto.Shared.Tcp;

class ClientSession : TcpSession
{
    public ClientSession(TcpServer server) : base(server) { }
}
