using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
    public Dictionary<int, int> currentEstimation = new Dictionary<int, int>(); 
    public bool pElimination = true;
    public int[][] rows = new int[9][];
    public int[][] columns = new int[9][];
    public int[][] blocks = new int[9][];

    public MainWindow()
    {
        InitializeComponent();
        SudokuGridCreation();
        currentEstimation.Add(0, 0);
    }

    #region

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
                SudokuGrid.Children.Remove(Position[i - 1, j - 1].textBox);
                SudokuGrid.Children.Add(Position[i - 1, j - 1].textBox);
                Grid.SetRow(Position[i - 1, j - 1].textBox, i - 1);
                Grid.SetColumn(Position[i - 1, j - 1].textBox, j - 1);
                Position[i - 1, j - 1].row = i;
                Position[i - 1, j - 1].column = j;
                Position[i - 1, j - 1].block = (i <= 3 ? 0 : i <= 6 ? 3 : 6) + (j <= 3 ? 0 : j <= 6 ? 1 : 2) + 1;
            }
        }
    }

    private void AssignValue()
    {
        for (int i = 0; i < 9; i++)
        {
            for (int j = 0; j < 9; j++)
            {
                Position[i, j].value = Position[i, j].textBox.Text.Length != 1 ? 0 : int.Parse(Position[i, j].textBox.Text);
            }
        }

        GetAllPosibility();
    }

    private void LockValue()
    {
        foreach(Cell tb in Position)
        {
            tb.textBox.IsReadOnly = true;
            if (tb.textBox.Text == ""){
                tb.textBox.FontStyle = FontStyles.Italic;
                tb.textBox.Foreground = Brushes.Blue; 
            }
        }
    }

    private async void ButtonFastSolve_Click(object sender, RoutedEventArgs e)
    {
        if (btn_Lock.IsEnabled == true) ButtonLock_Click(sender, e);

        while (true)
        {
            var solved = Position.Cast<Cell>().Where(p => p.value == 0);
            if (solved.Count() == 0) break;
            else SolvePossibility();
            await Task.Delay(100);
        }
    }


    private void ButtonSolve_Click(object sender, RoutedEventArgs e)
    {
        if(btn_Lock.IsEnabled == true) ButtonLock_Click(sender, e);

        var solved = Position.Cast<Cell>().Where(p => p.value == 0);

        if (solved.Count() == 0)
        {
            MessageBox.Show("Sudoku already solved");
            return;
        }

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

    #endregion

    private void SolvePossibility()
    {
        var checkEmpty = Position.Cast<Cell>().Where(p => p.value == 0).Where(p => p.PossibleValueWithoutEmpty().Length == 0);
        //if exist cell with no possible value, then the estimation is wrong
        if (checkEmpty.Count() > 0)
        {
            ClearEstimation(currentEstimation.Last().Key);//cell with estimation level of the current key will be reset
            currentEstimation[currentEstimation.Last().Key]++;//increase the value of the current key for the second possible value
            EstimationSolving(currentEstimation.Last().Key);//call the estimation solving again
            return;
        }

        #region solving algorithm
        //row solving
        for (int i = 0; i <9 ;i++)
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
                    pElimination = true;
                    return;
                }
            }
        }

        #endregion 

        if (pElimination)
        {
            PossibilityElimination1();
            PossibilityElimination2();
            pElimination = false;
            return;
        }

        currentEstimation.Add(currentEstimation.Count, 0);
        EstimationSolving(currentEstimation.Last().Key);
    }

    #region

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

    #endregion

    private void ClearEstimation(int key)
    {
        //update the estimation solving level that is larger that key
        var reset = Position.Cast<Cell>().Where(p => p.temp_solving >= key);
        foreach (var r in reset)
        {
            r.value = 0;
            r.textBox.Text = "";
            r.temp_solving = key;
            r.PossibleValueReset();
        }
        GetAllPosibility();//after reset, update the possible value again for the empty
    }

    private void EstimationSolving(int key)
    {
        //rethink logic here
        FontColor = Brushes.Purple;

        if (currentEstimation[key] > 1)
        {
            currentEstimation.Remove(key);
            key--;
            currentEstimation[key]++;
            ClearEstimation(key);
            EstimationSolving(key);
            return;
        }   

        var unknown = Position.Cast<Cell>()
            .Where(p => p.value == 0);//filter out cell without answer yet

        //update all the empty cell with new key for estimation level
        foreach (var p in unknown)
        {
            p.temp_solving = key;
        }


        var Shortest = unknown
            .Where(p => p.PossibleValueWithoutEmpty().Length == 2);//filter out cell with the least possible value

        if (Shortest.Count() == 0)
        {
            currentEstimation.Remove(key);
            key--;
            currentEstimation[key]++;
            ClearEstimation(key);
            EstimationSolving(key);
            return;
           
        }

        var firstShortest = Shortest.First();//choose the first cell with least possible value

        

        firstShortest.value = int.Parse(firstShortest.PossibleValueWithoutEmpty()[currentEstimation.Last().Value].ToString());//Choose the first possible value, then second if the first one is wrong
        firstShortest.textBox.Text = firstShortest.value.ToString();//Update the text box with the value
        TBFixed(firstShortest.row - 1, firstShortest.column - 1);//Update the format of the textbox
        firstShortest.PossibleValueReset();//reset the possible value
        RemovePossibility(firstShortest.row, firstShortest.column, firstShortest.block, firstShortest.value);//remove the possibility from the same row, column and block
    }

    private void TBFixed(int row, int col)
    {

        Position[row, col].textBox.Foreground = FontColor;
        Position[row, col].textBox.FontSize = 30;
        Position[row, col].textBox.HorizontalContentAlignment = HorizontalAlignment.Center;
        Position[row, col].textBox.VerticalContentAlignment = VerticalAlignment.Center;
    }

    private void ButtonReset_Click(object sender, RoutedEventArgs e)
    {
        Position = new Cell[9, 9];
        FontColor = Brushes.Blue;
        currentEstimation.Clear();
        currentEstimation.Add(0, 0);
        pElimination = true;
        rows = new int[9][];
        columns = new int[9][];
        blocks = new int[9][];
        SudokuGridCreation();
        btn_Lock.IsEnabled = true;
    }

    private void ButtonLock_Click(object sender, RoutedEventArgs e)
    {
        btn_Lock.IsEnabled = false;
        AssignValue();
        LockValue();
    }
}
