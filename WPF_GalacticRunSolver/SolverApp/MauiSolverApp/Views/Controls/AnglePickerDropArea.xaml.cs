using Android.Graphics;
using Android.Icu.Number;
using MauiSolverApp.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using System.Diagnostics;
using static Android.InputMethodServices.Keyboard;

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
                    IsOnLine(x, y, Corners[3], Corners[0]);
        }

        static bool IsOnLine(int x, int y, SKPoint p1, SKPoint p2)
        {
            var length = Math.Sqrt((p1.X - p2.X) * (p1.X - p2.X) + (p1.Y - p2.Y) * (p1.Y - p2.Y));
            var crossProduct = (y - p1.Y) * (p2.X - p1.X) - (x - p1.X) * (p2.Y - p1.Y);

            if (Math.Abs(crossProduct) > length * 3)
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
        const int CORNER = 30;

#pragma warning disable CS8618
        public AnglePickerDropArea()
        {
            InitializeComponent();

            var tapGestureRecognizer = new TapGestureRecognizer();
            tapGestureRecognizer.Tapped += OnTapGestureRecognizerTapped;
            TapRectangle.GestureRecognizers.Add(tapGestureRecognizer);
        }
#pragma warning restore CS8618


        void OnTapGestureRecognizerTapped(object? sender, TappedEventArgs e)
        {
            var position = e.GetPosition(TapRectangle);
            if (position == null)
                return;
            // convert this point in reference of the surface without pan and zoom
            var refPoint = new SKPoint((float)position.Value.X / (float)TapRectangle.Width, (float)position.Value.Y / (float)TapRectangle.Height);
            // Calculate X and Y ratio 
            float xRatio = 1;
            float yRatio = 1;
            if (bitmap.Width / TapRectangle.Width > bitmap.Height / TapRectangle.Height)
            {
                var actualHeightInRectangle = bitmap.Height / (bitmap.Width / TapRectangle.Width);
                yRatio = (float)(TapRectangle.Height / actualHeightInRectangle);
            }
            else
            {
                var actualWidthInRectangle = bitmap.Width / (bitmap.Height / TapRectangle.Height);
                xRatio = (float)(TapRectangle.Width / actualWidthInRectangle);
            }

            // Now convert to pixel coordinate considering pan and zoom
            var panPoint = PanPinchContainer.GetPanPoint();
            var scale = PanPinchContainer.GetScale();
            Debug.WriteLine(panPoint);
            var pixelPoint = new SKPoint(
                xRatio * (refPoint.X * bitmap.Width / (float)scale - (float)panPoint.X * bitmap.Width),
                yRatio * (refPoint.Y * bitmap.Height / (float)scale - (float)panPoint.Y * bitmap.Height));
            // Now draw the corner that was chosen
            cornerSelection.Corners[cornerToBeSelected] = pixelPoint;
            canvasView.InvalidateSurface();
            // Point was chosen, hide tap rectangle
            TapRectangle.IsVisible = false;
        }


        int cornerToBeSelected = -1;
        internal void SelectCorner(int v)
        {
            TapRectangle.IsVisible = true;
            cornerToBeSelected = v;
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

        void OnCanvasViewPaintSurface(object sender, SKPaintSurfaceEventArgs args)
        {
            SKImageInfo info = args.Info;
            SKSurface surface = args.Surface;
            SKCanvas canvas = surface.Canvas;

            canvas.Clear(SKColors.Transparent);

            if (bitmap != null)
            {
                // Calculate rectangle for displaying bitmap
                // Scale and offset are handled by PanPinchContainer
                float scale = Math.Min((float)info.Width / bitmap.Width, (float)info.Height / bitmap.Height);
                float xOffset = 0;
                float yOffset = 0;
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
            PanPinchContainer.ResetPanAndScale();
            canvasView.InvalidateSurface();
        }

        public void StartRecognition()
        {
            // My 4 corners of output
            var outputCorners = new SKPoint[]
            {
                new SKPoint(0, 0),
                new SKPoint(1000, 0),
                new SKPoint(0, 1000),
                new SKPoint(1000, 1000),
            };

            // Get the 4 corners selected
            var inputCorners = cornerSelection.Corners;
            var temp = inputCorners[2];
            inputCorners[2] = inputCorners[3];
            inputCorners[3] = temp;

            //Compute H
            // Start by computing A
            double[,] A = new double[8,9];

            for (int i = 0; i < 4; i++)
            {
                var X = inputCorners[i];
                var x = outputCorners[i].X;
                var y = outputCorners[i].Y;
                // pair rows
                A[2 * i,0] = 0;
                A[2 * i,1] = 0;
                A[2 * i,2] = 0;
                A[2 * i,3] = -X.X;
                A[2 * i,4] = -X.Y;
                A[2 * i,5] = -1;
                A[2 * i,6] = y * X.X;
                A[2 * i,7] = y * X.Y;
                A[2 * i,8] = y;
                // odd rows
                A[2 * i + 1, 0] = X.X;
                A[2 * i + 1, 1] = X.Y;
                A[2 * i + 1, 2] = 1;
                A[2 * i + 1, 3] = 0;
                A[2 * i + 1, 4] = 0;
                A[2 * i + 1, 5] = 0;
                A[2 * i + 1, 6] = -x * X.X;
                A[2 * i + 1, 7] = -x * X.Y;
                A[2 * i + 1, 8] = -x;
            }

            // Output variables for SVD
            double[] w;       // Singular values
            double[,] u;      // Left singular vectors
            double[,] vt;     // Right singular vectors transposed

            // Perform SVD
            // Flags:
            //   true  -> compute U
            //   true  -> compute VT
            //   2     -> algorithm type (0=default, 1=QR, 2=Divide-and-Conquer)
            alglib.rmatrixsvd(A, A.GetLength(0), A.GetLength(1),
                              1, 2, 2, // compute U, VT, algorithm type
                              out w, out u, out vt);

            // Print VT
            //Console.WriteLine("\nMatrix VT:");
            //PrintMatrix(vt);

            // Get H
            double N = vt[8, 8];
            double[,] H = new double[3, 3];
            for (int i = 0; i < 3; i++)
            {
                H[i, 0] = vt[8, i * 3] / N;
                H[i, 1] = vt[8, i * 3 + 1] / N;
                H[i, 2] = vt[8, i * 3 + 2] / N;
            }
            //PrintMatrix(H);

            // Let's inverse H to find proper points
            int info;
            alglib.matinvreport rep;
            alglib.rmatrixinverse(ref H, out info, out rep);

            //Now we have H, we can create our new bitmap and populate each pixel with pixels from the original bitmap
            SKBitmap outputBitmap = new SKBitmap(1000, 1000);
            for (int i = 0; i < 1000; i++) 
            {
                for (int j = 0; j < 200; j++)
                {
                    // Find corresponding pixel in input image
                    var p = GetInputPoint(i, j, H);
                    p.X = Math.Max(0, p.X);
                    p.Y = Math.Max(0, p.Y);
                    p.X = Math.Min(bitmap.Width - 1, p.X);
                    p.Y = Math.Min(bitmap.Height - 1, p.Y);
                    var color = bitmap.GetPixel((int)p.X, (int)p.Y);
                    //outputBitmap.SetPixel(i, j, color);
                }
            }

            // Now we replace the bitmap and draw it on screen
            bitmap = outputBitmap;

            SKRect bitmapRect = new SKRect(0, 0, bitmap.Width, bitmap.Height);
            cornerSelection = new BoardSelectionZone(bitmapRect);

            canvasView.InvalidateSurface();
        }

        static SKPoint GetInputPoint(double x, double y, double[,] H)
        {
            double[] p = new double[3] { x, y, 1 };
            double[] p2 = new double[3];
            for (int i = 0; i < 3; i++)
            {
                p2[i] = H[i, 0] * p[0] + H[i, 1] * p[1] + H[i, 2] * p[2];
            }
            return new SKPoint((float)(p2[0] / p2[2]), (float)(p2[1] / p2[2]));
        }

        // Helper method to print a 2D matrix
        static void PrintMatrix(double[,] mat)
        {
            int rows = mat.GetLength(0);
            int cols = mat.GetLength(1);
            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < cols; j++)
                    Console.Write($"{mat[i, j],10:F6} ");
                Console.WriteLine();
            }
        }

        //public void StartRecognition_Old()
        //{
        //    // Crop and reset to center
        //    bitmap = GetCroppedBitmap();
        //    cornerSelection.MoveBackToBounds();
        //    ResetPanAndZoom();

        //    // Modify colors
        //    IntPtr pixelsAddr = bitmap.GetPixels();
        //    const int blackWhiteThreshold = 175;
        //    unsafe
        //    {
        //        byte* ptr = (byte*)pixelsAddr.ToPointer();

        //        for (int row = 0; row < bitmap.Height; row++)
        //            for (int col = 0; col < bitmap.Width; col++)
        //            {
        //                if (cornerSelection.IsPointInside(col, row))
        //                {
        //                    if (cornerSelection.IsOnAnyEdge(col, row))
        //                    {
        //                        *ptr++ = 255;    // red
        //                        *ptr++ = 0;    // green
        //                        *ptr++ = 0;    // blue
        //                        *ptr++ = 0x00; // alpha
        //                    }
        //                    else if (cornerSelection.IsOnLateralLine(col, row))
        //                    {
        //                        *ptr++ = 255;    // red
        //                        *ptr++ = 0;    // green
        //                        *ptr++ = 125;    // blue
        //                        *ptr++ = 0x00; // alpha
        //                    }
        //                    else if (cornerSelection.IsOnVerticalLine(col, row))
        //                    {
        //                        *ptr++ = 255;    // red
        //                        *ptr++ = 125;    // green
        //                        *ptr++ = 0;    // blue
        //                        *ptr++ = 0x00; // alpha
        //                    }
        //                    else
        //                    {
        //                        //var red = *(ptr);
        //                        //var green = *(ptr + 1);
        //                        //var blue = *(ptr + 2);
        //                        //var grayshade = 0.2126 * red + 0.7152 * green + 0.0722 * blue;
        //                        //byte result = (byte)(grayshade > blackWhiteThreshold ? 255 : 0);
        //                        //*ptr++ = result;   // red
        //                        //*ptr++ = result;   // green
        //                        //*ptr++ = result;   // blue
        //                        // ptr++;                // alpha
        //                        ptr++;                
        //                        ptr++;                
        //                        ptr++;               
        //                        ptr++;                // alpha
        //                    }
        //                }
        //                else
        //                {
        //                    *ptr++ = 0;    // red
        //                    *ptr++ = 0;    // green
        //                    *ptr++ = 0;    // blue
        //                    *ptr++ = 0x00; // alpha
        //                }
        //            }
        //    }

        //    // Redraw
        //    canvasView.InvalidateSurface();

        //    // recognize walls
        //    //for (int i  = 0; i < 16-15; i++)
        //    //{
        //    //    for (int j = 0; j < 16; j++)
        //    //    {
        //    //        var leftPoint = cornerSelection.GetCellPosition(i, j, EPosition.Left);
        //    //    }
        //    //}
        //}
    }
}