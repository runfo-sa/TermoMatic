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

namespace TermoMatic_MVVM.ViewModels
{
    public class VentanaEmergenteViewModel : BaseViewModel
    {
        private string _textoMostrado = "Esto es un ejemplo.";
        private string _textoBoton = string.Empty;

        public string TextoMostrado
        {
            get { return _textoMostrado; }
            set 
            {
                _textoMostrado = value;
                OnPropertyChanged(nameof(TextoMostrado)); 
            }
        }

        public string TextoBoton
        {
            get { return _textoBoton; }
            set
            {
                _textoBoton = value;
                OnPropertyChanged(nameof(TextoBoton));
            }
        }

        public VentanaEmergenteViewModel() { }

        public VentanaEmergenteViewModel(string texto, string boton)
        {
            TextoMostrado = texto;
            TextoBoton = boton;
        }

        public void CambiarTexto(string texto, string boton)
        {
            TextoMostrado = texto;
            TextoBoton = boton;
        }
    }
}
