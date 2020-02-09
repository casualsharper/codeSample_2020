using ScooterRentalServiceBusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;

namespace ScooterRentalServiceBusinessLogic.Concrete
{
    public class RentalCompany : IRentalCompany
    {
        private IDateTimeHelper dateTimeHelper { get; } = new DateTimeHelper();
        public IScooterServiceExtended ScooterService { get; } = new ScooterService();
        //In real world application this value should not be hardcoded, not to mention that this value will change periodically
        //So in real world application it would have been stored on the same data/context layer as rented period
        public decimal DailyLimit { get; } = 20m;
        public string Name { get; }

        public RentalCompany(string name)
        {
            Name = name;
        }
        public RentalCompany(string name, IDateTimeHelper dateTimeHelper)
        {
            Name = name;
            this.dateTimeHelper = dateTimeHelper;
        }
        public RentalCompany(string name, IScooterServiceExtended scooterService)
        {
            Name = name;

            if (scooterService == null)
                return;

            ScooterService = scooterService;
        }
        public RentalCompany(string name, IScooterServiceExtended scooterService, IDateTimeHelper dateTimeHelper)
        {
            Name = name;

            if (scooterService == null)
                return;

            ScooterService = scooterService;
            this.dateTimeHelper = dateTimeHelper;
        }

        public decimal CalculateIncome(int? year, bool includeNotCompletedRentals)
        {
            var scooters = ScooterService.GetExtendedScooters();

            var totalIncome = 0m;

            foreach (var scooter in scooters)
            {
                totalIncome += GetScooterIncome(scooter, year, includeNotCompletedRentals);
            }

            return totalIncome;
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

            scooter.RentPeriods.Add(new RentPeriod { RentStarted = dateTimeHelper.GetUtcDateTimeNow() });

            scooter.IsRented = true;
        }
        private decimal CalculatePeriodCost(DateTime dte, DateTime dts)
        {
            return DailyLimitApplier(GetDatesTotalMinutes(dte, dts));
        }
        private decimal DailyLimitApplier(decimal calculatedCost)
        {
            return calculatedCost > DailyLimit ? DailyLimit : calculatedCost;
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

            var endDate = dateTimeHelper.GetUtcDateTimeNow();

            var dayDiff = (int)Math.Round((endDate.Date - rentTime.RentStarted.Date).TotalDays, MidpointRounding.AwayFromZero);

            if (dayDiff < 0)
                throw new RentalCompanyException("Corrupted scooter rent periods. End datetime is less than start datetime");

            if (dayDiff == 0)
            {
                rentTime.RentEnded = endDate;
                cost = GetDatesTotalMinutes(rentTime.RentEnded.Value, rentTime.RentStarted) * scooter.PricePerMinute;

                cost = cost > DailyLimit ? DailyLimit : cost;

                return cost;
            }

            var closingPeriod = MoreThanOneDayPeriodProcess(rentPeriods, rentTime);

            cost = GetCost(scooter, rentTime, dayDiff, closingPeriod);

            return cost;
        }

        private RentPeriod MoreThanOneDayPeriodProcess(List<RentPeriod> rentPeriods, RentPeriod rentTime)
        {
            var endDate = dateTimeHelper.GetUtcDateTimeNow();
            rentTime.RentEnded = rentTime.RentEnded.HasValue ? rentTime.RentEnded : endDate;

            var dayDiff = (int)Math.Round((endDate.Date - rentTime.RentStarted.Date).TotalDays, MidpointRounding.AwayFromZero);

            rentTime.RentEnded = rentTime.RentStarted.Date.AddDays(1).AddSeconds(-1);

            for (int i = 1; i < dayDiff; i++)
            {
                var dtStarted = rentTime.RentStarted.AddDays(i).Date;
                var dtEnded = dtStarted.AddDays(1).AddSeconds(-1);

                rentPeriods.Add(new RentPeriod
                {
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
            return closingPeriod;
        }

        private decimal GetCost(ScooterExtended scooter, RentPeriod rentTime, int dayDiff, RentPeriod closingPeriod)
        {
            decimal cost;
            var fullDays = dayDiff - 1;

            var fullDayCost = 24 * 60 * fullDays * scooter.PricePerMinute;
            var startDayCost = GetDatesTotalMinutes(rentTime.RentEnded.Value, rentTime.RentStarted) * scooter.PricePerMinute;
            var endDayCost = GetDatesTotalMinutes(closingPeriod.RentEnded.Value, closingPeriod.RentStarted) * scooter.PricePerMinute;

            cost = (24 * 60 * scooter.PricePerMinute > DailyLimit ? DailyLimit * fullDays : fullDayCost)
                + (startDayCost > DailyLimit ? DailyLimit : startDayCost)
                + (endDayCost > DailyLimit ? DailyLimit : endDayCost);
            return cost;
        }

        private decimal GetScooterIncome(ScooterExtended scooter, int? year, bool includeNotCompletedRentals)
        {
            //It was not clear for me whether to completely remove scooter from income or only open renting period
            //I assumed that logically only open transaction should be excluded instead of complete scooter
            var result = 0m;

            //we do not want to modify original collection on each report
            var rentPeriods = new List<RentPeriod>();

            foreach (var period in scooter.RentPeriods)
            {
                if (!includeNotCompletedRentals && !period.RentEnded.HasValue)
                    continue;

                if (year.HasValue && year != period.RentStarted.Year)
                    continue;

                if(!period.RentEnded.HasValue)
                {
                    var dtNow = dateTimeHelper.GetUtcDateTimeNow();
                    var periodClone = new RentPeriod { RentStarted = period.RentStarted , RentEnded = dtNow };

                    if(dtNow.Date != period.RentStarted.Date)
                    {
                        MoreThanOneDayPeriodProcess(rentPeriods, periodClone);
                    }
                    else
                    {
                        rentPeriods.Add(periodClone);
                    }
                }
                else
                {
                    rentPeriods.Add(period);
                }
            }

            result += rentPeriods.Sum(s =>
            CalculatePeriodCost(s.RentEnded.Value, s.RentStarted) * scooter.PricePerMinute
            );

            return result;
        }
    }
}
