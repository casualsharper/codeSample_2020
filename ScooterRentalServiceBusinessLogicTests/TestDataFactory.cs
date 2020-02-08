using ScooterRentalServiceBusinessLogic.Concrete;
using System;
using System.Collections.Generic;
using System.Text;

namespace ScooterRentalServiceBusinessLogicTests
{
    public static class TestDataFactory
    {
        public static string generatedId => Guid.NewGuid().ToString();
        public static readonly decimal defaultPrice = 10m;
        public static List<ScooterExtended> GetScooterTestDataSet()
        {
            return new List<ScooterExtended> {
                new ScooterExtended("b01d04be-ba12-49b3-806e-b517e4084cea",0.01m),
                new ScooterExtended("71cd41bd-6a63-4c68-937a-d020298923f0",0.1m),
                new ScooterExtended("581b1339-e606-44de-80e0-b3cc37c5e9b5",1m),
                new ScooterExtended("97b1b854-1b32-428c-924c-a720cd26b080",10m)
            };
        }
    }
}
