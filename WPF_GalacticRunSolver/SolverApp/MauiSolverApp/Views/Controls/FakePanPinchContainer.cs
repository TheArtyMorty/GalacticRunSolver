using AndroidX.ConstraintLayout.Motion.Widget;
using System.Diagnostics;
using Debug = System.Diagnostics.Debug;

namespace SolverApp.Views.Controls
{
    public class FakePanPinchContainer : PanPinchContainer
    {
        private readonly TapGestureRecognizer _doubleTapGestureRecognizer;
        private readonly PanGestureRecognizer _panGestureRecognizer;
        private readonly PinchGestureRecognizer _pinchGestureRecognizer;
        private readonly SwipeGestureRecognizer _swipeGestureRecognizer;

        private bool isSwipeEnabled = true;

        public FakePanPinchContainer() : base()
        {
            GestureRecognizers.Clear();
            IsClippedToBounds = true;

            _panGestureRecognizer = new PanGestureRecognizer();
            _panGestureRecognizer.TouchPoints = 2;
            _panGestureRecognizer.PanUpdated += OnPanUpdatedAsyncForwarded;
            GestureRecognizers.Add(_panGestureRecognizer);

            _pinchGestureRecognizer = new PinchGestureRecognizer();
            _pinchGestureRecognizer.PinchUpdated += OnPinchUpdatedForwarded;
            GestureRecognizers.Add(_pinchGestureRecognizer);

            _doubleTapGestureRecognizer = new TapGestureRecognizer
            {
                NumberOfTapsRequired = 2
            };

            _doubleTapGestureRecognizer.Tapped += DoubleTappedAsyncForwarded;
            GestureRecognizers.Add(_doubleTapGestureRecognizer);

            _swipeGestureRecognizer = new SwipeGestureRecognizer { Threshold = 100,  Direction = SwipeDirection.Left | SwipeDirection.Right };
            _swipeGestureRecognizer.Swiped += OnSwiped;

            GestureRecognizers.Add(_swipeGestureRecognizer);
        }

        PanPinchContainer? realContainer = null;
        public void ConnectToRealContainer(PanPinchContainer real)
        {
            realContainer = real;
        }

        void OnTimerEnd()
        {
            IsVisible = true;
        }

        void OnSwiped(object? sender, SwipedEventArgs e)
        {
            if (!isSwipeEnabled)
            {
                return;
            }

            int disableTime = e.Direction == SwipeDirection.Right ? 10 : 2;
            var timer = Application.Current.Dispatcher.CreateTimer();
            timer.IsRepeating = false;
            timer.Interval = TimeSpan.FromSeconds(disableTime);
            timer.Tick += (s, e) => OnTimerEnd();
            timer.Start();
            // Disable the fake ui to allow for other interactions
            IsVisible = false;
        }

        public void OnPanUpdatedAsyncForwarded(object? sender, PanUpdatedEventArgs e)
        {
            if (realContainer == null)
            {
                return;
            }

            switch (e.StatusType)
            {
                case GestureStatus.Started:
                case GestureStatus.Running:
                    isSwipeEnabled = false;
                    break;
                case GestureStatus.Completed:
                case GestureStatus.Canceled:
                default:
                    var timer = Application.Current.Dispatcher.CreateTimer();
                    timer.IsRepeating = false;
                    timer.Interval = TimeSpan.FromSeconds(0.5);
                    timer.Tick += (s, e) => { isSwipeEnabled = true; };
                    timer.Start();
                    break;
            }

            realContainer.OnPanUpdatedAsync(sender, e);
            OnPanUpdatedAsync(sender, e);
        }

        public void OnPinchUpdatedForwarded(object? sender, PinchGestureUpdatedEventArgs e)
        {
            if (realContainer == null)
            {
                return;
            }

            realContainer.OnPinchUpdatedAsync(sender, e);
            OnPinchUpdatedAsync(sender, e);
        }

        public void DoubleTappedAsyncForwarded(object? sender, TappedEventArgs e)
        {
            if(realContainer == null)
            {
                return;
            }

            realContainer.DoubleTappedAsync(sender, e);
            DoubleTappedAsync(sender, e);
        }
    }
}
