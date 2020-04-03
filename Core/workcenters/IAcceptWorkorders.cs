namespace Core.Workcenters
{
  using Core.Resources;
  using System.Collections.Generic;

  public interface IAcceptWorkorders
  {
    public void AddToQueue(IWork wo);
    public bool ReceivesType(string type);
    public void Work(DayTime dayTime);

    public string Name { get; }
    public ICollection<IWork> OutputBuffer { get; }
  }
}