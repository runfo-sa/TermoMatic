using System.Collections.ObjectModel;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using TermoMatic;
using TermoMatic_MVVM.Models;

namespace TermoMatic_MVVM.ViewModels
{
    public class TemperaturaPivot
    {
        public DateTime Registro { get; set; }
        public Dictionary<string, decimal> _lecturas { get; set; } = [];

        public void SetLectura(string lector, decimal lectura)
        {
            _lecturas[lector] = lectura;
        }

        public decimal GetLectura(string lector)
        {
            return _lecturas.TryGetValue(lector, out decimal value) ? value : default(decimal);
        }

        public static List<string> Lectores { get; set; } = [];

        public decimal this[string lector]
        {
            get => GetLectura(lector);
            set => SetLectura(lector, value);
        }
    }

    public class EditarTemperaturasViewModel : BaseViewModel
    {
        private DateTime _fechaSeleccionada = DateTime.Today;
        private DataTable? _temperaturasLeidas = new();
        private List<Temperatura> _temperaturasEditadas = [];
        //private ObservableCollection<TemperaturaObservable> _temperaturasObservables = [];
        //private ObservableCollection<Temperatura> _temperaturasLeidas2 = [];
        //ObservableCollection<TemperaturaPivot> _expandoTemp = [];

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
                    MessageBox.Show("No se pudieron leer las temperaturas\nDetalle: " + ex.Message, "¡Cáspitas!", MessageBoxButton.OK);
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

       // public ObservableCollection<Temperatura> TemperaturaObservables
       // {
       //     get { return _temperaturasLeidas2; }
       //     set
       //     {
       //         _temperaturasLeidas2 = value;
       //         OnPropertyChanged(nameof(TemperaturaObservables));
       //     }
       // }
       //
       // public ObservableCollection<TemperaturaPivot> TemperaturaPivots
       // {
       //     get { return _expandoTemp; }
       //     set
       //     {
       //         _expandoTemp = value;
       //         OnPropertyChanged(nameof(TemperaturaPivots));
       //     }
       // }
       //
        //public ObservableCollection<TemperaturaObservable> TemperaturaObservables
        //{
        //    get { return _temperaturasObservables; }
        //    set
        //    {
        //        _temperaturasObservables = value;
        //        OnPropertyChanged(nameof(TemperaturaObservables));
        //    }
        //}

        public ICommand? GuardarNuevasTemperaturasCommand { get; }
        public ICommand? EditarTemperaturasCommand { get; }

        public EditarTemperaturasViewModel()
        {
            GuardarNuevasTemperaturasCommand = new RelayCommand(GuardarNuevasTemperaturas);
            EditarTemperaturasCommand = new ActionCommand<DataGridCellEditEndingEventArgs>(EditarRegistroTemperaturas);
        }

        private void LeerTemperaturasDelDiaSeleccionado()
        {
            DateTime dt = FechaSeleccionada.Date;

            string archivoConfiguracion = Resources.Resources.ArchivoConfig;
            string cadConexion = Configuracion.CrearCadenaConexionSQL(archivoConfiguracion);

            List<Temperatura> temps = Temperatura.LeerTemperaturasDelDiaSQL(dt, cadConexion);

            TemperaturasLeidas = Temperatura.ConvertirListaEnDataTableVisual(temps);
            //TemperaturaObservables = new(temps);

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
                MessageBox.Show("No se pudo guardar las temperaturas editadas.\nDetalle: " + ex.Message, "¡Cáspitas!", MessageBoxButton.OK);
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

        public static dynamic pivot(IEnumerable<Temperatura> rows)
        {
            IDictionary<string, Object?> expando = new ExpandoObject();

            expando["HORA"] = rows.FirstOrDefault()?.Registro;

            foreach (var row in rows)
            {
                expando[row.Lector] = row.Lectura;
            }

            return expando;
        }
    }
}
