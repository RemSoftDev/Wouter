using FoodJournal.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.WinPhone.Common.ViewModels.Fragments
{
    public interface IAcceptsNewEntry
    {
        bool ShouldSaveNewEntry(Entry entry);
    }
}
