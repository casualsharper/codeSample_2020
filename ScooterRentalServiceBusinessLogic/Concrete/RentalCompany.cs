using ScooterRentalServiceBusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScooterRentalServiceBusinessLogic.Concrete
{
    public class RentalCompany : IRentalCompany
    {
        private IScooterServiceExtended scooterServiceExtended = new ScooterService();
        //In real world application this value should not be hardcoded, not to mention that this value will change periodically
        //So in real world application it would have been stored on the same data/context layer as rented period
        private readonly decimal dailyLimit = 20m;
        public string Name { get; }

        public RentalCompany(string name)
        {
            Name = name;
        }

        public decimal CalculateIncome(int? year, bool includeNotCompletedRentals)
        {
            throw new NotImplementedException();
        }

        public decimal EndRent(string id)
        {
            throw new NotImplementedException();
        }

        public void StartRent(string id)
        {
            throw new NotImplementedException();
        }
    }
}
