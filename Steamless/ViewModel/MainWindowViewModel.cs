/**
 * Steamless - Copyright (c) 2015 - 2019 atom0s [atom0s@live.com]
 *
 * This work is licensed under the Creative Commons Attribution-NonCommercial-NoDerivatives 4.0 International License.
 * To view a copy of this license, visit http://creativecommons.org/licenses/by-nc-nd/4.0/ or send a letter to
 * Creative Commons, PO Box 1866, Mountain View, CA 94042, USA.
 *
 * By using Steamless, you agree to the above license and its terms.
 *
 *      Attribution - You must give appropriate credit, provide a link to the license and indicate if changes were
 *                    made. You must do so in any reasonable manner, but not in any way that suggests the licensor
 *                    endorses you or your use.
 *
 *   Non-Commercial - You may not use the material (Steamless) for commercial purposes.
 *
 *   No-Derivatives - If you remix, transform, or build upon the material (Steamless), you may not distribute the
 *                    modified material. You are, however, allowed to submit the modified works back to the original
 *                    Steamless project in attempt to have it added to the original project.
 *
 * You may not apply legal terms or technological measures that legally restrict others
 * from doing anything the license permits.
 *
 * No warranties are given.
 */

namespace Steamless.ViewModel
{
    using API.Events;
    using API.Model;
    using API.Services;
    using GalaSoft.MvvmLight.Command;
    using Microsoft.Win32;
    using Model;
    using Model.Tasks;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.IO;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Windows;
    using System.Windows.Documents;
    using System.Windows.Input;

    public class MainWindowViewModel : ViewModelBase
    {
        /// <summary>
        /// Internal data service instance.
        /// </summary>
        private readonly IDataService m_DataService;

        /// <summary>
        /// Internal thread used to process tasks.
        /// </summary>
        private Thread m_TaskThread;

        /// <summary>
        /// Default Constructor
        /// </summary>
        /// <param name="dataService"></param>
        /// <param name="logService"></param>
        public MainWindowViewModel(IDataService dataService, LoggingService logService)
        {
            // Store the data service instance..
            this.m_DataService = dataService;

            // Initialize the model..
            this.State = ApplicationState.Initializing;
            this.Tasks = new ConcurrentBag<BaseTask>();
            this.Options = new SteamlessOptions();
            this.Log = new ObservableCollection<LogMessageEventArgs>();
            this.ShowAboutView = false;
            this.InputFilePath = string.Empty;

            // Register command callbacks..
            this.OnWindowCloseCommand = new RelayCommand(this.WindowClose);
            this.OnWindowMinimizeCommand = new RelayCommand(WindowMinimize);
            this.OnWindowMouseDownCommand = new RelayCommand<MouseButtonEventArgs>(WindowMouseDown);
            this.OnShowAboutViewCommand = new RelayCommand(() => this.ShowAboutView = !this.ShowAboutView);
            this.OnOpenHyperlinkCommand = new RelayCommand<object>(o =>
            {
                if (o is Hyperlink link)
                    Process.Start(link.NavigateUri.AbsoluteUri);
            });
            this.OnDragDropCommand = new RelayCommand<DragEventArgs>(this.InputFileDragDrop);
            this.OnPreviewDragEnterCommand = new RelayCommand<DragEventArgs>(this.InputFilePreviewDragEnter);
            this.OnBrowseForInputFileCommand = new RelayCommand(this.BrowseForInputFile);
            this.OnUnpackFileCommand = new RelayCommand(this.UnpackFile);
            this.OnClearLogCommand = new RelayCommand(() => this.ClearLogMessages(this, EventArgs.Empty));

            // Attach logging service events..
            logService.AddLogMessage += this.AddLogMessage;
            logService.ClearLogMessages += this.ClearLogMessages;

            this.AddLogMessage(this, new LogMessageEventArgs("Steamless (c) 2015 - 2019 atom0s [atom0s@live.com]", LogMessageType.Debug));
            this.AddLogMessage(this, new LogMessageEventArgs("Website: http://atom0s.com/", LogMessageType.Debug));

            // Initialize this model..
            this.Initialize();
        }

        /// <summary>
        /// Internal async call to load the main view model.
        /// </summary>
        private async void Initialize()
        {
            // Obtain the Steamless version..
            this.CurrentTask = new StatusTask("Initializing..");
            this.SteamlessVersion = await this.m_DataService.GetSteamlessVersion();

            // Load the Steamless plugins..
            this.Tasks.Add(new LoadPluginsTask());

            // Start the application..
            this.Tasks.Add(new StartSteamlessTask());

            // Start the tasks thread..
            if (this.m_TaskThread != null)
                return;
            this.m_TaskThread = new Thread(this.ProcessTasksThread) { IsBackground = true };
            this.m_TaskThread.Start();
        }

        /// <summary>
        /// Sets the applications current status.
        /// </summary>
        /// <param name="state"></param>
        /// <param name="msg"></param>
        public void SetApplicationStatus(ApplicationState state, string msg)
        {
            this.State = state;
            this.CurrentTask = new StatusTask(msg);
        }

