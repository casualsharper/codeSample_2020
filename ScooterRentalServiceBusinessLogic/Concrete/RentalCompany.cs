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
                throw new RentalCompanyException("Scooter is not rented");

            var cost = ProcessRentPeriod(scooter.RentPeriods);

            scooter.IsRented = false;

            return cost;
        }

        public void StartRent(string id)
        {
            var scooter = ScooterService.GetExtendedScooter(id);

            if (scooter.IsRented)
                throw new RentalCompanyException("Scooter is already in rent");

            var rentTime = scooter
                .RentPeriods
                .FirstOrDefault(f => f.RentEnded == null);

            if (rentTime != null)
                throw new RentalCompanyException("Corrupted scooter rent periods. Period is not closed");

            scooter.RentPeriods.Add(new RentPeriod { RentStarted = DateTime.Now });

            scooter.IsRented = true;
        }

        private decimal ProcessRentPeriod(List<RentPeriod> rentPeriods)
        {
            var rentTime = rentPeriods
            .FirstOrDefault(f => f.RentEnded == null);

            if (rentTime == null)
                throw new RentalCompanyException("Corrupted scooter rent periods. Period is not open");

            return 0m;
        }
    }
}
