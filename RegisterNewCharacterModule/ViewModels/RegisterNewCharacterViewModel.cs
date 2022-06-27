using Doreto.Shared.Commands;
using Doreto.Shared.DofusClient;
using Doreto.Shared.Helpers;
using Doreto.Shared.Services.Proxy;
using Doreto.Shared.Wpf.Dialogs;
using Doreto.Shared.Wpf.Extensions;
using Doreto.Shared.Wpf.ViewModels.Base;

using Microsoft.Extensions.Logging;

using Prism.Commands;
using Prism.Regions;
using Prism.Services.Dialogs;

using System.Diagnostics;
using System.Threading.Tasks;

namespace RegisterNewCharacterModule.ViewModels
{
    public class RegisterNewCharacterViewModel : ViewModelBase
    {
        #region Private fields

        private bool _isDofusStarted;
        private bool _isCharacterConnected;
        private bool _hasWrittenHeyInChat;
        private readonly ILogger<RegisterNewCharacterViewModel> _logger;
        private readonly IDialogService _dialogService;
        private readonly CharacterRegistrator _registrator;
        private readonly IRegionManager _regionManager;
        private readonly DofusProxyServer _dofusProxyServer;
        private readonly IApplicationCommands _applicationCommands;

        #endregion

        #region Public properties

        /// <summary>
        ///     Is the Dofus client started.
        /// </summary>
        public bool IsDofusStarted
        {
            get => _isDofusStarted;
            set => SetProperty(ref _isDofusStarted, value);
        }


        /// <summary>
        ///     Is the character connected in game.
        /// </summary>
        public bool IsCharacterConnected
        {
            get => _isCharacterConnected;
            set => SetProperty(ref _isCharacterConnected, value);
        }

        /// <summary>
        ///     Has the user written and sent *Hey* in the chat.
        /// </summary>
        public bool HasWrittenHeyInChat
        {
            get => _hasWrittenHeyInChat;
            set => SetProperty(ref _hasWrittenHeyInChat, value);
        }

        #endregion

        #region Commands

        private DelegateCommand _startRegisteringCharacter;
        public DelegateCommand StartRegisteringCharacter =>
            _startRegisteringCharacter ??= new DelegateCommand(ExecuteStartRegisteringCharacter);



        #endregion

        #region Contructor

        public RegisterNewCharacterViewModel(
            ILogger<RegisterNewCharacterViewModel> logger,
            IDialogService dialogService,
            CharacterRegistrator registrator,
            IRegionManager regionManager,
            DofusProxyServer dofusProxyServer,
            IApplicationCommands applicationCommands)
        {
            _logger = logger;
            _dialogService = dialogService;
            _registrator = registrator;
            _regionManager = regionManager;
            _dofusProxyServer = dofusProxyServer;
            _applicationCommands = applicationCommands;
        }

        #endregion


        public async Task<Process> WaitForDofusRetroProcess()
        {
            using var dofusProcess = await _registrator.WaitForProcessToSpawn("Dofus Retro", "network.mojo");
            _logger.LogDebug("Process found: {pid}", dofusProcess.Id);
            return dofusProcess;
        }

        private async void ExecuteStartRegisteringCharacter()
        {
            if (CertificatesHelper.IsCertificateInstalled("Keytrap Developments") == false)
            {
                string message = "test";
                _applicationCommands.ShowNotificationDialog.Execute("Test");
                return;
            }

            StartDofusViaLauncher();

            var process = await WaitForDofusRetroProcess();
        }

        private static void StartDofusViaLauncher()
        {
            var startCommand = DofusHelper.GetLauncherStartCommand();
            var startInfo = new ProcessStartInfo
            {
                UseShellExecute = true,
                FileName = startCommand,
            };
            var process = new Process
            {
                StartInfo = startInfo
            };

            process.Start();
        }

    }
}
