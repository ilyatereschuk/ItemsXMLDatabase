using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ItemsXMLDatabase
{
    public partial class AddItemsClass : Window
    {
        public AddItemsClass()
        {
            InitializeComponent();
        }

        private void btnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false; Close();
        }

        private void btnAdd_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true; Close();
        }

        public new String ShowDialog()
        {
            bool? result = base.ShowDialog();
            if (result ?? false) return textBox1.Text;
            else return String.Empty;
        }
    }
}
