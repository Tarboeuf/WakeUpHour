using System;
using System.Collections.Generic;

namespace WakeUpHourLib {
    public class DayInformation
    {
        public Dictionary<string, TimeSpan> HoursPerCalendar { get; } = new Dictionary<string, TimeSpan>();

        public TimeSpan SunriseHour { get; set; }
    }
}