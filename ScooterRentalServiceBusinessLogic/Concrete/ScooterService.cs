using ScooterRentalServiceBusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ScooterRentalServiceBusinessLogic.Concrete
{
    public class ScooterService : IScooterServiceExtended
    {
        protected Dictionary<string, ScooterExtended> scooters = new Dictionary<string, ScooterExtended>();

        private bool scooterExists(string id)
        {
            return scooters.Any(a => a.Key.ToUpper() == id.ToUpper());
        }
        public ScooterService() 
        { 

        }
        public ScooterService(List<ScooterExtended> scooters)
        {
            if (scooters == null)
                return;

            var distinctCount = scooters
                .Select(s => s.Id)
                .Distinct()
                .ToList()
                .Count;

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
            if (scooterExists(id))
                throw new ScooterServiceScooterDuplicateIdException("Duplicate id add attempt");

            scooters.Add(id, new ScooterExtended(id, pricePerMinute));
        }

        public Scooter GetScooterById(string scooterId)
        {
            scooterId = scooterId.ToUpper();

            if (!scooterExists(scooterId))
                throw new ScooterServiceScooterNotFoundException("Non existing scooter id detected");

            return scooters.First(f => f.Key.ToUpper() == scooterId).Value;
        }

        public IList<Scooter> GetScooters()
        {
            return scooters?
                .Select(s => (Scooter)s.Value)
                .ToList();
        }

        public IList<ScooterExtended> GetExtendedScooters()
        {
            return scooters?
                .Select(s => s.Value)
                .ToList();
        }

        public void RemoveScooter(string id)
        {
            if (!scooterExists(id))
                throw new ScooterServiceScooterNotFoundException("Non existing scooter id detected");

            //Since we are adding "as is" but checking uniqueness as case insensitive
            var scooterToDelete = scooters.First(f => f.Key.ToUpper() == id.ToUpper());

            if(scooterToDelete.Value.IsRented)
                throw new ScooterServiceScooterInRentException("Scooter is being used");

            //Also it is not clear regarding yearly profit income, do we count (keep) profit for deleted scooters?
            //In common sence we should, but if there is a real reason for deletion, like it was a ghost scooter that existed
            //and was rented only on paper etc?            
            scooters.Remove(scooterToDelete.Key);
        }
    }
}