        /// <summary>
        /// Thread callback to process application tasks.
        /// </summary>
        private async void ProcessTasksThread()
        {
            while (Interlocked.CompareExchange(ref this.m_TaskThread, null, null) != null && this.State != ApplicationState.Closing)
            {
                // Obtain a task from the task list..
                if (this.Tasks.TryTake(out var task))
                {
                    this.CurrentTask = task;
                    await this.CurrentTask.StartTask();
                }
                else
                {
                    // No tasks left, set application to a running state..
                    if (this.State == ApplicationState.Initializing)
                        this.State = ApplicationState.Running;
                }

                Thread.Sleep(200);
            }
        }

        /// <summary>
        /// Adds a message to the message log.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddLogMessage(object sender, LogMessageEventArgs e)
        {
            // Do not log debug messages if verbose output is disabled..
            if (!this.Options.VerboseOutput && e.MessageType == LogMessageType.Debug)
                return;

            // Check if we need to invoke from the dispatcher thread..
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => this.AddLogMessage(sender, e));
                return;
            }

            // Prefix the parent to the message..
            try
            {
                if (sender != null)
                {
                    var baseName = sender.GetType().Assembly.GetName().Name;
                    e.Message = $"[{baseName}] {e.Message}";
                }
                else
                    e.Message = "[Unknown] " + e.Message;
            }
            catch
            {
                // Do nothing with this exception..
            }

