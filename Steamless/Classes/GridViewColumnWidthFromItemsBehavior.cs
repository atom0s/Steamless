/**
 * Steamless - Copyright (c) 2015 - 2022 atom0s [atom0s@live.com]
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
    using API.Events;
    using System.Collections.Specialized;
    using System.Globalization;
    using System.Linq;
    using System.Windows;
    using System.Windows.Controls;
    using System.Windows.Controls.Primitives;
    using System.Windows.Media;

    public static class GridViewColumnWidthFromItemsBehavior
    {
        public static readonly DependencyProperty GridViewColumnWidthFromItemsProperty =
            DependencyProperty.RegisterAttached("Enabled", typeof(bool), typeof(GridViewColumnWidthFromItemsBehavior), new UIPropertyMetadata(false, OnGridViewColumnWidthFromItemsPropertyChanged));

        public static bool GetEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(GridViewColumnWidthFromItemsProperty);
        }

        public static void SetEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(GridViewColumnWidthFromItemsProperty, value);
        }

        private static void OnGridViewColumnWidthFromItemsPropertyChanged(DependencyObject dpo, DependencyPropertyChangedEventArgs e)
        {
            if (dpo is ListView lv)
            {
                if ((bool)e.NewValue)
                    lv.Loaded += OnListViewLoaded;
                else
                    lv.Loaded -= OnListViewLoaded;
            }
        }

        private static void OnListViewLoaded(object sender, RoutedEventArgs routedEventArgs)
        {
            var lv = sender as ListView;
            var nc = ((INotifyCollectionChanged)lv?.Items);

            if (nc == null)
                return;

            nc.CollectionChanged += (o, args) =>
                {
                    if (lv.ItemContainerGenerator.Status != GeneratorStatus.ContainersGenerated)
                        return;

                    // Obtain the largest item width..
                    var width = lv.Items.OfType<LogMessageEventArgs>()
                                  .Select(msg => new FormattedText(msg.Message, CultureInfo.CurrentCulture, FlowDirection.LeftToRight, new Typeface("Tahoma"), 11, Brushes.Black))
                                  .Select(txt => txt.Width)
                                  .Concat(new double[] { 0.0f })
                                  .Max();

                    // Resize the message column..
                    var gv = lv.View as GridView;
                    if (gv != null)
                        gv.Columns[1].Width = width + 24.0f;

                    // Scroll to the last item..
                    if (lv.Items.Count > 0)
                        lv.ScrollIntoView(lv.Items[lv.Items.Count - 1]);
                    else
                    {
                        if (gv != null)
                            gv.Columns[1].Width = 600;
                    }
                };
        }
    }
}