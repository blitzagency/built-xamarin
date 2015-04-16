#if __IOS__
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UIKit;
using Foundation;
using BUILT.iOS.Events;

namespace BUILT.iOS
{

    public abstract class UITableViewController<T>: UITableViewController
    {
        protected Type _cellType;

        public event EventHandler<RowSelectionEventArgs> Selection;
        public virtual List<T> Models { get; set; }

        public bool LoadAsync { get; set; }
        public NSIndexPath SelectedIndexPath { get; set; }
        public virtual string ReuseIdentifier { get; set; }
                

        protected abstract void Initialize();
        public abstract List<T> LoadData();

        public async Task<List<T>> LoadDataAsync()
        {
            return await Task.Run(() => LoadData());
        }
            
        public virtual Type CellType { 

            get {
                return _cellType;
            } 

            set {
                
                if (value.IsSubclassOf(typeof(UITableViewCell)))
                {
                    _cellType = value;
                    return;
                }

                throw new ArgumentException(string.Format(
                    "Type {0} is not a subclass of ModelTableViewCell<{1}>",
                    value.Name, typeof(T).Name)
                );
            } 
        }

        protected UITableViewController(IntPtr handle) : base(handle)
        {
            
        }
            
        public override void ViewDidLoad()
        {
            base.ViewDidLoad();
            OnLoadSetup();
            Initialize();
            InitializeData();
        }

        public TCell CellAt<TCell>(NSIndexPath indexPath)
            where TCell: UITableViewCell
        {
            return (TCell)TableView.CellAt(indexPath);
        }

        protected virtual void OnLoadSetup()
        {
            if (Models == null)
                Models = new List<T>();

            ReuseIdentifier = typeof(T).Name + "Cell";
        }

        protected virtual void InitializeData()
        {
            if (LoadAsync)
            {
                Task.Run(async delegate {
                    var results = await LoadDataAsync();

                    BeginInvokeOnMainThread(delegate {
                        Models = results;
                        TableView.ReloadData();
                    });
                });
            }
            else
            {
                Models = LoadData();
            }
        }

        [Export("numberOfSectionsInTableView:")]
        protected virtual nint numberOfSectionsInTableView(UITableView tableView)
        {
            return 1;
        }

        [Export("tableView:numberOfRowsInSection:")]
        protected virtual nint numberOfRowsInSection(UITableView tableView, NSIndexPath indexPath)
        {
            return Models.Count;
        }

        [Export("tableView:cellForRowAtIndexPath:")]
        protected virtual UITableViewCell cellForRowAtIndexPath(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = TableView.DequeueReusableCell(ReuseIdentifier) as ITableCellFor<T>;
            cell.Model = modelForRowAtIndexPath(tableView, indexPath);
            return cell as UITableViewCell;
        }

        protected virtual T modelForRowAtIndexPath(UITableView tableView, NSIndexPath indexPath)
        {
            return Models[indexPath.Row];
        }

        [Export("tableView:didSelectRowAtIndexPath:")]
        protected virtual void didSelectRow(UITableView tableView, NSIndexPath indexPath)
        {
            SelectedIndexPath = indexPath;
            OnRowSelection(tableView, indexPath);
        }

        [Export("tableView:willDisplayCell:forRowAtIndexPath:")]
        protected virtual void cellWillAppear(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            var modelCell = cell as ITableCellFor<T>;
            modelCell.ViewWillAppear(tableView, indexPath);
        }

        [Export("tableView:didEndDisplayingCell:forRowAtIndexPath:")]
        protected virtual void cellDidDisapper(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            var modelCell = cell as ITableCellFor<T>;
            modelCell.ViewDidDisappear(tableView, indexPath);
        }

        protected virtual void OnRowSelection(UITableView tableView, NSIndexPath indexPath)
        {
            var eventHandler = Selection;

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

