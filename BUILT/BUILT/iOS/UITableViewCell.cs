#if __IOS__
using System;
using Foundation;
using UIKit;
using BUILT.Shared;

namespace BUILT.iOS
{
    public interface ITableCellFor<T>: IModel<T>
    {
        new T Model { get; set; }
        void ViewWillAppear(UITableView tableView, NSIndexPath indexPath);
        void ViewDidDisappear(UITableView tableView, NSIndexPath indexPath);
    }

    public abstract class UITableViewCell<T>: UITableViewCell, ITableCellFor<T>
    {
        public T Model { get; set; }

        protected UITableViewCell(IntPtr handle) : base(handle)
        {
        
        }

        public virtual void ViewWillAppear(UITableView tableView, NSIndexPath indexPath)
        {
            
        }
        public virtual void ViewDidDisappear(UITableView tableView, NSIndexPath indexPath)
        {
            
        }
    }
}
#endif
