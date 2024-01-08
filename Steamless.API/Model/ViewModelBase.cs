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

namespace Steamless.API.Model
{
    using System;
    using System.ComponentModel;
    using System.Windows;

    public abstract class ViewModelBase : NotifiableModel
    {
        /// <summary>
        /// Default Constructor
        /// </summary>
        protected ViewModelBase()
        {
            this.Events = new EventHandlerList();
        }

        /// <summary>
        /// Internal static design mode flag.
        /// </summary>
        private static bool? m_IsInDesignMode;

        /// <summary>
        /// Gets if this ViewModelBase is in design mode.
        /// </summary>
        public bool IsInDesignMode => IsInDesignModeStatic;

        /// <summary>
        /// Gets the static ViewModelBase design mode flag.
        /// </summary>
        public static bool IsInDesignModeStatic
        {
            get
            {
                if (m_IsInDesignMode.HasValue)
                    return m_IsInDesignMode.Value;

                var isInDesignModeProperty = DesignerProperties.IsInDesignModeProperty;
                m_IsInDesignMode = (bool)DependencyPropertyDescriptor.FromProperty(isInDesignModeProperty, typeof(FrameworkElement)).Metadata.DefaultValue;
                return m_IsInDesignMode.Value;
            }
        }

        /// <summary>
        /// Gets or sets the internal event handler list.
        /// </summary>
        private EventHandlerList Events
        {
            get => this.Get<EventHandlerList>("Events");
            set => this.Set("Events", value);
        }

        /// <summary>
        /// Event to subscribe to to be notified when a view is navigated from.
        /// </summary>
        public event EventHandler<NavigatedEventArgs> NavigatedFrom
        {
            add => this.Events.AddHandler("NavigatedFromEvent", value);
            remove => this.Events.RemoveHandler("NavigatedFromEvent", value);
        }

        /// <summary>
        /// Event to subscribe to to be notified when a view is navigated to.
        /// </summary>
        public event EventHandler<NavigatedEventArgs> NavigatedTo
        {
            add => this.Events.AddHandler("NavigatedToEvent", value);
            remove => this.Events.RemoveHandler("NavigatedToEvent", value);
        }

        /// <summary>
        /// Internal navigated from event invoker.
        /// </summary>
        /// <param name="e"></param>
        public void OnNavigatedFrom(NavigatedEventArgs e)
        {
            var eventHandler = (EventHandler<NavigatedEventArgs>)this.Events["NavigatedFromEvent"];
            eventHandler?.Invoke(this, e);
        }

        /// <summary>
        /// Internal navigated to event invoker.
        /// </summary>
        /// <param name="e"></param>
        public void OnNavigatedTo(NavigatedEventArgs e)
        {
            var eventHandler = (EventHandler<NavigatedEventArgs>)this.Events["NavigatedToEvent"];
            eventHandler?.Invoke(this, e);
        }
    }
}