using ScooterRentalServiceBusinessLogic.Concrete;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScooterRentalServiceBusinessLogic.Interfaces
{
    public interface IScooterServiceExtended : IScooterService
    {
        IList<ScooterExtended> GetExtendedScooters();
        ScooterExtended GetExtendedScooter(string scooterId);
    }
}
