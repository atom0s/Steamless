/**
 * Steamless - Copyright (c) 2015 - 2023 atom0s [atom0s@live.com]
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

namespace Steamless.Model.Tasks
{
    using API.Events;
    using API.Model;
    using System.Collections.ObjectModel;
    using System.Linq;
    using System.Threading.Tasks;
    using System.Windows;
    using ViewModel;

    public class LoadPluginsTask : BaseTask
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        public LoadPluginsTask()
        {
            this.Text = "Loading plugins...";
        }

        /// <summary>
        /// The tasks main function to execute when started.
        /// </summary>
        public override Task DoTask()
        {
            return Task.Run(async () =>
                {
                    // Obtain the view model locator..
                    var vml = Application.Current.FindResource("ViewModelLocator") as ViewModelLocator;
                    if (vml == null)
                        return;

                    vml.MainWindow.SelectedPluginIndex = -1;

                    // Obtain the list of plugins..
                    var plugins = await vml.DataService.GetSteamlessPlugins();

                    // Sort the plugins..
                    var sorted = plugins.OrderBy(p => p.Name).ToList();

                    // Print out the loaded plugins..
                    sorted.ForEach(p =>
                        {
                            Application.Current.Dispatcher.Invoke(() =>
                                {
                                    vml.LoggingService.OnAddLogMessage(this, new LogMessageEventArgs($"Loaded plugin: {p.Name} - by {p.Author} (v.{p.Version})", LogMessageType.Success));
                                });
                        });

                    // Add the automatic plugin at the start of the list..
                    var auto = new AutomaticPlugin();
                    auto.Initialize(vml.LoggingService);
                    sorted.Insert(0, auto);

                    // Set the plugins..
                    vml.MainWindow.Plugins = new ObservableCollection<SteamlessPlugin>(sorted);
                    vml.MainWindow.SelectedPluginIndex = 0;
                });
        }
    }
}