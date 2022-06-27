using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;

using RegisterNewCharacterModule.Views;

namespace Doreto.Modules;

public class RegisterNewCharacterModule : IModule
{
    private readonly IRegionManager _regionManager;

    public RegisterNewCharacterModule(IRegionManager regionManager)
    {
        _regionManager = regionManager;
    }

    public void OnInitialized(IContainerProvider containerProvider)
    {
       
    }

    public void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterForNavigation<RegisterNewCharacterView>();
    }
}