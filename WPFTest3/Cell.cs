using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WPFTest3
{
    public class Cell
    {
        public TextBox textBox { get; set; } = new TextBox();
        public int value { get; set; }
        public int temp_solving { get; set; } = 0;
        public int row { get; set; }
        public int column { get; set; }
        public int block { get; set; }
        public string[] PossibleValue { get; set; } = new string[9] {"  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  "};

        public string PossibleValueinRow()
        {
            return 
                $"{this.PossibleValue[0]} {this.PossibleValue[1]} {this.PossibleValue[2]}\n" +
                $"{this.PossibleValue[3]} {this.PossibleValue[4]} {this.PossibleValue[5]}\n" +
                $"{this.PossibleValue[6]} {this.PossibleValue[7]} {this.PossibleValue[8]}";

        }

        public string PossibleValueWithoutEmpty()
        {
            string result = "";
            for (int i = 0; i < 9; i++)
            {
                if (this.PossibleValue[i] != "  ")
                {
                    result += "" + this.PossibleValue[i];
                }
            }
            return result;
        }

        public bool PossibleValueLeft()
        {
            foreach(string s in PossibleValue)
                if (Int32.TryParse(s, out int result)) return true;
            return false;
        }

        public void PossibleValueReset()
        {
            this.PossibleValue = new string[9] { "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  " };
        }

    }
}
