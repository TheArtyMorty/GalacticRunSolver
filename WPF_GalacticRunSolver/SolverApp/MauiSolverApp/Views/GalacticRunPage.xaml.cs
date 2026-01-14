using MauiSolverApp.Utilities;
using SolverApp.Models;
using SolverApp.ViewModels;

namespace MauiSolverApp.Views;

public partial class GalacticRunPage : ContentPage
{
	public GalacticRunPage(SolverViewModel solvervm)
	{
		InitializeComponent();
        LoadMap.IsEnabled = false;
        _solvervm = solvervm;
    }

    private SolverViewModel _solvervm;

    private async void LoadMapFromURL(object sender, EventArgs e)
    {
        var url = userInputURL.Text;
        if (IsURLValid(url))
        {
            // Load the map
            try
            {
                var map = GetMapFromWeburl(userInputURL.Text);
                _solvervm.CreateNewMap(map);
                await Shell.Current.GoToAsync("//SolverPage");
            }
            catch
            {
                await DisplayAlert("Error", "Could not load map from URL.", "OK");
                return;
            }
        }
    }

    private void userInputURL_TextChanged(object sender, TextChangedEventArgs e)
    {
        LoadMap.IsEnabled = IsURLValid(e.NewTextValue);
    }

    private bool IsURLValid(string url)
    {
        return url.Contains("https://galactic.run/p/");
    }

    // Copied from WPF solver 

    private static EWallType StringToWallType(string t)
    {
        if (t == "NE") return EWallType.TopRight;
        if (t == "SE") return EWallType.BottomRight;
        if (t == "SW") return EWallType.BottomLeft;
        if (t == "NW") return EWallType.TopLeft;
        return EWallType.None;
    }
    private static Position GetPositionFromCellId(int id, int mapsize)
    {
        int y = id / mapsize;
        int x = id - y * mapsize;
        return new Position(x, y);
    }
    private static MapViewModel GetMapFromUrlBoardString(string board)
    {
        int mapsize = 16;
        var map = new MapViewModel(mapsize);
        //splitboard
        string[] splittedBoard = board.Split(' ');
        //target and robots
        map._Target._Position = GetPositionFromCellId(Int16.Parse(splittedBoard[0]), mapsize);
        map._Target._Color = (EColor)Int16.Parse(splittedBoard[1]);
        for (int i = 0; i < 4; i++)
        {
            map._Robots[i]._Position = GetPositionFromCellId(Int16.Parse(splittedBoard[i + 2]), mapsize);
        }
        //walls
        string[] walls = splittedBoard[6].Split('-');
        foreach (string wall in walls)
        {
            int cellID = Int16.Parse(wall.Split(':').First());
            var position = GetPositionFromCellId(cellID, mapsize);
            string walltype = wall.Split(':').Last();
            map._Cases[position.Y][position.X]._WallType = StringToWallType(walltype);
        }
        return map;
    }

    public static MapViewModel GetMapFromWeburl(string url)
    {
        var id = url.Split('/').Last();
        var boardAsString = GalacticRunBoardFromUrl.GetBoardFromBoardID(id).GetString();
        return GetMapFromUrlBoardString(boardAsString);
    }

    protected override bool OnBackButtonPressed()
    {
        Shell.Current.GoToAsync("//SolverPage");
        return true;
    }
}