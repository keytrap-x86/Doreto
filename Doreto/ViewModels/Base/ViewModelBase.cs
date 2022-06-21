
using System;

using Prism.Mvvm;
using Prism.Regions;

namespace Doreto.ViewModels.Base;

/// <summary>
///     Base class for all view models.
/// </summary>
public class ViewModelBase : BindableBase, IConfirmNavigationRequest
{
    public virtual void ConfirmNavigationRequest(NavigationContext navigationContext, Action<bool> continuationCallback)
    {
        continuationCallback(true);
    }

    public virtual bool IsNavigationTarget(NavigationContext navigationContext)
    {
        return true;
    }

    public virtual void OnNavigatedFrom(NavigationContext navigationContext)
    {

    }

    public virtual void OnNavigatedTo(NavigationContext navigationContext)
    {

    }
}
