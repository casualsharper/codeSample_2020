using System;
using System.Collections.Generic;
using System.Text;

namespace ScooterRentalServiceBusinessLogic.Concrete
{
    public class RentalCompanyException : Exception
    {
        public RentalCompanyException()
        {
        }

        public RentalCompanyException(string message)
            : base(message)
        {
        }

        public RentalCompanyException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
