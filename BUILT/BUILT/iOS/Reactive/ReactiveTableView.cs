#if __IOS__
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Reactive;
using UIKit;
using Foundation;
using System.Reactive.Linq;
using BUILT.iOS;
using BUILT.iOS.Events;


namespace BUILT.iOS.Reactive
{

    public abstract class ReactiveTableView<T>: UITableViewController<T>
    {
        event EventHandler<RowSelectionEventArgs> _selection;
        public new IObservable<EventPattern<RowSelectionEventArgs>> Selection { get; private set;}

        protected ReactiveTableView(IntPtr handle) : base(handle)
        {
            Selection = Observable.FromEventPattern<RowSelectionEventArgs>(
                x => _selection += x, 
                x => _selection -= x);
        }
            
        protected override void OnRowSelection(UITableView tableView, NSIndexPath indexPath)
        {
            var eventHandler = _selection;

            if (eventHandler != null)
            {
                var args = new RowSelectionEventArgs {
                    TableView = tableView,
                    IndexPath = indexPath,
                };

                eventHandler(this, args);
            }
        }

    }
}
#endif

