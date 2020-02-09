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
        public void CanEndRent()
        {
            var rentStart = DateTime.UtcNow
                .AddMinutes((int)Math.Round(-dailyLimit / TestDataFactory.DefaultPrice, MidpointRounding.AwayFromZero) + 1);

            var scooterExtendedOneMinuteOver = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice,
                new List<RentPeriod> {
                    new RentPeriod {
                        RentStarted = rentStart,
                        RentEnded = null}
                });
            var scooterExtendedMoreThanDay = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice,
            new List<RentPeriod> {
                            new RentPeriod {
                                RentStarted = rentStart,
                                RentEnded = null}
            });
            var scooterExtendedLessThanDailyLimit = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice,
            new List<RentPeriod> {
                                new RentPeriod {
                                    RentStarted = rentStart,
                                    RentEnded = null}
            });

            var testDataSet = new List<ScooterExtended> {
               scooterExtendedOneMinuteOver,
               scooterExtendedMoreThanDay,
               scooterExtendedLessThanDailyLimit
            };

            scooterExtendedOneMinuteOver.IsRented = true;
            scooterExtendedMoreThanDay.IsRented = true;
            scooterExtendedLessThanDailyLimit.IsRented = true;

            var scooterService = new ScooterService(testDataSet);

            var testRentalCompany = new RentalCompany(companyName, scooterService);

            var costOneMinuteOver = testRentalCompany.EndRent(scooterExtendedOneMinuteOver.Id);
            var costMoreThanDay = testRentalCompany.EndRent(scooterExtendedMoreThanDay.Id);
            var costLessThanDailyLimit = testRentalCompany.EndRent(scooterExtendedLessThanDailyLimit.Id);

            Assert.False(scooterExtendedOneMinuteOver.IsRented);
            Assert.True(costOneMinuteOver == dailyLimit);

            Assert.False(scooterExtendedMoreThanDay.IsRented);
            Assert.True(costMoreThanDay == dailyLimit);

            Assert.False(scooterExtendedLessThanDailyLimit.IsRented);
            Assert.True(costLessThanDailyLimit == dailyLimit);
        }
    }
}
