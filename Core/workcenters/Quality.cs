namespace Core.Workcenters
{
  using Core.Enterprise;
  using Core.Resources;

  public class Quality
  {
    private readonly int _inspectionTime;

    public Quality()
    {
      _inspectionTime = 3;
      CurrentInspectionTime = 0;
      CurrentWo = null;
      Buffer = new NeoQueue();
    }

    public IWork CurrentWo { get; private set; }
    public int CurrentInspectionTime { get; private set; }
    public ICustomQueue Buffer { get; }

    public void AddToQueue(IWork workorder)
    {
      Buffer.Enqueue(workorder);
    }
    public IWork Work(DayTime dayTime, IHandleBigData bigData, string name)
    {
      if(CurrentWo == null)
      {
        if (Buffer.Count > 0)
        {
          CurrentWo = Buffer.Dequeue();
          CurrentInspectionTime = _inspectionTime;
        }
        return null;
      }

      IWork answer = null;
      CurrentInspectionTime--;
      if (CurrentInspectionTime == 0)
      {
        answer = CurrentWo;
        CurrentWo = null;

        answer.NonConformance = bigData.IsNonConformance(name);
        if (!answer.NonConformance)
        {
          answer.SetNextOp();
        }
      }

      return answer;
    }
  }
}