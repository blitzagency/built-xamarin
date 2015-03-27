﻿#if __IOS__
using System;
using Foundation;
using UIKit;

namespace BUILT.iOS
{
    public abstract class UITableViewCell<T>: UITableViewCell, IModel<T>
    {
        public T Model { get; set; }

        protected UITableViewCell(IntPtr handle) : base(handle)
        {
        
        }

        abstract public void CellWillDisplay(UITableView tableView, NSIndexPath indexPath);
    }
}
#endif