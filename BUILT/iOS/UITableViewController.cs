#if __IOS__
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using UIKit;
using Foundation;


namespace BUILT.iOS
{
    public class RowSelectionEventArgs<T> : EventArgs    // guideline: derive from EventArgs
    {
        public UITableView TableView { get; set; }
        public NSIndexPath IndexPath { get; set; }
        public T Model { get; set; }
    }

    public abstract class UITableViewController<T>: UITableViewController
    {
        Type _cellType;

        public event EventHandler<RowSelectionEventArgs<T>> RowSelection;
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
                
                if (value.IsSubclassOf(typeof(UITableViewCell<T>)))
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
        protected virtual UITableViewCell numberOfSectionsInTableView(UITableView tableView, NSIndexPath indexPath)
        {
            var cell = TableView.DequeueReusableCell(ReuseIdentifier) as UITableViewCell<T>;
            cell.Model = Models[indexPath.Row];
            return cell;
        }

        [Export("tableView:didSelectRowAtIndexPath:")]
        protected void DidSelectRow(UITableView tableView, NSIndexPath indexPath)
        {
            SelectedIndexPath = indexPath;
            var index = indexPath.Row;
            var eventHandler = RowSelection;

            if (eventHandler != null)
            {
                var args = new RowSelectionEventArgs<T> {
                    TableView = tableView,
                    IndexPath = indexPath,
                    Model = Models[index]
                };
                    
                eventHandler(this, args);
            }
        }

        [Export("tableView:willDisplayCell:forRowAtIndexPath:")]
        protected void WillDisplayCell(UITableView tableView, UITableViewCell cell, NSIndexPath indexPath)
        {
            var modelCell = cell as UITableViewCell<T>;
            modelCell.CellWillDisplay(tableView, indexPath);
        }

    }
}
#endif

