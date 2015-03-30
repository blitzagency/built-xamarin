#if __IOS__
using System;
using System.Linq;
using System.Collections.Generic;
using UIKit;
using Foundation;
using CoreGraphics;

namespace BUILT.iOS
{
    public class CV: UIView
    {
        public CV(CGRect frame): base(frame)
        {
        
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();
            var foo = 1;
        }

        public override void UpdateConstraints()
        {
            base.UpdateConstraints();
            var foo = 1;
        }
    }
    [Register("MenuScrollView")]
    public class MenuScrollView : UIView
    {
        public int Padding { get; set; }
        public UIScrollView ScrollView { get; set;}
        public UIView ContentView { get; set;}


        NSLayoutConstraint ContentViewWidth { get; set; }
        List<UIView> queue { get; set;}
        CGSize lastSize { get; set; }

        public MenuScrollView (IntPtr handle) : base(handle)
        {

        }

        [Export("initWithCoder:")]
        public MenuScrollView (NSCoder coder) : base(coder)
        {
            Initialize();
        }

        protected void Initialize()
        {
            queue = new List<UIView> ();
            Padding = 50;

            ScrollView = new UIScrollView (CGRect.Empty);
            ContentView = new CV (CGRect.Empty);
            ContentView.BackgroundColor = UIColor.Red;

            ScrollView.TranslatesAutoresizingMaskIntoConstraints = false;
            ContentView.TranslatesAutoresizingMaskIntoConstraints = false;

            configureScrollView(ScrollView);

            AddSubview(ScrollView);
            ScrollView.AddSubview(ContentView);

            applyInitialConstraints();
        }

        public void AddView(UIView view)
        {
            view.TranslatesAutoresizingMaskIntoConstraints = false;
            queue.Add(view);

            SetNeedsUpdateConstraints();
            SetNeedsLayout();
        }

        protected virtual void configureScrollView(UIScrollView scrollView)
        {
            scrollView.Bounces = false;
            scrollView.PagingEnabled = false;
            scrollView.ShowsVerticalScrollIndicator = true;
            scrollView.ShowsHorizontalScrollIndicator = true;
        }

        protected void applyInitialConstraints()
        {
            applyScrollViewConstraints();
            applyContentViewConstraints();
        }

        protected void applyContentViewConstraints()
        {
            var t1 = NSLayoutConstraint.Create(
                view1: ContentView,
                attribute1: NSLayoutAttribute.Top,
                relation: NSLayoutRelation.Equal,
                view2: this,
                attribute2: NSLayoutAttribute.Top,
                constant: 0,
                multiplier: 1
            );

            var b1 = NSLayoutConstraint.Create(
                view1: ContentView,
                attribute1: NSLayoutAttribute.Bottom,
                relation: NSLayoutRelation.Equal,
                view2: this,
                attribute2: NSLayoutAttribute.Bottom,
                constant: 0,
                multiplier: 1
            );
               

            AddConstraint(t1);
            AddConstraint(b1);
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


        public override void UpdateConstraints()
        {
            base.UpdateConstraints();

//            if (queue.Count > 0)
//                applyViewConstraints();

            //updateContentViewConstraints();
        }

        public override void LayoutSubviews()
        {
            base.LayoutSubviews();

            if (queue.Count > 0)
                applyViewConstraints();


            // layout the menu items
            ContentView.LayoutIfNeeded();

            var lastView = ContentView.Subviews.Last();
            var firstView = ContentView.Subviews.First();
            var width = lastView.Bounds.Width + lastView.Frame.X;
            var totalWidth = (nfloat)(width + (0.5 * Bounds.Width) - (lastView.Bounds.Width / 2));
            var size = new CGSize (totalWidth, Bounds.Height);

            if(ContentViewWidth != null)
                RemoveConstraint(ContentViewWidth);
            
            ContentViewWidth = NSLayoutConstraint.Create(
                view1: ContentView,
                attribute1: NSLayoutAttribute.Width,
                relation: NSLayoutRelation.Equal,
                view2: null,
                attribute2: NSLayoutAttribute.NoAttribute,
                constant: size.Width,
                multiplier: 1
            );

            AddConstraint(ContentViewWidth);
            ScrollView.LayoutIfNeeded();
            ScrollView.ContentSize = size;
        }

        protected void updateContentViewConstraints()
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
                multiplier: 1
            );

            AddConstraint(ContentViewWidth);
        }

