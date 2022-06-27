using System.Net;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;

using Doreto.Shared.Models.Proxy;

using Microsoft.Extensions.Logging;

using Titanium.Web.Proxy;
using Titanium.Web.Proxy.Models;

namespace Doreto.Shared.Services.Proxy;

public class DofusProxyServer
{
    public event EventHandler<AutoLoginInfo> NewAutoLoginInfoReceived;
    private readonly ProxyServer _proxyServer;
    private readonly ILogger<DofusProxyServer> _logger;

    public DofusProxyServer(ILogger<DofusProxyServer> logger)
    {
        _logger = logger;
        _proxyServer = new ProxyServer();

    }

    public void Start()
    {
        _logger.LogDebug("Starting proxy server");

        _proxyServer.CertificateManager.LoadRootCertificate(Path.Combine(AppContext.BaseDirectory, "proxy.pfx"),
            "*", false, X509KeyStorageFlags.EphemeralKeySet);
        _proxyServer.BeforeRequest += DofusProxyServer_BeforeRequest;

        _proxyServer.Start();
        var ep = new ExplicitProxyEndPoint(IPAddress.Any, 8851, true);
        ep.BeforeTunnelConnectRequest += Ep_BeforeTunnelConnectRequest;
        _proxyServer.AddEndPoint(ep);
        _proxyServer.SetAsSystemHttpsProxy(ep);
    }

    private Task Ep_BeforeTunnelConnectRequest(object sender, Titanium.Web.Proxy.EventArguments.TunnelConnectSessionEventArgs e)
    {
        // Filter out all requests that are not for the Dofus haapi server
        e.DecryptSsl = e.HttpClient.Request.Host.Contains("haapi.ankama.com", StringComparison.InvariantCultureIgnoreCase);
        return Task.CompletedTask;
    }

    public void Stop()
    {
        _proxyServer.Stop();
    }

    public void StopAndDispose()
    {
        _proxyServer.Stop();
        _proxyServer.Dispose();
        _logger.LogDebug("Stopped and disposed proxy server");
    }

    private Task DofusProxyServer_BeforeRequest(object sender, Titanium.Web.Proxy.EventArguments.SessionEventArgs e)
    {
        // Filter out all requests that don't match the following regex pattern
        var regexMatch = Regex.Match(e.HttpClient.Request.RequestUri.Query, @"game=101&certificate_id=([0-9]+)&certificate_hash=([a-z0-9]+)");
        if (!regexMatch.Success)
        {
            return Task.CompletedTask;
        }

        // Extract the certificate id and hash from the url
        var certificateId = regexMatch.Groups[1].Value;
        var certificateHash = regexMatch.Groups[2].Value;

        // Extract the apikey from headers
        var apiKey = e.HttpClient.Request.Headers.Headers["apikey"].Value;

        var autoLoginInfo = new AutoLoginInfo(certificateId, certificateHash, apiKey);
        NewAutoLoginInfoReceived?.Invoke(this, autoLoginInfo);

        return Task.CompletedTask;
    }


}
