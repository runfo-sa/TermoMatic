using Microsoft.Win32;
using System.Data;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TermoMatic
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly string ArchivoConfiguracion = "appsettings.json";
        private List<Temperatura> TemperaturasEditadas = [];

        public MainWindow()
        {
            InitializeComponent();
        }

        private void btSeleccionarArchivoClick(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new()
            {
                DefaultExt = ".txt",
                Filter = "Documentos de Texto|*.txt"
            };

            bool? busqueda = ofd.ShowDialog();

            if (busqueda == null || busqueda == false)
                MessageBox.Show("Error al seleccionar un archivo.", "¡Cáspitas!", MessageBoxButton.OK);
            
            if(busqueda == true)
            {
                tbRutaArchivo.Text = ofd.FileName;
                tbNombreArchivo.Text = System.IO.Path.GetFileName(ofd.FileName);
            }
        }

        private void btImportarArchivoClick(object sender, RoutedEventArgs e)
        {
            string cadConexion = Configuracion.CrearCadenaConexionSQL(ArchivoConfiguracion);

            List<Temperatura> temps = Temperatura.LeerTemperaturasDeArchivo(tbRutaArchivo.Text);

            Temperatura.InsertarTemperaturasPorLotes(temps, cadConexion);

            MessageBox.Show("Archivo importado.", "OK", MessageBoxButton.OK);
        }

        private void dpFechaRegistrosSelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            DateTime dt = dpFechaRegistros.SelectedDate ?? DateTime.MinValue;

            if (dt == DateTime.MinValue) 
                return;

            string cadConexion = Configuracion.CrearCadenaConexionSQL(ArchivoConfiguracion);

            List<Temperatura> temps = Temperatura.LeerTemperaturasDelDiaSQL(dt, cadConexion);

            DataTable dtTemps = Temperatura.ConvertirListaEnDataTableVisual(temps);

            dgTemperaturas.ItemsSource = dtTemps.DefaultView;
        }

        private void dgTemperaturasCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            string lector = (string)e.Column.Header;
            //var lectura = e.Row.GetIndex();
            //var columna = e.Column.DisplayIndex;

            DataGrid dg = (DataGrid)sender;
            DataGridRow dgRow = (DataGridRow)dg.ItemContainerGenerator.ContainerFromIndex(dg.SelectedIndex);
            DataGridCell dgCell = (DataGridCell)dg.Columns[0].GetCellContent(dgRow).Parent;
            string registro = ((TextBlock)dgCell.Content).Text;

            TextBox tbLectura = (TextBox)e.EditingElement;

            string lectura = tbLectura.Text;

            Temperatura temperatura = new Temperatura(lector, lectura, registro);

            TemperaturasEditadas.Add(temperatura);
        }
    }
}