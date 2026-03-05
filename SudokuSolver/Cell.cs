using System;
using System.Collections.Generic;
using System.Windows.Controls;

namespace SudokuSolver
{
    public class Cell
    {
        public TextBox TextBox { get; set; } = new TextBox();
        public int Value { get; set; } = 0;
        public int TempSolving { get; set; } = 0;
        
        // 0-indexed Grid Positions
        public int Row { get; set; }
        public int Column { get; set; }
        public int Block { get; set; }
        
        // Optimized Data Structure: List of ints instead of string array
        public List<int> PossibleValues { get; set; } = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };

        // Formats the possibilities into the 3x3 UI text format on demand
        public string PossibleValueInRow()
        {
            string[] arr = new string[9] { "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  " };
            foreach (int v in PossibleValues)
            {
                arr[v - 1] = v.ToString();
            }
            
            return $"{arr[0]} {arr[1]} {arr[2]}\n" +
                   $"{arr[3]} {arr[4]} {arr[5]}\n" +
                   $"{arr[6]} {arr[7]} {arr[8]}";
        }

        // Returns a string like "149" instead of "1  4    9"
        public string PossibleValueWithoutEmpty()
        {
            return string.Join("", PossibleValues);
        }

        public bool PossibleValueLeft()
        {
            return PossibleValues.Count > 0;
        }

        public void PossibleValueReset()
        {
            PossibleValues = new List<int> { 1, 2, 3, 4, 5, 6, 7, 8, 9 };
        }
    }
}