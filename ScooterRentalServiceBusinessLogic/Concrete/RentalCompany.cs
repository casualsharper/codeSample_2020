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

            var cost = ProcessRentPeriod(scooter);

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
        private int GetDatesTotalMinutes(DateTime dte, DateTime dts)
        {
            return (int)Math.Round((dte - dts).TotalMinutes, MidpointRounding.AwayFromZero);
        }
        private decimal ProcessRentPeriod(ScooterExtended scooter)
        {
            var cost = 0m;

            var rentPeriods = scooter.RentPeriods;

            var rentTime = rentPeriods
            .FirstOrDefault(f => f.RentEnded == null);

            if (rentTime == null)
                throw new RentalCompanyException("Corrupted scooter rent periods. Period is not open");
            
            var endDate = DateTime.UtcNow;

            var dayDiff = (int)Math.Round((endDate.Date - rentTime.RentStarted.Date).TotalDays, MidpointRounding.AwayFromZero);

            if(dayDiff == 0)
            {
                rentTime.RentEnded = endDate;
                cost = GetDatesTotalMinutes(rentTime.RentEnded.Value, rentTime.RentStarted) * scooter.PricePerMinute;

                cost = cost > dailyLimit ? dailyLimit : cost;

                return cost;
            }

            rentTime.RentEnded = rentTime.RentStarted.Date.AddDays(1).AddSeconds(-1);

            for (int i = 1; i < dayDiff; i++)
            {
                var dtStarted = rentTime.RentStarted.AddDays(i).Date;
                var dtEnded = dtStarted.AddDays(1).AddSeconds(-1);

                rentPeriods.Add(new RentPeriod { 
                    RentStarted = dtStarted,
                    RentEnded = dtEnded
                });
            }

            var closingPeriod = new RentPeriod
            {
                RentStarted = endDate.Date,
                RentEnded = endDate
            };

            rentPeriods.Add(closingPeriod);

            var fullDays = dayDiff - 1;

            var fullDayCost = 24 * 60 * fullDays * scooter.PricePerMinute;
            var startDayCost = GetDatesTotalMinutes(rentTime.RentEnded.Value, rentTime.RentStarted) * scooter.PricePerMinute;
            var endDayCost = GetDatesTotalMinutes(closingPeriod.RentEnded.Value, closingPeriod.RentStarted) * scooter.PricePerMinute;

            cost = (24 * 60 * scooter.PricePerMinute > dailyLimit ? dailyLimit * fullDays : fullDayCost) 
                + (startDayCost > dailyLimit ? dailyLimit : startDayCost)
                + (endDayCost > dailyLimit ? dailyLimit : endDayCost);

            return cost;
        }
    }
}
