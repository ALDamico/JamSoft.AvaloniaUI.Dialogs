﻿using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using JamSoft.AvaloniaUI.Dialogs.ViewModels;
using JamSoft.AvaloniaUI.Dialogs.Views;

namespace JamSoft.AvaloniaUI.Dialogs;

/// <summary>
/// The dialog service
/// </summary>
internal class DialogService : IDialogService
{
    private readonly DialogServiceConfiguration _config;
    private string? _lastDirectorySelected;
    private readonly HashSet<IChildWindowViewModel?> _openChildren = new();
    
    /// <summary>
    /// Default constructor
    /// </summary>
    /// <param name="config"></param>
    public DialogService(DialogServiceConfiguration config)
    {
        _config = config;
    }
    
    /// <summary>
    /// Shows a dialog with a callback to return the view model based on the result of the dialog.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <param name="viewModel">The view model.</param>
    /// <param name="callback">The callback.</param>
    public void ShowDialog<TViewModel>(TViewModel viewModel, Action<TViewModel> callback)
        where TViewModel : IDialogViewModel
    {
        Control view;

        if (string.IsNullOrWhiteSpace(_config.ViewsAssemblyName))
            throw new ArgumentNullException(nameof(_config.ViewsAssemblyName),
                "You must set the assembly name containing your views in the DialogServiceConfiguration instance");
        
        var name = !string.IsNullOrWhiteSpace(_config.ViewsAssemblyName) 
            ? $"{viewModel.GetType().FullName!.Replace("ViewModel", "View")},{_config.ViewsAssemblyName}" 
            : "";
        
        var type = Type.GetType(name);

        if (type != null)
        {
            view = (Control)Activator.CreateInstance(type)!;

            if (view != null)
            {
                ShowDialog(view, viewModel, callback);
            }
            else
            {
                throw new ArgumentNullException($"Could not find a view with name {name}");
            }
        }
    }
    
    /// <summary>
    /// Shows a dialog with a callback to return the view model based on the result of the dialog.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <param name="view">The view.</param>
    /// <param name="viewModel">The view model.</param>
    /// <param name="callback">The callback.</param>
    public async void ShowDialog<TViewModel, TView>(TView view, TViewModel viewModel, Action<TViewModel> callback)
        where TView : Control where TViewModel : IDialogViewModel
    {
        var win = new DialogWindow
        {
            WindowStartupLocation = WindowStartupLocation.CenterOwner
        };
        
        var contentControl = win.FindControl<ContentControl>("Host");
        contentControl.Content = view;
        win.DataContext = viewModel;
        
        var accept = await win.ShowDialog<bool>(GetActiveWindowOrMainWindow());
        if (accept)
        {
            callback(viewModel);
        }
    }

    /// <summary>
    /// Shows a child window.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <param name="viewModel">The view model.</param>
    /// <param name="callback">the callback to received the view model instance on close</param>
    public void ShowChildWindow<TViewModel>(TViewModel viewModel, Action<TViewModel>? callback)
        where TViewModel : IChildWindowViewModel
    {
        Control view;

        if (string.IsNullOrWhiteSpace(_config.ViewsAssemblyName))
            throw new ArgumentNullException(nameof(_config.ViewsAssemblyName),
                "You must set the assembly name containing your views in the DialogServiceConfiguration instance");
        
        var name = !string.IsNullOrWhiteSpace(_config.ViewsAssemblyName) 
            ? $"{viewModel.GetType().FullName!.Replace("ViewModel", "View")},{_config.ViewsAssemblyName}" 
            : "";
        
        var type = Type.GetType(name);

        if (type != null)
        {
            view = (Control)Activator.CreateInstance(type)!;

            if (view != null)
            {
                ShowChildWindow(view, viewModel, callback);
            }
            else
            {
                throw new ArgumentNullException($"Could not find a view with name {name}");
            }
        }        
    }
    
