using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using TermoMatic_MVVM.Resources;

namespace TermoMatic_MVVM.Views
{
    /// <summary>
    /// Lógica de interacción para EditarTemperaturas.xaml
    /// </summary>
    public partial class EditarTemperaturas : UserControl
    {
        public EditarTemperaturas()
        {
            InitializeComponent();
        }

        private void dgTemperaturas_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (e.EditAction == DataGridEditAction.Commit)
            {
                if (e.EditingElement is TextBox textBox)
                {
                    if (!decimal.TryParse(textBox.Text, out decimal result))
                    {
                        MessageBox.Show("Por favor ingrese un número válido.", "Entrada inválida", MessageBoxButton.OK, MessageBoxImage.Error);
                        e.Cancel = true;
                    }
                }
            }
        }
    }
}
