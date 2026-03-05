using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Data;

namespace SudokuSolver
{
    public partial class MainWindow : Window
    {
        private static readonly Regex _regex = new Regex("[^1-9.-]+");
        private static bool IsTextAllowed(string text) => !_regex.IsMatch(text);
        private void Integer(object sender, TextCompositionEventArgs e) => e.Handled = !IsTextAllowed(e.Text);

        public TextBox[,] UIGrid = new TextBox[9, 9];
        public SudokuEngine Engine;

        public MainWindow()
        {
            InitializeComponent();
            
            Cell[,] startingData = new Cell[9, 9];
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    startingData[i, j] = new Cell { Row = i, Column = j, Block = (i / 3) * 3 + (j / 3) + 1 };
            
            Engine = new SudokuEngine(startingData);
            SudokuGridCreation();
        }

        private void SudokuGridCreation()
        {
            var oldTextBoxes = SudokuGrid.Children.OfType<TextBox>().ToList();
            foreach (var oldTb in oldTextBoxes) SudokuGrid.Children.Remove(oldTb);

            UIGrid = new TextBox[9, 9];

            for (int i = 0; i < 9; i++)
            {
                Brush brush1 = Brushes.LightGreen;
                Brush brush2 = Brushes.White;
                if (i >= 3 && i < 6) { brush1 = Brushes.White; brush2 = Brushes.LightGreen; }
                
                for (int j = 0; j < 9; j++)
                {
                    TextBox tb = new TextBox
                    {
                        HorizontalContentAlignment = HorizontalAlignment.Center,
                        VerticalContentAlignment = VerticalAlignment.Center,
                        MaxLength = 1,
                        Background = (j >= 3 && j < 6) ? brush2 : brush1
                    };
                    
                    tb.PreviewTextInput += Integer;
                    
                    // Add the Arrow Key Handler
                    tb.PreviewKeyDown += TextBox_PreviewKeyDown;

                    // ==========================================
                    // THE MVVM MAGIC: Binding XAML to Data Properties
                    // ==========================================
                    tb.DataContext = Engine.Position[i, j];
                    tb.SetBinding(TextBox.TextProperty, new Binding("DisplayText") { Mode = BindingMode.TwoWay, UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged });
                    tb.SetBinding(TextBox.ForegroundProperty, new Binding("TextForeground"));
                    tb.SetBinding(TextBox.FontSizeProperty, new Binding("TextSize"));
                    tb.SetBinding(TextBox.IsReadOnlyProperty, new Binding("IsReadOnly"));
                    tb.SetBinding(TextBox.FontStyleProperty, new Binding("TextStyle"));
                    
                    UIGrid[i, j] = tb;
                    SudokuGrid.Children.Add(tb);
                    Grid.SetRow(tb, i);
                    Grid.SetColumn(tb, j);
                }
            }
        }

        // Arrow Key Navigation Logic
        private void TextBox_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (sender is TextBox tb && tb.DataContext is Cell cell)
            {
                int r = cell.Row;
                int c = cell.Column;

                switch (e.Key)
                {
                    case Key.Up: r--; break;
                    case Key.Down: r++; break;
                    case Key.Left: c--; break;
                    case Key.Right: c++; break;
                    default: return; // Ignore standard typing keys
                }

                // Seamless wraparound navigation
                if (r < 0) r = 8;
                if (r > 8) r = 0;
                if (c < 0) c = 8;
                if (c > 8) c = 0;

                UIGrid[r, c].Focus();
                UIGrid[r, c].SelectAll(); // Selects the text so you can overwrite it instantly
                e.Handled = true;
            }
        }

        private void ReadUserInputIntoEngine()
        {
            for (int i = 0; i < 9; i++)
            {
                for (int j = 0; j < 9; j++)
                {
                    // Because we are using MVVM, we read the data directly from the DisplayText!
                    string text = Engine.Position[i, j].DisplayText?.Trim() ?? "";
                    
                    if (text.Length == 1 && int.TryParse(text, out int val))
                    {
                        Engine.Position[i, j].Value = val;
                        Engine.Position[i, j].Status = CellStatus.Given;
                    }
                    else
                    {
                        Engine.Position[i, j].Value = 0;
                        Engine.Position[i, j].Status = CellStatus.Empty;
                        
                        // Turn on the tiny red possibility numbers for empty cells!
                        Engine.Position[i, j].ShowPossibilities = true; 
                    }
                }
            }
            Engine.GetAllPossibilities();
        }

        private async void ButtonFastSolve_Click(object sender, RoutedEventArgs e)
        {
            if (btn_Lock.IsEnabled) ButtonLock_Click(sender, e);

            while (true)
            {
                var solved = Engine.Position.Cast<Cell>().Where(p => p.Value == 0);
                if (!solved.Any()) break;
                else Engine.SolvePossibility(); 
                await Task.Delay(100);
            }
        }

        private void ButtonSolve_Click(object sender, RoutedEventArgs e)
        {
            if (btn_Lock.IsEnabled) ButtonLock_Click(sender, e);

            var solved = Engine.Position.Cast<Cell>().Where(p => p.Value == 0);

            if (!solved.Any()) MessageBox.Show("Sudoku already solved");
            else Engine.SolvePossibility();
        }

        private void ButtonReset_Click(object sender, RoutedEventArgs e)
        {
            Cell[,] newStartingData = new Cell[9, 9];
            for (int i = 0; i < 9; i++)
                for (int j = 0; j < 9; j++)
                    newStartingData[i, j] = new Cell { Row = i, Column = j, Block = (i / 3) * 3 + (j / 3) + 1 };
            
            Engine = new SudokuEngine(newStartingData);
            SudokuGridCreation();
            btn_Lock.IsEnabled = true;
        }

        private void ButtonLock_Click(object sender, RoutedEventArgs e)
        {
            btn_Lock.IsEnabled = false;
            ReadUserInputIntoEngine();
        }
    }
}