#if __IOS__
using System;
using UIKit;
using Foundation;

namespace BUILT.iOS.Events
{
    public class RowSelectionEventArgs<T> : EventArgs
    {
        public UITableView TableView { get; set; }
        public NSIndexPath IndexPath { get; set; }
        public T Model { get; set; }
    }
}
#endif