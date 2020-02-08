using ScooterRentalServiceBusinessLogic.Concrete;
using System;
using System.Collections.Generic;
using System.Text;
using Xunit;

namespace ScooterRentalServiceBusinessLogicTests
{
    public class ScooterServiceExtendedTests : ScooterServiceTests
    {
        [Fact]
        public void CanGetExtendedScooters()
        {
            var scotterService = new ScooterServiceExtended((List<ScooterExtended>)null);

            var scooters = scotterService.GetExtendedScooters();

            Assert.True(scooters.Count == 0);

            scotterService = new ScooterServiceExtended((Dictionary<string, ScooterExtended>)null);

            scooters = scotterService.GetExtendedScooters();

            Assert.True(scooters.Count == 0);

            var testDataSet = ScooterTestDataFactory();

            scotterService = new ScooterServiceExtended(testDataSet);

            scooters = scotterService.GetExtendedScooters();

            Assert.True(scooters.Count == testDataSet.Count);
        }
    }
}
