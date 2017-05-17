using System;
using System.Collections.Generic;
using System.Text;

namespace WakeUpHourLib
{
    public class GoogleCalendar : ICalendar
    {
        public string GoogleMail { get; set; }

        public GoogleCalendar(string googleMail)
        {
            GoogleMail = googleMail;
        }

        public DateTime GetWakeUpHourOfDay(DateTime day)
        {
        }
    }
}
