namespace simulationCode
{
  using Core.Resources;
  using Core.Plant;
  using Core.Workcenters;
  using System.Collections.Generic;

  public class SimulationSetup
  {
    static readonly string drill = "Drill", lathe = "Lathe", cnc = "CNC";
    static readonly string drillOpType1 = "drillOpType1", 
        drillOpType2 = "drillOpType2", 
        latheOpType1 = "latheOpType1", 
        latheOpType2 = "latheOpType2",
        cncOpType1 = "cncOpType1",
        cncOpType2 = "cncOpType2",
        cncOpType3 = "cncOpType3",
        cncOpType4 = "cncOpType4",
        shippingOpType = "shippingOp";

    public static List<IAcceptWorkorders> GenerateWorkCenters()
    {

      List<IAcceptWorkorders> workcenters = new List<IAcceptWorkorders>();

      Machine a = new Machine(drill, new Core.Schedulers.MachineScheduler(), new List<string>{drillOpType1, drillOpType2});
      Workcenter wc = new Workcenter("drill_WC_a", a);

      workcenters.Add(wc);

      Machine b = new Machine(lathe, new Core.Schedulers.MachineScheduler(), new List<string>{latheOpType1, latheOpType2});
      Workcenter wc2 = new Workcenter("lathe_WC_b", b);

      workcenters.Add(wc2);

      Machine c = new Machine(cnc, new Core.Schedulers.MachineScheduler(), new List<string>{cncOpType1, cncOpType2});
      Workcenter wc3 = new Workcenter("cnc_WC_c", c);

      workcenters.Add(wc3);

      Machine d = new Machine(lathe, new Core.Schedulers.MachineScheduler(), new List<string>{cncOpType2, cncOpType3, cncOpType4});
      Workcenter wc4 = new Workcenter("cnc_WC_d", d);

      workcenters.Add(wc4);

      Dock dock = new Dock();

      workcenters.Add(dock);

      return workcenters;
    }

    public static List<Workorder> GenerateWorkorders()
    {
      List<Workorder> workorders = new List<Workorder>();

      Op drillOp1 = new Op(drillOpType1, 4, 1);
      Op drillOp2 = new Op(drillOpType2, 5, 1);
      Op latheOp1 = new Op(latheOpType1, 15, 5);
      Op latheOp2 = new Op(latheOpType2, 13, 5);
      Op cncOp1 = new Op(cncOpType1, 30, 10);
      Op cncOp2 = new Op(cncOpType2, 35, 15);
      Op cncOp3 = new Op(cncOpType3, 40, 12);
      Op cncOp4 = new Op(cncOpType4, 45, 10);
      Op shippingOp = new Op(shippingOpType, 0, 0);

      workorders.Add(new Workorder(1, new List<Op> { drillOp1, drillOp2, latheOp1, shippingOp }));
      workorders.Add(new Workorder(2, new List<Op> { drillOp2, drillOp2, drillOp1, latheOp2, shippingOp }));
      workorders.Add(new Workorder(3, new List<Op> { latheOp1, latheOp1, shippingOp }));
      workorders.Add(new Workorder(4, new List<Op> { latheOp2, latheOp2, drillOp1, shippingOp }));
      workorders.Add(new Workorder(5, new List<Op> { latheOp1, latheOp1, drillOp2, shippingOp }));
      workorders.Add(new Workorder(6, new List<Op> { drillOp2, latheOp2, cncOp1, shippingOp }));
      workorders.Add(new Workorder(7, new List<Op> { drillOp1, latheOp1, cncOp2, shippingOp }));
      workorders.Add(new Workorder(8, new List<Op> { latheOp2, latheOp2, cncOp3, shippingOp }));
      workorders.Add(new Workorder(9, new List<Op> { latheOp1, cncOp1, cncOp4, shippingOp }));

      return workorders;
    }
    
    public static List<Plant> GeneratePlants()
    {
      List<Plant> plants = new List<Plant>();
      
      List<Workorder> wo_list = SimulationSetup.GenerateWorkorders();
      List<IAcceptWorkorders> wc_list = SimulationSetup.GenerateWorkCenters();

      IAcceptWorkorders wc1 = wc_list[0];
      IAcceptWorkorders wc2 = wc_list[1];
      IAcceptWorkorders wc3 = wc_list[2];
      IAcceptWorkorders wc4 = wc_list[4];
      
      foreach(Workorder wo in wo_list)
      {
          if (wc1.ReceivesType(wo.CurrentOpType))
          {
              wc1.AddToQueue(wo);
          }
          else if (wc2.ReceivesType(wo.CurrentOpType))
          {
              wc2.AddToQueue(wo);
          }
          else if (wc3.ReceivesType(wo.CurrentOpType))
          {
              wc3.AddToQueue(wo);
          }
          else if (wc4.ReceivesType(wo.CurrentOpType))
          {
              wc4.AddToQueue(wo);
          }
          else
          {
            throw new System.ArgumentException("No WC for wo: " + wo.ToString());
          }
      }

      Plant plant = new Plant("plantA", wc_list);

      Transportation transport = new Transportation(wc1, new Core.Schedulers.TransportationScheduler(plant));
      plant.InternalTransportation = transport;

      plants.Add(plant);

      return plants;
    }
  }
}