        protected NSLayoutConstraint[] createInitialViewConstraints(UIView view)
        {
            // https://github.com/evgenyneu/center-vfl

//            var names = NSDictionary.FromObjectsAndKeys(
//                new NSObject[] { view, this },
//                new NSObject[] { new NSString ("view"), new NSString ("superview") }
//            );

            // we are going to reset this constraint
            // in LayoutSubviews to be relative to the ContentView
            // we are setting it to the center of the containing view 
            // so we can get the center coords.
            var viewWidth = view.Bounds.Width;
            var centerSuperview = (Bounds.Width / 2);
            var left = centerSuperview - (viewWidth / 2);

            var h1 = NSLayoutConstraint.Create(
                view1: view,
                attribute1: NSLayoutAttribute.Left,
                relation: NSLayoutRelation.Equal,
                view2: ContentView,
                attribute2: NSLayoutAttribute.Left,
                constant: left,
                multiplier: 1
            );

//            var cv = NSLayoutConstraint.Create(
//                view1: view,
//                attribute1: NSLayoutAttribute.CenterY,
//                relation: NSLayoutRelation.Equal,
//                view2: this,
//                attribute2: NSLayoutAttribute.CenterY,
//                constant: 0,
//                multiplier: 1
//            );

            return new NSLayoutConstraint[] {h1};
        }

        protected NSLayoutConstraint[] createWidthAndHeightConstraints(UIView view)
        {
            var w1 = NSLayoutConstraint.Create(
                view1: view,
                attribute1: NSLayoutAttribute.Width,
                relation: NSLayoutRelation.Equal,
                view2: null,
                attribute2: NSLayoutAttribute.NoAttribute,
                constant: view.Bounds.Width,
                multiplier: 1
            );

            var h1 = NSLayoutConstraint.Create(
                view1: view,
                attribute1: NSLayoutAttribute.Height,
                relation: NSLayoutRelation.Equal,
                view2: null,
                attribute2: NSLayoutAttribute.NoAttribute,
                constant: view.Bounds.Height,
                multiplier: 1
            );

            return new NSLayoutConstraint[] {w1, h1};
        }

        protected NSLayoutConstraint[] centerVerticallyConstraints(UIView view)
        {
//            var names = NSDictionary.FromObjectsAndKeys(
//                new NSObject[] { view, this },
//                new NSObject[] { new NSString ("view"), new NSString ("superview") }
//            );
//
//            // center vertically
//            var cv = NSLayoutConstraint.FromVisualFormat("H:[superview]-(<=1)-[view]", 
//                NSLayoutFormatOptions.AlignAllCenterY, null, names);
//
//            return cv;

            var cv = NSLayoutConstraint.Create(
                         view1: view,
                         attribute1: NSLayoutAttribute.CenterY,
                         relation: NSLayoutRelation.Equal,
                         view2: ContentView,
                         attribute2: NSLayoutAttribute.CenterY,
                         constant: 0,
                         multiplier: 1
                     );

            return new NSLayoutConstraint[] {cv};
        }

        protected NSLayoutConstraint[] createViewConstraints(UIView previous, UIView view)
        {
            var names = NSDictionary.FromObjectsAndKeys(
                new NSObject[] { previous, view },
                new NSObject[] { new NSString ("previous"), new NSString ("view") }
            );

            var vfl = string.Format("H:[previous]-{0}-[view]", Padding);
            var h1 = NSLayoutConstraint.FromVisualFormat(vfl, 0, null, names);

            var c1 = createWidthAndHeightConstraints(view);

            var result = new NSLayoutConstraint[h1.Length + c1.Length];
            h1.CopyTo(result, 0);
            c1.CopyTo(result, h1.Length);

            return result;
        }

        protected void applyViewConstraints()
        {
            if (ContentView.Subviews.Length == 0)
            {
                for (var i = 0; i < queue.Count; i++)
                {
                    var view = queue [i];
                    ContentView.AddSubview(view);

                    if (i == 0)
                    {
                        // the inital view is centered against the parent view
                        // this will be reset to be relative to the content view
                        // in LayoutSubviews.
                        ContentView.AddConstraints(createInitialViewConstraints(view));

                        var cv = centerVerticallyConstraints(view);
                        ContentView.AddConstraints(cv);

                        var c1 = createWidthAndHeightConstraints(view);
                        ContentView.AddConstraints(c1);
                    }
                    else
                    {
                        var previous = ContentView.Subviews [i - 1];

                        var cv = centerVerticallyConstraints(view);
                        ContentView.AddConstraints(cv);

                        var c1 = createViewConstraints(previous, view);
                        ContentView.AddConstraints(c1);
                    }
                }
            }
            else
            {
//                var baseIndex = ContentView.Subviews.Length - 1;
//                for (var i = 0; i < queue.Count; i++)
//                {
//                    var view = queue [i];
//                    ContentView.AddSubview(view);
//
//                    var previous = ContentView.Subviews [baseIndex + i];
//                    ContentView.AddConstraints(createAdditionalHorizontalConstraints(previous, view));
//                }
            }

           queue.Clear();
        }
    }
}

#endif