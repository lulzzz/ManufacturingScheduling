namespace Core.Enterprise
{
    using System.Collections.Generic;
    using System.Linq;
    using Core.Resources;

    public interface IRequestWork
    {
        List<string> ActiveOrders { get; }
        List<string> CompleteOrders { get; }
        void AddEnterprise(IEnterprise enterprise);
        void CreateOrder(Workorder.PoType po_type, DayTime due, int initialOp=0);
        void ReceiveProduct(Workorder.PoType type, DayTime dayTime);
        void Work(DayTime dayTime);
    }

    public class Customer : IRequestWork
    {
        private IEnterprise _enterprise;
        private List<Order> _orders;

        public Customer()
        {
            _enterprise = null;
            _orders = new List<Order>();
        }

        public List<string> ActiveOrders {
            get => Orders(false).Select(x => x.ToString()).ToList();
        }

        public List<string> CompleteOrders {
            get => Orders(true).Select(x => x.ToString()).ToList();
        }

        private List<Order> Orders(bool complete) {
            return _orders
                .Where(x => complete ? x.IsComplete : x.IsIncomplete )
                .OrderBy(x => x.DueString())
                .ThenBy(x => x.Type)
                .ToList();
        }

        public void AddEnterprise(IEnterprise enterprise)
        {
            if(_enterprise != null) { return; }
            _enterprise = enterprise;
        }

        public void CreateOrder(Workorder.PoType po, DayTime due, int initialOp=0)
        {
            _orders.Add(new Order(po, due, initialOp));
        }

        public void ReceiveProduct(Workorder.PoType type, DayTime dayTime)
        {
            Orders(false).First(x => x.Type == type).CompletePo(dayTime);
        }

        public float Result()
        {
            int denominator = _orders.Count();
            int ontime = Orders(true).Where( x => x.Complete.LteDay(x.Due)).Count();

            return ((float) ontime) / denominator;
        }


        public void Work(DayTime dayTime)
        {
            if (_enterprise == null) { return; }

            var ordersToSend = _orders.Where(x => x.Sent == false);
            foreach(var order in ordersToSend)
            {
                _enterprise.StartOrder(order.Type, order.Due, order.InitialOp);
                order.MarkAsSent();
            }
        }

        private class Order
        {
// Properties
            public DayTime Due { get; private set;}
            public DayTime Complete { get; private set;}
            public int InitialOp { get; }
            public bool IsComplete { get => Complete != null; }
            public bool IsIncomplete { get => Complete == null; }
            public bool Sent { get; private set; }
            public Workorder.PoType Type { get; }

// Constructor
            public Order(Workorder.PoType type, DayTime due, int initialOp=0)
            {
                Type = type;
                Due = due.CreateTimestamp(0);
                InitialOp = initialOp;
                Complete = null;
                Sent = false;
            }

// Pure Methods
            public string CompleteString() 
            { 
                if (IsIncomplete) { return ""; }
                return ConvertTime(Complete);
            }

            public string DueString() { return ConvertTime(Due); }

            public override string ToString()
            {
                const string deliminator = " ; ";
                string answer = Type + deliminator + ConvertTime(Due);
                if(IsComplete)
                {
                    answer += deliminator + ConvertTime(Complete);
                }
                return answer;
            }

// Impure Methods
            public void CompletePo(DayTime dayTime)
            {
                Complete = dayTime.CreateTimestamp(0);
            }

            public void MarkAsSent()
            {
                Sent = true;
            }

// Private Methods
            private string ConvertTime(DayTime time)
            {
                return time.Day.ToString();
            }
        }
    }
}