using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace SudokuSolver
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static readonly Regex _regex = new Regex("[^1-9.-]+");
        private static bool IsTextAllowed(string text) => !_regex.IsMatch(text);
        private void Integer(object sender, TextCompositionEventArgs e) => e.Handled = !IsTextAllowed(e.Text);

        public Cell[,] Position = new Cell[9, 9];
        public Brush FontColor = Brushes.Blue;
        public Dictionary<int, int> currentEstimation = new Dictionary<int, int>();
        public bool pElimination = true;

        public MainWindow()
        {
            InitializeComponent();
            SudokuGridCreation();
            currentEstimation.Add(0, 0);
        }

        public void CellAdd(int x, int y, int value)
        {
            Position[x, y].TextBox.Text = value.ToString();
            Position[x, y].Value = value;
            Position[x, y].TextBox.IsReadOnly = true;
        }

        #region Initialization

        private void SudokuGridCreation()
        {
            Position = new Cell[9, 9];
            for (int i = 0; i < 9; i++) // Use 0-indexed loop
            {
                Brush brush1 = Brushes.LightGreen;
                Brush brush2 = Brushes.White;
                
                if (i >= 3 && i < 6)
                {
                    brush1 = Brushes.White;
                    brush2 = Brushes.LightGreen;
                }
                
                for (int j = 0; j < 9; j++)
                {
                    Position[i, j] = new Cell();
                    Position[i, j].TextBox.FontSize = 30;
                    Position[i, j].TextBox.HorizontalContentAlignment = HorizontalAlignment.Center;
                    Position[i, j].TextBox.VerticalContentAlignment = VerticalAlignment.Center;
                    Position[i, j].TextBox.MaxLength = 1;
                    Position[i, j].TextBox.PreviewTextInput += Integer; 
                    
                    Position[i, j].TextBox.Background = brush1;
                    if (j >= 3 && j < 6) Position[i, j].TextBox.Background = brush2;
                    
                    SudokuGrid.Children.Remove(Position[i, j].TextBox);
                    SudokuGrid.Children.Add(Position[i, j].TextBox);
                    
                    Grid.SetRow(Position[i, j].TextBox, i);
                    Grid.SetColumn(Position[i, j].TextBox, j);
                    
                    Position[i, j].Row = i;
                    Position[i, j].Column = j;
                    Position[i, j].Block = (i / 3) * 3 + (j / 3) + 1; // Block 1 to 9
                }
            }
        }

        private void AssignValue()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    Position[i, j].Value = Position[i, j].TextBox.Text.Length != 1 ? 0 : int.Parse(Position[i, j].TextBox.Text);
                }
            }

            GetAllPosibility();
        }

        private void LockValue()
        {
            foreach (Cell tb in Position)
            {
                tb.TextBox.IsReadOnly = true;
                if (tb.TextBox.Text == "")
                {
                    tb.TextBox.FontStyle = FontStyles.Italic;
                    tb.TextBox.Foreground = Brushes.Blue;
                }
            }
        }

        private async void ButtonFastSolve_Click(object sender, RoutedEventArgs e)
        {
            if (btn_Lock.IsEnabled) ButtonLock_Click(sender, e);

            while (true)
            {
                var solved = Position.Cast<Cell>().Where(p => p.Value == 0);
                if (!solved.Any()) break;
                else SolvePossibility();
                await Task.Delay(100);
            }
        }

        private void ButtonSolve_Click(object sender, RoutedEventArgs e)
        {
            if (btn_Lock.IsEnabled) ButtonLock_Click(sender, e);

            var solved = Position.Cast<Cell>().Where(p => p.Value == 0);

            if (!solved.Any())
            {
                MessageBox.Show("Sudoku already solved");
                return;
            }

            SolvePossibility();
        }

        private void GetAllPosibility()
        {
            // Reset all empty cells to full 1-9 possibilities
            foreach (var cell in Position)
            {
                if (cell.Value == 0) cell.PossibleValueReset();
                else cell.PossibleValues.Clear();
            }

            // Remove possibilities based on existing numbers on the board
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Position[i, j].Value != 0)
                    {
                        RemovePossibility(Position[i, j].Row, Position[i, j].Column, Position[i, j].Block, Position[i, j].Value);
                    }
                }
            }

            // Update UI for empty cells
            foreach (var cell in Position)
            {
                if (cell.Value == 0)
                {
                    cell.TextBox.Foreground = Brushes.Red;
                    cell.TextBox.FontSize = 10;
                    cell.TextBox.HorizontalContentAlignment = HorizontalAlignment.Left;
                    cell.TextBox.VerticalContentAlignment = VerticalAlignment.Top;
                    cell.TextBox.Text = cell.PossibleValueInRow();
                }
            }
        }

        #endregion

        private void SolvePossibility()
        {
            var checkEmpty = Position.Cast<Cell>().Where(p => p.Value == 0 && p.PossibleValues.Count == 0);
            
            // if exist cell with no possible value, then the estimation is wrong
            if (checkEmpty.Any())
            {
                ClearEstimation(currentEstimation.Last().Key); 
                currentEstimation[currentEstimation.Last().Key]++; 
                EstimationSolving(currentEstimation.Last().Key); 
                return;
            }

            #region solving algorithm
            
            // Row solving
            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, int> posibilitycheck = new Dictionary<int, int>();
                for (int j = 0; j < 9; j++)
                {
                    foreach (int s in Position[i, j].PossibleValues)
                    {
                        if (posibilitycheck.ContainsKey(s)) posibilitycheck[s]++;
                        else posibilitycheck[s] = 1;
                    }
                }
                foreach (KeyValuePair<int, int> kvp in posibilitycheck)
                {
                    if (kvp.Value == 1)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            if (Position[i, j].PossibleValues.Contains(kvp.Key))
                            {
                                TBFixed(i, j);
                                Position[i, j].TextBox.Text = kvp.Key.ToString();
                                Position[i, j].Value = kvp.Key;
                                Position[i, j].PossibleValues.Clear();
                                RemovePossibility(Position[i, j].Row, Position[i, j].Column, Position[i, j].Block, Position[i, j].Value);
                                pElimination = true;
                                return;
                            }
                        }
                    }
                }
            }

            // Column solving
            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, int> posibilitycheck = new Dictionary<int, int>();
                for (int j = 0; j < 9; j++)
                {
                    foreach (int s in Position[j, i].PossibleValues)
                    {
                        if (posibilitycheck.ContainsKey(s)) posibilitycheck[s]++;
                        else posibilitycheck[s] = 1;
                    }
                }
                foreach (KeyValuePair<int, int> kvp in posibilitycheck)
                {
                    if (kvp.Value == 1)
                    {
                        for (int j = 0; j < 9; j++)
                        {
                            if (Position[j, i].PossibleValues.Contains(kvp.Key))
                            {
                                TBFixed(j, i);
                                Position[j, i].TextBox.Text = kvp.Key.ToString();
                                Position[j, i].Value = kvp.Key;
                                Position[j, i].PossibleValues.Clear();
                                RemovePossibility(Position[j, i].Row, Position[j, i].Column, Position[j, i].Block, Position[j, i].Value);
                                pElimination = true;
                                return;
                            }
                        }
                    }
                }
            }

            // Box solving
            for (int i = 0; i < 9; i++)
            {
                Dictionary<int, int> posibilitycheck = new Dictionary<int, int>();
                foreach (Cell c in Position)
                {
                    if (c.Block == i + 1)
                    {
                        foreach (int s in c.PossibleValues)
                        {
                            if (posibilitycheck.ContainsKey(s)) posibilitycheck[s]++;
                            else posibilitycheck[s] = 1;
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
                            if (Position[row, column].PossibleValues.Contains(kvp.Key))
                            {
                                TBFixed(row, column);
                                Position[row, column].TextBox.Text = kvp.Key.ToString();
                                Position[row, column].Value = kvp.Key;
                                Position[row, column].PossibleValues.Clear();
                                RemovePossibility(Position[row, column].Row, Position[row, column].Column, Position[row, column].Block, Position[row, column].Value);
                                pElimination = true;
                                return;
                            }
                        }
                    }
                }
            }

            // Remaining solving (Naked singles)
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    if (Position[i, j].Value == 0 && Position[i, j].PossibleValues.Count == 1)
                    {
                        TBFixed(i, j);
                        int val = Position[i, j].PossibleValues[0];
                        Position[i, j].TextBox.Text = val.ToString();
                        Position[i, j].Value = val;
                        Position[i, j].PossibleValues.Clear();
                        RemovePossibility(Position[i, j].Row, Position[i, j].Column, Position[i, j].Block, Position[i, j].Value);
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
                PossibilityElimination3();
                pElimination = false;
                return;
            }

            currentEstimation.Add(currentEstimation.Count, 0);
            EstimationSolving(currentEstimation.Last().Key);
        }

        #region Logic Eliminations

        private void PossibilityElimination1()
        {
            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0).ToList();

            for (int i = 0; i < unknown.Count; i++)
            {
                for (int j = i + 1; j < unknown.Count; j++)
                {
                    var p1 = unknown[i];
                    var p2 = unknown[j];

                    if (p1.PossibleValues.Count == 2 && p1.PossibleValues.SequenceEqual(p2.PossibleValues))
                    {
                        int t_row = -1, t_column = -1, t_block = -1;

                        if (p1.Row == p2.Row) t_row = p1.Row;
                        else if (p1.Column == p2.Column) t_column = p1.Column;
                        
                        if (p1.Block == p2.Block) t_block = p1.Block;

                        if (t_row == -1 && t_column == -1 && t_block == -1) continue;

                        int val1 = p1.PossibleValues[0];
                        int val2 = p1.PossibleValues[1];

                        RemovePossibility(t_row, t_column, t_block, val1);
                        RemovePossibility(t_row, t_column, t_block, val2);

                        // Re-add the pair possibilities since RemovePossibility wiped them out
                        p1.PossibleValues = new List<int> { val1, val2 };
                        p2.PossibleValues = new List<int> { val1, val2 };

                        p1.TextBox.Text = p1.PossibleValueInRow();
                        p2.TextBox.Text = p2.PossibleValueInRow();
                    }
                }
            }
        }

        private void PossibilityElimination2()
        {
            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0).ToList();

            // Rows
            for (int row = 0; row < 9; row++)
            {
                var cellinrow = unknown.Where(p => p.Row == row).ToList();
                var keyValuePairs = new Dictionary<int, int>();
                
                foreach (var p in cellinrow)
                {
                    foreach (int s in p.PossibleValues)
                    {
                        if (keyValuePairs.ContainsKey(s)) keyValuePairs[s]++;
                        else keyValuePairs[s] = 1;
                    }
                }

                var pair = keyValuePairs.Where(kvp => kvp.Value == 2).Select(kvp => kvp.Key).ToList();
                
                for (int i = 0; i < pair.Count; i++)
                {
                    for (int j = i + 1; j < pair.Count; j++)
                    {
                        var exist = cellinrow.Where(u => u.PossibleValues.Contains(pair[i]) && u.PossibleValues.Contains(pair[j])).ToList();
                        if (exist.Count == 2)
                        {
                            int t_block = exist[0].Block == exist[1].Block ? exist[0].Block : -1;
                            var p1 = exist[0];
                            var p2 = exist[1];

                            RemovePossibility(row, -1, t_block, pair[i]);
                            RemovePossibility(row, -1, t_block, pair[j]);

                            p1.PossibleValues = new List<int> { pair[i], pair[j] };
                            p2.PossibleValues = new List<int> { pair[i], pair[j] };

                            p1.TextBox.Text = p1.PossibleValueInRow();
                            p2.TextBox.Text = p2.PossibleValueInRow();
                        }
                    }
                }
            }

            // Columns
            for (int col = 0; col < 9; col++)
            {
                var cellincol = unknown.Where(p => p.Column == col).ToList();
                var keyValuePairs = new Dictionary<int, int>();
                
                foreach (var p in cellincol)
                {
                    foreach (int s in p.PossibleValues)
                    {
                        if (keyValuePairs.ContainsKey(s)) keyValuePairs[s]++;
                        else keyValuePairs[s] = 1;
                    }
                }

                var pair = keyValuePairs.Where(kvp => kvp.Value == 2).Select(kvp => kvp.Key).ToList();
                
                for (int i = 0; i < pair.Count; i++)
                {
                    for (int j = i + 1; j < pair.Count; j++)
                    {
                        var exist = cellincol.Where(u => u.PossibleValues.Contains(pair[i]) && u.PossibleValues.Contains(pair[j])).ToList();
                        if (exist.Count == 2)
                        {
                            int t_block = exist[0].Block == exist[1].Block ? exist[0].Block : -1;
                            var p1 = exist[0];
                            var p2 = exist[1];

                            RemovePossibility(-1, col, t_block, pair[i]);
                            RemovePossibility(-1, col, t_block, pair[j]);

                            p1.PossibleValues = new List<int> { pair[i], pair[j] };
                            p2.PossibleValues = new List<int> { pair[i], pair[j] };

                            p1.TextBox.Text = p1.PossibleValueInRow();
                            p2.TextBox.Text = p2.PossibleValueInRow();
                        }
                    }
                }
            }

            // Blocks
            for (int block = 1; block <= 9; block++)
            {
                var cellsinblock = unknown.Where(p => p.Block == block).ToList();
                var keyValuePairs = new Dictionary<int, int>();
                
                foreach (var p in cellsinblock)
                {
                    foreach (int s in p.PossibleValues)
                    {
                        if (keyValuePairs.ContainsKey(s)) keyValuePairs[s]++;
                        else keyValuePairs[s] = 1;
                    }
                }

                var pair = keyValuePairs.Where(kvp => kvp.Value == 2).Select(kvp => kvp.Key).ToList();

                for (int i = 0; i < pair.Count; i++)
                {
                    for (int j = i + 1; j < pair.Count; j++)
                    {
                        var exist = cellsinblock.Where(u => u.PossibleValues.Contains(pair[i]) && u.PossibleValues.Contains(pair[j])).ToList();

                        if (exist.Count == 2)
                        {
                            var p1 = exist[0];
                            var p2 = exist[1];

                            RemovePossibility(-1, -1, block, pair[i]);
                            RemovePossibility(-1, -1, block, pair[j]);

                            p1.PossibleValues = new List<int> { pair[i], pair[j] };
                            p2.PossibleValues = new List<int> { pair[i], pair[j] };

                            p1.TextBox.Text = p1.PossibleValueInRow();
                            p2.TextBox.Text = p2.PossibleValueInRow();
                        }
                    }
                }
            }
        }

        private void PossibilityElimination3()
        {
            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0).ToList();
            
            #region row
            for (int i = 1; i <= 9; i++)
            {
                Dictionary<int, string> blockrowsPair = new Dictionary<int, string>();
                var temp = unknown.Where(p => p.PossibleValues.Contains(i));
                
                foreach (var p in temp)
                {
                    if (blockrowsPair.ContainsKey(p.Block) && !blockrowsPair[p.Block].Contains(p.Row.ToString()))
                        blockrowsPair[p.Block] += p.Row.ToString();
                    else if (!blockrowsPair.ContainsKey(p.Block))
                        blockrowsPair[p.Block] = p.Row.ToString();
                }
                
                for (int j = 1; j <= 9; j += 3)
                {
                    if (!(blockrowsPair.ContainsKey(j) && blockrowsPair.ContainsKey(j + 1) && blockrowsPair.ContainsKey(j + 2))) continue;
                    
                    string blockx = String.Concat(blockrowsPair[j].OrderBy(c => c));
                    string blocky = String.Concat(blockrowsPair[j + 1].OrderBy(c => c));
                    string blockz = String.Concat(blockrowsPair[j + 2].OrderBy(c => c));

                    int blocktochange = 0;
                    string excluding = "";

                    if (blockx == blocky && blockx.Length == 2) { excluding = blockx; blocktochange = j + 2; }
                    else if (blockx == blockz && blockx.Length == 2) { excluding = blockx; blocktochange = j + 1; }
                    else if (blocky == blockz && blocky.Length == 2) { excluding = blocky; blocktochange = j; }
                    else continue;

                    var exist = unknown
                        .Where(u => u.Block == blocktochange)
                        .Where(u => u.PossibleValues.Contains(i))
                        .Where(u => excluding.Contains(u.Row.ToString()));

                    foreach (var e in exist)
                    {
                        e.PossibleValues.Remove(i);
                        e.TextBox.Text = e.PossibleValueInRow();
                    }
                }
            }
            #endregion
            
            #region column
            for (int i = 1; i <= 9; i++)
            {
                Dictionary<int, string> blockcolumnsPair = new Dictionary<int, string>();
                var temp = unknown.Where(p => p.PossibleValues.Contains(i));
                
                foreach (var p in temp)
                {
                    if (blockcolumnsPair.ContainsKey(p.Block) && !blockcolumnsPair[p.Block].Contains(p.Column.ToString()))
                        blockcolumnsPair[p.Block] += p.Column.ToString();
                    else if (!blockcolumnsPair.ContainsKey(p.Block))
                        blockcolumnsPair[p.Block] = p.Column.ToString();
                }
                
                for (int j = 1; j <= 9; j += 3)
                {
                    if (!(blockcolumnsPair.ContainsKey(j) && blockcolumnsPair.ContainsKey(j + 3) && blockcolumnsPair.ContainsKey(j + 6))) continue;
                    
                    string blockx = String.Concat(blockcolumnsPair[j].OrderBy(c => c));
                    string blocky = String.Concat(blockcolumnsPair[j + 3].OrderBy(c => c));
                    string blockz = String.Concat(blockcolumnsPair[j + 6].OrderBy(c => c));
                    
                    int blocktochange = 0;
                    string excluding = "";
                    
                    if (blockx == blocky && blockx.Length == 2) { excluding = blockx; blocktochange = j + 6; }
                    else if (blockx == blockz && blockx.Length == 2) { excluding = blockx; blocktochange = j + 3; }
                    else if (blocky == blockz && blocky.Length == 2) { excluding = blocky; blocktochange = j; }
                    else continue;
                    
                    var exist = unknown
                        .Where(u => u.Block == blocktochange)
                        .Where(u => u.PossibleValues.Contains(i))
                        .Where(u => excluding.Contains(u.Column.ToString()));
                        
                    foreach (var e in exist)
                    {
                        e.PossibleValues.Remove(i);
                        e.TextBox.Text = e.PossibleValueInRow();
                    }
                }
            }
            #endregion
        }

        private void RemovePossibility(int row, int column, int block, int value)
        {
            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0);

            foreach (var p in unknown)
            {
                // Passing -1 means we ignore that check entirely
                if (p.Row == row || p.Column == column || p.Block == block)
                {
                    p.PossibleValues.Remove(value);
                    if (p.Value == 0) p.TextBox.Text = p.PossibleValueInRow();
                }
            }
        }

        #endregion

        private void ClearEstimation(int key)
        {
            var reset = Position.Cast<Cell>().Where(p => p.TempSolving >= key);
            foreach (var r in reset)
            {
                r.Value = 0;
                r.TextBox.Text = "";
                r.TempSolving = key;
                r.PossibleValueReset();
            }
            GetAllPosibility(); 
        }

        private void EstimationSolving(int key)
        {
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

            var unknown = Position.Cast<Cell>().Where(p => p.Value == 0);

            foreach (var p in unknown)
            {
                p.TempSolving = key;
            }

            var shortest = unknown.Where(p => p.PossibleValues.Count == 2);

            if (!shortest.Any())
            {
                currentEstimation.Remove(key);
                key--;
                currentEstimation[key]++;
                ClearEstimation(key);
                EstimationSolving(key);
                return;
            }

            var firstShortest = shortest.First();

            firstShortest.Value = firstShortest.PossibleValues[currentEstimation.Last().Value];
            firstShortest.TextBox.Text = firstShortest.Value.ToString();
            
            TBFixed(firstShortest.Row, firstShortest.Column);
            firstShortest.PossibleValues.Clear();
            RemovePossibility(firstShortest.Row, firstShortest.Column, firstShortest.Block, firstShortest.Value);
        }

        private void TBFixed(int row, int col)
        {
            Position[row, col].TextBox.Foreground = FontColor;
            Position[row, col].TextBox.FontSize = 30;
            Position[row, col].TextBox.HorizontalContentAlignment = HorizontalAlignment.Center;
            Position[row, col].TextBox.VerticalContentAlignment = VerticalAlignment.Center;
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            Position = new Cell[9, 9];
            FontColor = Brushes.Blue;
            currentEstimation.Clear();
            currentEstimation.Add(0, 0);
            pElimination = true;
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
}