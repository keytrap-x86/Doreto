using System.Text;
using System.Windows;

using Doreto.Views;

using DryIoc;
using DryIoc.Microsoft.DependencyInjection;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using NLog.Extensions.Logging;

using Prism.DryIoc;
using Prism.Ioc;

using Serilog;

namespace Doreto
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        /// <summary>
        ///     Register types in container
        /// </summary>
        /// <param name="containerRegistry"></param>
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
           
        }

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

        protected override IContainerExtension CreateContainerExtension()
        {
            var serviceCollection = new ServiceCollection();
            serviceCollection.AddLogging(loggingBuilder =>
                loggingBuilder.AddSerilog(dispose: true));

            return new DryIocContainerExtension(new Container(CreateContainerRules())
                .WithDependencyInjectionAdapter(serviceCollection));
        }
    }
}
