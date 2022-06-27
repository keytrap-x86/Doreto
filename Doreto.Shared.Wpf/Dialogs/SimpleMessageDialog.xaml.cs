using Prism.Services.Dialogs;

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Doreto.Shared.Wpf.Dialogs;

/// <summary>
/// Interaction logic for SimpleMessageDialog.xaml
/// </summary>
public partial class SimpleMessageDialog : UserControl, IDialogWindow
{
    public SimpleMessageDialog()
    {
        InitializeComponent();
    }

    public void Close()
    {
        
    }

    public void Show()
    {
        
    }

    public bool? ShowDialog()
    {
        return true;
    }

    public Window Owner { get; set; }
    public IDialogResult Result { get; set; }

    public event EventHandler Closed;
    public event CancelEventHandler Closing;
}
