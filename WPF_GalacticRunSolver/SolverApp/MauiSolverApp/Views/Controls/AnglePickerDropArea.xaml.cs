using Android.Graphics;
using Android.Icu.Number;
using Android.Service.QuickAccessWallet;
using Dalvik.SystemInterop;
using Kotlin;
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

        private void DisplayAlert(string title, string message)
        {
            var Parent = this.Parent;
            while (Parent != null && !(Parent is Page))
            {
                Parent = Parent.Parent;
            }

            if (Parent is Page parentPage)
            {
                parentPage.DisplayAlert(title, message, "Ok");
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

        public async void StartRecognition()
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
                        //Increase contrast to max
                        var red = ContrastColor(color.Red);
                        var green = ContrastColor(color.Green);
                        var blue = ContrastColor(color.Blue);
                        //Change output color
                        *ptr++ = (byte)red;    // red
                        *ptr++ = (byte)green;  // green
                        *ptr++ = (byte)blue;   // blue
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
            if (File.Exists(newFile))
            {
                File.Delete(newFile);
            }

            using (var newStream = File.OpenWrite(newFile))
            {
                bitmap.Encode(newStream, SKEncodedImageFormat.Png, 90);
            }

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

            // And send it to the solver page
            if (Parent is PhotoHelperPage photoHelperPage2)
            {
                var dataContext = photoHelperPage2.BindingContext as PhotoHelperViewModel;
                if (dataContext != null)
                    dataContext.SetRecognizedMap(map);
            }
        }

        static int contrastFactor = 2; // contrast value is 100, and factor is ((100+v)/100)²
        private int ContrastColor(byte color)
        {
            var newColor = (((float)(color / 255f) - 0.5f) * contrastFactor + 0.5f) * 255f;
            if (newColor < 0) newColor = 0;
            if (newColor > 255) newColor = 255;
            return (int)newColor;
        }

        private MapViewModel RecognizeMap()
        {
            var map = new MapViewModel(16);
            double caseSize = bitmap.Width / 16.0;
            // First, let's find all 'interesting' cells by looking at center of each cell
            var interestingCells = new List<Tuple<int, int, SKColor>> { };
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if ((i == 7 || i == 8) && (j == 7 || j == 8))
                        continue; // Skip center area
                    var x = (int)((i + 0.5) * caseSize);
                    var y = (int)((j + 0.5) * caseSize);
                    var color = GetColorForCellCenter(x, y);
                    interestingCells.Add(new Tuple<int, int, SKColor>(i, j, color));
                    //ColorCenter(x, y, color);
                }
            }
            //From those we can find robots
            var RobotsShouldBeThereSomewhere = GetOutliersUsingIQR(interestingCells,8);
            var BlueRobot = GetMostProbableRobotPosition(RobotsShouldBeThereSomewhere, SKColors.Blue);
            var GreenRobot = GetMostProbableRobotPosition(RobotsShouldBeThereSomewhere, SKColors.Green);
            var YellowRobot = GetMostProbableRobotPosition(RobotsShouldBeThereSomewhere, SKColors.Yellow);
            var RedRobot = GetMostProbableRobotPosition(RobotsShouldBeThereSomewhere, SKColors.Red);
            map._Robots.Where(r => r._Color == Models.EColor.Blue).First()._Position = new Models.Position(BlueRobot.Item1, BlueRobot.Item2);
            map._Robots.Where(r => r._Color == Models.EColor.Red).First()._Position = new Models.Position(RedRobot.Item1, RedRobot.Item2);
            map._Robots.Where(r => r._Color == Models.EColor.Green).First()._Position = new Models.Position(GreenRobot.Item1, GreenRobot.Item2);
            map._Robots.Where(r => r._Color == Models.EColor.Yellow).First()._Position = new Models.Position(YellowRobot.Item1, YellowRobot.Item2);

            // Then, let's observe all 60 outside connexions, we should find 8 walls 
            var possibleTopLeftWalls = new List<Tuple<int,int,SKColor, bool>> {};
            var possibleTopRightWalls = new List<Tuple<int,int,SKColor, bool>> {};
            var possibleBottomLeftWalls = new List<Tuple<int,int,SKColor, bool>> {};
            var possibleBottomRightWalls = new List<Tuple<int,int,SKColor, bool>> {};
            //Get vertical wall
            for (int i = 0; i < 15; i++)
            {
                for (int j = 0; j < 16; j++)
                {
                    if ((i >= 6 && i <= 8) && (j >= 7 && j <= 8))
                        continue; // Skip center area
                    var x = (int)((i + 1) * caseSize);
                    var y = (int)((j + 0.5) * caseSize);
                    var color = GetColorForPoint(x, y, false);
                    //ColorPoint(x, y, SKColors.Pink, false);
                    if (i <= 7)
                    {
                        if (j <= 7)
                            possibleTopLeftWalls.Add(new Tuple<int, int, SKColor, bool>(i, j, color, false));
                        else
                            possibleBottomLeftWalls.Add(new Tuple<int, int, SKColor, bool>(i, j, color, false));
                    }
                    else
                    {
                        if (j <= 7)
                            possibleTopRightWalls.Add(new Tuple<int, int, SKColor, bool>(i, j, color, false));
                        else
                            possibleBottomRightWalls.Add(new Tuple<int, int, SKColor, bool>(i, j, color, false));
                    }
                }
            }
            //Get horizontal wall
            for (int i = 0; i < 16; i++)
            {
                for (int j = 0; j < 15; j++)
                {
                    if ((i >= 7 && i <= 8) && (j >= 6 && j <= 8))
                        continue; // Skip center area
                    var x = (int)((i+0.5)*caseSize);
                    var y = (int)((j + 1) * caseSize);
                    var color = GetColorForPoint(x, y, true);
                    //ColorPoint(x, y, SKColors.Pink, true);
                    if (i <= 7)
                    {
                        if (j <= 7)
                            possibleTopLeftWalls.Add(new Tuple<int, int, SKColor, bool>(i, j, color, true));
                        else
                            possibleBottomLeftWalls.Add(new Tuple<int, int, SKColor, bool>(i, j, color, true));
                    }
                    else
                    {
                        if (j <= 7)
                            possibleTopRightWalls.Add(new Tuple<int, int, SKColor, bool>(i, j, color, true));
                        else
                            possibleBottomRightWalls.Add(new Tuple<int, int, SKColor, bool>(i, j, color, true));
                    }
                }
            }
            // Let's keep the darkest ones as walls in each quadrant
            var robotsInTopLeft = map._Robots.Where(r => r._Position.X <= 7 && r._Position.Y <= 7).Count();
            var robotsInTopRight = map._Robots.Where(r => r._Position.X > 7 && r._Position.Y <= 7).Count();
            var robotsInBottomLeft = map._Robots.Where(r => r._Position.X <= 7 && r._Position.Y > 7).Count();
            var robotsInBottomRight = map._Robots.Where(r => r._Position.X > 7 && r._Position.Y > 7).Count();
            var probableWalls = new List<List<Tuple<int, int, SKColor, bool>>>();
            foreach (var wallListAndSafeCount in new List<Tuple<List<Tuple<int, int, SKColor, bool>>, int>> {
                new Tuple<List<Tuple<int, int, SKColor, bool>>, int>(possibleTopLeftWalls,robotsInTopLeft),
                new Tuple<List<Tuple<int, int, SKColor, bool>>, int>(possibleTopRightWalls,robotsInTopRight),
                new Tuple<List<Tuple<int, int, SKColor, bool>>, int>(possibleBottomLeftWalls,robotsInBottomLeft),
                new Tuple<List<Tuple<int, int, SKColor, bool>>, int>(possibleBottomRightWalls,robotsInBottomRight),})
            {
                probableWalls.Add(new List<Tuple<int, int, SKColor, bool>>());
                var wallnumbersToBeSafe = 12 + wallListAndSafeCount.Item2;
                var walls = GetOutliersUsingIQR(wallListAndSafeCount.Item1, wallnumbersToBeSafe);
                foreach (var wall in walls)
                {
                    probableWalls.Last().Add(wall);
                    if (wall.Item4)
                    {
                        var x = (int)((wall.Item1 + 0.5) * caseSize);
                        var y = (int)((wall.Item2 + 1) * caseSize);
                        ColorPoint(x, y, SKColors.Red, true);
                    }
                    else
                    {
                        var x = (int)((wall.Item1 + 1) * caseSize);
                        var y = (int)((wall.Item2 + 0.5) * caseSize);
                        ColorPoint(x, y, SKColors.Red, false);
                    }
                }
            }
            // Lets add the center square
            probableWalls[0].Add(new Tuple<int, int, SKColor, bool>(6, 7, SKColors.Pink, false));
            probableWalls[0].Add(new Tuple<int, int, SKColor, bool>(7, 6, SKColors.Pink, true));
            probableWalls[1].Add(new Tuple<int, int, SKColor, bool>(8, 7, SKColors.Pink, false));
            probableWalls[1].Add(new Tuple<int, int, SKColor, bool>(8, 6, SKColors.Pink, true));
            probableWalls[2].Add(new Tuple<int, int, SKColor, bool>(6, 8, SKColors.Pink, false));
            probableWalls[2].Add(new Tuple<int, int, SKColor, bool>(7, 8, SKColors.Pink, true));
            probableWalls[3].Add(new Tuple<int, int, SKColor, bool>(7, 8, SKColors.Pink, false));
            probableWalls[3].Add(new Tuple<int, int, SKColor, bool>(8, 8, SKColors.Pink, true));
            // Then we have to try and recognize those walls :o
            var quadrants = new List<string> { "TopLeft", "TopRight", "BottomLeft", "BottomRight" };
            string messageForUser = "";
            for (int quadrantIndex = 0; quadrantIndex < 4; quadrantIndex++)
            {
                var boardScores = new List<Tuple<int, double>>();
                var wallsRecognized = probableWalls[quadrantIndex];
                for (int i = 1; i <= 16; i++)
                {
                    var boardWallsAsWallType = BoardUtilities.GetWallsForQuadrant(i, 0);
                    // Let's rotate them if needed :
                    for (int j = 0; j < boardWallsAsWallType.Count; j++)
                    {
                        switch (quadrants[quadrantIndex])
                        {
                            case "TopRight":
                                MapViewModel.RotateRight(ref boardWallsAsWallType, j);
                                break;
                            case "BottomLeft":
                                MapViewModel.RotateLeft(ref boardWallsAsWallType, j);
                                break;
                            case "BottomRight":
                                MapViewModel.RotateTwice(ref boardWallsAsWallType, j);
                                break;
                            case "TopLeft":
                            default:
                                break;
                        }
                    }
                    // Lets split walls to vertical and horizontal walls
                    var boardWalls = new List<Tuple<int, int, bool>>();
                    foreach (var wall in boardWallsAsWallType)
                    {
                        var x = wall.Item2;
                        var y = wall.Item1;
                        switch (wall.Item3)
                        {
                            case Models.EWallType.TopLeft:
                                //Vertical wall
                                if (x > 0)
                                    boardWalls.Add(new Tuple<int, int, bool>(x - 1, y, false));
                                //Horizontal wall
                                if (y > 0)
                                    boardWalls.Add(new Tuple<int, int, bool>(x, y - 1, true));
                                break;
                            case Models.EWallType.TopRight:
                                //Vertical wall
                                if (x < 16)
                                    boardWalls.Add(new Tuple<int, int, bool>(x, y, false));
                                //Horizontal wall
                                if (y > 0)
                                    boardWalls.Add(new Tuple<int, int, bool>(x, y - 1, true));
                                break;
                            case Models.EWallType.BottomLeft:
                                //Vertical wall
                                if (x > 0)
                                    boardWalls.Add(new Tuple<int, int, bool>(x - 1, y, false));
                                //Horizontal wall
                                if (y < 16)
                                    boardWalls.Add(new Tuple<int, int, bool>(x, y, true));
                                break;
                            case Models.EWallType.BottomRight:
                                //Vertical wall
                                if (x < 16)
                                    boardWalls.Add(new Tuple<int, int, bool>(x, y, false));
                                //Horizontal wall
                                if (y < 16)
                                    boardWalls.Add(new Tuple<int, int, bool>(x, y, true));
                                break;
                            case Models.EWallType.None:
                            default:
                                break;
                        }
                    }
                    // Now I have to check how many boards walls are in the recognized walls
                    int matchedWalls = 0;
                    int failedWalls = 0;
                    foreach (var wall in boardWalls)
                    {
                        if (wallsRecognized.Any(wr => wr.Item1 == wall.Item1 && wr.Item2 == wall.Item2 && wr.Item4 == wall.Item3))
                        {
                            matchedWalls++;
                        }
                        else
                        {
                            failedWalls++;
                        }
                    }
                    // Let's keep the score for this board
                    double score = (double)(matchedWalls * 100) / (double)(matchedWalls + failedWalls + (wallsRecognized.Count - matchedWalls));
                    boardScores.Add(new Tuple<int, double>(i, score));
                }
                // Now we keep the board with highest score :
                boardScores.Sort((t1, t2) => t2.Item2.CompareTo(t1.Item2));
                var bestBoard = boardScores.First();
                messageForUser += string.Format("Quadrant {0} recognized as Board {1} with score {2:0.00}%\n", quadrants[quadrantIndex], bestBoard.Item1, bestBoard.Item2);
                map.SetQuadrant(quadrants[quadrantIndex], bestBoard.Item1, 0);
            }
            // Little message for user
            DisplayAlert("Map Recognized !", messageForUser);

            return map;
        }

        static Tuple<int,int> GetMostProbableRobotPosition(List<Tuple<int, int, SKColor>> possibleRobotPositions, SKColor robotColor)
        {
            if (possibleRobotPositions.Count == 0)
                return new Tuple<int,int>(0,0);
            possibleRobotPositions.Sort((t1, t2) => GetColorDistance(t1.Item3, robotColor).CompareTo(GetColorDistance(t2.Item3, robotColor)));
            var robot = possibleRobotPositions.First();
            possibleRobotPositions.Remove(robot);
            return new Tuple<int, int>(robot.Item1, robot.Item2);
        }

        static List<Tuple<int, int, SKColor>> GetOutliersUsingIQR(List<Tuple<int, int, SKColor>> colors, int minResult = 0)
        {
            var result = new List<Tuple<int, int, SKColor>>();
            colors.Sort((t1, t2) => GetColorDistance(t1.Item3, SKColors.Black).CompareTo(GetColorDistance(t2.Item3, SKColors.Black)));
            var distances = colors.ConvertAll(c => (double)GetColorDistance(c.Item3, SKColors.Black));
            var Q1 = Quantile(distances, 0.25);
            var Q3 = Quantile(distances, 0.75);
            var IQR = Q3 - Q1;
            var lowerBound = Q1 - 1.5 * IQR;
            var upperBound = Q3 + 1.5 * IQR;
            Console.WriteLine(string.Format("Lowerbound : {0}", lowerBound));
            Console.WriteLine(string.Format("Upperbound : {0}", upperBound));
            foreach (var item in colors)
            {
                var distance = GetColorDistance(item.Item3, SKColors.Black);
                Console.WriteLine(string.Format("Item at : {0} , {1} , value = {2}", item.Item1, item.Item2, distance));
                if (distance < lowerBound) // || distance > upperBound)  // The item above upperbound is outlier, however I'm only interested in darkest one, so closer to 0
                {
                    result.Add(item);
                }
            }
            // if we have enough result, let's stop here, otherwise, we take from the sorted result 
            if (result.Count < minResult)
            {
                result.Clear();
                for (int i = 0; i < minResult; i++)
                {
                    result.Add(colors[i]);
                }
            }
            return result;
        }

        static List<Tuple<int, int, SKColor, bool>> GetOutliersUsingIQR(List<Tuple<int, int, SKColor, bool>> colors, int minResult = 0)
        {
            var result = new List<Tuple<int, int, SKColor, bool>>();
            colors.Sort((t1, t2) => GetColorDistance(t1.Item3, SKColors.Black).CompareTo(GetColorDistance(t2.Item3, SKColors.Black)));
            var distances = colors.ConvertAll(c => (double)GetColorDistance(c.Item3, SKColors.Black));
            var Q1 = Quantile(distances, 0.25);
            var Q3 = Quantile(distances, 0.75);
            var IQR = Q3 - Q1;
            var lowerBound = Q1 - 1.5 * IQR;
            var upperBound = Q3 + 1.5 * IQR;
            Console.WriteLine(string.Format("Lowerbound : {0}", lowerBound));
            Console.WriteLine(string.Format("Upperbound : {0}", upperBound));
            foreach (var item in colors)
            {
                var distance = GetColorDistance(item.Item3, SKColors.Black);
                Console.WriteLine(string.Format("Item at : {0} , {1} , value = {2}", item.Item1, item.Item2, distance));
                if (distance < lowerBound) // || distance > upperBound)  // The item above upperbound is outlier, however I'm only interested in darkest one, so closer to 0
                {
                    result.Add(item);
                }
            }
            // if we have enough result, let's stop here, otherwise, we take from the sorted result 
            if (result.Count < minResult)
            {
                result.Clear();
                for (int i = 0; i < minResult; i++)
                {
                    result.Add(colors[i]);
                }
            }
            return result;
        }

        public static double Quantile(List<double> sortedData, double q)
        {
            if (sortedData == null || sortedData.Count == 0)
                throw new ArgumentException("Data array must not be null or empty.");
            if (q < 0 || q > 1)
                throw new ArgumentOutOfRangeException(nameof(q), "Quantile must be between 0 and 1.");

            int n = sortedData.Count;
            double pos = (n - 1) * q;
            int lowerIndex = (int)Math.Floor(pos);
            int upperIndex = (int)Math.Ceiling(pos);

            if (lowerIndex == upperIndex)
                return sortedData[lowerIndex];

            double weight = pos - lowerIndex;
            return sortedData[lowerIndex] * (1 - weight) + sortedData[upperIndex] * weight;
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
            var xOffset = horizontal ? lateralOffset : offset;
            var yOffset = horizontal ? offset : lateralOffset;
            var colorsWithOffset = new List<SKColor>();
            for (int j = -xOffset; j < xOffset; j++)
            {
                for (int k = -yOffset; k < yOffset; k++)
                {
                    var color = bitmap.GetPixel(x + j, y + k);
                    colorsWithOffset.Add(color);
                }
            }
            return GetMediumColor(colorsWithOffset);
        }

        SKColor GetColorForCellCenter(int x, int y)
        {
            var xOffset = lateralOffset;
            var yOffset = lateralOffset;
            var colorsWithOffset = new List<SKColor>();
            for (int j = -xOffset; j < xOffset; j++)
            {
                for (int k = -yOffset; k < yOffset; k++)
                {
                    var color = bitmap.GetPixel(x + j, y + k);
                    colorsWithOffset.Add(color);
                }
            }
            return GetMediumColor(colorsWithOffset);
        }

        void ColorPoint(int x, int y, SKColor color, bool horizontal)
        {
            var xOffset = horizontal ? lateralOffset : offset;
            var yOffset = horizontal ? offset : lateralOffset;
            for (int j = -xOffset; j < xOffset; j++)
            {
                for (int k = -yOffset; k < yOffset; k++)
                {
                    bitmap.SetPixel(x + j, y + k, color);
                }
            }
        }

        void ColorCenter(int x, int y, SKColor color)
        {
            var xOffset = lateralOffset;
            var yOffset = lateralOffset;
            for (int j = -xOffset; j < xOffset; j++)
            {
                for (int k = -yOffset; k < yOffset; k++)
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
    }
}