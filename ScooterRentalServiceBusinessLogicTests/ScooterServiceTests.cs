using ScooterRentalServiceBusinessLogic.Concrete;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace ScooterRentalServiceBusinessLogicTests
{
    public class ScooterServiceTests
    {
        [Fact]
        public void CanThrowDuplicateItemInit()
        {
            var duplicateId = TestDataFactory.generatedId;
            var scotters = new List<ScooterExtended> {
                new ScooterExtended(duplicateId, TestDataFactory.defaultPrice),
                new ScooterExtended(duplicateId, TestDataFactory.defaultPrice)
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

            var testDataSet = TestDataFactory.GetScooterTestDataSet();

            scotterService = new ScooterService(testDataSet);

            scooters = scotterService.GetScooters();

            Assert.Equal(scooters.Count,testDataSet.Count);
        }
        [Fact]
        public void CanThrowWrongScooterIdGet()
        {
            var scotterService = new ScooterService(TestDataFactory.GetScooterTestDataSet());

            Assert.Throws<ScooterServiceScooterNotFoundException>(() => scotterService.GetScooterById(TestDataFactory.generatedId));
        }
        [Fact]
        public void CanGetScooter()
        {
            var testDataSet = TestDataFactory.GetScooterTestDataSet();

            var testItem = testDataSet.First();

            var scotterService = new ScooterService(testDataSet);

            var retrievedScooter = scotterService.GetScooterById(testItem.Id);

            Assert.Equal(testItem, retrievedScooter);
        }
        [Fact]
        public void CanGetExtendedScooters()
        {
            var scotterService = new ScooterService((List<ScooterExtended>)null);

            var scooters = scotterService.GetExtendedScooters();

            Assert.True(scooters.Count == 0);

            scotterService = new ScooterService((Dictionary<string, ScooterExtended>)null);

            scooters = scotterService.GetExtendedScooters();

            Assert.True(scooters.Count == 0);

            var testDataSet = TestDataFactory.GetScooterTestDataSet();

            scotterService = new ScooterService(testDataSet);

            scooters = scotterService.GetExtendedScooters();

            Assert.Equal(scooters.Count, testDataSet.Count);
        }
        [Fact]
        public void CanThrowWrongScooterIdRemove()
        {
            var testDataSet = TestDataFactory.GetScooterTestDataSet();

            var testItem = testDataSet.First();

            testItem.IsRented = true;

            var scotterService = new ScooterService(testDataSet);

            Assert.Throws<ScooterServiceScooterNotFoundException>(() => scotterService.RemoveScooter(TestDataFactory.generatedId));

            Assert.Throws<ScooterServiceScooterInRentException>(() => scotterService.RemoveScooter(testItem.Id));
        }
        [Fact]
        public void CanRemoveScooter()
        {
            var testDataSet = TestDataFactory.GetScooterTestDataSet().ToDictionary(k => k.Id, val => val);
            var baseDataSet = TestDataFactory.GetScooterTestDataSet();

            var scotterService = new ScooterService(testDataSet);

            var itemToRemove = testDataSet.First().Key;

            scotterService.RemoveScooter(itemToRemove);

            Assert.True(testDataSet.Count + 1 == baseDataSet.Count);

            Assert.True(!testDataSet.Any(a => a.Key == itemToRemove)
                && baseDataSet.Any(a => a.Id == itemToRemove)
                );
        }
        [Fact]
        public void CanThrowDuplicateScooterAdd()
        {
            var testDataSet = TestDataFactory.GetScooterTestDataSet();
            var scotterService = new ScooterService(testDataSet);

            Assert.Throws<ScooterServiceScooterDuplicateIdException>(() => scotterService.AddScooter(testDataSet.First().Id, TestDataFactory.defaultPrice));
        }
        [Fact]
        public void CanAddScooter()
        {
            var testDataSet = TestDataFactory.GetScooterTestDataSet().ToDictionary(k => k.Id, val => val);

            var baseDataSet = TestDataFactory.GetScooterTestDataSet();

            var scotterService = new ScooterService(testDataSet);

            var newScooterId = TestDataFactory.generatedId;

            scotterService.AddScooter(newScooterId, TestDataFactory.defaultPrice);

            Assert.True(testDataSet.Count == baseDataSet.Count + 1);

            Assert.True(testDataSet.Any(a => a.Value.Id == newScooterId) 
                && !baseDataSet.Any(a => a.Id == newScooterId)
                );
        }
    }
}
