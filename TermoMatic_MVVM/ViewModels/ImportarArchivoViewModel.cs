using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;
using System.Windows.Input;
using TermoMatic_MVVM.Models;
using System.IO;
using TermoMatic;
using System.Windows.Threading;

namespace TermoMatic_MVVM.ViewModels
{
    public class ImportarArchivoViewModel : BaseViewModel
    {
        private string? _rutaArchivo;
        private string? _nombreArchivo;

        public string? RutaArchivo
        {
            get { return _rutaArchivo; }
            set
            {
                _rutaArchivo = value;
                OnPropertyChanged(nameof(RutaArchivo));
            }
        }

        public string? NombreArchivo
        {
            get { return _nombreArchivo; }
            set
            {
                _nombreArchivo = value;
                OnPropertyChanged(nameof(NombreArchivo));
            }
        }

        public ICommand? SeleccionarArchivoCommand { get; }
        public ICommand? ImportarArchivoCommand { get; }

        public ImportarArchivoViewModel()
        {
            SeleccionarArchivoCommand = new RelayCommand(SeleccionarArchivo);
            ImportarArchivoCommand = new RelayCommand(ImportarArchivo);
        }

        private void SeleccionarArchivo()
        {
            OpenFileDialog selectorArchivo = new()
            {
                DefaultExt = ".txt",
                Filter = "Documentos de texto|*.txt"
            };

            bool? busqueda = selectorArchivo.ShowDialog();

            if (busqueda == true)
            {
                RutaArchivo = selectorArchivo.FileName;
                NombreArchivo = Path.GetFileName(RutaArchivo);
            }
            else
            {
               MessageBox.Show("Error al seleccionar un archivo.", "¡Cáspitas!", MessageBoxButton.OK);

                //VentanaEmergente ventanaError = new("No se pudo leer el archivo seleciconado.", "OK", "¡Cáspitas!");
                //ventanaError.ShowDialog();
            }
        }

        private void ImportarArchivo() 
        {

            string archivoConfiguracion = Resources.Resources.ArchivoConfig;
            string cadConexion = Configuracion.CrearCadenaConexionSQL(archivoConfiguracion);

            if (RutaArchivo == null || RutaArchivo == "")
            {
                //VentanaEmergente ventanaErrorRutaArchivo = new("Debe seleccionar un archivo válido primero.", "OK", "¡Cáspitas!");
                //ventanaErrorRutaArchivo.ShowDialog();

                MessageBox.Show("Debe seleccionar un archivo válido primero.", "¡Cáspitas!", MessageBoxButton.OK);
                return;
            }

            //VentanaEmergente ventanaProcesandoTemperaturas = new("Procesando las temperaturas del archivo...\nEspere por favor.", "", "¡Operación en proceso!");
            try
            {
                //ventanaProcesandoTemperaturas.Show();
                //Application.Current.Dispatcher.Invoke(DispatcherPriority.Normal, new Action(delegate { ventanaProcesandoTemperaturas.Show(); }));

                List<Temperatura> temps = Temperatura.LeerTemperaturasDeArchivo(RutaArchivo);

                Temperatura.InsertarTemperaturasPorLotes(temps, cadConexion);

                //ventanaProcesandoTemperaturas.CambiarTexto("Archivo importado.", "OK", "¡Operación Completada!");
                MessageBox.Show("Archivo importado.", "¡Operación Completada!", MessageBoxButton.OK);
            }
            catch(Exception ex) 
            {
                MessageBox.Show("Error al importar el archivo.\nDetalle: " + ex.Message, "¡Cáspitas!", MessageBoxButton.OK);
            }

        }
    }
}
