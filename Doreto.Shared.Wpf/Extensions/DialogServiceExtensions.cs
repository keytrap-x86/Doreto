using Doreto.Shared.Wpf.Dialogs;

using Prism.Services.Dialogs;

using System;

namespace Doreto.Shared.Wpf.Extensions;

public static class DialogServiceExtensions
{
    public static void ShowNotification(this IDialogService dialogService, string message, Action<IDialogResult> callBack)
    {
        dialogService.ShowDialog(nameof(SimpleMessageDialog), new DialogParameters($"message={message}"), callBack);
    }
}
