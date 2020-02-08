using System;
using System.Collections.Generic;
using System.Text;

namespace ScooterRentalServiceBusinessLogic.Concrete
{
    public class ScooterServiceException : Exception
    {
        public ScooterServiceException(string message)
            : base(message)
        {
        }
    }
    public class ScooterServiceScooterNotFoundException : ScooterServiceException
    {
        public ScooterServiceScooterNotFoundException(string message)
            : base(message)
        {
        }
    }
    public class ScooterServiceScooterDuplicateIdException : ScooterServiceException
    {
        public ScooterServiceScooterDuplicateIdException(string message)
            : base(message)
        {
        }
    }
    public class ScooterServiceScooterInRentException : ScooterServiceException
    {
        public ScooterServiceScooterInRentException(string message)
            : base(message)
        {
        }
    }
}
