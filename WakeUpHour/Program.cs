using System;
using WakeUpHourLib;

namespace WakeUpHour
{
    class Program
    {
        static void Main(string[] args)
        {
            GoogleCalendar cal = new GoogleCalendar();
            CalendarBLL bll = new CalendarBLL
            {
                Calendar = cal
            };
            cal.Connection();

            while (true)
            {
                Console.WriteLine("Date ?");
                string date = Console.ReadLine();
                DateTime dateJour;
                if (DateTime.TryParse(date, out dateJour))
                {
                    var info = bll.GetInformation(dateJour);
                    foreach( var timeSpan in info.HoursPerCalendar )
                    {
                        Console.WriteLine(timeSpan.Key + " : " + timeSpan.Value);
                    }
                }
            }
        }
    }
}