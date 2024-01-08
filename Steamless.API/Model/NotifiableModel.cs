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
    using System.Collections.Generic;
    using System.ComponentModel;

    [Serializable]
    public class NotifiableModel : INotifyPropertyChanged
    {
        /// <summary>
        /// Internal properties container.
        /// </summary>
        private readonly Dictionary<string, object> m_Properties;

        /// <summary>
        /// Default Constructor
        /// </summary>
        public NotifiableModel()
        {
            this.m_Properties = new Dictionary<string, object>();
        }

        /// <summary>
        /// Event triggered when a property is changed.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Method used to raise the PropertyChanged event.
        /// </summary>
        /// <param name="prop"></param>
        public void OnPropertyChanged(string prop)
        {
            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(prop));
        }

        /// <summary>
        /// Method to raise the PropertyChanged event.
        /// </summary>
        /// <param name="property"></param>
        protected void RaisePropertyChanged(string property)
        {
            if (string.IsNullOrEmpty(property))
                throw new ArgumentNullException(property);
            this.OnPropertyChanged(property);
        }

        /// <summary>
        /// Gets a property from the internal container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <returns></returns>
        protected T Get<T>(string prop)
        {
            if (this.m_Properties.ContainsKey(prop))
            {
                return (T)this.m_Properties[prop];
            }
            return default(T);
        }

        /// <summary>
        /// Sets a property in the internal container.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="prop"></param>
        /// <param name="val"></param>
        protected void Set<T>(string prop, T val)
        {
            var curr = this.Get<T>(prop);
            if (Equals(curr, val))
                return;

            this.m_Properties[prop] = val;
            this.OnPropertyChanged(prop);
        }
    }
}