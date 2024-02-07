using SkiaSharp.Views.Forms;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System;

namespace SolverApp.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AnglePickerDropArea : ContentView
    {
        public AnglePickerDropArea()
        {
            InitializeComponent();
            ZoomInOrOut(2, defaultSize);

            TouchEffect touchEffect = new TouchEffect();
            touchEffect.TouchAction += OnPictureTouched;
            AnglePickerArea.Effects.Add(touchEffect);
        }

        static int AnglePickerSize = 25;
        public void ResetAngles()
        {
            var width = ZoomableArea.Width;
            var height = ZoomableArea.Height;
            if (width <= 0 || height <= 0)
            {
                width = defaultSize * 2 - 50;
                height = defaultSize * 2 - 50;
            }
            AnglePickerArea.Children.Clear();
            Angles.Clear();
            AddBoxViewToLayout(new Point(0, 0));
            AddBoxViewToLayout(new Point(width - AnglePickerSize, 0));
            AddBoxViewToLayout(new Point(width - AnglePickerSize, height - AnglePickerSize));
            AddBoxViewToLayout(new Point(0, height - AnglePickerSize));
            canvasView.InvalidateSurface();
        }

        private List<BoxView> Angles = new List<BoxView>();
        void AddBoxViewToLayout(Point position)
        {
            BoxView boxView = new BoxView
            {
                WidthRequest = AnglePickerSize,
                HeightRequest = AnglePickerSize,
                Color = Color.Red
            };

            TouchEffect touchEffect = new TouchEffect();
            touchEffect.TouchAction += OnAngleTouched;
            boxView.Effects.Add(touchEffect);
            AnglePickerArea.Children.Add(boxView, position);

            Angles.Add(boxView);
        }

        BoxView selected = null;

        void OnAngleTouched(object sender, TouchActionEventArgs args)
        {
            BoxView boxView = sender as BoxView;

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (selected == null)
                    {
                        selected = boxView;
                        boxView.Color = Color.Orange;
                        // Set Capture property to true
                        TouchEffect touchEffect = (TouchEffect)boxView.Effects.FirstOrDefault(e => e is TouchEffect);
                        touchEffect.Capture = true;
                    }
                    break;
            }
        }

        void OnPictureTouched(object sender, TouchActionEventArgs args)
        {
            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    if (selected != null)
                    {
                        var boxView = selected;
                        boxView.Color = Color.Red;
                        // Move to position
                        Rectangle rect = AbsoluteLayout.GetLayoutBounds(boxView);
                        rect.X = args.Location.X - AnglePickerSize / 2;
                        rect.Y = args.Location.Y - AnglePickerSize / 2;
                        AbsoluteLayout.SetLayoutBounds(boxView, rect);
                        canvasView.InvalidateSurface();
                        // Set Capture property to true
                        TouchEffect touchEffect = (TouchEffect)boxView.Effects.FirstOrDefault(e => e is TouchEffect);
                        touchEffect.Capture = true;
                        selected = null;
                    }
                    break;
            }
        }

        SKPaint paintRed = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            StrokeWidth = 10,
            StrokeCap = SKStrokeCap.Round,
            StrokeJoin = SKStrokeJoin.Round
        };

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKCanvas canvas = args.Surface.Canvas;
            canvas.Clear();

            if (Angles.Count > 0)
            {
                SKPath path = new SKPath();
                path.MoveTo(ConvertToPixel(AbsoluteLayout.GetLayoutBounds(Angles[0]).Location));
                path.LineTo(ConvertToPixel(AbsoluteLayout.GetLayoutBounds(Angles[1]).Location));
                path.LineTo(ConvertToPixel(AbsoluteLayout.GetLayoutBounds(Angles[2]).Location));
                path.LineTo(ConvertToPixel(AbsoluteLayout.GetLayoutBounds(Angles[3]).Location));
                path.LineTo(ConvertToPixel(AbsoluteLayout.GetLayoutBounds(Angles[0]).Location));
                canvas.DrawPath(path, paintRed);
            }
        }

        SKPoint ConvertToPixel(Point pt)
        {
            var center = new Point(pt.X + AnglePickerSize/2, pt.Y + AnglePickerSize/2);
            return new SKPoint((float)(canvasView.CanvasSize.Width * center.X / canvasView.Width),
                               (float)(canvasView.CanvasSize.Height * center.Y / canvasView.Height));
        }

        public void SetPhoto(string path)
        {
            PhotoViewer.Source = path;
        }

        public void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            ZoomInOrOut(1 + e.NewValue / 500, Math.Max(TheScrollView.Width, TheScrollView.Height));
        }
        private static int defaultSize = 400;
        public int _ZoomSize { get; set; } = defaultSize;
        internal void ZoomInOrOut(double zoom, double scrollViewWidth)
        {
            _ZoomSize = (int)(zoom * scrollViewWidth - 50);
            var previousWidth = ZoomableArea.Width;
            var previousHeight = ZoomableArea.Height;
            ZoomableArea.WidthRequest = _ZoomSize;
            ZoomableArea.HeightRequest = _ZoomSize;
            foreach (var boxView in Angles)
            {
                // getPreviousPosition
                var previousX = boxView.X;
                var previousY = boxView.Y;

                // Move to position
                Rectangle rect = AbsoluteLayout.GetLayoutBounds(boxView);
                rect.X = previousX * _ZoomSize / previousWidth;
                rect.Y = previousY * _ZoomSize / previousWidth;
                AbsoluteLayout.SetLayoutBounds(boxView, rect);
            }
            canvasView.InvalidateSurface();
        }
    }
}