            this.Log.Add(e);
        }

        /// <summary>
        /// Clears the message log.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ClearLogMessages(object sender, EventArgs e)
        {
            // Check if we need to invoke from the dispatcher thread..
            if (!Application.Current.Dispatcher.CheckAccess())
            {
                Application.Current.Dispatcher.Invoke(() => this.ClearLogMessages(sender, e));
                return;
            }

            this.Log.Clear();
        }

        #region == Window Function Callbacks ==================================================================
        /// <summary>
        /// Command callback for when the window is being closed.
        /// </summary>
        private void WindowClose()
        {
            // Set the launcher state to closing..
            this.State = ApplicationState.Closing;

            // Shutdown the application..
            Application.Current.Shutdown(0);
        }

        /// <summary>
        /// Command callback for when the window is being minimized.
        /// </summary>
        private static void WindowMinimize()
        {
            // Minimize the window..
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.WindowState = WindowState.Minimized;
        }

        /// <summary>
        /// Command callback for when the window is being clicked down. (To drag the window.)
        /// </summary>
        /// <param name="args"></param>
        private static void WindowMouseDown(MouseButtonEventArgs args)
        {
            if (Application.Current.MainWindow != null)
                Application.Current.MainWindow.DragMove();
        }

        /// <summary>
        /// Handles drag and drop events over the input file textbox.
        /// </summary>
        /// <param name="args"></param>
        private void InputFileDragDrop(DragEventArgs args)
        {
            args.Handled = true;

            // Check for files being dragged..
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Ensure only 1 file is being dropped..
                var files = (string[])args.Data.GetData(DataFormats.FileDrop);
                if (files != null && files.Length >= 1)
                    this.InputFilePath = files[0];
            }
        }

        /// <summary>
        /// Handles drag and drop events over the input file textbox.
        /// </summary>
        /// <param name="args"></param>
        private void InputFilePreviewDragEnter(DragEventArgs args)
        {
            args.Handled = true;

            // Check for files being dragged..
            if (args.Data.GetDataPresent(DataFormats.FileDrop))
            {
                // Ensure only 1 file is being dropped..
                var files = (string[])args.Data.GetData(DataFormats.FileDrop);
                args.Effects = files != null && files.Length == 1 ? DragDropEffects.Move : DragDropEffects.None;
            }
            else
                args.Effects = DragDropEffects.None;
        }

        /// <summary>
        /// Browses for the input file to be unpacked.
        /// </summary>
        private void BrowseForInputFile()
        {
            // Display the find file dialog..
            var ofd = new OpenFileDialog
            {
                CheckFileExists = true,
                CheckPathExists = true,
                DefaultExt = "*.exe",
                Filter = "Executable Files (*.exe)|*.exe|All Files (*.*)|*.*",
                FilterIndex = 0,
                InitialDirectory = Path.GetFullPath(AppDomain.CurrentDomain.BaseDirectory),
                Multiselect = false,
                RestoreDirectory = true
            };

            // Update the input file path..
            var showDialog = ofd.ShowDialog();
            if (showDialog != null && (bool)showDialog)
                this.InputFilePath = ofd.FileName;
        }

        /// <summary>
        /// Unpacks the selected file using the selected plugin.
        /// </summary>
        private async void UnpackFile()
        {
            await Task.Run(() =>
            {
                // Validation checks..
                if (this.SelectedPluginIndex == -1)
                    return;
                if (this.SelectedPluginIndex > this.Plugins.Count)
                    return;
                if (string.IsNullOrEmpty(this.InputFilePath))
                    return;

                try
                {
                    // Select the plugin..
                    var plugin = this.Plugins[this.SelectedPluginIndex];
                    if (plugin == null)
                        throw new Exception("Invalid plugin selected.");

                    // Allow the plugin to process the file..
                    if (plugin.CanProcessFile(this.InputFilePath))
                        this.AddLogMessage(this, !plugin.ProcessFile(this.InputFilePath, this.Options) ? new LogMessageEventArgs("Failed to unpack file.", LogMessageType.Error) : new LogMessageEventArgs("Successfully unpacked file!", LogMessageType.Success));
                    else
                        this.AddLogMessage(this, new LogMessageEventArgs("Failed to unpack file.", LogMessageType.Error));
                }
                catch (Exception ex)
                {
                    this.AddLogMessage(this, new LogMessageEventArgs("Caught unhandled exception trying to unpack file.", LogMessageType.Error));
                    this.AddLogMessage(this, new LogMessageEventArgs("Exception:", LogMessageType.Error));
                    this.AddLogMessage(this, new LogMessageEventArgs(ex.Message, LogMessageType.Error));
                }
            });
        }
        #endregion

        #region == Window Related Properties ==================================================================
        /// <summary>
        /// Gets or sets the window close command.
        /// </summary>
        public RelayCommand OnWindowCloseCommand { get; set; }

        /// <summary>
        /// Gets or sets the window minimize command.
        /// </summary>
        public RelayCommand OnWindowMinimizeCommand { get; set; }

        /// <summary>
        /// Gets or sets the window mouse down command.
        /// </summary>
        public RelayCommand<MouseButtonEventArgs> OnWindowMouseDownCommand { get; set; }

        /// <summary>
        /// Gets or sets the show about view command.
        /// </summary>
        public RelayCommand OnShowAboutViewCommand { get; set; }

        /// <summary>
        /// Gets or sets the open hyperlink command.
        /// </summary>
        public RelayCommand<object> OnOpenHyperlinkCommand { get; set; }

        /// <summary>
        /// Gets or sets the input file textbox drag drop command.
        /// </summary>
        public RelayCommand<DragEventArgs> OnDragDropCommand { get; set; }

        /// <summary>
        /// Gets or sets the input file textbox drag enter command.
        /// </summary>
        public RelayCommand<DragEventArgs> OnPreviewDragEnterCommand { get; set; }

        /// <summary>
        /// Gets or sets the input file browse command.
        /// </summary>
        public RelayCommand OnBrowseForInputFileCommand { get; set; }

        /// <summary>
        /// Gets or sets the unpack file command.
        /// </summary>
        public RelayCommand OnUnpackFileCommand { get; set; }

        /// <summary>
        /// Gets or sets the clear log command.
        /// </summary>
        public RelayCommand OnClearLogCommand { get; set; }
        #endregion

        #region == (All) ViewModel Related Properties =========================================================
        /// <summary>
        /// Gets or sets the applications current state.
        /// </summary>
        public ApplicationState State
        {
            get => this.Get<ApplicationState>("State");
            set => this.Set("State", value);
        }

        /// <summary>
        /// Gets or sets the Steamless version.
        /// </summary>
        public Version SteamlessVersion
        {
            get => this.Get<Version>("SteamlessVersion");
            set => this.Set("SteamlessVersion", value);
        }

        /// <summary>
        /// Gets or sets the current task.
        /// </summary>
        public BaseTask CurrentTask
        {
            get => this.Get<BaseTask>("CurrentTask");
            set => this.Set("CurrentTask", value);
        }

        /// <summary>
        /// Gets or sets the list of tasks.
        /// </summary>
        public ConcurrentBag<BaseTask> Tasks
        {
            get => this.Get<ConcurrentBag<BaseTask>>("Tasks");
            set => this.Set("Tasks", value);
        }

        /// <summary>
        /// Gets or sets if the about view should be seen.
        /// </summary>
        public bool ShowAboutView
        {
            get => this.Get<bool>("ShowAboutView");
            set => this.Set("ShowAboutView", value);
        }
        #endregion

        #region == (Main) ViewModel Related Properties ========================================================
        /// <summary>
        /// Gets or sets the list of plugins.
        /// </summary>
        public ObservableCollection<SteamlessPlugin> Plugins
        {
            get => this.Get<ObservableCollection<SteamlessPlugin>>("Plugins");
            set => this.Set("Plugins", value);
        }

        /// <summary>
        /// Gets or sets the selected plugin index.
        /// </summary>
        public int SelectedPluginIndex
        {
            get => this.Get<int>("SelectedPluginIndex");
            set => this.Set("SelectedPluginIndex", value);
        }

        /// <summary>
        /// Gets or sets the input file path.
        /// </summary>
        public string InputFilePath
        {
            get => this.Get<string>("InputFilePath");
            set => this.Set("InputFilePath", value);
        }

        /// <summary>
        /// Gets or sets the Steamless options.
        /// </summary>
        public SteamlessOptions Options
        {
            get => this.Get<SteamlessOptions>("Options");
            set => this.Set("Options", value);
        }

        /// <summary>
        /// Gets or sets the message log.
        /// </summary>
        public ObservableCollection<LogMessageEventArgs> Log
        {
            get => this.Get<ObservableCollection<LogMessageEventArgs>>("Log");
            set => this.Set("Log", value);
        }
        #endregion
    }
}