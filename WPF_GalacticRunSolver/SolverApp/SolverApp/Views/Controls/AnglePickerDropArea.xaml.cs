using SkiaSharp.Views.Forms;
using SkiaSharp;
using System.Collections.Generic;
using System.Linq;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using System;
using System.IO;
using Xamarin.Forms.Shapes;

namespace SolverApp.Views.Controls
{
    class BoardSelectionZone
    {
        const float MINIMUM = 10;   // pixels width or height

        SKRect maxRect;             // generally the size of the bitmap

        public BoardSelectionZone(SKRect maxRect)
        {
            this.maxRect = maxRect;

            var Left = maxRect.Left;
            var Top = maxRect.Top;
            var Right = maxRect.Right;
            var Bottom = maxRect.Bottom;

            Corners = new SKPoint[] { 
                    new SKPoint(Left, Top),
                    new SKPoint(Right, Top),
                    new SKPoint(Right, Bottom),
                    new SKPoint(Left, Bottom) };
        }

        public SKPoint[] Corners { get; set; }

        public int HitTest(SKPoint point, float radius)
        {
            SKPoint[] corners = Corners;

            for (int index = 0; index < corners.Length; index++)
            {
                SKPoint diff = point - corners[index];

                if ((float)Math.Sqrt(diff.X * diff.X + diff.Y * diff.Y) < radius)
                {
                    return index;
                }
            }

            return -1;
        }

        public void MoveCorner(int index, SKPoint point)
        {
            Corners[index].X = Math.Min(Math.Max(point.X, maxRect.Left), maxRect.Right - MINIMUM);
            Corners[index].Y = Math.Min(Math.Max(point.Y, maxRect.Top), maxRect.Bottom - MINIMUM);
        }
    }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AnglePickerDropArea : ContentView
    {
        BoardSelectionZone cornerSelection;
        SKMatrix inverseBitmapMatrix;
        const int CORNER = 30;
        const int RADIUS = 200;     // pixel radius of touch hit-test

        public AnglePickerDropArea()
        {
            InitializeComponent();

            touchEffect.TouchAction += OnTouchEffectTouchAction;
            canvasView.Effects.Add(touchEffect);
        }

        // Touch tracking
        TouchEffect touchEffect = new TouchEffect();
        struct TouchPoint
        {
            public int CornerIndex { set; get; }
            public SKPoint Offset { set; get; }
        }

        Dictionary<long, TouchPoint> touchPoints = new Dictionary<long, TouchPoint>();
        private long panningTouchPoint = -1;
        private SKPoint panningOffset;

        void OnTouchEffectTouchAction(object sender, TouchActionEventArgs args)
        {
            SKPoint pixelLocation = ConvertToPixel(args.Location);
            SKPoint bitmapLocation = inverseBitmapMatrix.MapPoint(pixelLocation);

            switch (args.Type)
            {
                case TouchActionType.Pressed:
                    // Convert radius to bitmap/cropping scale
                    float radius = inverseBitmapMatrix.ScaleX * RADIUS;

                    // Find corner that the finger is touching
                    int cornerIndex = cornerSelection.HitTest(bitmapLocation, radius);

                    if (cornerIndex != -1 && !touchPoints.ContainsKey(args.Id))
                    {
                        TouchPoint touchPoint = new TouchPoint
                        {
                            CornerIndex = cornerIndex,
                            Offset = bitmapLocation - cornerSelection.Corners[cornerIndex]
                        };

                        touchPoints.Add(args.Id, touchPoint);
                    }
                    else if (panningTouchPoint == -1)
                    {
                        panningTouchPoint = args.Id;
                        panningOffset = bitmapLocation;
                    }
                    break;

                case TouchActionType.Moved:
                    if (touchPoints.ContainsKey(args.Id))
                    {
                        TouchPoint touchPoint = touchPoints[args.Id];
                        cornerSelection.MoveCorner(touchPoint.CornerIndex,
                                                bitmapLocation - touchPoint.Offset);
                        canvasView.InvalidateSurface();
                    }
                    else if (panningTouchPoint == args.Id)
                    {
                        PanPoint += (bitmapLocation - panningOffset);
                        canvasView.InvalidateSurface();
                    }
                    break;

                case TouchActionType.Released:
                case TouchActionType.Cancelled:
                    if (touchPoints.ContainsKey(args.Id))
                    {
                        touchPoints.Remove(args.Id);
                    }
                    else
                    {
                        panningTouchPoint = -1;
                    }
                    break;
            }
        }

        SKPoint ConvertToPixel(TouchTrackingPoint pt)
        {
            return new SKPoint((float)(canvasView.CanvasSize.Width * pt.X / Width),
                               (float)(canvasView.CanvasSize.Height * pt.Y / Height));
        }

        // Drawing objects
        SKPaint cornerStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            StrokeWidth = 15
        };

