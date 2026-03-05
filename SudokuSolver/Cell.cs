using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace SudokuSolver
{
    public enum CellStatus { Empty, Given, Solved, Estimated }

    public class Cell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        // Fix 1: Add a constructor so the cell formats itself immediately on startup!
        public Cell()
        {
            UpdateUIProperties(); 
        }

        public int Row { get; set; }
        public int Column { get; set; }
        public int Block { get; set; }
        public int TempSolving { get; set; }

        private bool _showPossibilities = false;
        public bool ShowPossibilities
        {
            get => _showPossibilities;
            set { if (_showPossibilities != value) { _showPossibilities = value; OnPropertyChanged(); UpdateUIProperties(); } }
        }

        private int _value = 0;
        public int Value
        {
            get => _value;
            set { if (_value != value) { _value = value; OnPropertyChanged(); UpdateUIProperties(); } }
        }

        private CellStatus _status = CellStatus.Empty;
        public CellStatus Status
        {
            get => _status;
            set { if (_status != value) { _status = value; OnPropertyChanged(); UpdateUIProperties(); } }
        }

        public List<int> PossibleValues { get; set; } = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        // Fix 2: Adding 'if (_variable != value)' to ALL properties stops the UI from freezing!
        private string _displayText;
        public string DisplayText
        {
            get => _displayText;
            set { if (_displayText != value) { _displayText = value; OnPropertyChanged(); } }
        }

        // Change defaults to Black and 30 so the UI starts clean
        private Brush _textForeground = Brushes.Black;
        public Brush TextForeground
        {
            get => _textForeground;
            set { if (_textForeground != value) { _textForeground = value; OnPropertyChanged(); } }
        }

        private int _textSize = 30;
        public int TextSize
        {
            get => _textSize;
            set { if (_textSize != value) { _textSize = value; OnPropertyChanged(); } }
        }

        private bool _isReadOnly = false;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { if (_isReadOnly != value) { _isReadOnly = value; OnPropertyChanged(); } }
        }

        private FontStyle _textStyle = FontStyles.Normal;
        public FontStyle TextStyle
        {
            get => _textStyle;
            set { if (_textStyle != value) { _textStyle = value; OnPropertyChanged(); } }
        }

        public void UpdateUIProperties()
        {
            if (Status == CellStatus.Empty)
            {
                IsReadOnly = false;
                TextStyle = FontStyles.Normal;

                if (ShowPossibilities)
                {
                    TextForeground = Brushes.Red;
                    TextSize = 10;
                    string[] arr = new string[9] { "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  " };
                    foreach (int v in PossibleValues) arr[v - 1] = v.ToString();
                    
                    DisplayText = $"{arr[0]} {arr[1]} {arr[2]}\n{arr[3]} {arr[4]} {arr[5]}\n{arr[6]} {arr[7]} {arr[8]}";
                }
                else
                {
                    TextForeground = Brushes.Black;
                    TextSize = 30;
                    if (DisplayText != null && DisplayText.Contains("\n")) 
                    {
                        DisplayText = ""; 
                    }
                }
            }
            else
            {
                TextSize = 30;
                IsReadOnly = true;
                DisplayText = Value.ToString();

                if (Status == CellStatus.Given) { TextForeground = Brushes.Black; TextStyle = FontStyles.Normal; }
                else if (Status == CellStatus.Solved) { TextForeground = Brushes.Blue; TextStyle = FontStyles.Italic; }
                else if (Status == CellStatus.Estimated) { TextForeground = Brushes.Purple; TextStyle = FontStyles.Italic; }
            }
        }

        public void PossibleValueReset()
        {
            PossibleValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
            UpdateUIProperties();
        }
    }
}