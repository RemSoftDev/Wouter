using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.Logging
{

    public enum LogEntryType
    {
        Performance,
        Milestone,
        Trace,
        Property,
        Query,
        SQL,
        Entry,
        Exception,
        Footer
    }

}
