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
using static SolverApp.Views.Controls.BoardSelectionZone;

namespace SolverApp.Views.Controls
{
    class BoardSelectionZone
    {
        public enum EPosition
        {
            Left,
            Top,
            Right,
            Bottom,
        }

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

        public SKRect GetBounds()
        {
            return new SKRect(Math.Min(Corners[0].X, Corners[3].X),
                Math.Min(Corners[0].Y, Corners[1].Y),
                Math.Max(Corners[1].X, Corners[2].X),
                Math.Max(Corners[2].Y, Corners[3].Y));
        }

        public void MoveBackToBounds()
        {
            var bounds = GetBounds();
            for (int i = 0; i < Corners.Length; i++)
            {
                Corners[i].X -= bounds.Left;
                Corners[i].Y -= bounds.Top;
            }
        }

        public void MoveCorner(int index, SKPoint point)
        {
            Corners[index].X = Math.Min(Math.Max(point.X, maxRect.Left), maxRect.Right - MINIMUM);
            Corners[index].Y = Math.Min(Math.Max(point.Y, maxRect.Top), maxRect.Bottom - MINIMUM);
        }

        public bool IsOnLeftSideOf(SKPoint Pt1, SKPoint Pt2, int x, int y)
        {
            return (Pt2.X - Pt1.X) * (y - Pt1.Y) - (Pt2.Y - Pt1.Y) * (x - Pt1.X) >= 0;
        }

        internal bool IsPointInside(int x, int y)
        {
            return IsOnLeftSideOf(Corners[0], Corners[1], x, y) &&
                    IsOnLeftSideOf(Corners[1], Corners[2], x, y) &&
                    IsOnLeftSideOf(Corners[2], Corners[3], x, y) &&
                    IsOnLeftSideOf(Corners[3], Corners[0], x, y);
        }

        internal object GetCellPosition(int i, int j, EPosition pos)
        {
            throw new NotImplementedException();
        }

        internal bool IsOnAnyEdge(int x, int y)
        {
            return IsOnLine(x, y, Corners[0], Corners[1]) ||
                    IsOnLine(x, y, Corners[1], Corners[2]) ||
                    IsOnLine(x, y, Corners[2], Corners[3]) ||
                    IsOnLine(x, y, Corners[3], Corners[0]) ;
        }

        static bool IsOnLine(int x, int y, SKPoint p1, SKPoint p2)
        {
            var length = Math.Sqrt((p1.X-p2.X)* (p1.X - p2.X)+ (p1.Y-p2.Y)* (p1.Y - p2.Y));
            var crossProduct = (y - p1.Y) * (p2.X - p1.X) - (x - p1.X) * (p2.Y - p1.Y);

            if (Math.Abs(crossProduct) > length*3)
            {
                return false;
            }
            return true;
        }

        internal bool IsOnLateralLine(int x, int y)
        {
            for (int i = 1; i < 16; i++)
            {
                var x1 = Corners[0].X + (Corners[3].X - Corners[0].X) / 16 * i;
                var y1 = Corners[0].Y + (Corners[3].Y - Corners[0].Y) / 16 * i;
                var p1 = new SKPoint(x1, y1);
                var x2 = Corners[1].X + (Corners[2].X - Corners[1].X) / 16 * i;
                var y2 = Corners[1].Y + (Corners[2].Y - Corners[1].Y) / 16 * i;
                var p2 = new SKPoint(x2, y2);
                if (IsOnLine(x, y, p1, p2))
                {
                    return true;
                }
            }
            
            return false;
        }

        internal bool IsOnVerticalLine(int x, int y)
        {
            for (int i = 1; i < 16; i++)
            {
                var x1 = Corners[0].X + (Corners[1].X - Corners[0].X) / 16 * i;
                var y1 = Corners[0].Y + (Corners[1].Y - Corners[0].Y) / 16 * i;
                var p1 = new SKPoint(x1, y1);
                var x2 = Corners[3].X + (Corners[2].X - Corners[3].X) / 16 * i;
                var y2 = Corners[3].Y + (Corners[2].Y - Corners[3].Y) / 16 * i;
                var p2 = new SKPoint(x2, y2);
                if (IsOnLine(x, y, p1, p2))
                {
                    return true;
                }
            }

            return false;
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

            canvas.Clear(SKColors.Transparent);

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
            ResetPanAndZoom();
        }

