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


    public Cell[,] Position = new Cell[9, 9];
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
        Position = new Cell[9, 9];
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
                Position[i - 1, j - 1] = new Cell();
                Position[i - 1, j - 1].textBox.FontSize = 30;
                Position[i - 1, j - 1].textBox.HorizontalContentAlignment = HorizontalAlignment.Center;
                Position[i - 1, j - 1].textBox.VerticalContentAlignment = VerticalAlignment.Center;
                Position[i - 1, j - 1].textBox.MaxLength = 1;
                Position[i - 1, j - 1].textBox.PreviewTextInput += Integer; //only allow 1~9
                Position[i - 1, j - 1].textBox.Background = brush1;
                if (j > 3 && j < 7) Position[i - 1, j - 1].textBox.Background = brush2;
                SudokuGrid.Children.Add(Position[i - 1, j - 1].textBox);
                Grid.SetRow(Position[i - 1, j - 1].textBox, i - 1);
                Grid.SetColumn(Position[i - 1, j - 1].textBox, j - 1);
                Position[i - 1, j - 1].row = i;
                Position[i - 1, j - 1].column = j;
                Position[i - 1, j - 1].block = (i <= 3 ? 0 : i <= 6 ? 3 : 6) + (j <= 3 ? 0 : j <= 6 ? 1 : 2) + 1;
                Position[i - 1, j - 1].cellinblock = i % 3 * 3 + j % 3+1;
            }
        }
    }

    private void AssignValue()
    {
        //temporarily assign value to cells
        Position[0, 2].textBox.Text = "3";
        Position[1, 2].textBox.Text = "2";
        Position[2, 0].textBox.Text = "8";
        Position[1, 3].textBox.Text = "9";
        Position[2, 4].textBox.Text = "6";
        Position[0, 7].textBox.Text = "7";
        Position[1, 6].textBox.Text = "6";
        Position[3, 0].textBox.Text = "3";
        Position[3, 3].textBox.Text = "5";
        Position[3, 4].textBox.Text = "8";
        Position[3, 5].textBox.Text = "2";
        Position[4, 3].textBox.Text = "1";
        Position[5, 3].textBox.Text = "3";
        Position[5, 4].textBox.Text = "9";
        Position[3, 7].textBox.Text = "4";
        Position[4, 7].textBox.Text = "8";
        Position[5, 8].textBox.Text = "1";
        Position[6, 0].textBox.Text = "5";
        Position[6, 2].textBox.Text = "8";
        Position[7, 2].textBox.Text = "9";
        Position[8, 1].textBox.Text = "6";
        Position[6, 4].textBox.Text = "2";
        Position[7, 4].textBox.Text = "1";
        Position[8, 4].textBox.Text = "7";
        Position[7, 8].textBox.Text = "8";
        Position[8, 7].textBox.Text = "2";
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                Position[i, j].value = Position[i, j].textBox.Text == "" ? 0 : int.Parse(Position[i, j].textBox.Text);
            }
        }

        GetAllPosibility();
    }

    private void LockValue()
    {
        foreach(Cell tb in Position)
        {
            if (tb.textBox.Text != "") tb.textBox.IsReadOnly = true;
            else{
                tb.textBox.FontStyle = FontStyles.Italic;
                tb.textBox.Foreground = Brushes.Blue; 
            }
        }
    }
    private void ButtonSolve_Click(object sender, RoutedEventArgs e)
    {

        //testing phase
        SolvePossibility();
        //for(int i = 0; i < 9; i++)
        //{
        //    rows[i] = new int[9];
        //    columns[i] = new int[9];
        //    blocks[i] = new int[9];
        //}
        //for(int i = 0; i < 9; i++)
        //{
        //    for (int j = 0; j < 9; j++)
        //    {
        //        if (Position[i, j].textBox.Text != "")
        //        {
        //            int num = int.Parse(Position[i, j].textBox.Text);
        //            rows[i][j] = num;
        //            columns[j][i] = num;
        //            int block = i / 3 * 3 + j / 3;
        //            int cellinblock = i % 3 * 3 + j % 3;
        //            blocks[block][cellinblock] = num;
        //        }
        //    }
        //}
    }

    private void GetAllPosibility()
    {
        for (int i = 1; i <= 9; i++)
        {
            var exist = Position.Cast<Cell>().Where(p => p.textBox.Text == i.ToString());
            int[] t_row = exist.Select(p => p.row).ToArray();
            int[] t_column = exist.Select(p => p.column).ToArray();
            int[] t_block = exist.Select(p => p.block).ToArray();
            var possibility = Position.Cast<Cell>()
                .Where(p => p.value == 0)
                .Where(p => !t_row.Contains(p.row))
                .Where(p => !t_column.Contains(p.column))
                .Where(p => !t_block.Contains(p.block));
            foreach (var p in possibility) {
                Position[p.row - 1, p.column - 1].PossibleValue[i-1] = i.ToString();
                Position[p.row - 1, p.column - 1].textBox.Foreground = Brushes.Red;
                Position[p.row - 1, p.column - 1].textBox.FontSize = 10;
                Position[p.row - 1, p.column - 1].textBox.HorizontalContentAlignment = HorizontalAlignment.Left;
                Position[p.row - 1, p.column - 1].textBox.VerticalContentAlignment = VerticalAlignment.Top;
                Position[p.row - 1, p.column - 1].textBox.Text = Position[p.row - 1, p.column - 1].PossibleValueinRow();
            }
        }
    }

    private void SolvePossibility()
    {
        //row solving
        for(int i = 0; i <9 ;i++)
        {
            Dictionary<int,int> posibilitycheck = new Dictionary<int,int>();
            for(int j = 0; j < 9; j++)
            {
                foreach(string s in Position[i, j].PossibleValue)
                {
                    if (s.Trim() != "")
                    {
                        if (posibilitycheck.ContainsKey(int.Parse(s)))
                            posibilitycheck[int.Parse(s)]++;
                        else
                            posibilitycheck[int.Parse(s)] = 1;
                    }
                }
            }
            foreach (KeyValuePair<int, int> kvp in posibilitycheck)
            {
                if (kvp.Value == 1)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (Position[i, j].PossibleValue.Contains(kvp.Key.ToString()))
                        {
                            Position[i, j].textBox.Foreground = Brushes.Blue;
                            Position[i, j].textBox.FontSize = 30;
                            Position[i, j].textBox.HorizontalContentAlignment = HorizontalAlignment.Center;
                            Position[i, j].textBox.VerticalContentAlignment = VerticalAlignment.Center;
                            Position[i, j].textBox.Text = kvp.Key.ToString();
                            Position[i, j].value = kvp.Key;
                            Position[i, j].PossibleValueReset();
                            RemovePossibility(Position[i, j].row, Position[i, j].column, Position[i, j].block);
                            //SolvePossibility();
                            Trace.WriteLine("Row Solve");
                            return;
                        }
                    }
                }
            }
        }


        //column solving
        for(int i=0; i<9; i++)
        {
            Dictionary<int, int> posibilitycheck = new Dictionary<int, int>();
            for (int j = 0; j < 9; j++)
            {
                foreach (string s in Position[j, i].PossibleValue)
                {
                    if (s.Trim() != "")
                    {
                        if (posibilitycheck.ContainsKey(int.Parse(s)))
                            posibilitycheck[int.Parse(s)]++;
                        else
                            posibilitycheck[int.Parse(s)] = 1;
                    }
                }
            }
            foreach (KeyValuePair<int, int> kvp in posibilitycheck)
            {
                if (kvp.Value == 1)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        if (Position[j, i].PossibleValue.Contains(kvp.Key.ToString()))
                        {
                            Position[j, i].textBox.Foreground = Brushes.Blue;
                            Position[j, i].textBox.FontSize = 30;
                            Position[j, i].textBox.HorizontalContentAlignment = HorizontalAlignment.Center;
                            Position[j, i].textBox.VerticalContentAlignment = VerticalAlignment.Center;
                            Position[j, i].textBox.Text = kvp.Key.ToString();
                            Position[j, i].value = kvp.Key;
                            Position[j, i].PossibleValueReset();
                            RemovePossibility(Position[j, i].row, Position[j, i].column, Position[j, i].block);
                            //SolvePossibility();
                            Trace.WriteLine("Column Solve");
                            return;
                        }
                    }
                }
            }
        }


        // box solving
        for(int i=0; i<9; i++)
        {
            Dictionary<int, int> posibilitycheck = new Dictionary<int, int>();
            foreach (Cell c in Position)
            {
                if(c.block == i + 1)
                {
                    foreach (string s in c.PossibleValue)
                    {
                        if (s.Trim() != "")
                        {
                            if (posibilitycheck.ContainsKey(int.Parse(s)))
                                posibilitycheck[int.Parse(s)]++;
                            else
                                posibilitycheck[int.Parse(s)] = 1;
                        }
                    }
                }
            }
            foreach (KeyValuePair<int, int> kvp in posibilitycheck)
            {
                if (kvp.Value == 1)
                {
                    for (int j = 0; j < 9; j++)
                    {
                        int row = (i / 3) * 3 + j / 3;
                        int column = (i % 3) * 3 + j % 3;
                        if (Position[row, column].PossibleValue.Contains(kvp.Key.ToString()))
                        {
                            Position[row, column].textBox.Foreground = Brushes.Blue;
                            Position[row, column].textBox.FontSize = 30;
                            Position[row, column].textBox.HorizontalContentAlignment = HorizontalAlignment.Center;
                            Position[row, column].textBox.VerticalContentAlignment = VerticalAlignment.Center;
                            Position[row, column].textBox.Text = kvp.Key.ToString();
                            Position[row, column].value = kvp.Key;
                            Position[row, column].PossibleValueReset();
                            RemovePossibility(Position[row, column].row, Position[row, column].column, Position[row, column].block);
                            //SolvePossibility();
                            Trace.WriteLine("Block Solve");
                            return;
                        }
                    }
                }
            }
        }


        //remaining solving
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                if (Position[i, j].PossibleValueinRow().Trim().Length == 1)
                {
                    Position[i, j].textBox.Foreground = Brushes.Blue;
                    Position[i, j].textBox.FontSize = 30;
                    Position[i, j].textBox.HorizontalContentAlignment = HorizontalAlignment.Center;
                    Position[i, j].textBox.VerticalContentAlignment = VerticalAlignment.Center;
                    Position[i, j].textBox.Text = Position[i, j].PossibleValueinRow().Trim();
                    Position[i, j].value = int.Parse(Position[i, j].textBox.Text);
                    Position[i, j].PossibleValueReset();
                    RemovePossibility(Position[i, j].row, Position[i, j].column, Position[i, j].block);
                    //SolvePossibility();
                    Trace.WriteLine("One Possibility Only Solve");
                    return;
                }
            }
        }




        Trace.WriteLine("no more obvious answer");
    }

    private void RemovePossibility(int row, int column, int block)
    {
        for (int i = 0; i < 9; i++)
        {
            for(int j = 0;j < 9; j++)
            {
                if (Position[i, j].row == row || Position[i, j].column == column || Position[i, j].block == block)
                {
                    Position[i, j].PossibleValue[Position[row - 1, column - 1].value - 1] = "  ";
                    if (Position[i,j].value==0)
                        Position[i, j].textBox.Text = Position[i, j].PossibleValueinRow();
                }
            }
        }
    }


    private void SudokuSolving()
    {

    }
}
