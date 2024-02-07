using SkiaSharp.Views.Forms;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System;
using Xamarin.Forms.Internals;
using SolverApp.ViewModels;
using System.ComponentModel;

namespace SolverApp.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AnglePickerDropArea : ContentView
    {
        public AnglePickerDropArea()
        {
            InitializeComponent();
            ZoomInOrOut(3, defaultSize);
        }

        class DragInfo
        {
            public DragInfo(long id, Point pressPoint)
            {
                Id = id;
                PressPoint = pressPoint;
            }

            public long Id { private set; get; }

            public Point PressPoint { private set; get; }
        }

        static int AnglePickerSize = 25;
        public void ResetAngles()
        {
            var width = ZoomableArea.Width;
            var height = ZoomableArea.Height;
            if (width == 0  || height == 0)
            {
                width = defaultSize * 3 - 50;
                height = defaultSize * 3 - 50;
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
            touchEffect.TouchAction += OnTouchEffectAction;
            boxView.Effects.Add(touchEffect);
            AnglePickerArea.Children.Add(boxView, position);

            Angles.Add(boxView);
        }

        Dictionary<BoxView, DragInfo> dragDictionary = new Dictionary<BoxView, DragInfo>();

        void OnTouchEffectAction(object sender, TouchActionEventArgs args)
        {
            BoxView boxView = sender as BoxView;

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    // Don't allow a second touch on an already touched BoxView
                    if (!dragDictionary.ContainsKey(boxView))
                    {
                        dragDictionary.Add(boxView, new DragInfo(args.Id, new Point(args.Location.X, args.Location.Y)));

                        // Set Capture property to true
                        TouchEffect touchEffect = (TouchEffect)boxView.Effects.FirstOrDefault(e => e is TouchEffect);
                        touchEffect.Capture = true;
                    }
                    break;

                case TouchActionType.Moved:
                    if (dragDictionary.ContainsKey(boxView) && dragDictionary[boxView].Id == args.Id)
                    {
                        Rectangle rect = AbsoluteLayout.GetLayoutBounds(boxView);
                        Point initialLocation = dragDictionary[boxView].PressPoint;
                        rect.X += args.Location.X - initialLocation.X;
                        rect.Y += args.Location.Y - initialLocation.Y;
                        rect.X = Math.Max(rect.X, -AnglePickerSize/2);
                        rect.Y = Math.Max(rect.Y, -AnglePickerSize / 2);
                        rect.X = Math.Min(rect.X, AnglePickerArea.Width - AnglePickerSize / 2);
                        rect.Y = Math.Min(rect.Y, AnglePickerArea.Height - AnglePickerSize / 2);
                        AbsoluteLayout.SetLayoutBounds(boxView, rect);
                        canvasView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                    if (dragDictionary.ContainsKey(boxView) && dragDictionary[boxView].Id == args.Id)
                    {
                        dragDictionary.Remove(boxView);
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
            ZoomInOrOut(e.NewValue / 1000, Math.Max(TheScrollView.Width, TheScrollView.Height));
        }
        private static int defaultSize = 400;
        public int _ZoomSize { get; set; } = defaultSize;
        internal void ZoomInOrOut(double v, double scrollViewWidth)
        {
            _ZoomSize = (int)(-50 + scrollViewWidth + v * scrollViewWidth);
            ZoomableArea.WidthRequest = _ZoomSize;
            ZoomableArea.HeightRequest = _ZoomSize;
        }
    }
}