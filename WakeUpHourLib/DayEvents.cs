using System;

namespace WakeUpHourLib {
    public class DayEvents
    {
        public string IdCalendar { get; set; }

        public string Name { get; set; }

        public DateTime? StartHour { get; set; }

        public bool IsDayEvent { get; set; }
    }
}