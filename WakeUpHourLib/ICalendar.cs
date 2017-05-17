using System;
using System.Collections.Generic;

namespace WakeUpHourLib
{
    public interface ICalendar
    {
        IEnumerable<DayEvents> GetWakeUpHourOfDay(DateTime day);
    }
}
