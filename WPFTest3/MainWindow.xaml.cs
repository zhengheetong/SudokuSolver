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
    public int[][] rows = new int[9][];
    public int[][] columns = new int[9][];
    public int[][] blocks = new int[9][];

    public MainWindow()
    {
        InitializeComponent();
        SudokuGridCreation();
        AssignValue();
        LockValue();
    }

    private void SudokuGridCreation()
    {
        Position = new TextBox[9, 9];
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
                SudokuGrid.Children.Add(Position[i - 1, j - 1] = new TextBox());
                Position[i - 1, j - 1].FontSize = 30;
                Position[i - 1, j - 1].HorizontalContentAlignment = HorizontalAlignment.Center;
                Position[i - 1, j - 1].VerticalContentAlignment = VerticalAlignment.Center;
                Position[i - 1, j - 1].MaxLength = 1;
                Position[i - 1, j - 1].PreviewTextInput += Integer; //only allow 1~9
                Position[i - 1, j - 1].Background = brush1;
                if (j > 3 && j < 7) Position[i - 1, j - 1].Background = brush2;
                Grid.SetRow(Position[i - 1, j - 1], i - 1);
                Grid.SetColumn(Position[i - 1, j - 1], j - 1);
            }
        }
    }

    private void AssignValue()
    {
        //temporarily assign value to cells
        Position[0, 2].Text = "3";
        Position[1, 2].Text = "2";
        Position[2, 0].Text = "8";
        Position[1, 3].Text = "9";
        Position[2, 4].Text = "6";
        Position[0, 7].Text = "7";
        Position[1, 6].Text = "6";
        Position[3, 0].Text = "3";
        Position[3, 3].Text = "5";
        Position[3, 4].Text = "8";
        Position[3, 5].Text = "2";
        Position[4, 3].Text = "1";
        Position[5, 3].Text = "3";
        Position[5, 4].Text = "9";
        Position[3, 7].Text = "4";
        Position[4, 7].Text = "8";
        Position[5, 8].Text = "1";
        Position[6, 0].Text = "5";
        Position[6, 2].Text = "8";
        Position[7, 2].Text = "9";
        Position[8, 1].Text = "6";
        Position[6, 4].Text = "2";
        Position[7, 4].Text = "1";
        Position[8, 4].Text = "7";
        Position[7, 8].Text = "8";
        Position[8, 7].Text = "2";
    }

    private void LockValue()
    {
        foreach(TextBox tb in Position)
        {
            if (tb.Text != "") tb.IsReadOnly = true;
            else{
                tb.FontStyle = FontStyles.Italic;
                tb.Foreground = Brushes.Blue; 
            }
        }
    }
    private void ButtonSolve_Click(object sender, RoutedEventArgs e)
    {
        for(int i = 0; i < 9; i++)
        {
            rows[i] = new int[9];
            columns[i] = new int[9];
            blocks[i] = new int[9];
        }
        for(int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (Position[i, j].Text != "")
                {
                    int num = int.Parse(Position[i, j].Text);
                    rows[i][j] = num;
                    columns[j][i] = num;
                    int block = i / 3 * 3 + j / 3;
                    int cellinblock = i % 3 * 3 + j % 3;
                    blocks[block][cellinblock] = num;
                }
            }
        }
    }

    private void SudokuSolving()
    {

    }
}
