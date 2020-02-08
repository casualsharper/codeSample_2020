using ScooterRentalServiceBusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ScooterRentalServiceBusinessLogic.Concrete
{
    public class RentalCompany : IRentalCompany
    {
        public IScooterServiceExtended ScooterService { get; } = new ScooterService();
        //In real world application this value should not be hardcoded, not to mention that this value will change periodically
        //So in real world application it would have been stored on the same data/context layer as rented period
        private readonly decimal dailyLimit = 20m;
        public string Name { get; }

        public RentalCompany(string name)
        {
            Name = name;
        }

        public RentalCompany(string name, IScooterServiceExtended scooterService)
        {
            Name = name;

            if (scooterService == null)
                return;

            ScooterService = scooterService;
        }

        public decimal CalculateIncome(int? year, bool includeNotCompletedRentals)
        {
            throw new NotImplementedException();
        }

        public decimal EndRent(string id)
        {
            var scooter = ScooterService.GetExtendedScooter(id);

            if (!scooter.IsRented)
                throw new RentalCompanyException();

            var rentTime = scooter.RentPeriods.First(f => f.RentEnded == null);

            rentTime.RentEnded = DateTime.UtcNow;

            scooter.IsRented = false;

            return 0m;
        }

        public void StartRent(string id)
        {
            throw new NotImplementedException();
        }
    }
}
