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
using System.Windows.Shapes;

namespace TermoMatic_MVVM.ViewModels
{
    /// <summary>
    /// Lógica de interacción para VentanaEmergente.xaml
    /// </summary>
    public partial class VentanaEmergente : Window
    {

        public VentanaEmergente()
        {
            InitializeComponent();
        }

        private void EventoBotonOk_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        public VentanaEmergente(string texto, string boton, string titulo)
        {
            InitializeComponent();
            Title = titulo;

            var viewModel = (VentanaEmergenteViewModel)DataContext;

            viewModel.CambiarTexto(texto, boton);
        }

        public void CambiarTexto(string texto, string boton, string titulo)
        {
            Title = texto;

            var viewModel = (VentanaEmergenteViewModel)DataContext;

            viewModel.CambiarTexto(texto, boton);
        }
    }
}
