using System.Collections.Generic;
using System.Linq;
using TouchTracking;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace SolverApp.Views.Controls
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class AnglePickerDropArea : ContentView
    {
        public AnglePickerDropArea()
        {
            InitializeComponent();
            AddBoxViewToLayout();
            AddBoxViewToLayout();
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

        void AddBoxViewToLayout()
        {
            BoxView boxView = new BoxView
            {
                WidthRequest = 15,
                HeightRequest = 15,
                Color = Color.BlueViolet
            };

            TouchEffect touchEffect = new TouchEffect();
            touchEffect.TouchAction += OnTouchEffectAction;
            boxView.Effects.Add(touchEffect);
            AnglePickerArea.Children.Add(boxView);
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
                        AbsoluteLayout.SetLayoutBounds(boxView, rect);
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
    }
}