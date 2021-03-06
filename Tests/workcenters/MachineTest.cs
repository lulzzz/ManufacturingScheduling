namespace Tests.Workcenters
{
  using Core;
  using Core.Plant;
  using Core.Resources;
  using Core.Workcenters;
  using Core.Schedulers;
  using NUnit.Framework;
  using System.Collections.Generic;
  using NSubstitute;

  [TestFixture]
  public class MachineTest
  {
    private Machine _subject;
    private IScheduleMachines machineScheduler;
    private DayTime _dayTime;
    private const int WO_ID = 1;

    [SetUp]
    protected void SetUp()
    {
      Test test = new Test("Default", 0, 0, 0, 0, 0);
      Configuration.Initialize(test);
      
      IPlant plant = Substitute.For<IPlant>();
      ISchedulePlants ps = Substitute.For<ISchedulePlants>();
      plant.PlantScheduler.Returns(ps);
      ps.ValidateWoForMachines(Arg.Any<int>(), Arg.Any<string>()).Returns(x => x[0]);
      _subject = new Machine("test subject", Machine.Types.SmallDrill);
      _subject.AddPlant(plant);
      _dayTime = new DayTime();
    }

    [Test]
    public void Work_WhenEmpty_ReturnsNull()
    {
      var answer = _subject.Work(_dayTime);
      Assert.IsNull(answer);
    }

    [Test]
    public void Work_WhenNoCurrent_SetsCurrent()
    {
      SetupSubject(0);

      var answer = _subject.Work(_dayTime);

      Assert.IsNull(answer);
      Assert.IsTrue(_subject.InputBuffer.Empty());
      Assert.AreEqual(1, _subject.CurrentWorkorder.Id);
      Assert.AreEqual(4, _subject.SetupTime);
      Assert.AreEqual(5, _subject.EstTimeToComplete);
    }

    [Test]
    public void Work_WhenSetup_ReduceSetupTime()
    {
      SetupSubject(4);

      var answer = _subject.Work(_dayTime);

      Assert.IsNull(answer);
      Assert.IsTrue(_subject.InputBuffer.Empty());
      Assert.AreEqual(1, _subject.CurrentWorkorder.Id);
      Assert.AreEqual(0, _subject.SetupTime);
      Assert.AreEqual(5, _subject.EstTimeToComplete);
    }

    [Test]
    public void Work_WhenRunning_ReduceEstTimeToComplete()
    {
      SetupSubject(9);

      var answer = _subject.Work(_dayTime);

      Assert.AreEqual(WO_ID, answer.Id);
      Assert.AreEqual(Op.OpTypes.DrillOpType3, _subject.LastType);
      Assert.IsNull(_subject.CurrentWorkorder);
      Assert.IsTrue(_subject.InputBuffer.Empty());
      Assert.AreEqual(0, _subject.SetupTime);
      Assert.AreEqual(0, _subject.EstTimeToComplete);
    }

    //Prep Helpers
    public void SetupSubject(int workIterations)
    {
      Workorder wo = new Workorder(1, Workorder.PoType.p7,4);
      _subject.AddToQueue(wo);

      for(int i = 0; i < workIterations; i++)
      {
        _subject.Work(_dayTime);
      }
    }
  }
}