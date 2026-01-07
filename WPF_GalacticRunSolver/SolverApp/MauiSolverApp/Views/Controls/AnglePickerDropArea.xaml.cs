using Android.Graphics;
using Android.Icu.Number;
using Kotlin.Coroutines;
using MauiSolverApp.Utilities;
using SkiaSharp;
using SkiaSharp.Views.Maui;
using SolverApp.ViewModels;
using System.Diagnostics;
using static Android.InputMethodServices.Keyboard;

namespace SolverApp.Views.Controls
{
    class BoardSelectionZone
    {
        SKRect maxRect;             // generally the size of the bitmap

        public BoardSelectionZone(SKRect rect)
        {
            maxRect = rect;

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
            var outputSize = 500;
            // My 4 corners of output
            var outputCorners = new SKPoint[]
            {
                new SKPoint(0, 0),
                new SKPoint(outputSize, 0),
                new SKPoint(0, outputSize),
                new SKPoint(outputSize, outputSize),
            };

            // Get the 4 corners selected
            var inputCorners = new List<SKPoint> { };
            foreach (var corner in cornerSelection.Corners)
            {
                inputCorners.Add(corner);
            }
                
            if (true) // Hack
            {
                inputCorners.Clear();
                inputCorners.Add(new SKPoint(29, 232));
                inputCorners.Add(new SKPoint(1138, 270));
                inputCorners.Add(new SKPoint(1148, 1318));
                inputCorners.Add(new SKPoint(48, 1406));
            }
            // Exchange bottom corners to have proper order
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
            SKBitmap outputBitmap = new SKBitmap(outputSize, outputSize);

            IntPtr outputPixelsAddr = outputBitmap.GetPixels();
            unsafe
            {
                byte* ptr = (byte*)outputPixelsAddr.ToPointer();
                for (int row = 0; row < outputBitmap.Height; row++)
                {
                    for (int col = 0; col < outputBitmap.Width; col++)
                    {
                        var p = GetInputPoint(col, row, H);
                        var x = (int)p.X;
                        var y = (int)p.Y;
                        var color = bitmap.GetPixel(x, y);
                        //Change output color
                        *ptr++ = (byte)color.Red;    // red
                        *ptr++ = (byte)color.Green;  // green
                        *ptr++ = (byte)color.Blue;   // blue
                        *ptr++ = (byte)color.Alpha;  // alpha
                    }
                }   
            }

            // Now we replace the bitmap and draw it on screen
            bitmap = outputBitmap;

            SKRect bitmapRect = new SKRect(0, 0, bitmap.Width, bitmap.Height);
            cornerSelection = new BoardSelectionZone(bitmapRect);

            PanPinchContainer.ResetPanAndScale();
            canvasView.InvalidateSurface();

            //Let's also copy this image to the solver page background
            var newFile = System.IO.Path.Combine(FileSystem.CacheDirectory, "BackgroundPhotoForSolver");
            using (var newStream = File.OpenWrite(newFile))
                bitmap.Encode(newStream, SKEncodedImageFormat.Jpeg, 90);

            var Parent = this.Parent;
            while (Parent != null && !(Parent is PhotoHelperPage))
            {
                Parent = Parent.Parent;
            }

            if (Parent is PhotoHelperPage photoHelperPage)
            {
                var dataContext = photoHelperPage.BindingContext as PhotoHelperViewModel;
                if (dataContext != null)
                    dataContext.SetBackGroundImage(newFile);
            }

            // Ok so let's start recognition !
            var map = RecognizeMap();
        }

        private MapViewModel RecognizeMap()
        {
            var map = new MapViewModel(16);
            double caseSize = bitmap.Width / 16.0;
            // First, let's observe all 60 outside connexions, we should find 8 walls 
            var outsideConnexions = new List<List<SKColor>> { new List<SKColor>(), new List<SKColor>(), new List<SKColor>(), new List<SKColor>() };
            for (int i = 0; i < 15; i++)
            {
                // First row
                {
                    var x = (int)((i + 1) * caseSize);
                    var y = (int)(caseSize/2);
                    var color = GetColorForPoint(x, y, true);
                    outsideConnexions[0].Add(color);
                    ColorPoint(x, y, color, true);
                }
                // Last row
                {
                    var x = (int)((i + 1) * caseSize);
                    var y = (int)(15 * caseSize + caseSize / 2);
                    var color = GetColorForPoint(x, y, true);
                    outsideConnexions[1].Add(color);
                    ColorPoint(x, y, color, true);
                }
                // First Column
                {
                    var x = (int)(caseSize / 2);
                    var y = (int)((i + 1) * caseSize);
                    var color = GetColorForPoint(x, y, false);
                    outsideConnexions[2].Add(color);
                    ColorPoint(x, y, color, false);
                }
                // Last Column
                {
                    var x = (int)(15 * caseSize + caseSize / 2);
                    var y = (int)((i + 1) * caseSize);
                    var color = GetColorForPoint(x, y, false);
                    outsideConnexions[3].Add(color);
                    ColorPoint(x, y, color, false);
                }
            }
            // We expect the first and last connexions not to be walls, so we can decide this is the not wall color)
            // Let's print all distances to that color
            var notWallColor = GetMediumColor(new List<SKColor> {
                outsideConnexions[0].First(), outsideConnexions[0].Last(),
                 outsideConnexions[1].First(), outsideConnexions[1].Last(),
                  outsideConnexions[2].First(), outsideConnexions[2].Last(),
                   outsideConnexions[3].First(), /*outsideConnexions[3].Last()*/ });
            foreach (var rowOrColumn in outsideConnexions)
            {
                foreach (var color in rowOrColumn)
                {
                    var distance = GetColorDistance(color, notWallColor);
                    Debug.WriteLine($"Distance to not wall color: {distance}");
                }
            }

            return map;
        }

        static int GetColorDistance(SKColor c1, SKColor c2)
        {
            int redDiff = c1.Red - c2.Red;
            int greenDiff = c1.Green - c2.Green;
            int blueDiff = c1.Blue - c2.Blue;
            return Math.Abs(redDiff)+ Math.Abs(greenDiff) + Math.Abs(blueDiff);
        }

        static int offset = 5;
        static int lateralOffset = 10;
        SKColor GetColorForPoint(int x, int y, bool horizontal)
        {
            var yOffset = horizontal ? offset : lateralOffset;
            var kOffset = horizontal ? lateralOffset : offset;
            var colorsWithOffset = new List<SKColor>();
            for (int j = -yOffset; j < yOffset; j++)
            {
                for (int k = -kOffset; k < kOffset; k++)
                {
                    var color = bitmap.GetPixel(x + j, y + k);
                    bitmap.SetPixel(x + j, y, SKColors.Red);
                    colorsWithOffset.Add(color);
                }
            }
            return GetMediumColor(colorsWithOffset);
        }

        void ColorPoint(int x, int y, SKColor color, bool horizontal)
        {
            var yOffset = horizontal ? offset : lateralOffset;
            var kOffset = horizontal ? lateralOffset : offset;
            for (int j = -yOffset; j < yOffset; j++)
            {
                for (int k = -kOffset; k < kOffset; k++)
                {
                    bitmap.SetPixel(x + j, y + k, color);
                }
            }
        }

        static SKColor GetMediumColor(List<SKColor> colors)
        {
            int red = 0;
            int blue = 0;
            int green = 0;
            foreach (var color in colors)
            {
                red += color.Red;
                green += color.Green;
                blue += color.Blue;
            }
            int count = colors.Count;
            return new SKColor((byte)(red/count), (byte)(green/count), (byte)(blue/count));
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
    }
}