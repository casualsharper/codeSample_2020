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
            var duplicateId = TestDataFactory.GeneratedId;
            var scotters = new List<ScooterExtended> {
                new ScooterExtended(duplicateId, TestDataFactory.DefaultPrice),
                new ScooterExtended(duplicateId, TestDataFactory.DefaultPrice)
            };

            Assert.Throws<ScooterServiceException>(() => new ScooterService(scotters));
        }
        [Fact]
        public void CanGetScooters()
        {
            var scotterServiceNullList = new ScooterService((List<ScooterExtended>)null);          
            var scotterServiceNullDict = new ScooterService((Dictionary<string, ScooterExtended>)null);
            var testDataSet = TestDataFactory.GetScooterTestDataSet();
            var scotterService = new ScooterService(testDataSet);

            var scootersNullList = scotterServiceNullList.GetScooters();
            var scootersNullDict = scotterServiceNullDict.GetScooters();
            var scooters = scotterService.GetScooters();          

            Assert.True(scootersNullList.Count == 0);
            Assert.True(scootersNullDict.Count == 0);
            Assert.True(scooters.Count == testDataSet.Count);
        }
        [Fact]
        public void CanThrowWrongScooterIdGet()
        {
            var scotterService = new ScooterService(TestDataFactory.GetScooterTestDataSet());

            Assert.Throws<ScooterServiceScooterNotFoundException>(() => scotterService.GetScooterById(TestDataFactory.GeneratedId));
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
            var scotterServiceNullList = new ScooterService((List<ScooterExtended>)null);
            var scotterServiceNullDict = new ScooterService((Dictionary<string, ScooterExtended>)null);
            var testDataSet = TestDataFactory.GetScooterTestDataSet();
            var scotterService = new ScooterService(testDataSet);

            var scootersNullList = scotterServiceNullList.GetExtendedScooters();
            var scootersNullDict = scotterServiceNullDict.GetExtendedScooters();
            var scooters = scotterService.GetExtendedScooters();

            Assert.True(scootersNullList.Count == 0);
            Assert.True(scootersNullDict.Count == 0);
            Assert.True(scooters.Count == testDataSet.Count);
        }
        [Fact]
        public void CanThrowWrongScooterIdRemove()
        {
            var testDataSet = TestDataFactory.GetScooterTestDataSet();

            var testItem = testDataSet.First();

            testItem.IsRented = true;

            var scotterService = new ScooterService(testDataSet);

            Assert.Throws<ScooterServiceScooterNotFoundException>(() => scotterService.RemoveScooter(TestDataFactory.GeneratedId));
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

            Assert.Throws<ScooterServiceScooterDuplicateIdException>(() => scotterService.AddScooter(testDataSet.First().Id, TestDataFactory.DefaultPrice));
        }
        [Fact]
        public void CanAddScooter()
        {
            var testDataSet = TestDataFactory.GetScooterTestDataSet().ToDictionary(k => k.Id, val => val);

            var baseDataSet = TestDataFactory.GetScooterTestDataSet();

            var scotterService = new ScooterService(testDataSet);

            var newScooterId = TestDataFactory.GeneratedId;

            scotterService.AddScooter(newScooterId, TestDataFactory.DefaultPrice);

            Assert.True(testDataSet.Count == baseDataSet.Count + 1);
            Assert.True(testDataSet.Any(a => a.Value.Id == newScooterId) 
                && !baseDataSet.Any(a => a.Id == newScooterId)
                );
        }
    }
}
