using System;
using System.Collections.Generic;
using System.Text;

namespace ScooterRentalServiceBusinessLogic
{
    public interface IDateTimeHelper
    {
        DateTime GetUtcDateTimeNow();
    }

    public class DateTimeHelper : IDateTimeHelper
    {
        public DateTime GetUtcDateTimeNow()
        {
            return DateTime.UtcNow;
        }
    }
}
