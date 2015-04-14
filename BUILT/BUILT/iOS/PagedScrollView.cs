#if __IOS__
using System;
using UIKit;
using System.Collections.Generic;
using Foundation;
using CoreGraphics;

namespace BUILT
{
    public enum LayoutDirection
    {
        Horizontal,
        Vertical
    }

    [Register("PagedScrollView")]
    public class PagedScrollView: UIView
    {
        public UIView ContentView { get; private set; }
        public UIScrollView ScrollView { get; private set; }
        public LayoutDirection Direction { get; set; }
        NSLayoutConstraint ContentViewWidth { get; set; }
        NSLayoutConstraint ContentViewHeight { get; set; }
        List<UIView>pageQueue { get; set; }

        public PagedScrollView (IntPtr handle) : base(handle)
        {

        }

        [Export("initWithCoder:")]
        public PagedScrollView (NSCoder coder) : base(coder)
        {
            Initialize();
        }

        protected void Initialize()
        {
            Direction = LayoutDirection.Horizontal;
            pageQueue = new List<UIView> ();

            ScrollView = new UIScrollView (CGRect.Empty);
            ContentView = new UIView (CGRect.Empty);

            ScrollView.TranslatesAutoresizingMaskIntoConstraints = false;
            ContentView.TranslatesAutoresizingMaskIntoConstraints = false;

            configureScrollView(ScrollView);

            AddSubview(ScrollView);
            ScrollView.AddSubview(ContentView);

            applyScrollViewConstraints();
        }

        protected virtual void configureScrollView(UIScrollView scrollView)
        {
            scrollView.Bounces = true;
            scrollView.PagingEnabled = true;
            scrollView.ShowsVerticalScrollIndicator = false;
            scrollView.ShowsHorizontalScrollIndicator = false;
        }

        public void AddPageview(UIView view)
        {
            view.TranslatesAutoresizingMaskIntoConstraints = false;
            pageQueue.Add(view);


            if (Direction == LayoutDirection.Horizontal && ContentViewWidth != null)
                RemoveConstraint(ContentViewWidth);
            else if (Direction == LayoutDirection.Vertical && ContentViewHeight != null)
                RemoveConstraint(ContentViewHeight);
            
            SetNeedsUpdateConstraints();
            SetNeedsLayout();
        }

        public override void UpdateConstraints()
        {
            base.UpdateConstraints();

            if (pageQueue.Count > 0)
                applyPageConstraints();

            updateContentViewConstraints();

        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            // don't understand why ContentView has not been calculated here, 
            // it's constraints are dependant on this view is all I can think of.
            // to that end, we force it.
            ContentView.LayoutIfNeeded();

            var width = ContentView.Bounds.Width;
            var height = ContentView.Bounds.Height;
            ScrollView.ContentSize = new CGSize (width, height);

            base.LayoutSubviews();
        }

        protected void applyScrollViewConstraints()
        {
            var names = NSDictionary.FromObjectsAndKeys(
                new NSObject[] { ScrollView },
                new NSObject[] { new NSString ("ScrollView") }
            );

            var h1 = NSLayoutConstraint.FromVisualFormat("H:|[ScrollView]|", 0, null, names);
            var v1 = NSLayoutConstraint.FromVisualFormat("V:|[ScrollView]|", 0, null, names);

            AddConstraints(h1);
            AddConstraints(v1);
        }

        protected void updateContentViewConstraints()
        {
            if (Direction == LayoutDirection.Horizontal)
            {
                // if the operational dimension is null
                // it means we have never initialized this 
                // view before. We will be calling updateContentViewFor{{direction}}Direction
                // which produces that operational dimension constraint.
                // so we apply the default opposite dimension, since we will be
                // generating a new target dimension.

                if (ContentViewWidth == null)
                    applyInitialHeightContentViewConstraint();
                else
                    RemoveConstraint(ContentViewWidth);

                updateContentViewForHorizontalDirection();
            }

            if (Direction == LayoutDirection.Vertical)
            {
                // see note above for why we invoke the 
                // opposite dimension here.

                if (ContentViewHeight == null)
                    applyInitialWidthContentViewConstraint();
                else
                    RemoveConstraint(ContentViewHeight);

                updateContentViewForVerticalDirection();
            }
        }

        protected void applyInitialWidthContentViewConstraint()
        {
            ContentViewWidth = NSLayoutConstraint.Create(
                view1: ContentView,
                attribute1: NSLayoutAttribute.Width,
                relation: NSLayoutRelation.Equal,
                view2: this,
                attribute2: NSLayoutAttribute.Width,
                constant: 0,
                multiplier: 1
            );

            var left = NSLayoutConstraint.Create(
                view1: ContentView,
                attribute1: NSLayoutAttribute.Left,
                relation: NSLayoutRelation.Equal,
                view2: ScrollView,
                attribute2: NSLayoutAttribute.Left,
                constant: 0,
                multiplier: 1
            );

            AddConstraint(ContentViewWidth);
            AddConstraint(left);
        }

