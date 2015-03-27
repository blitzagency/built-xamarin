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
        }

        public override void UpdateConstraints()
        {
            if (pageQueue.Count > 0)
                applyPageConstraints();

            updateContentViewConstraints();
            base.UpdateConstraints();
        }

        protected void updateContentViewConstraints()
        {
            if (Direction == LayoutDirection.Horizontal)
            {
                if (ContentViewWidth != null)
                    RemoveConstraint(ContentViewWidth);

                updateContentViewForHorizontalDirection();
            }

            if (Direction == LayoutDirection.Vertical)
            {
                if (ContentViewHeight != null)
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

            AddConstraint(ContentViewWidth);
        }

        protected void applyInitialHeightContentViewConstraint()
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

            AddConstraint(ContentViewWidth);
        }

        protected void updateContentViewForHorizontalDirection()
        {
            var children = ContentView.Subviews;
            var count = children.Length == 0 ? 1 : children.Length;
            var width = count * Bounds.Width;
            var height = Bounds.Height;

            ContentViewWidth = NSLayoutConstraint.Create(
                view1: ContentView,
                attribute1: NSLayoutAttribute.Width,
                relation: NSLayoutRelation.Equal,
                view2: null,
                attribute2: NSLayoutAttribute.NoAttribute,
                constant: width,
                multiplier: 1
            );

            AddConstraint(ContentViewWidth);
            ScrollView.ContentSize = new CGSize (width, height);
        }

        protected void updateContentViewForVerticalDirection()
        {
            
        }

        protected virtual void configureScrollView(UIScrollView scrollView)
        {
            scrollView.Bounces = false;
            scrollView.PagingEnabled = true;
            scrollView.ShowsVerticalScrollIndicator = false;
            scrollView.ShowsHorizontalScrollIndicator = false;
        }

        public override void LayoutSubviews()
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

            ContentViewHeight = NSLayoutConstraint.Create(
                view1: ContentView,
                attribute1: NSLayoutAttribute.Height,
                relation: NSLayoutRelation.Equal,
                view2: this,
                attribute2: NSLayoutAttribute.Height,
                constant: 0,
                multiplier: 1
            );

            var names = NSDictionary.FromObjectsAndKeys(
                            new NSObject[] { ScrollView },
                            new NSObject[] { new NSString ("ScrollView") }
                        );
                
            var h1 = NSLayoutConstraint.FromVisualFormat("H:|[ScrollView]|", 0, null, names);
            var v1 = NSLayoutConstraint.FromVisualFormat("V:|[ScrollView]|", 0, null, names);

            //applyInitialWidthContentViewConstraint();
            //applyInitialHeightContentViewConstraint();
            AddConstraints(h1);
            AddConstraints(v1);
            AddConstraint(ContentViewWidth);
            AddConstraint(ContentViewHeight);

            base.LayoutSubviews();
        }

        public void AddPageview(UIView view)
        {
            view.TranslatesAutoresizingMaskIntoConstraints = false;
            pageQueue.Add(view);

//            if (Direction == LayoutDirection.Horizontal)
//                RemoveConstraint(ContentViewWidth);
//
//            if (Direction == LayoutDirection.Vertical)
//                RemoveConstraint(ContentViewHeight);

            SetNeedsUpdateConstraints();
        }

        protected void applyPageConstraints()
        {
            if (Direction == LayoutDirection.Horizontal)
                applyPageConstraintsForHorizontalDirection();

            if (Direction == LayoutDirection.Vertical)
                applyPageConstraintsForHorizontalDirection();
        }

        protected NSLayoutConstraint[] createInitialiHorizontalConstraints(UIView view)
        {
            var width = Bounds.Width;
            var names = NSDictionary.FromObjectsAndKeys(
                            new NSObject[] { view },
                            new NSObject[] { new NSString ("view") }
                        );

            var h1 = NSLayoutConstraint.FromVisualFormat(string.Format("H:|[view({0})]", width), 0, null, names);
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

            var h1 = NSLayoutConstraint.FromVisualFormat(string.Format("H:[previous({0})][view({0})]", width), 0, null, names);
            var v1 = NSLayoutConstraint.FromVisualFormat("V:|[view]|", 0, null, names);

            var results = new NSLayoutConstraint[h1.Length + v1.Length];
            h1.CopyTo(results, 0);
            v1.CopyTo(results, h1.Length);

            return results;
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
                        ContentView.AddConstraints(createInitialiHorizontalConstraints(view));
                    }
                    else
                    {
                        var previous = ContentView.Subviews [i - 1];
                        var c = createAdditionalHorizontalConstraints(previous, view);
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