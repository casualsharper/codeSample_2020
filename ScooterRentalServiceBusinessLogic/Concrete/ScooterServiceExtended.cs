using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using ScooterRentalServiceBusinessLogic.Interfaces;

namespace ScooterRentalServiceBusinessLogic.Concrete
{
    public class ScooterServiceExtended : ScooterService, IScooterServiceExtended
    {
        public ScooterServiceExtended() : base()
        {

        }
        public ScooterServiceExtended(List<ScooterExtended> scooters) : base(scooters)
        {

        }
        public ScooterServiceExtended(Dictionary<string, ScooterExtended> scooters) : base(scooters)
        {

        }
        public IList<ScooterExtended> GetExtendedScooters()
        {
            return scooters?
                .Select(s => s.Value)
                .ToList();
        }
    }
}
