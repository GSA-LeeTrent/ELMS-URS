using lmsextreg.Data;
using lmsextreg.Models;
using lmsextreg.Constants;

namespace lmsextreg.Services
{
    public interface IEventLogService
    {
        void LogEvent(string eventTypeCode, ApplicationUser appUser);
        void LogEvent(string eventTypeCode, ApplicationUser appUser, ProgramEnrollment enrollment);
        void LogEvent(string eventTypeCode, ApplicationUser appUser, int programEnrollmentID);
    }
}