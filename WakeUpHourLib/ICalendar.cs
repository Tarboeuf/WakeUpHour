using System;

namespace WakeUpHourLib
{
    public interface ICalendar
    {
        DateTime GetWakeUpHourOfDay(DateTime day);
    }
}
