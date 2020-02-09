using ScooterRentalServiceBusinessLogic.Concrete;
using ScooterRentalServiceBusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;

namespace ScooterRentalServiceBusinessLogicTests
{
    public class RentalCompanyTests
    {
        private readonly decimal dailyLimit = 20m;
        private readonly string companyName = "TestCompany";
        private int GetDatesTotalMinutes(DateTime dte, DateTime dts)
        {
            return (int)Math.Round((dte - dts).TotalMinutes,MidpointRounding.AwayFromZero);
        }
        [Fact]
        public void CanThrowStartRent()
        {
            var scooterExtended = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice);
            var scooterExtendedCorrupted = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice, 
                new List<RentPeriod> { new RentPeriod { RentStarted = DateTime.Now, RentEnded = null } }
                );

            var testDataSet = new List<ScooterExtended> {
                scooterExtended,
                scooterExtendedCorrupted
            };

            var scooterService = new ScooterService(testDataSet);

            var testRentalCompany = new RentalCompany(companyName, scooterService);

            scooterExtended.IsRented = true;
            scooterExtendedCorrupted.IsRented = false;

            Assert.Throws<RentalCompanyException>(() => { testRentalCompany.StartRent(scooterExtended.Id); });
            Assert.Throws<RentalCompanyException>(() => { testRentalCompany.StartRent(scooterExtendedCorrupted.Id); });
        }
        [Fact]
        public void CanStartRent()
        {
            var scooterExtended = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice);

            var testDataSet = new List<ScooterExtended> {
                scooterExtended
            };

            var scooterService = new ScooterService(testDataSet);

            var testRentalCompany = new RentalCompany(companyName, scooterService);

            scooterExtended.IsRented = false;

            testRentalCompany.StartRent(scooterExtended.Id);

            Assert.True(scooterExtended.IsRented);
            Assert.True(scooterExtended.RentPeriods.Count == 1);
            Assert.True(scooterExtended.RentPeriods.First().RentEnded == null);
        }
        [Fact]
        public void CanThrowEndRent()
        {
            var scooterExtendedBoolCorrupted = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice,
                new List<RentPeriod> { new RentPeriod { RentStarted = DateTime.Now, RentEnded = null } });
            var scooterExtendedPeriodCorrupted = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice,
               new List<RentPeriod> { new RentPeriod { RentStarted = DateTime.Now, RentEnded = DateTime.Now } });

            var testDataSet = new List<ScooterExtended> {
                scooterExtendedBoolCorrupted,
                scooterExtendedPeriodCorrupted
            };

            var scooterService = new ScooterService(testDataSet);

            var testRentalCompany = new RentalCompany(companyName, scooterService);

            scooterExtendedBoolCorrupted.IsRented = false;
            scooterExtendedPeriodCorrupted.IsRented = true;

            Assert.Throws<RentalCompanyException>(() => { testRentalCompany.EndRent(scooterExtendedBoolCorrupted.Id); });
            Assert.Throws<RentalCompanyException>(() => { testRentalCompany.EndRent(scooterExtendedPeriodCorrupted.Id); });
        }
        [Fact]
        public void CanEndRentLessThanLimit()
        {
            var rentStartLessThanDailyLimit = DateTime.UtcNow
                .AddMinutes((int)Math.Round(-dailyLimit / TestDataFactory.DefaultPrice, MidpointRounding.AwayFromZero) + 1);

            var scooterExtendedLessThanDailyLimit = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice,
            new List<RentPeriod> {
                                new RentPeriod {
                                    RentStarted = rentStartLessThanDailyLimit,
                                    RentEnded = null}
            });

            var expectedCostLessThanDailyLimit = GetDatesTotalMinutes(DateTime.UtcNow, rentStartLessThanDailyLimit) * TestDataFactory.DefaultPrice;

            var testDataSet = new List<ScooterExtended> {
               scooterExtendedLessThanDailyLimit
            };

            scooterExtendedLessThanDailyLimit.IsRented = true;

            var scooterService = new ScooterService(testDataSet);
            var testRentalCompany = new RentalCompany(companyName, scooterService);

            var costLessThanDailyLimit = testRentalCompany.EndRent(scooterExtendedLessThanDailyLimit.Id);

            Assert.False(scooterExtendedLessThanDailyLimit.IsRented);
            Assert.True(costLessThanDailyLimit == expectedCostLessThanDailyLimit);
        }
        [Fact]
        public void CanEndRentMoreThanDay()
        {
            var daysDiff = 3;
            var rentStartMoreThanDay = DateTime.UtcNow
                .AddDays(-daysDiff);

            var scooterExtendedMoreThanDay = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice,
            new List<RentPeriod> {
                            new RentPeriod {
                                RentStarted = rentStartMoreThanDay,
                                RentEnded = null}
            });

            var startCost = GetDatesTotalMinutes(rentStartMoreThanDay.Date.AddDays(1).AddSeconds(-1), rentStartMoreThanDay) * scooterExtendedMoreThanDay.PricePerMinute;
            var endCost = GetDatesTotalMinutes(DateTime.UtcNow, DateTime.UtcNow.Date) * scooterExtendedMoreThanDay.PricePerMinute;

            var expectedCostMoreThanDay = (daysDiff - 1) * dailyLimit
                + (endCost > dailyLimit ? dailyLimit : endCost)
                + (startCost > dailyLimit ? dailyLimit : startCost);

            var testDataSet = new List<ScooterExtended> {
               scooterExtendedMoreThanDay
            };

            scooterExtendedMoreThanDay.IsRented = true;

            var scooterService = new ScooterService(testDataSet);
            var testRentalCompany = new RentalCompany(companyName, scooterService);

            var costMoreThanDay = testRentalCompany.EndRent(scooterExtendedMoreThanDay.Id);

            Assert.False(scooterExtendedMoreThanDay.IsRented);
            Assert.True(scooterExtendedMoreThanDay.RentPeriods.Count == daysDiff + 1);
            Assert.True(costMoreThanDay == expectedCostMoreThanDay);
        }
        [Fact]
        public void CanEndRentMoreThanLimit()
        {
            var rentStartMoreThanDailyLimit = DateTime.UtcNow
                .AddMinutes((int)Math.Round(-dailyLimit / TestDataFactory.DefaultPrice, MidpointRounding.AwayFromZero) - 1);

            var scooterExtendedMoreThanDailyLimit = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice,
                new List<RentPeriod> {
                    new RentPeriod {
                        RentStarted = rentStartMoreThanDailyLimit,
                        RentEnded = null}
                });

            var expectedCostMoreThanDailyLimit = rentStartMoreThanDailyLimit.Date != DateTime.UtcNow.Date ?
                (GetDatesTotalMinutes(DateTime.UtcNow, rentStartMoreThanDailyLimit) * scooterExtendedMoreThanDailyLimit.PricePerMinute) : dailyLimit;

            var testDataSet = new List<ScooterExtended> {
               scooterExtendedMoreThanDailyLimit
            };

            scooterExtendedMoreThanDailyLimit.IsRented = true;

            var scooterService = new ScooterService(testDataSet);
            var testRentalCompany = new RentalCompany(companyName, scooterService);

            var costMoreThanDailyLimit = testRentalCompany.EndRent(scooterExtendedMoreThanDailyLimit.Id);

            Assert.False(scooterExtendedMoreThanDailyLimit.IsRented);
            Assert.True(costMoreThanDailyLimit == expectedCostMoreThanDailyLimit);
        }
    }
}
