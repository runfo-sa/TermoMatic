using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TermoMatic_MVVM.Models
{
    public class Memento
    {
        public DataTable State { get; private set; }
        
        public Memento(DataTable state) 
        {
            State = state.Copy();
        }
    }

    public class Originator
    {
        public DataTable DataTable { get; set; }

        public Memento CreateMemento()
        {
            return new Memento(DataTable);
        }

        public void RestoreMemento(Memento memento)
        {
            DataTable = memento.State.Copy();
        }
    }

    public class Caretaker
    {
        private Stack<Memento> _mementoStack = new();

        public void SaveState(Memento memento)
        {
            _mementoStack.Push(memento);
        }

        public Memento? RestoreState()
        {
            if (_mementoStack.Count > 0)
            {
                return _mementoStack.Pop();
            }
            return null;
        }
    }
}
