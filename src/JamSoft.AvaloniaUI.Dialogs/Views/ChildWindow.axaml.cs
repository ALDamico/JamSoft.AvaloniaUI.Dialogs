﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using JamSoft.AvaloniaUI.Dialogs.Events;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;

namespace JamSoft.AvaloniaUI.Dialogs.Views;

public partial class ChildWindow : Window
{
    private bool _isClosed = false;

    private ChildWindowViewModel? _vm { get; set; }
    
    public ChildWindow()
    {
        InitializeComponent();
// #if DEBUG
//         this.AttachDevTools();
// #endif
        PointerPressed += OnPointerPressed;

        this.FindControl<ContentControl>("Host").DataContextChanged += DialogPresenterDataContextChanged;
        Closed += DialogWindowClosed;
        PositionChanged += OnPositionChanged;
    }

    private void OnPositionChanged(object? sender, PixelPointEventArgs e)
    {
        if (_vm == null) return;
        
        _vm.RequestedLeft = e.Point.X;
        _vm.RequestedTop = e.Point.Y;
    }
    
    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (_vm == null) return;
        
        var p = e.GetCurrentPoint(null);
        if (p.Properties.IsLeftButtonPressed)
        {
            ClientSizeProperty.Changed.Subscribe(size =>
            {
                if (ReferenceEquals(size.Sender, this))
                {
                    _vm.RequestedLeft = Position.X;
                    _vm.RequestedTop = Position.Y;
                }
            });

            BeginMoveDrag(e);
            e.Handled = false;
        }
    }
    
    void DialogWindowClosed(object? sender, EventArgs e)
    {
        PointerPressed -= OnPointerPressed;
        PositionChanged -= OnPositionChanged;
        _isClosed = true;
    }
    
    private void DialogPresenterDataContextChanged(object? sender, EventArgs e)
    {
        _vm = DataContext as ChildWindowViewModel;
        var dialogResultVmHelper = DataContext as IDialogResultVmHelper;
        var windowPositionAware = DataContext as IWindowPositionAware;

        if (dialogResultVmHelper == null)
        {
            return;
        }

        dialogResultVmHelper.RequestCloseDialog += new EventHandler<RequestCloseDialogEventArgs>(
            DialogResultTrueEvent).MakeWeak(eh => dialogResultVmHelper.RequestCloseDialog -= eh);
		
        if (windowPositionAware == null) return;
		
        Position = new PixelPoint(
            Convert.ToInt32(windowPositionAware.RequestedLeft), 
            Convert.ToInt32(windowPositionAware.RequestedTop));
    }
    
    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
    
    private void DialogResultTrueEvent(object? sender, RequestCloseDialogEventArgs eventargs)
    {
        // Important: Do not set DialogResult for a closed window
        // GC clears windows anyway and with MakeWeak it
        // closes out with IDialogResultVMHelper
        if (_isClosed)
        {
            return;
        }

        Close();
    }
}