using System.Configuration;
using System.IO;
using System.Runtime;
using System.Windows;

using Doreto.Shared.Commands;
using Doreto.Shared.DofusClient;
using Doreto.Shared.Models;
using Doreto.Shared.Services.Proxy;
using Doreto.Shared.Wpf.Dialogs;
using Doreto.Shared.Wpf.ViewModels;
using Doreto.Views;

using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

using Example;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

using Prism.DryIoc;
using Prism.Ioc;
using Prism.Modularity;

using RegisterNewCharacterModule;

using Serilog;

namespace Doreto;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override Window CreateShell()
    {
        // Configure Serilog and the sinks at the startup of the app
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .Enrich.FromLogContext()
            .WriteTo.File(path: $"{nameof(Doreto.App)}.log")
            .WriteTo.Debug(Serilog.Events.LogEventLevel.Verbose)
            .CreateLogger();

        return Container.Resolve<ShellView>();
    }

    /// <summary>
    ///     Register types in container
    /// </summary>
    /// <param name="containerRegistry"></param>
    protected override void RegisterTypes(IContainerRegistry containerRegistry)
    {
        containerRegistry.RegisterSingleton<IApplicationCommands, ApplicationCommands>();
        containerRegistry.RegisterForNavigation<SimpleMessageDialog>();
        containerRegistry.RegisterDialog<SimpleMessageDialog, SimpleDialogViewModel>();
        containerRegistry.RegisterSingleton<DofusProxyServer>();
        containerRegistry.Register<CharacterRegistrator>();
    }

    /// <summary>
    ///     Add modules
    /// </summary>
    /// <param name="moduleCatalog"></param>
    protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
    {
        moduleCatalog.AddModule<Modules.RegisterNewCharacterModule>();
    }


    /// <summary>
    ///     Add services to the container
    /// </summary>
    /// <returns></returns>
    protected override IContainerExtension CreateContainerExtension()
    {
        var services = new ServiceCollection();
        services.AddLogging(loggingBuilder =>
            loggingBuilder.AddSerilog(dispose: true));


        var builder = new ConfigurationBuilder();
        builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true);

        return new DryIocContainerExtension(new Container(CreateContainerRules())
            .WithDependencyInjectionAdapter(services));
    }
}
