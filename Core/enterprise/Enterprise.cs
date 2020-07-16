namespace Core.Enterprise
{
  using System.Collections.Generic;
  using System.Linq;
  using Core;
  using Plant;
  using Resources;
  using Schedulers;

  public interface IEnterprise
  {
    IRequestWork Customer { get; }
    IErp Erp { get; }
    IEnumerable<IPlant> Plants { get; }
    EnterpriseScheduler Scheduler { get; }
    ITransportWorkBetweenPlants Transport { get; }
    void Add(IPlant plant);
    void Add(ITransportWorkBetweenPlants transport);
    void Add(IWork workorder);
    void StartOrder(Workorder.PoType type, DayTime due, int initialOp=0);
    void Work(DayTime dayTime);
  }

  public class Enterprise : IEnterprise
  {
// Properties
    public IRequestWork Customer { get; private set;}
    public IErp Erp { get; }
    public IEnumerable<IPlant> Plants { get => _plants; }
    public EnterpriseScheduler Scheduler { get; }
    public ITransportWorkBetweenPlants Transport { get; private set; }

    private List<Plant> _plants { get; }

// Constructor
    public Enterprise(Customer customer)
    {
      _plants = new List<Plant>();
      Erp = (IErp) new Erp("ERP");
      Scheduler = new EnterpriseScheduler(Erp);
      Transport = null;
      Customer = customer;
      customer.AddEnterprise(this);
    }

// Pure Methods

// Impure Methods
    public void Add(IPlant plant)
    {
      Erp.Add(plant);
      plant.Mes.AddErp(Erp);
      plant.Add(this);
      _plants.Add((Plant) plant);
    }

    public void Add(ITransportWorkBetweenPlants transport)
    {
      if(Transport != null) { return; }
      Transport = transport;
    }

    public void Add(IWork workorder)
    {
      Op.OpTypes type = workorder.CurrentOpIndex > 0 ? workorder.CurrentOpType : workorder.Operations[1].Type;
      Plant plant = _plants.FirstOrDefault(x => x.CanWorkOnType(type));
      
      plant.Add(workorder);
      Erp.AddWorkorder(plant.Name, workorder);
    }

    public void StartOrder(Workorder.PoType type, DayTime due, int initialOp=0)
    {
      Erp.CreateWorkorder(type, due);
    }

    public void Work(DayTime dayTime)
    {
      foreach(IPlant plant in Plants)
      {
        plant.Work(dayTime);
      }

      Transport?.Work(dayTime);
      Erp.Work(dayTime);
    }

// Private Methods

  }
}