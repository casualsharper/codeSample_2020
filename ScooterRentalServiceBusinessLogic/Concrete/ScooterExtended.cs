using System;
using System.Collections.Generic;
using System.Text;

namespace ScooterRentalServiceBusinessLogic.Concrete
{
    public class ScooterExtended : Scooter
    {
        public List<RentPeriod> RentPeriods { get; } = new List<RentPeriod>();

        public ScooterExtended(string id, decimal pricePerMinute) : base(id, pricePerMinute)
        {
        }
    }
    public class RentPeriod
    {
        public DateTime RentStarted { get; set; }
        public DateTime? RentEnded { get; set; }
    }
}