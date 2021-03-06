namespace Core.Plant
{
  using Core;
  using System.Collections.Generic;
  using Enterprise;
  using Resources;
  using Resources.Virtual;
  using Workcenters;

  // TODO Non conformance
  // TODO - API for Machine Breakdowns
  public enum MesSchedule { DEFAULT=0 };

  public interface IMes
  {
    Dictionary<string, List<IWork>> LocationInventories { get; }
    Dictionary<string, VirtualWorkcenter> Locations { get; }
    Dictionary<int, IWork> Workorders { get; }

    void Add(IAcceptWorkorders workcenter);
    void AddErp(IErp erp);
    void AddWorkorder(string location, IWork wo);
    void Complete(int wo_id);
    List<int> GetLocationWoIds(string location);
    IWork GetWorkorder(int id);
    void Move(int wo_id, string source_name, string destination_name);
    void NonConformance(int wo_id);
    void Ship(int wo_id);
    void StartProgress(int wo_id);
    void StartTransit(int wo_id, string workcenterName);
    void StopProgress(int wo_id);
    void StopTransit(int wo_id, string workcenterName);
    void Work(DayTime dayTime);
  }

  public class Mes : IMes
  {
// Properties
    public Dictionary<string, List<IWork>> LocationInventories { get; }
    public Dictionary<string, VirtualWorkcenter> Locations { get; }
    public string Name { get; }
    public Dictionary<int, IWork> Workorders { get; }

// TODO Remove MesSchedule from Constructor, add to configuration.
// Constructor
    public Mes(string name, MesSchedule schedule=(MesSchedule) 0)
    {
      Erp = null;
      Name = name;
      Workorders = new Dictionary<int, IWork>();
      LocationInventories = new Dictionary<string, List<IWork>>();
      Locations = new Dictionary<string, VirtualWorkcenter>();
      Changes = new List<Change>();

      _schedule = schedule;
      _nextDump = new DayTime();

    }

// Pure Methods
    public List<int> GetLocationWoIds(string location)
    {
      return LocationInventories[location].ConvertAll<int>(x => x.Id);
    }
    
    public IWork GetWorkorder(int id)
    {
      return Workorders[id];
    }

// Impure Methods
    public void AddErp(IErp erp)
    {
      if(Erp != null) { return; }
      Erp = erp;
    }

    public void Add(IAcceptWorkorders workcenter)
    {
      Locations.Add(workcenter.Name, new VirtualWorkcenter(workcenter.Name, workcenter.ListOfValidTypes()));
      LocationInventories.Add(workcenter.Name, new List<IWork>());
    }

    public void AddWorkorder(string location, IWork wo)
    {
      if(Workorders.ContainsKey(wo.Id))
      {
        throw new System.ArgumentException("Workorder already exists in MES");
      }
      VirtualWorkorder newWo = new VirtualWorkorder(wo.CurrentOpIndex, wo);
      
      Workorders[newWo.Id] = newWo;
      LocationInventories[location].Add(newWo);
      Changes.Add(new Change(newWo.Id, true));
    }

    public void Complete(int wo_id)
    {
      VirtualWorkorder wo = (VirtualWorkorder) Workorders[wo_id];
      wo.SetNextOp();
      wo.ChangeStatus(VirtualWorkorder.Statuses.Open);
    }

    public void Move(int wo_id, string source_name, string destination_name)
    {
      if (!Workorders.ContainsKey(wo_id))
      {
        throw new System.ArgumentOutOfRangeException("Workorder does not exist");
      }
      
      if (!Locations.ContainsKey(source_name))
      {
        throw new System.ArgumentException("Source Location does not exist");
      }
      
      if (!Locations.ContainsKey(destination_name))
      {
        throw new System.ArgumentException("Destination Location does not exist");
      }

      VirtualWorkorder wo = (VirtualWorkorder) Workorders[wo_id];
      LocationInventories[source_name].Remove(wo);
      LocationInventories[destination_name].Add(wo);
    }

    public void NonConformance(int wo_id)
    {
      ((VirtualWorkorder) Workorders[wo_id]).NonConformance = true;
      StopProgress(wo_id);
    }

    public void Ship(int wo_id)
    {
      VirtualWorkorder wo = (VirtualWorkorder) Workorders[wo_id];
      Workorders.Remove(wo_id);
      LocationInventories["Shipping Dock"].Remove(wo);
      Changes.Add(new Change(wo_id, false));
    }

    public void StartProgress(int wo_id)
    {
      var wo = (VirtualWorkorder) Workorders[wo_id];
      wo.ChangeStatus(VirtualWorkorder.Statuses.InProgress);
      wo.NonConformance = false;
    }

    public void StartTransit(int wo_id, string workcenterName)
    {
      VirtualWorkorder wo = (VirtualWorkorder) Workorders[wo_id];
      wo.ChangeStatus(VirtualWorkorder.Statuses.OnRoute);
      LocationInventories[workcenterName].Remove(wo);
    }

    public void StopProgress(int wo_id)
    {
      ((VirtualWorkorder) Workorders[wo_id]).ChangeStatus(VirtualWorkorder.Statuses.Open);
    }

    public void StopTransit(int wo_id, string workcenterName)
    {
      VirtualWorkorder wo = (VirtualWorkorder) Workorders[wo_id];
      wo.ChangeStatus(VirtualWorkorder.Statuses.Open);
      if (!LocationInventories[workcenterName].Contains(wo))
      {
        LocationInventories[workcenterName].Add(wo);
      }
    }

    public void Work(DayTime dayTime)
    {
      if(!_nextDump.Equals(dayTime)) { return; }
      
      foreach(Change change in Changes)
      {
        if(change.IsAddToPlant)
        {
          Erp.Receive(change.Woid, Name);
        }
        else
        {
          Erp.Ship(change.Woid, Name);
        }
      }

      _nextDump = NextDumpTime(dayTime);
    }

// Private
    private DayTime _nextDump;
    private MesSchedule _schedule;
    private List<Change> Changes { get; }
    private IErp Erp { get; set; }

    private DayTime NextDumpTime(DayTime currentDumpTime)
    {
      //TODO Update MES Dump Time to be based on a configuration.
      return _schedule switch
      {
        _ => currentDumpTime.CreateTimestamp(24*60)
      };
    }

    private class Change
    {
      public int Woid { get; }
      public bool IsAddToPlant { get; }

      public Change(int woid, bool add)
      {
        Woid = woid;
        IsAddToPlant = add;
      }
    }
  }
}