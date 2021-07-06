using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace WPF_GalacticRunSolver
{
    /// <summary>
    /// Interaction logic for MapArea.xaml
    /// </summary>
    public partial class MapArea : Window
    {
        public MapArea(MainWindow parent)
        {
            InitializeComponent();
            m_parent = parent;
        }

        MainWindow m_parent;

        public void StartSelection()
        {
            this.Show();
            m_clickedOnce = false;
            this.Selection.Visibility = Visibility.Hidden;
        }

        bool m_clickedOnce = false;

        System.Drawing.Point topLeft;
        System.Drawing.Point bottomRight;

        private void On_Click(object sender, RoutedEventArgs e)
        {
            if (!m_clickedOnce)
            {
                m_clickedOnce = true;
                topLeft = System.Windows.Forms.Cursor.Position;
                bottomRight = topLeft;
                Selection.Visibility = Visibility.Visible;
                UpdateSelection();
            }
            else
            {
                Selection.Visibility = Visibility.Hidden;
                Validate_Area();
            }
        }

        private void On_Move(object sender, RoutedEventArgs e)
        {
            if (m_clickedOnce)
            {
                bottomRight = System.Windows.Forms.Cursor.Position;

                int minX = Math.Min(bottomRight.X, topLeft.X);
                int minY = Math.Min(bottomRight.Y, topLeft.Y);
                int maxX = Math.Max(bottomRight.X, topLeft.X);
                int maxY = Math.Max(bottomRight.Y, topLeft.Y);

                topLeft.X = minX;
                topLeft.Y = minY;
                bottomRight.X = maxX;
                bottomRight.Y = maxY;

                UpdateSelection();
            }
        }

        private void UpdateSelection()
        {
            Selection.Margin = new Thickness(topLeft.X, topLeft.Y, Width-bottomRight.X, Height - bottomRight.Y);
        }

        private void Validate_Area()
        {
            m_parent.TopLeftCorner.SetNewPosition(topLeft);
            m_parent.BottomRightCorner.SetNewPosition(bottomRight);
            this.Hide();
        }
    }
}
