using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFTest3;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    public TextBox[,] Position = new TextBox[9, 9];

    public MainWindow()
    {
        InitializeComponent();
        for (int i = 1; i <= 9; i++)
        {
            for (int j = 1; j <= 9; j++)
            {
                Position[i-1,j-1] = (TextBox)FindName("Cell" + i + j);
            }
        }
    }

    private void ButtonSolve_Click(object sender, RoutedEventArgs e)
    {
        Position[0, 1].Text = Position[0, 0].Text;
    }
}
