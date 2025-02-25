using System.Data.Common;
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
    public Brush FontColor = Brushes.Blue;
    public int currentEstimation = 0;
    public Button[] nums_btn = new Button[9];//temp
    public bool pElimination = true;
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
            }
        }

        //temp button

        SudokuGrid.Height = 440;
        SudokuGrid.RowDefinitions.Add(new RowDefinition() { Height = new GridLength(40) });
        for (int i = 0; i < 9; i++)
        {
            nums_btn[i] = new Button();
            nums_btn[i].Content = i + 1;
            nums_btn[i].Click += ButtonNums_Click;
            nums_btn[i].Margin = new Thickness(2, 2, 2, 2);
            SudokuGrid.Children.Add(nums_btn[i]);
            Grid.SetRow(nums_btn[i], 11);
            Grid.SetColumn(nums_btn[i], i);
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

    //temporary button to show possible value
    private void ButtonNums_Click(object sender, RoutedEventArgs e)
    {
        Button? btn = sender as Button;
        
        var unknown = Position.Cast<Cell>()
                .Where(p => p.value == 0);

        foreach (var p in unknown)
        {
            p.textBox.FontWeight = FontWeights.Normal;
            p.textBox.Foreground = Brushes.Red;
            if (p.PossibleValueinRow().Contains(btn.Content.ToString()))
            {
                p.textBox.FontWeight = FontWeights.Bold;
                p.textBox.Foreground = Brushes.Purple;
            }
        }

    }

    private void ButtonSolve_Click(object sender, RoutedEventArgs e)
    {
        SolvePossibility();
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
                            TBFixed(i, j);
                            Position[i, j].textBox.Text = kvp.Key.ToString();
                            Position[i, j].value = kvp.Key;
                            Position[i, j].PossibleValueReset();
                            RemovePossibility(Position[i, j].row, Position[i, j].column, Position[i, j].block, Position[i, j].value);
                            //SolvePossibility();
                            Trace.WriteLine("Row Solve");
                            pElimination = true;
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
                            TBFixed(j, i);
                            Position[j, i].textBox.Text = kvp.Key.ToString();
                            Position[j, i].value = kvp.Key;
                            Position[j, i].PossibleValueReset();
                            RemovePossibility(Position[j, i].row, Position[j, i].column, Position[j, i].block, Position[j, i].value);
                            //SolvePossibility();
                            Trace.WriteLine("Column Solve");
                            pElimination = true;
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
                            TBFixed(row, column);
                            Position[row, column].textBox.Text = kvp.Key.ToString();
                            Position[row, column].value = kvp.Key;
                            Position[row, column].PossibleValueReset();
                            RemovePossibility(Position[row, column].row, Position[row, column].column, Position[row, column].block, Position[row, column].value);
                            //SolvePossibility();
                            Trace.WriteLine("Block Solve");
                            pElimination = true;
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
                    TBFixed(i, j);
                    Position[i, j].textBox.Text = Position[i, j].PossibleValueinRow().Trim();
                    Position[i, j].value = int.Parse(Position[i, j].textBox.Text);
                    Position[i, j].PossibleValueReset();
                    RemovePossibility(Position[i, j].row, Position[i, j].column, Position[i, j].block, Position[i, j].value);
                    //SolvePossibility();
                    Trace.WriteLine("One Possibility Only Solve");
                    pElimination = true;
                    return;
                }
            }
        }

        if (pElimination)
        {
            PossibilityElimination1();
            PossibilityElimination2();
            Trace.WriteLine("Possibility Eliminate");
            pElimination = false;
            return;
        }


        EstimationSolving(0);

        Trace.WriteLine("no more obvious answer");
    }

    private void PossibilityElimination1() //identify cell with same obvious pair under same row, column, or block
    {
        var unknown = Position.Cast<Cell>().Where(p => p.value == 0);

        foreach (var p1 in unknown)
        {
            foreach (var p2 in unknown)
            {
                string[] pair = new string[2];
                int t_row = 0, t_column = 0, t_block = 0;

                if (p1.row == p2.row && p1.column == p2.column) continue; // skip if same cell

                if (p1.PossibleValueWithoutEmpty() == p2.PossibleValueWithoutEmpty() && p1.PossibleValueWithoutEmpty().Length == 2)
                {
                    if (p1.row == p2.row) t_row = p1.row; // check if same row
                    else if (p1.column == p2.column) t_column = p1.column; // check if same column
                    if(p1.block == p2.block) t_block = p1.block;// check if same block
                }

                if(t_row == 0 && t_column == 0 && t_block == 0) continue; // skip if not in same row, column, or block

                pair[0] = "" + p1.PossibleValueWithoutEmpty()[0];
                pair[1] = "" + p1.PossibleValueWithoutEmpty()[1];

                RemovePossibility(t_row, t_column, t_block, int.Parse(pair[0]));
                RemovePossibility(t_row, t_column, t_block, int.Parse(pair[1]));

                foreach (string s in pair)
                {
                    p1.PossibleValue[int.Parse(s) - 1] = s;
                    p2.PossibleValue[int.Parse(s) - 1] = s;
                }

                p1.textBox.Text = p1.PossibleValueinRow();
                p2.textBox.Text = p2.PossibleValueinRow();

            }
        }
    }

    private void PossibilityElimination2() //identify cell with same less obvious pair under same row, column, or bloc
    {
        var unknown = Position.Cast<Cell>().Where(p => p.value == 0);
        for (int row = 0; row < 9; row++)
        {
            Dictionary<int, int> keyValuePairs = new Dictionary<int, int>();
            List<int> pair = new List<int>();
            foreach (var p in unknown)
            {
                if (p.row == row)
                {
                    foreach (string s in p.PossibleValue)
                    {
                        if (s.Trim() != "")
                        {
                            if (keyValuePairs.ContainsKey(int.Parse(s)))
                                keyValuePairs[int.Parse(s)]++;
                            else
                                keyValuePairs[int.Parse(s)] = 1;
                        }
                    }
                }
            }
            foreach (KeyValuePair<int, int> kvp in keyValuePairs)
            {
                if (kvp.Value == 2)
                {
                    pair.Add(kvp.Key);
                }
            }
            for (int i = 0; i < pair.Count; i++)
            {
                for(int j = i + 1; j < pair.Count; j++)
                {
                    var exist = unknown
                        .Where(u=> u.row ==row)
                        .Where(u=> u.PossibleValue.Contains(pair[i].ToString()))
                        .Where(u => u.PossibleValue.Contains(pair[j].ToString()));
                    if (exist.Count() == 2)
                    {
                        int t_block = 0;
                        if(exist.First().block == exist.Last().block)
                        {
                            t_block = exist.First().block;
                        }
                        int row1 = exist.First().row;
                        int row2 = exist.Last().row;
                        int col1 = exist.First().column;
                        int col2 = exist.Last().column;

                        RemovePossibility(row, 0, t_block, pair[i]);
                        RemovePossibility(row, 0, t_block, pair[j]);

                        Position[row1 - 1, col1 - 1].PossibleValue[pair[i] - 1] = pair[i].ToString();
                        Position[row1 - 1, col1 - 1].PossibleValue[pair[j] - 1] = pair[j].ToString();
                        Position[row2 - 1, col2 - 1].PossibleValue[pair[i] - 1] = pair[i].ToString();
                        Position[row2 - 1, col2 - 1].PossibleValue[pair[j] - 1] = pair[j].ToString();

                        Position[row1 - 1, col1 - 1].textBox.Text = Position[row1 - 1, col1 - 1].PossibleValueinRow();
                        Position[row2 - 1, col2 - 1].textBox.Text = Position[row2 - 1, col2 - 1].PossibleValueinRow();

                        string after = Position[row1 - 1, col1 - 1].PossibleValueWithoutEmpty() + " " + Position[row2 - 1, col2 - 1].PossibleValueWithoutEmpty();

                    }
                }
            }
        }

        for (int col = 0; col < 9; col++)
        {
            Dictionary<int, int> keyValuePairs = new Dictionary<int, int>();
            List<int> pair = new List<int>();
            foreach (var p in unknown)
            {
                if (p.column == col)
                {
                    foreach (string s in p.PossibleValue)
                    {
                        if (s.Trim() != "")
                        {
                            if (keyValuePairs.ContainsKey(int.Parse(s)))
                                keyValuePairs[int.Parse(s)]++;
                            else
                                keyValuePairs[int.Parse(s)] = 1;
                        }
                    }
                }
            }
            foreach (KeyValuePair<int, int> kvp in keyValuePairs)
            {
                if (kvp.Value == 2)
                {
                    pair.Add(kvp.Key);
                }
            }
            for (int i = 0; i < pair.Count; i++)
            {
                for (int j = i + 1; j < pair.Count; j++)
                {
                    var exist = unknown
                        .Where(u => u.column == col)
                        .Where(u => u.PossibleValue.Contains(pair[i].ToString()))
                        .Where(u => u.PossibleValue.Contains(pair[j].ToString()));
                    if (exist.Count() == 2)
                    {
                        int t_block = 0;
                        if (exist.First().block == exist.Last().block)
                        {
                            t_block = exist.First().block;
                        }
                        int row1 = exist.First().row;
                        int row2 = exist.Last().row;
                        int col1 = exist.First().column;
                        int col2 = exist.Last().column;

                        RemovePossibility(0, col, t_block, pair[i]);
                        RemovePossibility(0, col, t_block, pair[j]);

                        Position[row1 - 1, col1 - 1].PossibleValue[pair[i] - 1] = pair[i].ToString();
                        Position[row1 - 1, col1 - 1].PossibleValue[pair[j] - 1] = pair[j].ToString();
                        Position[row2 - 1, col2 - 1].PossibleValue[pair[i] - 1] = pair[i].ToString();
                        Position[row2 - 1, col2 - 1].PossibleValue[pair[j] - 1] = pair[j].ToString();

                        Position[row1 - 1, col1 - 1].textBox.Text = Position[row1 - 1, col1 - 1].PossibleValueinRow();
                        Position[row2 - 1, col2 - 1].textBox.Text = Position[row2 - 1, col2 - 1].PossibleValueinRow();

                    }
                }
            }
        }
    }

    private void RemovePossibility(int row, int column, int block, int value)
    {
        var unknown = Position.Cast<Cell>()
                .Where(p => p.value == 0);

        foreach(var p in unknown)
        {
            if (p.row == row || p.column == column || p.block == block)
            {
                p.PossibleValue[value - 1] = "  ";
                if (p.value == 0)
                    p.textBox.Text = p.PossibleValueinRow();
            }
        }
    }

    private void EstimationSolving(int n)
    {
        FontColor = Brushes.Purple;
        var unknown = Position.Cast<Cell>().Where(p => p.value == 0);
        foreach(var p in unknown)
        {
            p.temp_solving++;
        }
        currentEstimation++;
        Trace.WriteLine("Estimation Solving on");
        var Shortest = unknown
            .Where(p => p.PossibleValueWithoutEmpty().Length == 2);
        if(Shortest.Count() == 0)
        {
            var reset = Position.Cast<Cell>().Where(p => p.temp_solving == currentEstimation-1);
            foreach (var r in reset)
            {
                r.value = 0;
                r.textBox.Text = "";
                r.temp_solving--;
                GetAllPosibility();
            }
            currentEstimation--;
            EstimationSolving(n + 1);
        }
        var firstShortest = Shortest.First();
        firstShortest.value = int.Parse(firstShortest.PossibleValueWithoutEmpty()[n].ToString());
        firstShortest.textBox.Text = firstShortest.value.ToString();
        TBFixed(firstShortest.row - 1, firstShortest.column - 1);
        firstShortest.PossibleValueReset();
        RemovePossibility(firstShortest.row, firstShortest.column, firstShortest.block, firstShortest.value);
    }

    private void TBFixed(int row, int col)
    {

        Position[row, col].textBox.Foreground = FontColor;
        Position[row, col].textBox.FontSize = 30;
        Position[row, col].textBox.HorizontalContentAlignment = HorizontalAlignment.Center;
        Position[row, col].textBox.VerticalContentAlignment = VerticalAlignment.Center;
    }

    private void SudokuSolving()
    {

    }
}
