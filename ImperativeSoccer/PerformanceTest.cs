using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace ImperativeSoccer
{
    public class PerformanceTest
    {
        public static long NS_PER_TICK = (1000L * 1000L * 1000L) / Stopwatch.Frequency;
        public static long computation_point = 50000;

        public string PlayerName { get; private set; }

        //used for keeping track of number of output samples produced. 
        public long OutputCount { get; set; }
        public long InputCount { get; set; }

        //average computation time
        public double AverageComputationTime { get; private set; }
        //used for storing sum of computation time. 
        public double SumComputationTime { get; private set; }
        //for keeping track of min computation time 
        public double MinComputationTime { get; private set; }
        //for keeping track of max computation time. 
        public double MaxComputationTime { get; private set; }
        //throughput observed
        public double Throughput { get; private set; }
        public double InpRate { get; private set; }

        //stopwatch for latency and throughput calculation. 
        private Stopwatch sw;
        //stores time at which input sample is received. 
        private long prev_time;


        public PerformanceTest(string player_name)
        {
            OutputCount = 0;
            InputCount = 0;
            SumComputationTime = 0;
            MinComputationTime = Double.MaxValue;
            MaxComputationTime = Double.MinValue;
            AverageComputationTime = -1;
            prev_time = -1;
            Throughput = -1;
            InpRate = -1;
            PlayerName = player_name;

            sw = new Stopwatch();

        }

        //starts stopwatch used for calculations
        public void startSW()
        {
            sw.Start();
        }
        //records time at which input sample arrives. 
        public void recordTime()
        {
            if (OutputCount % computation_point == 0)
                prev_time = sw.ElapsedTicks;
        }

        //calculates computation time based on supplied input arrival_time and curr_time observed when 
        //output sample gets generated. 
        public void computeMetrics(double arrival_time, double curr_time)
        {

            if (OutputCount % computation_point == 0)
            {
                double computation_time_microSec = ((curr_time - arrival_time) * NS_PER_TICK) / 1000.0;
                updateMetrics(computation_time_microSec);
            }
            OutputCount++;


        }
        //calculates computation time based on recored prev_time with recordTime function. 
        public void computeMetrics()
        {

            if (OutputCount % computation_point == 0)
            {
                long curr_time = sw.ElapsedTicks;
                double computation_time_microSec = ((curr_time - prev_time) * NS_PER_TICK) / 1000.0;
                updateMetrics(computation_time_microSec);
            }
            OutputCount++;
        }

        //updates min,max and sum of computation times 
        public void updateMetrics(double computation_time_microSec)
        {

            SumComputationTime += computation_time_microSec;
            if (computation_time_microSec < MinComputationTime)
                MinComputationTime = computation_time_microSec;
            if (computation_time_microSec > MaxComputationTime)
                MaxComputationTime = computation_time_microSec;
        }
        //stops the stopwatch and calculates throughput observed and avg. computation time. 
        public void postProcess(long elapsedTicks)
        {
            Throughput = (double)(OutputCount * 1000 * 1000 * 1000)
                / (double)(elapsedTicks * PerformanceTest.NS_PER_TICK);
            InpRate = (double)(InputCount * 1000 * 1000 * 1000)
                / (double)(elapsedTicks * PerformanceTest.NS_PER_TICK);
            AverageComputationTime = SumComputationTime / OutputCount;
        }
        public void postProcess()
        {
            sw.Stop();
            Throughput = (double)(OutputCount * 1000 * 1000 * 1000)
                / (double)(sw.ElapsedTicks * PerformanceTest.NS_PER_TICK);
            InpRate = (double)(InputCount * 1000 * 1000 * 1000)
                / (double)(sw.ElapsedTicks * PerformanceTest.NS_PER_TICK);
            AverageComputationTime = SumComputationTime / OutputCount;

        }

    }
}
