using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace WPFTest3
{
    class Position
    {
        public TextBox TextBox { get; set; } = new TextBox();
        public int row { get; set; }
        public int column { get; set; }
    }
}
