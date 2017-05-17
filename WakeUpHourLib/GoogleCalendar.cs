using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util;
using Google.Apis.Util.Store;

namespace WakeUpHourLib
{
    public class GoogleCalendar : ICalendar
    {
        static readonly string[] Scopes = { CalendarService.Scope.CalendarReadonly };

        readonly Dictionary<string, CalendarListEntry> _calendarEntries = new Dictionary<string, CalendarListEntry>();

        private CalendarService _service;

        public string[] IdCalendar { get; set; }

        public GoogleCalendar(params string[] idCalendar)
        {
            IdCalendar = idCalendar;
        }

        public void Connection()
        {
            UserCredential credential;

            using (var stream =
                new FileStream(@"D:\Sources\WakeUpHour\client_secret.json", FileMode.Open, FileAccess.Read))
            {
                string credPath = @"D:\Sources\WakeUpHour\.credentials/calendar-dotnet-quickstart.json";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Calendar API service.
            _service = new CalendarService(new BaseClientService.Initializer()
            {
                HttpClientInitializer = credential,
                ApplicationName = "WakeUpHour",
            });

            var calRequest = _service.CalendarList.List();
            var cals = calRequest.Execute();

            foreach (var cal in cals.Items)
            {
                if( IdCalendar.Contains(cal.Summary) )
                {
                    _calendarEntries.Add(cal.Id, cal);
                }
            }
        }

        public IEnumerable<DayEvents> GetWakeUpHourOfDay(DateTime day)
        {
            foreach( var calId in IdCalendar )
            {
                // Define parameters of request.
                EventsResource.ListRequest request = _service.Events.List(_calendarEntries[calId].Id);
                request.TimeMin = day.Date;
                request.TimeMax = day.AddHours(24);
                request.ShowDeleted = false;
                request.SingleEvents = false;
                request.MaxResults = 10;

                // List events.
                Events events = request.Execute();

                if (events.Items != null && events.Items.Count > 0)
                {
                    foreach( var eventItem in events.Items )
                    {
                        yield return new DayEvents
                        {
                            IdCalendar = calId, 
                            Name = eventItem.Summary,
                            StartHour = eventItem.OriginalStartTime?.DateTime,
                            IsDayEvent = eventItem.OriginalStartTime == null
                        };
                    }
                }
            }
        }
    }
}
