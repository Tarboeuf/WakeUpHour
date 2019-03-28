using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Calendar.v3;
using Google.Apis.Calendar.v3.Data;
using Google.Apis.Services;
using Google.Apis.Util.Store;

namespace WakeUpHourLib
{
    public class GoogleCalendar : ICalendar
    {
        static readonly string[] Scopes = {CalendarService.Scope.Calendar};

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

            using( var stream =
                new FileStream(@"client_secret.json", FileMode.Open, FileAccess.Read) )
            {
                string credPath = @".credentials/calendar-dotnet-quickstart.json";

                credential = GoogleWebAuthorizationBroker.AuthorizeAsync(
                    GoogleClientSecrets.Load(stream).Secrets,
                    Scopes,
                    "user",
                    CancellationToken.None,
                    new FileDataStore(credPath, true)).Result;
            }

            // Create Google Calendar API service.
            _service = new CalendarService(new BaseClientService.Initializer
            {
                HttpClientInitializer = credential,
                ApplicationName = "WakeUpHour"
            });

            var calRequest = _service.CalendarList.List();
            var cals = calRequest.Execute();

            foreach( var cal in cals.Items )
            {
                if( IdCalendar.Contains(cal.Summary) )
                {
                    _calendarEntries.Add(cal.Summary, cal);
                }
            }
        }

        public IEnumerable<DayEvents> GetWakeUpHourOfDay(DateTime day)
        {
            foreach( var calId in IdCalendar )
            {
                // Define parameters of request.
                if( !_calendarEntries.ContainsKey(calId) )
                {
                    continue;
                }
                EventsResource.ListRequest request = _service.Events.List(_calendarEntries[calId].Id);
                request.TimeMin = day.Date;
                request.TimeMax = day.AddHours(24);
                request.ShowDeleted = false;
                request.SingleEvents = false;
                request.MaxResults = 10;

                // List events.
                Events events = request.Execute();

                if( events.Items != null && events.Items.Count > 0 )
                {
                    foreach( var eventItem in events.Items )
                    {
                        yield return new DayEvents
                        {
                            IdCalendar = calId,
                            Name = eventItem.Summary,
                            StartHour = eventItem.Start?.DateTime,
                            IsDayEvent = eventItem.Start == null
                        };
                    }
                }
            }
        }

        public IEnumerable<DayEvents> GetEvents(DateTime start, DateTime end)
        {
            foreach( var calId in IdCalendar )
            {
                // Define parameters of request.
                EventsResource.ListRequest request = _service.Events.List(_calendarEntries[calId].Id);
                request.TimeMin = start;
                request.TimeMax = end;
                request.ShowDeleted = false;
                request.SingleEvents = false;
                request.MaxResults = 10;

                // List events.
                Events events = request.Execute();

                if( events.Items != null && events.Items.Count > 0 )
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

        public void SetEvents(IEnumerable<DayEvents> events, string calendarId)
        {
            foreach( var evt in events )
            {
                var ret = _service.Events.Insert(new Event
                {
                    Summary = evt.Name,
                    Start = new EventDateTime
                    {
                        DateTime = evt.StartHour.Value.Date
                    },
                    End = new EventDateTime
                    {
                        DateTime = evt.StartHour.Value.Date.AddDays(1)
                    },
                    Recurrence = new List<string>()
            }, _calendarEntries.Values.Single().Id).Execute();

                Console.WriteLine(ret.Start.DateTime.ToString());
            }
        }

        public string AddCalenderEvents(string refreshToken, 
            string emailAddress, string summary, DateTime? start, DateTime? end, out string error)
        {
            string eventId = string.Empty;
            error = string.Empty;

            try
            {
                var calendarService = _service;

                if (calendarService != null)
                {
                    var list = calendarService.CalendarList.List().Execute();
                    var calendar = list.Items.SingleOrDefault(c => c.Summary == emailAddress);
                    if (calendar != null)
                    {
                        Event calenderEvent = new Event
                        {
                            Summary = summary,
                            Start = new EventDateTime
                            {
                                //DateTime = new DateTime(2018, 1, 20, 19, 00, 0)
                                DateTime = start //,
                                //TimeZone = "Europe/Istanbul"
                            },
                            End = new EventDateTime
                            {
                                //DateTime = new DateTime(2018, 4, 30, 23, 59, 0)
                                DateTime = start.Value.AddHours(12) //,
                                //TimeZone = "Europe/Istanbul"
                            },
                            Recurrence = new List<string>(),
                            Reminders = new Event.RemindersData
                            {
                                UseDefault = false,
                                Overrides = new[]
                                {
                                    new EventReminder
                                    {
                                        Method = "email",
                                        Minutes = 24 * 60
                                    },
                                    new EventReminder
                                    {
                                        Method = "popup",
                                        Minutes = 24 * 60
                                    }
                                }
                            },
                            Attendees = new[]
                            {
                                new EventAttendee
                                {
                                    Email = "kaptan.cse@gmail.com"
                                },
                                new EventAttendee
                                {
                                    Email = emailAddress
                                }
                            }
                        };

                        //calenderEvent.Description = summary;
                        //calenderEvent.Location = summary;

                        //Set Remainder

                        #region Attendees

                        //Set Attendees

                        #endregion

                        var newEventRequest = calendarService.Events.Insert(calenderEvent, calendar.Id);
                        newEventRequest.SendNotifications = true;
                        var eventResult = newEventRequest.Execute();
                        eventId = eventResult.Id;
                    }
                }
            }
            catch (Exception ex)
            {
                eventId = string.Empty;
                error = ex.Message;
            }
            return eventId;
        }
    }
}