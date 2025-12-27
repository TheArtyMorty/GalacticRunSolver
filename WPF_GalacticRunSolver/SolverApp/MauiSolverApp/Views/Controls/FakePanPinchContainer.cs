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
        }

        PanPinchContainer? realContainer = null;
        public void ConnectToRealContainer(PanPinchContainer real)
        {
            realContainer = real;
        }

        public void OnPanUpdatedAsyncForwarded(object? sender, PanUpdatedEventArgs e)
        {
            if (realContainer == null)
            {
                return;
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