        private void ResetPanAndZoom()
        {
            PanPoint = new SKPoint(0, 0);
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


        public SKBitmap GetCroppedBitmap()
        {
            SKRect cropRect = cornerSelection.GetBounds() ;
            SKBitmap croppedBitmap = new SKBitmap((int)cropRect.Width,
                                                    (int)cropRect.Height);
            SKRect dest = new SKRect(0, 0, cropRect.Width, cropRect.Height);
            SKRect source = new SKRect(cropRect.Left, cropRect.Top,
                                        cropRect.Right, cropRect.Bottom);

            using (SKCanvas canvas = new SKCanvas(croppedBitmap))
            {
                canvas.DrawBitmap(bitmap, source, dest);
            }

            return croppedBitmap;
        }

        private static SKMatrix ComputeMatrix(SKSize size, SKPoint ptUL, SKPoint ptUR, SKPoint ptLL)
        {
            // Scale transform
            SKMatrix S = SKMatrix.CreateScale(1 / size.Width, 1 / size.Height);

            // Affine transform
            SKMatrix A = new SKMatrix
            {
                ScaleX = ptUR.X - ptUL.X,
                SkewY = ptUR.Y - ptUL.Y,
                SkewX = ptLL.X - ptUL.X,
                ScaleY = ptLL.Y - ptUL.Y,
                TransX = ptUL.X,
                TransY = ptUL.Y,
                Persp2 = 1
            };

            SKMatrix result = SKMatrix.MakeIdentity();
            SKMatrix.Concat(ref result, A, S);
            return result;
        }
        public void StartRecognition()
        {
            // Crop and reset to center
            bitmap = GetCroppedBitmap();
            cornerSelection.MoveBackToBounds();
            ResetPanAndZoom();

            // Modify colors
            IntPtr pixelsAddr = bitmap.GetPixels();
            const int blackWhiteThreshold = 175;
            unsafe
            {
                byte* ptr = (byte*)pixelsAddr.ToPointer();

                for (int row = 0; row < bitmap.Height; row++)
                    for (int col = 0; col < bitmap.Width; col++)
                    {
                        if (cornerSelection.IsPointInside(col, row))
                        {
                            if (cornerSelection.IsOnAnyEdge(col, row))
                            {
                                *ptr++ = 255;    // red
                                *ptr++ = 0;    // green
                                *ptr++ = 0;    // blue
                                *ptr++ = 0x00; // alpha
                            }
                            else if (cornerSelection.IsOnLateralLine(col, row))
                            {
                                *ptr++ = 255;    // red
                                *ptr++ = 0;    // green
                                *ptr++ = 125;    // blue
                                *ptr++ = 0x00; // alpha
                            }
                            else if (cornerSelection.IsOnVerticalLine(col, row))
                            {
                                *ptr++ = 255;    // red
                                *ptr++ = 125;    // green
                                *ptr++ = 0;    // blue
                                *ptr++ = 0x00; // alpha
                            }
                            else
                            {
                                //var red = *(ptr);
                                //var green = *(ptr + 1);
                                //var blue = *(ptr + 2);
                                //var grayshade = 0.2126 * red + 0.7152 * green + 0.0722 * blue;
                                //byte result = (byte)(grayshade > blackWhiteThreshold ? 255 : 0);
                                //*ptr++ = result;   // red
                                //*ptr++ = result;   // green
                                //*ptr++ = result;   // blue
                                // ptr++;                // alpha
                                ptr++;                
                                ptr++;                
                                ptr++;               
                                ptr++;                // alpha
                            }
                        }
                        else
                        {
                            *ptr++ = 0;    // red
                            *ptr++ = 0;    // green
                            *ptr++ = 0;    // blue
                            *ptr++ = 0x00; // alpha
                        }
                    }
            }

            // Redraw
            canvasView.InvalidateSurface();

            // recognize walls
            //for (int i  = 0; i < 16-15; i++)
            //{
            //    for (int j = 0; j < 16; j++)
            //    {
            //        var leftPoint = cornerSelection.GetCellPosition(i, j, EPosition.Left);
            //    }
            //}
        }
    }
}