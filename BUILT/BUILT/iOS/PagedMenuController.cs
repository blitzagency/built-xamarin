using System;
using Foundation;
using UIKit;
using CoreGraphics;


namespace BUILT.iOS
{
    enum ScrollDirection {
        Forward = 1,
        Backward = -1,
    }

    [Register("PagedMenuController")]
    public class PagedMenuController: UIViewController
    {
        PagedScrollView _pagedScrollView;
        MenuScrollView _menuScrollView;

        CGPoint _lastPagedOffset = new CGPoint(0, 0);
        CGPoint _lastMenuOffset = new CGPoint(0, 0);
        int _currentPageIndex = 0;
        bool _pagedViewIsEndingDragging = false;
        ScrollDirection _lastPagedDirection = ScrollDirection.Forward;

        [Outlet]
        public MenuScrollView MenuScrollView { 
            get{ return _menuScrollView; } 
            set{ 
                if (_menuScrollView != null)
                    _menuScrollView.ScrollView.WeakDelegate = null;
                
                _menuScrollView = value;
                _menuScrollView.ScrollView.WeakDelegate = this;
                _menuScrollView.ScrollView.ScrollEnabled = false;
            }
        }

        [Outlet]
        public PagedScrollView PagedScrollView {
            get{ return _pagedScrollView; } 
            set { 
                if (_pagedScrollView != null)
                    _pagedScrollView.ScrollView.WeakDelegate = null;

                _pagedScrollView = value;
                _pagedScrollView.ScrollView.WeakDelegate = this;
            }
        }

        public PagedMenuController (PagedScrollView pagedScrollView = null, MenuScrollView menuScrollView = null)
        {
            if(pagedScrollView != null)
                PagedScrollView = pagedScrollView;

            if(menuScrollView != null)
                MenuScrollView = menuScrollView;
        }

        [Export("scrollViewDidScroll:")]
        public void scrollViewDidScroll(UIScrollView scrollView)
        {
            if (scrollView == PagedScrollView.ScrollView)
                pagedScrollViewDidScroll(scrollView);
            else if (scrollView == MenuScrollView.ScrollView)
                menuScrollViewDidScroll(scrollView);
        }

        [Export("scrollViewWillEndDragging:withVelocity:targetContentOffset:")]
        public void scrollViewWillEndDragging(UIScrollView scrollView, CGPoint velocity, ref CGPoint targetContentOffset)
        {
            if (scrollView == PagedScrollView.ScrollView)
                pagedScrollViewWillEndDragging(scrollView, velocity, ref targetContentOffset);
            else if (scrollView == MenuScrollView.ScrollView)
                menuScrollViewWillEndDragging(scrollView, velocity, ref targetContentOffset);

        }

        [Export("scrollViewDidEndDecelerating:")]
        public void scrollViewDidEndDecelerating(UIScrollView scrollView)
        {
            if (scrollView == PagedScrollView.ScrollView)
                pagedScrollViewDidEndDecelerating(scrollView);
            else if (scrollView == MenuScrollView.ScrollView)
                menuScrollViewDidEndDecelerating(scrollView);
        }

        protected void pagedScrollViewWillEndDragging(UIScrollView scrollView, CGPoint velocity, ref CGPoint targetContentOffset)
        {
            if (targetContentOffset.X == _lastPagedOffset.X)
                return;

            // user as started moving forward or backward, then released causeing the page to spring back.
            if ((targetContentOffset.X/PagedScrollView.Bounds.Width) == _currentPageIndex)
            {
                animateMenuToPoint(_lastMenuOffset);
                _lastPagedOffset = targetContentOffset;
                _pagedViewIsEndingDragging = true;
                return;
            }
                
            _lastPagedOffset = targetContentOffset;
            _pagedViewIsEndingDragging = true;

            var nextIndex = (int)(targetContentOffset.X / PagedScrollView.Bounds.Width);
            var currentView = MenuScrollView.ContentView.Subviews [_currentPageIndex];
            var nextView = MenuScrollView.ContentView.Subviews [nextIndex];
            var distance = (currentView.Bounds.Width / 2) + MenuScrollView.Padding + (nextView.Bounds.Width / 2);

            distance = _lastMenuOffset.X + ((int)_lastPagedDirection) * distance;

            var point = new CGPoint (distance , 0);
            _lastMenuOffset = point;

            animateMenuToPoint(point);

            _currentPageIndex = nextIndex;

            // this needs to be a delegate call
            currentView.Alpha = (nfloat)0.15;
            nextView.Alpha = 1;
        }

        protected void animateMenuToPoint(CGPoint point)
        {
            UIView.Animate(
                duration: 0.35, delay: 0, options: UIViewAnimationOptions.CurveEaseOut, 
                animation: delegate{
                    MenuScrollView.ScrollView.ContentOffset = point;
                }, completion: null);
        }

        protected void menuScrollViewWillEndDragging(UIScrollView scrollView, CGPoint velocity, ref CGPoint targetContentOffset)
        {
            // 333.5 - ContentOffset
            // origin {x: 428, y: 24.5}
            // Size {width: 186, height: 20.5}
            Console.WriteLine("menuScrollViewWillEndDragging");
        }

        protected void pagedScrollViewDidEndDecelerating(UIScrollView scrollView)
        {
            _pagedViewIsEndingDragging = false;
        }

        protected void menuScrollViewDidEndDecelerating(UIScrollView scrollView)
        {
            
        }

        protected void pagedScrollViewDidScroll(UIScrollView scrollView)
        {
            if (_pagedViewIsEndingDragging)
                return;
            
            ScrollDirection direction = ScrollDirection.Forward;

            if (scrollView.ContentOffset.X > _lastPagedOffset.X)
                direction = ScrollDirection.Forward;
            else if (scrollView.ContentOffset.X < _lastPagedOffset.X)
                direction = ScrollDirection.Backward;

            _lastPagedDirection = direction;

            var pageWidth = PagedScrollView.Bounds.Width;

            var nextIndex = _currentPageIndex + (int)direction;
            nextIndex = Math.Max(nextIndex, 0);
            nextIndex = Math.Min(nextIndex, MenuScrollView.ContentView.Subviews.Length - 1);

            var currentView = MenuScrollView.ContentView.Subviews [_currentPageIndex];
            var nextView = MenuScrollView.ContentView.Subviews [nextIndex];
            var distance = (currentView.Bounds.Width / 2) + MenuScrollView.Padding + (nextView.Bounds.Width / 2);

            var normalizedOffsetX = scrollView.ContentOffset.X - (pageWidth * _currentPageIndex);
            var normalizedPercentOffsetX = normalizedOffsetX / pageWidth;

            var delta = normalizedPercentOffsetX * distance;

            MenuScrollView.ScrollView.ContentOffset = new CGPoint ((delta + _lastMenuOffset.X), 0);
        }

        protected void menuScrollViewDidScroll(UIScrollView scrollView)
        {
        
        }
    }
}

