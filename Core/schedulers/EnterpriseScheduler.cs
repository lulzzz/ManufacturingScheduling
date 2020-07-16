namespace Core.Schedulers
{
  using System.Linq;
  using Core.Enterprise;
  using Core.Resources;

  public enum EnterpriseSchedule { DEFAULT=0 };

  public interface IScheduleEnterprise
  {
    string SelectDestinationForExternalTransport(int woid);
  }

  public class EnterpriseScheduler : IScheduleEnterprise
  {
    public EnterpriseScheduler(IErp erp)
    {
      _erp = erp;
    }

    public string SelectDestinationForExternalTransport(int woid)
    {
      EnterpriseSchedule schedule = (EnterpriseSchedule) Configuration.EnterpriseSchedule;
      return schedule switch
      {
        _ => GetFirstValidDestination(woid)
      };
    }

    private string GetFirstValidDestination(int woid)
    {
      Op.OpTypes type = _erp.Workorders[woid].CurrentOpType;
      
      if (type == Op.OpTypes.ShippingOp) { return "customer"; }

      return _erp.Locations.First(x => x.Value.CanWorkOnType(type)).Value.Name;
    }

    private readonly IErp _erp;
  }
}