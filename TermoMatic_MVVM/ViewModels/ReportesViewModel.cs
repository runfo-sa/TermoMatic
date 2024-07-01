using iText.Layout.Element;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using TermoMatic;
using TermoMatic_MVVM.Models;
using TermoMatic_MVVM.Views;

namespace TermoMatic_MVVM.ViewModels
{


    public class ReportesViewModel : BaseViewModel
    {
        private Window configReportes = new();

        private DateTime _fechaSeleccionada = DateTime.Today;
        private List<string> _lectoresSeleccionados = [];
        private DataTable? _temperaturasLeidas;

        private ObservableCollection<string> _lectoresObservable = [];

        public ICommand? ConfigurarReporteCommand { get; }
        public ICommand? AceptarConfiguracionCommand { get; }
        public ICommand? CancelarConfiguracionCommand { get; }
        public ICommand? ImprimirReporteCommand { get; }

        public DataTable? TemperaturasLeidas
        {
            get { return _temperaturasLeidas; }
            set
            {
                _temperaturasLeidas = value;
                OnPropertyChanged(nameof(TemperaturasLeidas));
            }
        }

        public ObservableCollection<string> LectoresObservables 
        {
            get { return _lectoresObservable; }
            set 
            { 
                _lectoresObservable = value;
                OnPropertyChanged(nameof(LectoresObservables));
            }
        }

        //public Style ListaLectoresStyle
        //{
        //    get => _listaLectoreStyle;
        //    set
        //    {
        //        _listaLectoreStyle = value;
        //        OnPropertyChanged(nameof(ListaLectoresStyle));
        //    }
        //}

        public DateTime FechaSeleccionada
        {
            get => _fechaSeleccionada;
            set 
            {
                _fechaSeleccionada = value;
                OnPropertyChanged(nameof(FechaSeleccionada));

                try
                {
                    SeleccionarFecha();
                }catch (Exception ex)
                {
                    MessageBox.Show("Error: " + ex.Message, "¡Cáspitas!", MessageBoxButton.OK);
                }

            }
        }

        public List<string> LectoresSeleccionados
        {
            get { return _lectoresSeleccionados; }
            set
            {
                _lectoresSeleccionados = value == null ? [] : value;
                OnPropertyChanged(nameof(LectoresSeleccionados));
            }
        }

        public ReportesViewModel()
        {
            ConfigurarReporteCommand = new RelayCommand(ConfigurarReporte);
            AceptarConfiguracionCommand = new RelayCommandParameter(AceptarConfiguracion);
            ImprimirReporteCommand = new RelayCommand(ImprimirReporte);
            CancelarConfiguracionCommand = new RelayCommand(CancelarConfiguracion);

            //ListaLectoresStyle.Setters.Add(new Setter(ListBoxItem.AllowDropProperty, true));
            //ListaLectoresStyle.Setters.Add(new EventSetter(ListBoxItem.PreviewMouseMoveEvent, new MouseEventHandler(s_PreviewMouseLeftButtonDown)));
            //ListaLectoresStyle.Setters.Add(new EventSetter(ListBoxItem.DropEvent, new DragEventHandler(listbox1_Drop)));
        }

        private void SeleccionarFecha()
        {
            DateTime dt = FechaSeleccionada.Date;

            string archivoConfiguracion = Resources.Resources.ArchivoConfig;
            string cadConexion = Configuracion.CrearCadenaConexionSQL(archivoConfiguracion);

            List<string> lectores = Temperatura.LeerLectoresPorFecha(cadConexion, dt);

            LectoresObservables.Clear();
            LectoresObservables = new(lectores);
            LectoresObservables.Order();
        }

