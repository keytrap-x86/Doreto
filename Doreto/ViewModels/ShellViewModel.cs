
using Doreto.Shared.Commands;
using Doreto.Shared.Services.Proxy;
using Doreto.Shared.Wpf.ViewModels.Base;

using Microsoft.Extensions.Logging;

using Prism.Commands;
using Prism.Regions;

using System;
using System.Threading.Tasks;

namespace Doreto.ViewModels;

public class ShellViewModel : ViewModelBase
{
    private readonly ILogger<ShellViewModel> _logger;
    private readonly IRegionManager _regionManager;
    private readonly IApplicationCommands _applicationCommands;
    private bool _isDialogShown;

    private DelegateCommand _showDialog;
    private DelegateCommand<object> _showNotificationDialogCommand;

    
    #region Public properties

    public bool IsDialogShown
    {
        get => _isDialogShown;
        set => SetProperty(ref _isDialogShown, value);
    }

    #endregion




    #region Commands



    public DelegateCommand ShowDialog => _showDialog ??= new DelegateCommand(ExecuteCommandName);

    /// <summary>
    ///     Main navigation command
    /// </summary>
    public DelegateCommand<object> ShowNotificationDialogCommand =>
        _showNotificationDialogCommand ??= new DelegateCommand<object>(ExecuteShowDialogCommand);

    


    #endregion

    /// <summary>
    ///     Constructor
    /// </summary>
    /// <param name="logger"></param>
    public ShellViewModel(ILogger<ShellViewModel> logger, IRegionManager regionManager, IApplicationCommands applicationCommands)
    {
        _logger = logger;
        _regionManager = regionManager;
        _applicationCommands = applicationCommands;
        _applicationCommands.ShowNotificationDialog.RegisterCommand(ShowNotificationDialogCommand);
    }


    
    private void ExecuteCommandName()
    {
        //IsDialogShown = true;
        _regionManager.RequestNavigate("InfosRegion", "RegisterNewCharacterView");
    }

    private void ExecuteShowDialogCommand(object obj)
    {
        IsDialogShown = true;
        _regionManager.RequestNavigate("OverlayDialog", "SimpleMessageDialog");
    }
}