        protected void applyInitialHeightContentViewConstraint()
        {
            ContentViewHeight = NSLayoutConstraint.Create(
                view1: ContentView,
                attribute1: NSLayoutAttribute.Height,
                relation: NSLayoutRelation.Equal,
                view2: this,
                attribute2: NSLayoutAttribute.Height,
                constant: 0,
                multiplier: 1
            );

            var top = NSLayoutConstraint.Create(
                view1: ContentView,
                attribute1: NSLayoutAttribute.Top,
                relation: NSLayoutRelation.Equal,
                view2: this,
                attribute2: NSLayoutAttribute.Top,
                constant: 0,
                multiplier: 1
            );

            var left = NSLayoutConstraint.Create(
                view1: ContentView,
                attribute1: NSLayoutAttribute.Left,
                relation: NSLayoutRelation.Equal,
                view2: ScrollView,
                attribute2: NSLayoutAttribute.Left,
                constant: 0,
                multiplier: 1
            );

            AddConstraint(ContentViewHeight);
            AddConstraint(top);
            AddConstraint(left);
        }

        protected void updateContentViewForHorizontalDirection()
        {
            var children = ContentView.Subviews;
            var count = children.Length == 0 ? 1 : children.Length;

            ContentViewWidth = NSLayoutConstraint.Create(
                view1: ContentView,
                attribute1: NSLayoutAttribute.Width,
                relation: NSLayoutRelation.Equal,
                view2: this,
                attribute2: NSLayoutAttribute.Width,
                constant: 0,
                multiplier: count
            );

            AddConstraint(ContentViewWidth);
        }

        protected void updateContentViewForVerticalDirection()
        {
            var children = ContentView.Subviews;
            var count = children.Length == 0 ? 1 : children.Length;

            ContentViewHeight = NSLayoutConstraint.Create(
                view1: ContentView,
                attribute1: NSLayoutAttribute.Height,
                relation: NSLayoutRelation.Equal,
                view2: this,
                attribute2: NSLayoutAttribute.Height,
                constant: 0,
                multiplier: count
            );

            AddConstraint(ContentViewHeight);
        }

        protected void applyPageConstraints()
        {
            if (Direction == LayoutDirection.Horizontal)
                applyPageConstraintsForHorizontalDirection();

            if (Direction == LayoutDirection.Vertical)
                applyPageConstraintsForHorizontalDirection();
        }

        protected NSLayoutConstraint[] createInitialHorizontalConstraints(UIView view)
        {
            var width = Bounds.Width;
            var names = NSDictionary.FromObjectsAndKeys(
                new NSObject[] { view },
                new NSObject[] { new NSString ("view") }
            );


            var h1 = NSLayoutConstraint.FromVisualFormat("H:|[view]", 0, null, names);
            var v1 = NSLayoutConstraint.FromVisualFormat("V:|[view]|", 0, null, names);

            var results = new NSLayoutConstraint[h1.Length + v1.Length];
            h1.CopyTo(results, 0);
            v1.CopyTo(results, h1.Length);

            return results;
        }

        protected NSLayoutConstraint[] createAdditionalHorizontalConstraints(UIView previous, UIView view)
        {
            var width = Bounds.Width;
            var names = NSDictionary.FromObjectsAndKeys(
                new NSObject[] { previous, view },
                new NSObject[] { new NSString ("previous"), new NSString ("view") }
            );

            var h1 = NSLayoutConstraint.FromVisualFormat("H:[previous][view]", 0, null, names);
            var v1 = NSLayoutConstraint.FromVisualFormat("V:|[view]|", 0, null, names);

            var results = new NSLayoutConstraint[h1.Length + v1.Length];
            h1.CopyTo(results, 0);
            v1.CopyTo(results, h1.Length);

            return results;
        }

        protected NSLayoutConstraint createPageWidthConstraint(UIView view)
        {
            return  NSLayoutConstraint.Create(
                view1: view,
                attribute1: NSLayoutAttribute.Width,
                relation: NSLayoutRelation.Equal,
                view2: this,
                attribute2: NSLayoutAttribute.Width,
                constant: 0,
                multiplier: 1);
        }

        protected NSLayoutConstraint createPageHeightConstraint(UIView view)
        {
            return  NSLayoutConstraint.Create(
                view1: view,
                attribute1: NSLayoutAttribute.Height,
                relation: NSLayoutRelation.Equal,
                view2: this,
                attribute2: NSLayoutAttribute.Height,
                constant: 0,
                multiplier: 1);
        }

        protected void applyPageConstraintsForHorizontalDirection()
        {
            if (ContentView.Subviews.Length == 0)
            {
                for (var i = 0; i < pageQueue.Count; i++)
                {
                    var view = pageQueue [i];
                    ContentView.AddSubview(view);

                    if (i == 0)
                    {
                        AddConstraint(createPageWidthConstraint(view));
                        ContentView.AddConstraints(createInitialHorizontalConstraints(view));
                    }
                    else
                    {
                        var previous = ContentView.Subviews [i - 1];
                        var c = createAdditionalHorizontalConstraints(previous, view);
                        AddConstraint(createPageWidthConstraint(view));
                        ContentView.AddConstraints(c);
                    }
                }
            }
            else
            {
                var baseIndex = ContentView.Subviews.Length - 1;
                for (var i = 0; i < pageQueue.Count; i++)
                {
                    var view = pageQueue [i];
                    ContentView.AddSubview(view);

                    var previous = ContentView.Subviews [baseIndex + i];
                    AddConstraint(createPageWidthConstraint(view));
                    ContentView.AddConstraints(createAdditionalHorizontalConstraints(previous, view));
                }
            }

            pageQueue.Clear();
        }

        protected void applyPageConstraintsForVerticalDirection()
        {

        }

    }
}

#endif