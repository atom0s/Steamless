﻿/**
 * Steamless - Copyright (c) 2015 - 2018 atom0s [atom0s@live.com]
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

namespace Steamless.Classes
{
    using System.Windows;

    /// <summary>
    /// Contains helper functions for WPF controls.
    /// 
    /// Credits to the functions within this class:
    /// https://github.com/MahApps/MahApps.Metro
    /// </summary>
    public static class ControlsHelper
    {
        /// <summary>
        /// This property can be used to set vertical scrollbar left side from the tabpanel (look at MetroAnimatedSingleRowTabControl)
        /// </summary>
        public static readonly DependencyProperty VerticalScrollBarOnLeftSideProperty =
            DependencyProperty.RegisterAttached("VerticalScrollBarOnLeftSide", typeof(bool), typeof(ControlsHelper),
                                                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.AffectsArrange | FrameworkPropertyMetadataOptions.Inherits));

        public static bool GetVerticalScrollBarOnLeftSide(DependencyObject obj)
        {
            return (bool)obj.GetValue(VerticalScrollBarOnLeftSideProperty);
        }

        public static void SetVerticalScrollBarOnLeftSide(DependencyObject obj, bool value)
        {
            obj.SetValue(VerticalScrollBarOnLeftSideProperty, value);
        }
    }
}