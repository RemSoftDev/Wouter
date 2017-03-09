using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace FoodJournal.Logging
{
    public class PerformanceScope : IDisposable
    {

        private string message;
        private string value;
        private DateTime start = DateTime.Now;

        public PerformanceScope(string Message, string Value) { message = Message; value = Value; }

        public string Message { get { return message; } }

        public void Dispose() { Dispose(true); GC.SuppressFinalize(this); }
        protected virtual void Dispose(bool boolarg)
        {
            SessionLog.RecordPerformance(message, value, (DateTime.Now - start));
        }

        public void SetState(string state) { this.value = state; }
    }
}
