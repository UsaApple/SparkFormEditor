using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace SparkFormEditor
{
    /// <summary>
    /// Control for displaying menu items, hosted in AutocompleteMenu.
    /// </summary>
    internal interface IAutocompleteListView
    {
        /// <summary>
        /// Image list
        /// </summary>
        ImageList ImageList { get; set; }

        /// <summary>
        /// Index of current selected item
        /// </summary>
        int SelectedItemIndex { get; set; }

        /// <summary>
        /// List of visible elements
        /// </summary>
        IList<AutocompleteItem> VisibleItems { get;set;}

        /// <summary>
        /// Duration (ms) of tooltip showing
        /// </summary>
        int ToolTipDuration { get; set; }

        /// <summary>
        /// Occurs when user selected item for inserting into text
        /// </summary>
        event EventHandler ItemSelected;

        /// <summary>
        /// Occurs when current hovered item is changing
        /// </summary>
        event EventHandler<HoveredEventArgs> ItemHovered;
    }
}
