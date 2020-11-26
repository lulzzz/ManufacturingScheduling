﻿using System;
using Core;

namespace simulationCode
{
    using Core;
    using Core.Resources;
    using Core.Plant;
    using Core.Enterprise;
    using Newtonsoft.Json;
    using System.Collections.Generic;
    using System.IO;

    public static class Program
    {
        public static void Main()
        {
            Console.WriteLine("Hello Simulation");
            DayTime dt = new DayTime();

            Console.WriteLine("Creating Customer and Enterprise");
            Customer customer = new Customer();
            Enterprise ent = new Enterprise(customer);
            BigData bigData = new BigData();

            Console.WriteLine("Generating Plants");
            List<Plant> plants = SimulationSetup.GeneratePlants();
            foreach(Plant plant in plants)
            {
                ent.Add(plant);
                foreach(var wc in plant.Workcenters)
                {
                    if(wc.Name == "Shipping Dock" || wc.Name == "Stage") { continue; }
                    bigData.AddWorkcenter(wc.Name);
                    ((Core.Workcenters.Workcenter) wc).AddBigData(bigData);
                }
            }
            ent.Add(bigData);

            Console.WriteLine("Generating Transport Routes");
            var routes = SimulationSetup.GenerateRoutes(plants);
            Transport transport = new Transport(ent, routes);
            ent.Add(transport);

            Console.WriteLine("Generating Simulation Node");
            SimulationNode sn = new SimulationNode(dt, ent);
            
            Console.WriteLine("Loading Workorders");
            int woCounter = 0;
            while(woCounter < Configuration.InitialNumberOfWorkorders)
            {
                Workorder.PoType type = SimulationSetup.SelectWoPoType(woCounter);
                DayTime due = SimulationSetup.SelectWoDueDate(dt, woCounter);
                int initialOp = SimulationSetup.SelectWoInitialOp(woCounter, Workorder.GetMaxOps(type) - 1);
                customer.CreateOrder(type, due, initialOp);
                woCounter++;
            }
            customer.Work(dt); // Load Workorders into Enterprise

            SaveToFile("default", 0, sn);

            Console.WriteLine("Starting Simulation");
            for(int i = 1; i < Configuration.MinutesForProgramToTest; i++)
            {
                dt.Next();
                ent.Work(dt);
                customer.Work(dt);

                var next = bigData.GetNextOrder(i);
                if(next.HasValue)
                {
                    customer.CreateOrder(next.Value.Item1, new DayTime((int) next.Value.Item2, 800));
                }

                if (i%500 == 0) 
                {
                    Console.Write(".");
                }
                
                SaveToFile("default", i, sn);
            }
            Console.WriteLine(".");
            Console.WriteLine("Finished with Simulation");
        }

        static private string ToJson(Object obj)
        {
            return JsonConvert.SerializeObject(obj);
        }

        static private void SaveToFile(string test, int time, SimulationNode simulationNode)
        {
            string path = Configuration.ResultFolder;
            string filename = path + test + "_" + time.ToString() + ".json";
            if(File.Exists(filename))
            {
                File.Delete(filename);
            }

            StreamWriter writer = new StreamWriter(filename);

            writer.WriteLine(ToJson(simulationNode));

            writer.Dispose();
            writer.Close();
        }
    }
}
