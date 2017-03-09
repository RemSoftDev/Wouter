using FoodJournal.FoodJournalService;
using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading.Tasks;

namespace FoodJournal.AppModel
{
    static class Services
    {

        private static BasicHttpBinding CreateBasicHttp()
        {
            BasicHttpBinding binding = new BasicHttpBinding
            {
                Name = "basicHttpBinding",
                MaxBufferSize = 2147483647,
                MaxReceivedMessageSize = 2147483647
            };
            TimeSpan timeout = new TimeSpan(0, 0, 30);
            binding.SendTimeout = timeout;
            binding.OpenTimeout = timeout;
            binding.ReceiveTimeout = timeout;
            return binding;
        }

        public static FoodJournalServiceClient NewServiceClient()
        {
#if DEBUG
            return new FoodJournalServiceClient(CreateBasicHttp(), new EndpointAddress("http://dailylogservice.cloudapp.net/FoodJournalService/FoodJournalService.svc"));

            //return new FoodJournalServiceClient("BasicHttpBinding_IFoodJournalService", "http://192.168.1.4:81/FoodJournalService/FoodJournalService.svc");
            //return new FoodJournalServiceClient("BasicHttpBinding_IFoodJournalService", "http://192.168.1.4/FoodJournalService/FoodJournalService.svc");
            ////return new DailyLogService.DailyJournalServiceClient("BasicHttpBinding_IDailyJournalService", "http://127.0.0.1:81/DailyJournalService.svc");
#else
            return new FoodJournalServiceClient(CreateBasicHttp(), new EndpointAddress("http://dailylogservice.cloudapp.net/FoodJournalService/FoodJournalService.svc"));
#endif

        }


    }
}
