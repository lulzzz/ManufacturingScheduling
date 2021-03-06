namespace Tests.Enterprise
{
    using Core;
    using Core.Enterprise;
    using Core.Plant;
    using Core.Resources;
    using Core.Workcenters;
    using NSubstitute;
    using NUnit.Framework;
    using System.Collections.Generic;

    [TestFixture]
    public class EnterpriseTest
    {
        private DayTime _dayTime;
        private Customer _customer;
        private Enterprise _subject;
        private IPlant _plant1, _plant2;

        [SetUp]
        protected void Setup()
        {
            _dayTime = new DayTime();
            _plant1 = GeneratePlant("P1", 1);
            _plant2 = GeneratePlant("P2", 2);
            
            //List<IPlant> list = new List<IPlant>() { _plant1, _plant2 };
            _customer = new Customer();
            
            _subject = new Enterprise(_customer);
            _subject.Add(_plant1);
            _subject.Add(_plant2);
        }

        [Test]
        public void Work_CallsBothPlants()
        {
            _subject.Work(_dayTime);

            _plant1.Received().Work(_dayTime);
            _plant2.Received().Work(_dayTime);
        }

        private IPlant GeneratePlant(string name, int wonumber)
        {
            IPlant plant = Substitute.For<IPlant>();
            plant.Name.Returns(name);

            IMes mes = Substitute.For<IMes>();
            IWork wo = Substitute.For<IWork>();
            Dictionary<int, IWork> dic = new Dictionary<int, IWork>();
            wo.Id.Returns(wonumber);
            wo.Operations.Returns(new List<Op>() { new Op(Op.OpTypes.DrillOpType1) });
            wo.CurrentOpIndex.Returns(1);
            dic.Add(wonumber, wo);
            mes.Workorders.Returns(dic);
            plant.Mes.Returns(mes);

            IAcceptWorkorders wc = Substitute.For<IAcceptWorkorders>();
            wc.Name.Returns("Wc" + wonumber);
            wc.ListOfValidTypes().Returns(new List<Op.OpTypes>(){Op.OpTypes.DrillOpType1});
            plant.Workcenters.Returns(new List<IAcceptWorkorders>(){ wc });
            
            return plant;
        }
    }
}