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
        private readonly string companyName = "TestCompany";
        [Fact]
        public void CanEndRent()
        {
            var testDataSet = TestDataFactory.GetScooterTestDataSet();

            var testItem = testDataSet.First();
            testItem.IsRented = true;

            var scooterService = new ScooterService(testDataSet);

            var testRentalCompany = new RentalCompany(companyName, scooterService);

            var cost = testRentalCompany.EndRent(testItem.Id);

            Assert.False(testItem.IsRented);
            Assert.True(cost == 0m);
        }
    }
}
