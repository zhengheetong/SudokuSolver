using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace SudokuSolver
{
    public enum CellStatus { Empty, Given, Solved, Estimated }

    // INotifyPropertyChanged is the backbone of MVVM XAML Data Binding!
    public class Cell : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

        public int Row { get; set; }
        public int Column { get; set; }
        public int Block { get; set; }
        public int TempSolving { get; set; }

        private int _value = 0;
        public int Value
        {
            get => _value;
            set { _value = value; OnPropertyChanged(); UpdateUIProperties(); }
        }

        private CellStatus _status = CellStatus.Empty;
        public CellStatus Status
        {
            get => _status;
            set { _status = value; OnPropertyChanged(); UpdateUIProperties(); }
        }

        public List<int> PossibleValues { get; set; } = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        // ==========================================
        // UI BINDING PROPERTIES 
        // XAML will listen to these and update instantly!
        // ==========================================
        
        private string _displayText;
        public string DisplayText
        {
            get => _displayText;
            set { _displayText = value; OnPropertyChanged(); }
        }

        private Brush _textForeground = Brushes.Red;
        public Brush TextForeground
        {
            get => _textForeground;
            set { _textForeground = value; OnPropertyChanged(); }
        }

        private int _textSize = 10;
        public int TextSize
        {
            get => _textSize;
            set { _textSize = value; OnPropertyChanged(); }
        }

        private bool _isReadOnly = false;
        public bool IsReadOnly
        {
            get => _isReadOnly;
            set { _isReadOnly = value; OnPropertyChanged(); }
        }

        private FontStyle _textStyle = FontStyles.Normal;
        public FontStyle TextStyle
        {
            get => _textStyle;
            set { _textStyle = value; OnPropertyChanged(); }
        }

        // Formats the data and broadcasts the changes to the UI
        public void UpdateUIProperties()
        {
            if (Status == CellStatus.Empty)
            {
                TextForeground = Brushes.Red;
                TextSize = 10;
                IsReadOnly = false;
                TextStyle = FontStyles.Normal;

                string[] arr = new string[9] { "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  " };
                foreach (int v in PossibleValues) arr[v - 1] = v.ToString();
                
                DisplayText = $"{arr[0]} {arr[1]} {arr[2]}\n{arr[3]} {arr[4]} {arr[5]}\n{arr[6]} {arr[7]} {arr[8]}";
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