        public void ImprimirReporte()
        {
            if (TemperaturasLeidas != null)
                TemperaturasLeidas = Temperatura.OrdenarTabla(TemperaturasLeidas, "HORA", "ASC");
            
            Table contenido = PDF.ConvertirDataTableEnTable(TemperaturasLeidas??new());

            OpenFolderDialog openFolderDialog = new();

            if(openFolderDialog.ShowDialog() == true)
            {
                PDF.CrearPDF(openFolderDialog.FolderName +@"\" + FechaSeleccionada.ToString("dd-MM-yyyy") + ".pdf", contenido, FechaSeleccionada);
                MessageBox.Show("Listorti", "¡Cáspitas!", MessageBoxButton.OK);
            }
            else
            { 
                MessageBox.Show("Error al seleccionar un archivo.", "¡Cáspitas!", MessageBoxButton.OK);
            }
        }

        public void ActualizarOrdenColumnas(DataGrid dataGrid)
        {
            if (TemperaturasLeidas == null) return;

            List<string?> nuevoOrdenColumnas = dataGrid.Columns
                                                       .OrderBy(c => c.DisplayIndex)
                                                       .Select(c => c.Header.ToString())
                                                       .ToList();


            DataTable nuevaTabla = new DataTable();

            foreach (string? columnName in nuevoOrdenColumnas)
            {
                nuevaTabla.Columns.Add(columnName, TemperaturasLeidas.Columns[columnName].DataType);
            }


            foreach (DataRow row in TemperaturasLeidas.Rows)
            {
                DataRow newRow = nuevaTabla.NewRow();
                foreach (string? columnName in nuevoOrdenColumnas)
                {
                    newRow[columnName??""] = row[columnName??""];
                }
                nuevaTabla.Rows.Add(newRow);
            }

            nuevaTabla.DefaultView.Sort = "HORA ASC";
            TemperaturasLeidas = nuevaTabla.DefaultView.ToTable();
        }

        public void ConfigurarReporte()
        {
            //string archivoConfiguracion = Resources.Resources.ArchivoConfig;
            //string cadConexion = Configuracion.CrearCadenaConexionSQL(archivoConfiguracion);

            //LectoresObservables = new(Temperatura.LeerLectoresActivos(cadConexion));
            //LectoresObservables.Order();

            configReportes = new()
            {
                Content = new ConfiguracionReportes(),
                Title = "Configuración del reporte",
                ResizeMode = ResizeMode.CanResize,
                DataContext = this
            };

            configReportes.Show();
        }

        public void AceptarConfiguracion(object? parameter)
        {
            if (parameter != null)
            {
                System.Collections.IList items = (System.Collections.IList)parameter;

                var collection = items.Cast<string>();

                foreach(var item in LectoresObservables)
                {
                    if(collection.Contains(item))
                        LectoresSeleccionados.Add(item);
                }
            }

            configReportes.Close();
            try
            {
                DateTime dt = FechaSeleccionada.Date;

                string archivoConfiguracion = Resources.Resources.ArchivoConfig;
                string cadConexion = Configuracion.CrearCadenaConexionSQL(archivoConfiguracion);

                List<Temperatura> temps = Temperatura.LeerAlgunasTemperaturasDelDiaSQL(LectoresSeleccionados, dt, cadConexion);

                TemperaturasLeidas = Temperatura.ConvertirListaEnDataTableVisual(temps);

            }catch(Exception ex)
            {
                MessageBox.Show("Error: " + ex.Message, "¡Cáspitas!", MessageBoxButton.OK);
            }

        }

        public void CancelarConfiguracion()
        {
            configReportes.Close();
        }

        //void s_PreviewMouseLeftButtonDown(object sender, MouseEventArgs e)
        //{
        //
        //    if (sender is ListBoxItem && e.LeftButton == MouseButtonState.Pressed)
        //    {
        //        if (sender is not ListBoxItem draggedItem)
        //            return;
        //
        //        DragDrop.DoDragDrop(draggedItem, draggedItem.DataContext, DragDropEffects.Move);
        //        draggedItem.IsSelected = true;
        //    }
        //}
        //
        //void listbox1_Drop(object sender, DragEventArgs e)
        //{
        //    if (e.Data.GetData(typeof(string)) is not string droppedData || 
        //        ((ListBoxItem)(sender)).DataContext is not string target)
        //        return;
        //
        //    int removedIdx = LectoresObservables.IndexOf(droppedData);
        //    int targetIdx = LectoresObservables.IndexOf(target);
        //
        //    if(removedIdx < targetIdx)
        //    {
        //        LectoresObservables.Insert(targetIdx + 1, droppedData);
        //        LectoresObservables.RemoveAt(removedIdx);
        //    }
        //    else
        //    {
        //        int remIdx = removedIdx + 1;
        //        if(LectoresObservables.Count + 1 > remIdx)
        //        {
        //            LectoresObservables.Insert(targetIdx, droppedData);
        //            LectoresObservables.RemoveAt(remIdx);
        //        }
        //    }
        //}
    }
}
