using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Jace.DemoApp
{
    /// <summary>
    /// Interaction logic for InputDialog.xaml
    /// </summary>
    public partial class InputDialog : Window
    {
        public InputDialog(string variableName)
        {
            InitializeComponent();
            questionLabel.Content = string.Format("Please provide a value for variable \"{0}\":", variableName);
        }

        public Complex Value 
        {
            get
            {
                string text = valueTextBox.Text.TrimStart('(').TrimEnd(')');
                string[] parts = text.Split(',').Select(p => p.Trim()).ToArray();
                double r = 0, i = 0;
                if (parts.Length == 1)
                {
                    if (parts[0].Contains('i'))
                        double.TryParse(parts[0].Replace('i', ' ').Trim(), out i);
                    else
                        double.TryParse(parts[0], out r);
                }
                else if (parts.Length == 2)
                {
                    if (parts[0].Contains('i'))
                    {
                        double.TryParse(parts[0].Replace('i', ' ').Trim(), out i);
                        double.TryParse(parts[1], out r);
                    }
                    else
                    {
                        double.TryParse(parts[0], out r);
                        double.TryParse(parts[1].Replace('i', ' ').Trim(), out i);
                    }
                }
                return new Complex(r, i);
            }
        }

        private void OK_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }
    }
}
