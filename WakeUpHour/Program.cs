using System;
using WakeUpHourLib;

namespace WakeUpHour
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            GoogleCalendar cal = new GoogleCalendar(null);
            cal.Connection("");
        }
    }
}