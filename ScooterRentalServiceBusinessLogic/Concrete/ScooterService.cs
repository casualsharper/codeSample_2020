using ScooterRentalServiceBusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ScooterRentalServiceBusinessLogic.Concrete
{
    public class ScooterService : IScooterService
    {
        private Dictionary<string, ScooterExtended> scooters = new Dictionary<string, ScooterExtended>();

        public ScooterService() 
        { 

        }
        public ScooterService(List<ScooterExtended> scooters)
        {
            if (scooters == null)
                return;

            var distinctCount = scooters.Select(s => s.Id).Distinct().ToList().Count;

            if (distinctCount != scooters.Count)
                throw new ScooterServiceException("Duplicate scooter id detected!");

            this.scooters = scooters.ToDictionary(key => key.Id, val => val);
        }
        public ScooterService(Dictionary<string, ScooterExtended> scooters)
        {
            if (scooters == null)
                return;

            this.scooters = scooters;
        }
        public void AddScooter(string id, decimal pricePerMinute)
        {
            if(scooters.Any(a => a.Key.ToUpper() == id.ToUpper()))
                throw new ScooterServiceScooterDuplicateIdException("Duplicate id add attempt");

            scooters.Add(id, new ScooterExtended(id, pricePerMinute));
        }

        public Scooter GetScooterById(string scooterId)
        {
            throw new ScooterServiceScooterNotFoundException("Non existing scooter id detected");
        }

        public IList<Scooter> GetScooters()
        {
            return scooters?
                .Select(s => (Scooter)s.Value)
                .ToList();
        }

        public void RemoveScooter(string id)
        {
            throw new ScooterServiceScooterNotFoundException("Non existing scooter id detected");
        }
    }
}
