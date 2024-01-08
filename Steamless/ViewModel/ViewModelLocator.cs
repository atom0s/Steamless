/**
 * Steamless - Copyright (c) 2015 - 2024 atom0s [atom0s@live.com]
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
    using API.Services;
    using GalaSoft.MvvmLight.Ioc;
    using Microsoft.Practices.ServiceLocation;
    using Model;

    public class ViewModelLocator
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        static ViewModelLocator()
        {
            // Setup the locator provider..
            ServiceLocator.SetLocatorProvider(() => SimpleIoc.Default);

            // Register our types..
            SimpleIoc.Default.Register<IDataService, DataService>();
            SimpleIoc.Default.Register<LoggingService>();
            SimpleIoc.Default.Register<MainWindowViewModel>();
        }

        /// <summary>
        /// Gets the main window view model.
        /// </summary>
        public MainWindowViewModel MainWindow => ServiceLocator.Current.GetInstance<MainWindowViewModel>();

        /// <summary>
        /// Gets the main data service.
        /// </summary>
        public IDataService DataService => ServiceLocator.Current.GetInstance<IDataService>();

        /// <summary>
        /// Gets the logging service.
        /// </summary>
        public LoggingService LoggingService => ServiceLocator.Current.GetInstance<LoggingService>();
    }
}