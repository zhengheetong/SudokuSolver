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
        public int row { get; set; }
        public int column { get; set; }
        public int block { get; set; }
        public int cellinblock { get; set; }
        public string[] PossibleValue { get; set; } = new string[9] {"  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  "};

        public string PossibleValueinRow()
        {
            return 
                $"{this.PossibleValue[0]} {this.PossibleValue[1]} {this.PossibleValue[2]}\n" +
                $"{this.PossibleValue[3]} {this.PossibleValue[4]} {this.PossibleValue[5]}\n" +
                $"{this.PossibleValue[6]} {this.PossibleValue[7]} {this.PossibleValue[8]}";

        }

        public void PossibleValueReset()
        {
            this.PossibleValue = new string[9] { "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  ", "  " };
        }

    }
}
