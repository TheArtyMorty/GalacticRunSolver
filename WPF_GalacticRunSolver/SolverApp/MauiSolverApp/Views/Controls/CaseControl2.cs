using SolverApp.Converters;
using SolverApp.ViewModels;

namespace MauiSolverApp.Views.Controls;

public class CaseControl2 : ContentView
{
    private readonly TapGestureRecognizer _singleTapGestureRecognizer;
    private readonly TapGestureRecognizer _doubleTapGestureRecognizer;
    private readonly DropGestureRecognizer _dropGestureRecognizer;
    public CaseControl2()
	{
		Content = new Image
		{
			Aspect = Aspect.AspectFill,
		};
        Content.SetBinding(Image.SourceProperty, new Binding("_WallType", BindingMode.OneWay, CaseTypeToImageConverter.Instance));

        // recognizers

        _doubleTapGestureRecognizer = new TapGestureRecognizer
        {
            NumberOfTapsRequired = 2
        };
        _singleTapGestureRecognizer = new TapGestureRecognizer
        {
            NumberOfTapsRequired = 1
        };
        _singleTapGestureRecognizer.Tapped += SingleTappedAsync;
        _doubleTapGestureRecognizer.Tapped += DoubleTappedAsync;
        GestureRecognizers.Add(_doubleTapGestureRecognizer);
        GestureRecognizers.Add(_singleTapGestureRecognizer);

        _dropGestureRecognizer = new DropGestureRecognizer();
        _dropGestureRecognizer.Drop += OnDrop;
        GestureRecognizers.Add(_dropGestureRecognizer);
    }

    private void DoubleTappedAsync(object? sender, EventArgs e)
    {
        if (this.BindingContext is CaseViewModel caseVM)
        {
            caseVM.DecrementWallType();
        }
    }

    private void SingleTappedAsync(object? sender, EventArgs e)
    {
        if (this.BindingContext is CaseViewModel caseVM)
        {
            caseVM.IncrementWallType();
        }
    }

    public void OnDrop(object? sender, DropEventArgs e)
    {
        if (e.Data != null)
        {
            if (e.Data.Properties.ContainsKey("Target"))
            {
                var target = e.Data.Properties["Target"] as TargetViewModel;
                if (target != null)
                    ((CaseViewModel)this.BindingContext).Drop(target);
            }
            if (e.Data.Properties.ContainsKey("Robot"))
            {
                var robot = e.Data.Properties["Robot"] as RobotViewModel;
                if (robot != null)
                    ((CaseViewModel)this.BindingContext).Drop(robot);
            }
        }
    }
}