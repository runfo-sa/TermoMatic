using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;

namespace TermoMatic_MVVM.Behaviors
{
    public class DataGridCellEditEndingBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty CommandProperty =
            DependencyProperty.Register(nameof(Command), typeof(ICommand), typeof(DataGridCellEditEndingBehavior));

        public ICommand Command
        {
            get { return (ICommand)GetValue(CommandProperty); }
            set { SetValue(CommandProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.CellEditEnding += OnCellEditEnding;
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();
            AssociatedObject.CellEditEnding -= OnCellEditEnding;
        }

        private void OnCellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            if (Command != null && Command.CanExecute(e))
            {
                Command.Execute(e);
            }
        }
    }
}
