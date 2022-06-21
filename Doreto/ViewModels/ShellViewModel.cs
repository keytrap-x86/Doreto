
using System;
using System.Threading.Tasks;
using System.Windows;

using Doreto.Shared.Services.Proxy;
using Doreto.Shared.Wpf.Controls;
using Doreto.ViewModels.Base;

using Microsoft.Extensions.Logging;

using Prism.Commands;

namespace Doreto.ViewModels;

public class ShellViewModel : ViewModelBase
{
    private readonly ILogger<ShellViewModel> _logger;

    private bool _isDialogShown;
    public bool IsDialogShown
    {
        get { return _isDialogShown; }
        set { SetProperty(ref _isDialogShown, value); }
    }

    private UIElement _infoSpace;
    public UIElement InfoSpace
    {
        get { return _infoSpace; }
        set { SetProperty(ref _infoSpace, value); }
    }

    #region Commands

    private DelegateCommand _showDialog;
    public DelegateCommand ShowDialog => _showDialog ??= new DelegateCommand(ExecuteCommandName);

    
    #endregion

    public ShellViewModel(ILogger<ShellViewModel> logger)
    {
        _logger = logger;

        InfoSpace = new YouHaveNoCharacters();
        //Capture();
    }

    private async void Capture()
    {
        var dofusProxyServer = new DofusProxyServer();
        dofusProxyServer.NewAutoLoginInfoReceived += (o, autoLoginInfo) => _logger.LogInformation("{al}", autoLoginInfo);
        dofusProxyServer.Start();

        await Task.Delay(TimeSpan.FromSeconds(10));

        dofusProxyServer.StopAndDispose();
    }


    private void ExecuteCommandName()
    {
        IsDialogShown = true;
    }
}