        SKPaint edgeStroke = new SKPaint
        {
            Style = SKPaintStyle.Stroke,
            Color = SKColors.Red,
            StrokeWidth = 5
        };

        private SKPoint PanPoint;
        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.White);

            if (bitmap != null)
            {
                // Calculate rectangle for displaying bitmap
                //float scale = Math.Min((float)info.Width / bitmap.Width, (float)info.Height / bitmap.Height);
                SetDefaultScaleIfNeeded(info);
                float scale = (float)_scale;
                ClampPanPointIfNeeded(info, scale);
                float xOffset = (float)PanPoint.X; 
                float yOffset = (float)PanPoint.Y;
                SKRect bitmapRect = new SKRect(xOffset, yOffset, xOffset + scale * bitmap.Width, yOffset + scale * bitmap.Height);
                canvas.DrawBitmap(bitmap, bitmapRect);

                // Calculate a matrix transform for displaying the cropping rectangle
                SKMatrix bitmapScaleMatrix = SKMatrix.CreateScaleTranslation(scale, scale, xOffset, yOffset);

                // Display corners and edges
                SKPath edgePath = new SKPath();
                edgePath.MoveTo(xOffset + scale * cornerSelection.Corners[3].X,
                    yOffset + scale * cornerSelection.Corners[3].Y);

                var cornerPath = new SKPath();

                foreach (var corner in cornerSelection.Corners)
                {
                    var X = xOffset + scale * corner.X;
                    var Y = yOffset + scale * corner.Y;
                    cornerPath.MoveTo(X, Y + CORNER);
                    cornerPath.LineTo(X, Y - CORNER);
                    cornerPath.MoveTo(X - CORNER, Y);
                    cornerPath.LineTo(X + CORNER, Y);
                    edgePath.LineTo(X, Y);
                }

                canvas.DrawPath(edgePath, edgeStroke);
                canvas.DrawPath(cornerPath, cornerStroke);

                // Invert the transform for touch tracking
                bitmapScaleMatrix.TryInvert(out inverseBitmapMatrix);
            }
        }

        private void ClampPanPointIfNeeded(SKImageInfo info, float scale)
        {
            var borderOffset = 0;
            var minX = Math.Min(0, info.Width - scale * bitmap.Width)- borderOffset;
            var minY = Math.Min(0, info.Height - scale * bitmap.Height)- borderOffset;
            var maxX = Math.Max(0, info.Width - scale * bitmap.Width) + borderOffset;
            var maxY = Math.Max(0, info.Height - scale * bitmap.Height) + borderOffset;
            PanPoint.X = Math.Max(PanPoint.X, minX);
            PanPoint.Y = Math.Max(PanPoint.Y, minY);
            PanPoint.X = Math.Min(PanPoint.X, maxX);
            PanPoint.Y = Math.Min(PanPoint.Y, maxY);
        }

        private void SetDefaultScaleIfNeeded(SKImageInfo info)
        {
            if (_defaultScale <= 0)
            {
                _defaultScale = Math.Min((float)info.Width / bitmap.Width, (float)info.Height / bitmap.Height); ;
                _scale = _defaultScale <= 0 ? 1 : _defaultScale;
            }
        }

        private SKBitmap bitmap;
        public void SetPhoto(string path)
        {
            if (path.Length > 0)
            {
                var fileStream = File.OpenRead(path);
                bitmap = SKBitmap.Decode(fileStream);

                SKRect bitmapRect = new SKRect(0, 0, bitmap.Width, bitmap.Height);
                cornerSelection = new BoardSelectionZone(bitmapRect);
            }
            else
            {
                bitmap = null;
            }
            _defaultScale = -1;
            ZoomSlider.Value = 0;
            canvasView.InvalidateSurface();
        }


        public void OnSliderValueChanged(object sender, ValueChangedEventArgs e)
        {
            ZoomInOrOut(e.NewValue, Math.Max(canvasView.Width, canvasView.Height));
        }

        private double _defaultScale;
        private double _scale;
 
        internal void ZoomInOrOut(double zoom, double scrollViewWidth)
        {
            _scale = _defaultScale + zoom;
            canvasView.InvalidateSurface();
        }
    }
}