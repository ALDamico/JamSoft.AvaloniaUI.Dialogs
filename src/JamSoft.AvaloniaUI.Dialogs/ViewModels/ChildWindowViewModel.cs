﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;

namespace JamSoft.AvaloniaUI.Dialogs.ViewModels;

/// <summary>
/// The default child window view model
/// </summary>
public class ChildWindowViewModel : DialogViewModel, IChildWindowViewModel
{
    private double _requestedTop;
    private double _requestedLeft;
    private double _requestedWidth;
    private double _requestedHeight;
    private string? _childWindowTitle;
    private IImage _closeIcon;

    public ChildWindowViewModel()
    {
        var assets = AvaloniaLocator.Current.GetService<IAssetLoader>();
        _closeIcon = new Bitmap(assets?.Open(new Uri("avares://JamSoft.AvaloniaUI.Dialogs/Assets/CloseIcon/icons8-close-30.png")));
    }

    /// <summary>
    /// The child window title
    /// </summary>
    public string? ChildWindowTitle
    {
        get => _childWindowTitle;
        set => RaiseAndSetIfChanged(ref _childWindowTitle, value);
    }

    /// <summary>
    /// The child window requested top value
    /// </summary>
    public double RequestedTop
    {
        get => _requestedTop;
        set => RaiseAndSetIfChanged(ref _requestedTop, value);
    }

    /// <summary>
    /// The child window requested left value
    /// </summary>
    public double RequestedLeft
    {
        get => _requestedLeft;
        set => RaiseAndSetIfChanged(ref _requestedLeft, value);
    }
    
    /// <summary>
    /// The child window width
    /// </summary>
    public double RequestedWidth
    {
        get => _requestedWidth;
        set => RaiseAndSetIfChanged(ref _requestedWidth, value);
    }

    /// <summary>
    /// The child window height
    /// </summary>
    public double RequestedHeight
    {
        get => _requestedHeight;
        set => RaiseAndSetIfChanged(ref _requestedHeight, value);
    }

    /// <summary>
    /// The child window startup location
    /// </summary>
    public WindowStartupLocation Location { get; set; }

    public IImage CloseIcon
    {
        get => _closeIcon;
        set => RaiseAndSetIfChanged(ref _closeIcon, value);
    }
}