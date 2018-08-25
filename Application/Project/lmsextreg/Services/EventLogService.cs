using System;
using System.Text;
using lmsextreg.Data;
using lmsextreg.Models;
using lmsextreg.Repositories;

namespace lmsextreg.Services
{
    public class EventLogService : IEventLogService
    {
        private readonly IEventLogRepository _eventLogRepository;
        private readonly IProgramEnrollmentRepository _programEnrollmentRepository;

        public EventLogService
        (
            IEventLogRepository eventLogRepo,
            IProgramEnrollmentRepository programEnrollmentRepo
        )
        {
            _eventLogRepository             = eventLogRepo;
            _programEnrollmentRepository    = programEnrollmentRepo;
        }

        public void LogEvent(string eventTypeCode, ApplicationUser appUser)
        {
            var eventLog = new EventLog
            {
                EventTypeCode   = eventTypeCode,
                UserCreatedID   = appUser.Id,
                UserCreatedName = appUser.UserName,
                DataValues = "User=" + appUser.ToString(),
                DateTimeCreated = DateTime.Now
            };

            _eventLogRepository.Add(eventLog);
        }

        public void LogEvent(string eventTypeCode, ApplicationUser appUser, ProgramEnrollment enrollment)
        {
            Console.WriteLine("[EventLogService][LogEvent] - (ApplicationUser.ToString()):\n" + appUser);
            Console.WriteLine("");
            Console.WriteLine("[EventLogService][LogEvent] - (ApplicationUser.ToEventLog()):\n" + appUser);

            var sb = new StringBuilder();
            sb.Append("User=");
            sb.Append(appUser.ToString());
            sb.Append("; ");
            sb.Append(enrollment.ToString());

            var eventLog = new EventLog
            {
                EventTypeCode   = eventTypeCode,
                UserCreatedID   = appUser.Id,
                UserCreatedName = appUser.UserName,
                DataValues = sb.ToString(),
                DateTimeCreated = DateTime.Now
            };

            _eventLogRepository.Add(eventLog);
        } 

        public void LogEvent(string eventTypeCode, ApplicationUser appUser, int programEnrollmentID)
        {
            Console.WriteLine("");
            Console.WriteLine("[EventLogService][LogEvent] - (ApplicationUser.ToString()):\n"
                                + appUser);
            Console.WriteLine("");
            Console.WriteLine("[EventLogService][LogEvent] - (ApplicationUser.ToEventLog()):\n" 
                                + appUser.ToEventLog());
            Console.WriteLine("");
            
            var programEnrollment = _programEnrollmentRepository.Retrieve(programEnrollmentID);

            Console.WriteLine("[EventLogService][LogEvent] - (ApplicationUser.ToString()):\n" 
                                + programEnrollment);
            Console.WriteLine("");
            Console.WriteLine("[EventLogService][LogEvent] - (ApplicationUser.ToEventLog()):\n" 
                                + programEnrollment.ToEventLog());
            Console.WriteLine("");

            var sb = new StringBuilder();
            sb.Append("User=");
            sb.Append(appUser);
            sb.Append("; ");
            sb.Append(programEnrollment.ToEventLog());

            var eventLog = new EventLog
            {
                EventTypeCode   = eventTypeCode,
                UserCreatedID   = appUser.Id,
                UserCreatedName = appUser.UserName,
                DataValues = sb.ToString(),
                DateTimeCreated = DateTime.Now
            };

            _eventLogRepository.Add(eventLog);
        }                
    }
}