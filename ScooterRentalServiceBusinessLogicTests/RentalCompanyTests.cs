using ScooterRentalServiceBusinessLogic.Concrete;
using ScooterRentalServiceBusinessLogic.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;
using System.Linq;
using Moq;
using ScooterRentalServiceBusinessLogic;

namespace ScooterRentalServiceBusinessLogicTests
{
    public class RentalCompanyTests
    {
        private readonly RentalCompany templateCompany;
        private readonly string companyName = "TestCompany";
        private decimal dailyLimit => templateCompany.DailyLimit;
        private Mock<IDateTimeHelper> mockDate(DateTime dateTime)
        {
            var mockDateTimeHelper = new Mock<IDateTimeHelper>();
            mockDateTimeHelper.Setup(o => o.GetUtcDateTimeNow()).Returns(dateTime);
            return mockDateTimeHelper;
        }
        private int GetDatesTotalMinutes(DateTime dte, DateTime dts)
        {
            return (int)Math.Round((dte - dts).TotalMinutes,MidpointRounding.AwayFromZero);
        }
        public RentalCompanyTests()
        {
            templateCompany = new RentalCompany(companyName);
        }
        [Fact]
        public void CanThrowStartRent()
        {
            var scooterExtended = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice);
            var scooterExtendedCorrupted = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice, 
                new List<RentPeriod> { new RentPeriod { RentStarted = DateTime.UtcNow, RentEnded = null } }
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
                new List<RentPeriod> { new RentPeriod { RentStarted = DateTime.UtcNow, RentEnded = null } });
            var scooterExtendedPeriodCorrupted = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice,
               new List<RentPeriod> { new RentPeriod { RentStarted = DateTime.UtcNow, RentEnded = DateTime.UtcNow } });

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
            var moqDate = new DateTime(2019, 10, 10, 10, 10, 10, 10);
            var moqDateProvider = mockDate(moqDate);

            var period = (int)(dailyLimit / TestDataFactory.DefaultPrice) - 1;

            var scooterExtendedLessThanDailyLimit = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice,
            new List<RentPeriod> {
                                new RentPeriod {
                                    RentStarted = moqDate.AddMinutes(-period),
                                    RentEnded = null}
            });

            var expectedCostLessThanDailyLimit = period * TestDataFactory.DefaultPrice;

            var testDataSet = new List<ScooterExtended> {
               scooterExtendedLessThanDailyLimit
            };

            scooterExtendedLessThanDailyLimit.IsRented = true;

            var scooterService = new ScooterService(testDataSet);
            var testRentalCompany = new RentalCompany(companyName, scooterService, moqDateProvider.Object);

            var costLessThanDailyLimit = testRentalCompany.EndRent(scooterExtendedLessThanDailyLimit.Id);

            Assert.False(scooterExtendedLessThanDailyLimit.IsRented);
            Assert.True(costLessThanDailyLimit == expectedCostLessThanDailyLimit);
        }
        [Fact]
        public void CanEndRentMoreThanDay()
        {
            var moqDate = new DateTime(2019, 10, 10, 10, 10, 10, 10);
            var moqDateProvider = mockDate(moqDate);

            var period = 3;

            var rentStart = moqDate.AddDays(-period);

            var scooterExtendedMoreThanDay = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice,
            new List<RentPeriod> {
                            new RentPeriod {
                                RentStarted = rentStart,
                                RentEnded = null}
            });

            var startCost = GetDatesTotalMinutes(rentStart.Date.AddDays(1).AddSeconds(-1), rentStart) * scooterExtendedMoreThanDay.PricePerMinute;
            var endCost = GetDatesTotalMinutes(moqDate, moqDate.Date) * scooterExtendedMoreThanDay.PricePerMinute;

            var expectedCostMoreThanDay = (period - 1) * dailyLimit
                + (endCost > dailyLimit ? dailyLimit : endCost)
                + (startCost > dailyLimit ? dailyLimit : startCost);

            var expectedPeriods = period + 1;

            var testDataSet = new List<ScooterExtended> {
               scooterExtendedMoreThanDay
            };

            scooterExtendedMoreThanDay.IsRented = true;

            var scooterService = new ScooterService(testDataSet);
            var testRentalCompany = new RentalCompany(companyName, scooterService, moqDateProvider.Object);

            var costMoreThanDay = testRentalCompany.EndRent(scooterExtendedMoreThanDay.Id);

            Assert.False(scooterExtendedMoreThanDay.IsRented);
            Assert.True(scooterExtendedMoreThanDay.RentPeriods.Count == expectedPeriods);
            Assert.True(costMoreThanDay == expectedCostMoreThanDay);
        }
        [Fact]
        public void CanEndRentMoreThanLimit()
        {
            var moqDate = new DateTime(2019, 10, 10, 10, 10, 10, 10);
            var moqDateProvider = mockDate(moqDate);

            var period = (int)(dailyLimit / TestDataFactory.DefaultPrice) + 1;

            var scooterExtendedMoreThanDailyLimit = new ScooterExtended(TestDataFactory.GeneratedId, TestDataFactory.DefaultPrice,
                new List<RentPeriod> {
                    new RentPeriod {
                        RentStarted = moqDate.AddMinutes(-period),
                        RentEnded = null}
                });

            var expectedCostMoreThanDailyLimit = dailyLimit;

            var testDataSet = new List<ScooterExtended> {
               scooterExtendedMoreThanDailyLimit
            };

            scooterExtendedMoreThanDailyLimit.IsRented = true;

            var scooterService = new ScooterService(testDataSet);
            var testRentalCompany = new RentalCompany(companyName, scooterService, moqDateProvider.Object);

            var costMoreThanDailyLimit = testRentalCompany.EndRent(scooterExtendedMoreThanDailyLimit.Id);

            Assert.False(scooterExtendedMoreThanDailyLimit.IsRented);
            Assert.True(costMoreThanDailyLimit == expectedCostMoreThanDailyLimit);
        }
        [Fact]
        public void CanCalculateIncomeWithoutDailyLimit()
        {
            var moqDate = new DateTime(2019, 10, 10, 10, 10, 10, 10);
            var moqDateProvider = mockDate(moqDate);

            var period = (int)(dailyLimit / TestDataFactory.DefaultPrice) - 1;
            var dateBase = moqDate.AddMinutes(-period * 2 - 1);

            var testDataSet = TestDataFactory.GetScooterTestDataSet(dateBase, periodDays: 1, periodMinutesShift: period);

            var scooterService = new ScooterService(testDataSet);

            var testRentalCompany = new RentalCompany(companyName, scooterService, moqDateProvider.Object);

            var expectedResultNull = testDataSet.Count * 3 * TestDataFactory.DefaultPrice * period;
            var expectedResultNotNull = testDataSet.Count * 2 * TestDataFactory.DefaultPrice * period;
            var expectedResultYear = testDataSet.Count * TestDataFactory.DefaultPrice * period;

            var incomeNotNull = testRentalCompany.CalculateIncome(null, false);
            var incomeNull = testRentalCompany.CalculateIncome(null, true);
            var incomeYear = testRentalCompany.CalculateIncome(moqDate.Year, false);

            Assert.True(incomeNotNull == expectedResultNotNull);
            Assert.True(incomeNull == expectedResultNull);
            Assert.True(incomeYear == expectedResultYear);
        }
        [Fact]
        public void CanCalculateIncomeWithDailyLimit()
        {
            var moqDate = new DateTime(2019, 10, 10, 10, 10, 10, 10);
            var moqDateProvider = mockDate(moqDate);

            var period = (int)(dailyLimit / TestDataFactory.DefaultPrice) + 1;
            var dateBase = moqDate.AddMinutes(-period * 2 - 1);

            var testDataSet = TestDataFactory.GetScooterTestDataSet(dateBase, periodDays: 1, periodMinutesShift: period);

            var scooterService = new ScooterService(testDataSet);

            var testRentalCompany = new RentalCompany(companyName, scooterService, moqDateProvider.Object);

            var expectedResultNull = testDataSet.Count * 3 * dailyLimit;
            var expectedResultNotNull = testDataSet.Count * 2 * dailyLimit;
            var expectedResultYear = testDataSet.Count * dailyLimit;

            var incomeNotNull = testRentalCompany.CalculateIncome(null, false);
            var incomeNull = testRentalCompany.CalculateIncome(null, true);
            var incomeYear = testRentalCompany.CalculateIncome(moqDate.Year, false);

            Assert.True(incomeNotNull == expectedResultNotNull);
            Assert.True(incomeNull == expectedResultNull);
            Assert.True(incomeYear == expectedResultYear);
        }
        [Fact]
        public void CanCalculateIncomeWithMoreThanOneDay()
        {
            var moqDate = new DateTime(2019, 10, 10, 10, 10, 10, 10);
            var moqDateProvider = mockDate(moqDate);

            var period = 3;
            var timeshift = 10;
            var dateBase = moqDate.AddDays(-period);

            var testDataSet = TestDataFactory.GetScooterTestDataSet(dateBase, periodDays: period, periodMinutesShift: timeshift);

            var scooterService = new ScooterService(testDataSet);

            var testRentalCompany = new RentalCompany(companyName, scooterService, moqDateProvider.Object);

            var expectedResultNotNull = testDataSet.Count * 2 * period * timeshift * TestDataFactory.DefaultPrice;
            var expectedResultNull = expectedResultNotNull + dailyLimit * TestDataFactory.DefaultPrice * testDataSet.Count * 2;
            var expectedResultYear = testDataSet.Count * period * timeshift * TestDataFactory.DefaultPrice;

            var incomeNotNull = testRentalCompany.CalculateIncome(null, false);
            var incomeNull = testRentalCompany.CalculateIncome(null, true);
            var incomeYear = testRentalCompany.CalculateIncome(moqDate.Year, false);

            Assert.True(incomeNotNull == expectedResultNotNull);
            Assert.True(incomeNull == expectedResultNull);
            Assert.True(incomeYear == expectedResultYear);
        }
    }
}
