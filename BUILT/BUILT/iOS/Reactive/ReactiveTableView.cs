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
        event EventHandler<RowSelectionEventArgs<T>> _selection;
        public new IObservable<EventPattern<RowSelectionEventArgs<T>>> Selection { get; private set;}

        protected ReactiveTableView(IntPtr handle) : base(handle)
        {
            Selection = Observable.FromEventPattern<RowSelectionEventArgs<T>>(
                x => _selection += x, 
                x => _selection -= x);
        }
            
        protected override void OnRowSelection(UITableView tableView, NSIndexPath indexPath)
        {
            var eventHandler = _selection;

            if (eventHandler != null)
            {
                var args = new RowSelectionEventArgs<T> {
                    TableView = tableView,
                    IndexPath = indexPath,
                    Model = Models[indexPath.Row]
                };

                eventHandler(this, args);
            }
        }

    }
}
#endif

