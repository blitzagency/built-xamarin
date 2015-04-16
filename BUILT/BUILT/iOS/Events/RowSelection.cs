#if __IOS__
using System;
using UIKit;
using Foundation;

namespace BUILT.iOS.Events
{
    public class RowSelectionEventArgs : EventArgs
    {
        public UITableView TableView { get; set; }
        public NSIndexPath IndexPath { get; set; }
    }
}
#endif