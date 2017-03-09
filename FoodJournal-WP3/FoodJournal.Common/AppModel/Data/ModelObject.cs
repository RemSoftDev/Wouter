using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel;

namespace FoodJournal.AppModel
{
    public abstract class ModelObject
    {

        protected DateTime lastChanged = DateTime.Now;
        public virtual DateTime LastChanged { get { return lastChanged; } }

        protected abstract void SaveIfNotNew();
        public abstract void Save();

        protected virtual void OnObjectPropertyChanged(string propertyName) { }

        public void OnPropertyChanged(string propertyName)
        {
            lastChanged = DateTime.Now;
            OnObjectPropertyChanged(propertyName);
        }
    }
}
