using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WPFTest3;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private static readonly Regex _regex = new("[^1-9.-]+");
    private static bool IsTextAllowed(string text) => !_regex.IsMatch(text);
    private void Integer(object sender, TextCompositionEventArgs e) => e.Handled = !IsTextAllowed(e.Text);


    public TextBox[,] Position = new TextBox[9, 9];

    public MainWindow()
    {
        InitializeComponent();
        for (int i = 1; i <= 9; i++)
        {
            Brush brush1 = Brushes.LightGreen;
            Brush brush2 = Brushes.White;
            if (i > 3 && i < 7)
            {
                brush1 = Brushes.White;
                brush2 = Brushes.LightGreen;
            }
            for (int j = 1; j <= 9; j++)
            {
                Position[i - 1, j - 1] = (TextBox)FindName("Cell" + i + j);
                Position[i - 1, j - 1].FontSize = 30;
                Position[i - 1, j - 1].HorizontalContentAlignment = HorizontalAlignment.Center;
                Position[i - 1, j - 1].VerticalContentAlignment = VerticalAlignment.Center;
                Position[i - 1, j - 1].MaxLength = 1;
                Position[i - 1, j - 1].PreviewTextInput += Integer; //only allow 1~9
                Position[i - 1, j - 1].Background = brush1;
                if (j > 3 && j < 7) Position[i - 1, j - 1].Background = brush2;
                Grid.SetRow(Position[i - 1, j - 1], i-1);
                Grid.SetColumn(Position[i - 1, j - 1], j-1);
            }
        }
    }

    private void ButtonSolve_Click(object sender, RoutedEventArgs e)
    {
        foreach(TextBox cell in Position)
        {
            Trace.WriteLine(cell.Name.ToString());
        }
    }

    private void SudokuSolving()
    {

    }
}
