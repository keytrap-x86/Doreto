using Prism.Commands;

namespace Doreto.Shared.Commands;

public interface IApplicationCommands
{
    /// <summary>
    ///     Makes the content region navigate to a view
    /// </summary>
    CompositeCommand NavigateContentCommand { get; }

    CompositeCommand ShowNotificationDialog { get; }
}

public class ApplicationCommands : IApplicationCommands
{
    public CompositeCommand NavigateContentCommand { get; } = new CompositeCommand();
    public CompositeCommand ShowNotificationDialog { get; } = new CompositeCommand();
}
