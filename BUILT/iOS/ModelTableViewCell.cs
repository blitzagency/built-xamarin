#if __IOS__
using System;
using Foundation;
using UIKit;

namespace BUILT.iOS
{
    public abstract class ModelTableViewCell<T>: UITableViewCell, IModel<T>
    {
        public T Model { get; set; }

        protected ModelTableViewCell(IntPtr handle) : base(handle)
        {
        
        }

        abstract public void CellWillDisplay(UITableView tableView, NSIndexPath indexPath);
    }
}
#endif
