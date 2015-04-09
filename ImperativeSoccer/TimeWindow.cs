using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImperativeSoccer
{
    public class TimeWindow<T,TAccumulate>
    {
        private LinkedList<T> timestamp_list;
        private TAccumulate seed;
        private long timespan;
        private string field_name;
        private Func<TAccumulate, T, IList<T>, long, TAccumulate> aggregator;

        public TimeWindow(long timespan,string field_name,TAccumulate seed, Func<TAccumulate,T,IList<T>,long,TAccumulate> aggregator)
        {
            timestamp_list = new LinkedList<T>();
            this.timespan = timespan;
            this.field_name = field_name;
            this.seed = seed;
            this.aggregator = aggregator;
        }
        public TAccumulate getUpdate(T value)
        {
            int ex_count = 0;
            IList<T> expiredList = new List<T>();
            while (true)
            {
                var item = timestamp_list.First;
                if (item == null)
                    break;
                long currentTs = (long)value.GetType().GetField(field_name).GetValue(value);
                long itemTs = (long)value.GetType().GetField(field_name).GetValue(item.Value);

                if (currentTs - itemTs > timespan)
                {
                    ex_count++;
                    if (timestamp_list == null)
                        Console.WriteLine("null list!");
                    timestamp_list.RemoveFirst();
                    expiredList.Add(item.Value);
                }
                else
                    break;
            }
            timestamp_list.AddLast(value);
           
            seed = aggregator(seed, value, expiredList, timestamp_list.Count);
            return seed;
        }      


    }
}
