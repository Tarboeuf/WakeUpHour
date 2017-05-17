using System;
using System.Linq;

namespace WakeUpHourLib
{
    public class CalendarBLL
    {
        public ICalendar Calendar { get; set; }

        public DayInformation GetInformation(DateTime day)
        {
            var events = Calendar.GetWakeUpHourOfDay(day).ToList();
            bool isMatin = false, isSoir = false, isJour = false, isConge = false;

            DayInformation information = new DayInformation();
            if (events.Count > 0)
            {
                foreach (var eventItem in events)
                {

                    if (eventItem.Name.Equals("M1", StringComparison.CurrentCultureIgnoreCase))
                        isMatin = true;
                    if (eventItem.Name.Equals("S1", StringComparison.CurrentCultureIgnoreCase))
                        isSoir = true;
                    if (eventItem.Name.Equals("M3", StringComparison.CurrentCultureIgnoreCase))
                        isJour = true;
                    if (eventItem.Name.Equals("M4", StringComparison.CurrentCultureIgnoreCase))
                        isJour = true;
                    if (eventItem.Name.Equals("RTT", StringComparison.CurrentCultureIgnoreCase))
                        isConge = true;
                    if (eventItem.Name.Equals("CA", StringComparison.CurrentCultureIgnoreCase))
                        isConge = true;

                }
            }

            if (day.DayOfWeek == DayOfWeek.Saturday || day.DayOfWeek == DayOfWeek.Sunday)
            {
                information.HoursPerCalendar.Add("pierre.epinat@gmail.com", new TimeSpan(10));
            }
            else
            {
                var ts = IsWorkingDay(day) ? new TimeSpan(7, 25, 0) : new TimeSpan(10);
                information.HoursPerCalendar.Add("pierre.epinat@gmail.com", ts);
            }
            if (isConge || isSoir)
            {
                information.HoursPerCalendar.Add("mathilde.pigot@gmail.com", new TimeSpan(10));
            }
            else if (isJour)
            {
                information.HoursPerCalendar.Add("mathilde.pigot@gmail.com", new TimeSpan(6, 55, 0));
            }
            else if (isMatin)
            {
                information.HoursPerCalendar.Add("mathilde.pigot@gmail.com", new TimeSpan(4, 45, 0));
            }
            else
            {
                information.HoursPerCalendar.Add("mathilde.pigot@gmail.com", new TimeSpan(10));
            }
            return information;
        }

        private bool IsWorkingDay(DateTime dtDate)
        {
            bool bolWorkingDay = true;
            Array arrDateFerie = new DateTime[8];
            // 01 Janvier
            arrDateFerie.SetValue(new DateTime(dtDate.Year, 1, 1), 0);
            // 01 Mai
            arrDateFerie.SetValue(new DateTime(dtDate.Year, 5, 1), 1);
            // 08 Mai
            arrDateFerie.SetValue(new DateTime(dtDate.Year, 5, 8), 2);
            // 14 Juillet
            arrDateFerie.SetValue(new DateTime(dtDate.Year, 7, 14), 3);
            // 15 Aout
            arrDateFerie.SetValue(new DateTime(dtDate.Year, 8, 15), 4);
            // 01 Novembre
            arrDateFerie.SetValue(new DateTime(dtDate.Year, 11, 1), 5);
            // 11 Novembre
            arrDateFerie.SetValue(new DateTime(dtDate.Year, 11, 11), 6);
            // Noël
            arrDateFerie.SetValue(new DateTime(dtDate.Year, 12, 25), 7);

            // Dimanche ou jour férié
            bolWorkingDay = !((dtDate.DayOfWeek == DayOfWeek.Sunday) || (Array.BinarySearch(arrDateFerie, dtDate) >= 0));
            if (bolWorkingDay)
            {
                // Calcul du jour de pâques (algorithme de Oudin (1940))
                //Calcul du nombre d'or - 1
                int intGoldNumber = (int)(dtDate.Year % 19);
                // Année divisé par cent
                int intAnneeDiv100 = (int)(dtDate.Year / 100);
                // intEpacte est = 23 - Epacte (modulo 30)
                int intEpacte = (int)((intAnneeDiv100 - intAnneeDiv100 / 4 - (8 * intAnneeDiv100 + 13) / 25 + (
                                           19 * intGoldNumber) + 15) % 30);
                //Le nombre de jours à partir du 21 mars pour atteindre la pleine lune Pascale
                int intDaysEquinoxeToMoonFull = (int)(intEpacte - (intEpacte / 28) * (1 - (intEpacte / 28) * (29 / (intEpacte + 1)) * ((21 - intGoldNumber) / 11)));
                //Jour de la semaine pour la pleine lune Pascale (0=dimanche)
                int intWeekDayMoonFull = (int)((dtDate.Year + dtDate.Year / 4 + intDaysEquinoxeToMoonFull +
                                                2 - intAnneeDiv100 + intAnneeDiv100 / 4) % 7);
                // Nombre de jours du 21 mars jusqu'au dimanche de ou 
                // avant la pleine lune Pascale (un nombre entre -6 et 28)
                int intDaysEquinoxeBeforeFullMoon = intDaysEquinoxeToMoonFull - intWeekDayMoonFull;
                // mois de pâques
                int intMonthPaques = (int)(3 + (intDaysEquinoxeBeforeFullMoon + 40) / 44);
                // jour de pâques
                int intDayPaques = (int)(intDaysEquinoxeBeforeFullMoon + 28 - 31 * (intMonthPaques / 4));
                // lundi de pâques
                DateTime dtMondayPaques = new DateTime(dtDate.Year, intMonthPaques, intDayPaques + 1);
                // Ascension
                DateTime dtAscension = dtMondayPaques.AddDays(38);
                //Pentecote
                DateTime dtMondayPentecote = dtMondayPaques.AddDays(49);
                bolWorkingDay = !((DateTime.Compare(dtMondayPaques, dtDate) == 0) || (DateTime.Compare(dtAscension, dtDate) == 0)
                                  || (DateTime.Compare(dtMondayPentecote, dtDate) == 0));
            }
            return bolWorkingDay;
        }
    }
}