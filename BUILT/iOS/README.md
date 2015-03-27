
### PagedScrollView

```csharp

using BUILT.iOS;

UIViewController BlackController { get; set; }
UIViewController PurpleController { get; set; }
UIViewController RedController { get; set; }

public override void ViewDidLoad()
{
    base.ViewDidLoad();

    BlackController = Storyboard.InstantiateViewController("BlackController");
    PurpleController = Storyboard.InstantiateViewController("PurpleController");

    // assumes this is an outlet
    myPagedScrollView.ScrollView.Bounces = false;

    myPagedScrollView.AddPageview(BlackController.View);
    myPagedScrollView.AddPageview(PurpleController.View);
}


partial void wantsAddRedController(Foundation.NSObject sender)
{
    if (RedController == null)
        addRedController();
}

void addRedController()
{
    RedController = Storyboard.InstantiateViewController("RedController");
    scrollView.AddPageview(RedController.View);
}

```


### UITableViewController<<T>> & UITableViewCell<<T>>

```csharp

using System;
using System.Collections.Generic;
using BUILT.iOS;

public class Workout
{
    public string Label { get; set; }
}

public partial class MyTableViewController : UITableViewController<Workout>
{
    public MyTableViewController(IntPtr handle) : base(handle)
    {
    }

    protected override void Initialize()
    {
        LoadAsync = false;
        CellType = typeof(MyCellView);
    }

    public override List<Workout> LoadData()
    {
        return new List<Workout> {

            new Workout {
                Label = "Foo"
            }
        };
    }
}

public partial class MyCellView : UITableViewCell<Workout>
{
    public MyCellView(IntPtr handle) : base(handle)
    {

    }

    public override void CellWillDisplay(UITableView tableView, NSIndexPath indexPath)
    {
        // assumes Label is an outlet
        Label.Text = Model.Label;
    }
}

```
