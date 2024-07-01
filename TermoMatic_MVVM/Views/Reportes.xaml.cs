﻿using System;
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
using TermoMatic_MVVM.ViewModels;

namespace TermoMatic_MVVM.Views
{
    /// <summary>
    /// Lógica de interacción para Reportes.xaml
    /// </summary>
    public partial class Reportes : UserControl
    {
        public Reportes()
        {
            InitializeComponent();
        }

        private void DataGrid_ColumnReordered(object sender, DataGridColumnEventArgs e)
        {
            if (sender is DataGrid dataGrid && dataGrid.DataContext is ReportesViewModel viewModel)
            {
                viewModel.ActualizarOrdenColumnas(dataGrid);
            }
        }
    }
}
