using ScooterRentalServiceBusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScooterRentalServiceBusinessLogic.Concrete
{
    public class RentalCompany : IRentalCompany
    {
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
