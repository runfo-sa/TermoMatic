using System.Collections.ObjectModel;
using System.Data;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TermoMatic;
using TermoMatic_MVVM.Models;

namespace TermoMatic_MVVM.ViewModels
{
    public class EditarTemperaturasViewModel : BaseViewModel
    {
        private DateTime _fechaSeleccionada = DateTime.Today;
        private DataTable? _temperaturasLeidas = new();
        private List<Temperatura> _temperaturasEditadas = [];


        public DateTime FechaSeleccionada
        {
            get { return _fechaSeleccionada; }
            set
            {
                _fechaSeleccionada = value;
                OnPropertyChanged(nameof(FechaSeleccionada));
                try
                {
                    LeerTemperaturasDelDiaSeleccionado();
                }catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "¡Cáspitas!", MessageBoxButton.OK);
                }
            }
        }

        public DataTable? TemperaturasLeidas
        {
            get { return _temperaturasLeidas; }
            set
            {
                _temperaturasLeidas = value;
                OnPropertyChanged(nameof(TemperaturasLeidas));
            }
        }

        //public ICommand? SeleccionarFechaCommand { get; }
        public ICommand? GuardarNuevasTemperaturasCommand { get; }
        public ICommand? EditarTemperaturasCommand { get; }

        public EditarTemperaturasViewModel()
        {
            //SeleccionarFechaCommand = new RelayCommand(SeleccionarFecha);
            GuardarNuevasTemperaturasCommand = new RelayCommand(GuardarNuevasTemperaturas);
            EditarTemperaturasCommand = new ActionCommand<DataGridCellEditEndingEventArgs>(EditarRegistroTemperaturas);
        }

        private void LeerTemperaturasDelDiaSeleccionado()
        {
            DateTime dt = FechaSeleccionada.Date;

            string archivoConfiguracion = Resources.Resources.ArchivoConfig;
            string cadConexion = Configuracion.CrearCadenaConexionSQL(archivoConfiguracion);

            List<Temperatura> temps = Temperatura.LeerTemperaturasDelDiaSQL(dt, cadConexion);

            //TemperaturaObservables = Temperatura.ConvertirListaEnObservableCollection(temps);
        }

        private void GuardarNuevasTemperaturas()
        {
            try
            {
                string archivoConfiguracion = Resources.Resources.ArchivoConfig;
                string cadConexion = Configuracion.CrearCadenaConexionSQL(archivoConfiguracion);

                string usuario = Environment.UserName;
                string equipo = Environment.MachineName;

                Temperatura.ActualizarTemperaturaPorLotes(_temperaturasEditadas, usuario, equipo, cadConexion);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "¡Cáspitas!", MessageBoxButton.OK);
            }
        }

        private void EditarRegistroTemperaturas(DataGridCellEditEndingEventArgs e)
        {

            if (e != null && e.EditAction == DataGridEditAction.Commit)
            {
                var filaEditada = e.Row.Item as DataRowView;
                if (filaEditada != null)
                {
                    var columnaEditada = e.Column as DataGridBoundColumn;

                    if (columnaEditada != null)
                    {
                        var binding = (columnaEditada.Binding as Binding);

                        if (binding != null)
                        {
                            var lector = binding.Path.Path;

                            var textBox = e.EditingElement as TextBox;

                            if (textBox != null)
                            {
                                var temperaturaEditada = textBox.Text;

                                var horaEditada = filaEditada["HORA"].ToString();

                                if (horaEditada != null)
                                {
                                    var fechaHora = FechaSeleccionada.ToString("dd/MM/yyyy ", CultureInfo.InvariantCulture) + horaEditada;

                                    Temperatura temp = new(lector, temperaturaEditada, fechaHora);

                                    _temperaturasEditadas.Add(temp);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
