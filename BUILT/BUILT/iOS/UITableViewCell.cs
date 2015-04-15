#if __IOS__
using System;
using Foundation;
using UIKit;
using BUILT.Shared;

namespace BUILT.iOS
{
    public abstract class UITableViewCell<T>: UITableViewCell, IModel<T>
    {
        public T Model { get; set; }

        protected UITableViewCell(IntPtr handle) : base(handle)
        {
        
        }

        abstract public void PrepeareForDisplay(UITableView tableView, NSIndexPath indexPath);
    }
}
#endif
