using System;
using System.Collections.Generic;
using System.Text;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;

namespace WakeUpHourLib
{
    public class GoogleCalendar : ICalendar
    {
        public string GoogleMail { get; set; }

        public GoogleCalendar(string googleMail)
        {
            GoogleMail = googleMail;
        }

        public void Connection(string apiKey)
        {
            Calendar c = new Calendar();
            var service = new CalendarService(new Google.Apis.Services.BaseClientService.Initializer
            {
                ApiKey = apiKey,
                ApplicationName = "WakeUpHour",
            });

            Console.WriteLine("Executing a list request...");
            var result = service.CalendarList.List().Execute();

            // Display the results.
            if (result.Items != null)
            {
                foreach (var api in result.Items)
                {
                    Console.WriteLine(api.Id + " - " + api.Description);
                }
            }
        }

        public DateTime GetWakeUpHourOfDay(DateTime day)
        {
            throw new Exception();
        }
    }
}
