using System;
using System.Windows;

namespace Doreto.Shared.Wpf.Behaviors;

/// <summary>
///     Class forn animation double properties (for example grid height, width...)
/// </summary>
public class AnimatedDoubleBehavior
{
    public static double GetAnimatedHeight(DependencyObject obj)
    {
        return (double)obj.GetValue(AnimatedHeightProperty);
    }

    public static void SetAnimatedHeight(DependencyObject obj, double value)
    {

        obj.SetValue(AnimatedHeightProperty, value);
    }

    public static readonly DependencyProperty AnimatedHeightProperty =
             DependencyProperty.RegisterAttached("AnimatedHeight", typeof(double), typeof(AnimatedDoubleBehavior), new UIPropertyMetadata(0d, PropertyChangedCallBack));

    private static void PropertyChangedCallBack(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        FrameworkElement sender = d as FrameworkElement;
        sender.Height = (double)e.NewValue;
        Console.WriteLine(e.NewValue);
    }
}