    /// <summary>
    /// Shows a child window.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model.</typeparam>
    /// <typeparam name="TView">The type of the view.</typeparam>
    /// <param name="view">The view.</param>
    /// <param name="viewModel">The view model.</param>
    /// <param name="callback">the callback to received the view model instance on close</param>
    public void ShowChildWindow<TViewModel, TView>(TView view, TViewModel viewModel, Action<TViewModel>? callback)
        where TView : Control where TViewModel : IChildWindowViewModel
    {
        // prevent multiple instances of the same child window
        if (_openChildren.FirstOrDefault(x => x?.GetType() == typeof(TViewModel)) != null)
            return;

        var win = new ChildWindow();

        viewModel.ChildWindowTitle = CreateTitle(viewModel.ChildWindowTitle);
        
        var contentControl = win.FindControl<ContentControl>("Host");
        contentControl.Content = view;
        win.DataContext = viewModel;

        _openChildren.Add(viewModel);
        win.Closing += (sender, args) =>
        {
            if (sender is ChildWindow window)
            {
                _openChildren.Remove(viewModel);
            }
            
            if (callback != null)
                callback(viewModel);
        };
        win.Show();
    }

    /// <summary>
    /// Launches a system folder dialog so the user can pick a system folder on disk.
    /// </summary>
    /// <param name="title">The dialog title</param>
    /// <param name="startDirectory">the root directory to browse</param>
    /// <returns>the selected folder path or null if the dialog was cancelled</returns>
    public async Task<string?> OpenFolder(string? title, string? startDirectory = null)
    {
        var fd = new OpenFolderDialog
        {
            Directory = startDirectory ?? _lastDirectorySelected,
            Title = CreateTitle(title)
        };

        var path = await fd.ShowAsync(GetActiveWindowOrMainWindow());

        _lastDirectorySelected = path!;

        return path;
    }
    
    /// <summary>
    /// Gets a path for a new file
    /// </summary>
    /// <param name="title">The dialog title</param>
    /// <param name="filters">The file extension filters</param>
    /// <param name="defaultExtension">The default file extension</param>
    /// <returns>the selected file path or null if the dialog was cancelled</returns>
    public async Task<string?> SaveFile(string title, IEnumerable<FileDialogFilter>? filters = null, string? defaultExtension = null)
    {
        var fd = new SaveFileDialog
        {
            Title = CreateTitle(title),
            Filters = filters?.ToList(),
            Directory = _lastDirectorySelected,
            DefaultExtension = defaultExtension
        };

        return await fd.ShowAsync(GetActiveWindowOrMainWindow());
    }

    /// <summary>
    /// The an individual file path
    /// </summary>
    /// <param name="title">The dialog title</param>
    /// <param name="filters">The file extension filters</param>
    /// <returns>the selected file path or null if the dialog was cancelled</returns>
    public async Task<string?> OpenFile(string title, IEnumerable<FileDialogFilter>? filters = null)
    {
        var paths = await OpenFile(title, false, filters);
        if (paths != null && paths.Any())
        {
            return paths[0];
        }

        return null;
    }
    
    /// <summary>
    /// Returns multiple existing file paths
    /// </summary>
    /// <param name="title">The dialog title</param>
    /// <param name="filters">The file extension filters</param>
    /// <returns>the selected file paths or null if the dialog was cancelled</returns>
    public async Task<string[]?> OpenFiles(string title, IEnumerable<FileDialogFilter>? filters = null)
    {
        var paths = await OpenFile(title, true, filters);
        if (paths != null && paths.Any())
        {
            return paths;
        }

        return null;
    }
    
    private async Task<string[]?> OpenFile(string title, bool allowMulti, IEnumerable<FileDialogFilter>? filters = null)
    {
        var fd = new OpenFileDialog
        {
            Title = CreateTitle(title),
            AllowMultiple = allowMulti,
            Filters = filters?.ToList(),
            Directory = _lastDirectorySelected
        };

        var paths = await fd.ShowAsync(GetActiveWindowOrMainWindow());
        if (paths != null && paths.Any())
        {
            return paths;
        }

        return null;
    }
    
    private Window GetActiveWindowOrMainWindow()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.Windows.SingleOrDefault(x => x.IsActive) ?? desktop.MainWindow;
        }

        throw new InvalidOperationException("Cannot find a Window when ApplicationLifetime is not ClassicDesktopStyleApplicationLifetime");
    }

    private string? CreateTitle(string? title)
    {
        if (_config.UseApplicationNameInTitle)
            return $"{_config.ApplicationName}-{title}";

        return title;
    }
}