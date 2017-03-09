//using FoodJournal.AppModel.SQLite;
using System;
using System.Collections.Generic;
using System.ComponentModel;
#if WINDOWS_PHONE
using System.Data.Linq.Mapping;
#else
using FoodJournal.AppModel.SQLite;
#endif
using System.Linq;
using System.Text;

namespace FoodJournal.DataModel
{

	[Table(Name="MessageQueue")]
    public partial class MessageQueueRow
    {

        public MessageQueueRow() { }

		#if WINDOWS_PHONE
		[Column(IsPrimaryKey = true, IsDbGenerated = true, CanBeNull = false)]
		#else
		[PrimaryKey, AutoIncrement]
		#endif
        public int Id { get; set; }

        [Column(Name="ServerKey")]
        public string ServerKey { get; set; }

        [Column(Name="MessageType")]
        public string MessageType { get; set; }

        [Column(Name="IsClientIncomming")]
        public bool IsClientIncomming { get; set; }

		#if WINDOWS_PHONE
		[Column(DbType = "NTEXT", UpdateCheck = UpdateCheck.Never)]
		#else
		[Column(Name = "Message")]
		#endif
        public string Message { get; set; }

        [Column(Name="Created")]
        public DateTime Created { get; set; }

		#if WINDOWS_PHONE
		[Column(CanBeNull = true)]
		#else
		[Column(Name = "Processed")]
		#endif
        public DateTime? Processed { get; set; }

    }
}
