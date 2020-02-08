using ScooterRentalServiceBusinessLogic.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ScooterRentalServiceBusinessLogicTests
{
    public class ScooterServiceTests
    {
        private string generatedId => Guid.NewGuid().ToString();
        private readonly decimal defaultPrice = 10m;
        private List<ScooterExtended> ScooterTestDataFactory()
        {
            return new List<ScooterExtended> {
                new ScooterExtended("b01d04be-ba12-49b3-806e-b517e4084cea",0.01m),
                new ScooterExtended("71cd41bd-6a63-4c68-937a-d020298923f0",0.1m),
                new ScooterExtended("581b1339-e606-44de-80e0-b3cc37c5e9b5",1m),
                new ScooterExtended("97b1b854-1b32-428c-924c-a720cd26b080",10m)
            };
        }
        [Fact]
        public void CanThrowDuplicateItemInit()
        {
            var duplicateId = "1";
            var scotters = new List<ScooterExtended> {
                new ScooterExtended(duplicateId, defaultPrice),
                new ScooterExtended(duplicateId, defaultPrice)
            };

            Assert.Throws<ScooterServiceException>(() => new ScooterService(scotters));
        }
        [Fact]
        public void CanGetScooters()
        {
            var scotterService = new ScooterService((List<ScooterExtended>)null);

            var scooters = scotterService.GetScooters();

            Assert.True(scooters.Count == 0);

            scotterService = new ScooterService((Dictionary<string, ScooterExtended>)null);

            scooters = scotterService.GetScooters();

            Assert.True(scooters.Count == 0);

            var testDataSet = ScooterTestDataFactory();

            scotterService = new ScooterService(testDataSet);

            scooters = scotterService.GetScooters();

            Assert.True(scooters.Count == testDataSet.Count);
        }
        [Fact]
        public void CanThrowWrongScooterIdGet()
        {
            var scotterService = new ScooterService(ScooterTestDataFactory());

            Assert.Throws<ScooterServiceScooterNotFoundException>(() => scotterService.GetScooterById(generatedId));
        }
        [Fact]
        public void CanThrowWrongScooterIdRemove()
        {
            var scotterService = new ScooterService(ScooterTestDataFactory());

            Assert.Throws<ScooterServiceScooterNotFoundException>(() => scotterService.RemoveScooter(generatedId));
        }
        [Fact]
        public void CanThrowDuplicateScooterAdd()
        {
            var scotterService = new ScooterService(ScooterTestDataFactory());

            var duplicateid = "B01D04BE-BA12-49B3-806E-B517E4084CEA";
            var pricePerMinute = 20;

            Assert.Throws<ScooterServiceScooterDuplicateIdException>(() => scotterService.AddScooter(duplicateid, pricePerMinute));
        }
        [Fact]
        public void CanAddScooter()
        {
            var testDataSet = ScooterTestDataFactory().ToDictionary(k => k.Id, val => val);

            var baseDataSet = ScooterTestDataFactory();

            var scotterService = new ScooterService(testDataSet);

            var newScooterId = generatedId;

            scotterService.AddScooter(newScooterId, defaultPrice);

            Assert.True(testDataSet.Count == baseDataSet.Count + 1);

            Assert.True(testDataSet.Any(a => a.Value.Id == newScooterId) 
                && !baseDataSet.Any(a => a.Id == newScooterId)
                );
        }
    }
}
