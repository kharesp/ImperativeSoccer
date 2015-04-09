using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace ImperativeSoccer
{
    class Program
    {
        private static string query_name;
        private static string strategy_name;
        private static int domainId;
        private static string test_iteration_no;

        private static SoccerStrategy strategy;

        static void Main(string[] args)
        {
            Console.WriteLine("Starting Imperative Soccer Processor on tid:{0}",Thread.CurrentThread.ManagedThreadId);
            try
            {
                //parse input arguments
                if (args.Length < 4)
                {
                    Console.WriteLine("Usage: ImperativeSoccer.exe <domain-id> <query-name> <strategy> <test_iteration_no>");
                    return;
                }
                domainId = Int32.Parse(args[0]);
                query_name = args[1];
                strategy_name = args[2];
                test_iteration_no = args[3];

                String output_file_name = query_name + "_" + strategy_name + "_" + test_iteration_no + ".csv";
                
                //initialize data-structures. 
                MetaData.initializePlayerData();
                MetaData.createPlayerMap();
                MetaData.createSensorMap();
                MetaData.createTeamMap();



                //create soccer context
                SoccerContext context = new SoccerContext();

                //initialize strategy
                if (strategy_name.Equals("singleThread"))
                    strategy = new SingleThreadStrategy(output_file_name);
                else if (strategy_name.Equals("newThreadSensorData"))
                    strategy = new NewThreadStrategySensorData(output_file_name);
                else if (strategy_name.Equals("newThreadPlayerData"))
                    strategy = new NewThreadStrategyPlayerData(output_file_name);
                else if (strategy_name.Equals("threadPoolSensorData"))
                    strategy = new ThreadPoolStrategy<SensorData>(output_file_name);
                else if (strategy_name.Equals("threadPoolPlayerData"))
                    strategy = new ThreadPoolStrategy<PlayerData>(output_file_name);
                else if (strategy_name.Equals("separateThread"))
                    strategy = new SeparateThreadStrategy(output_file_name);

                else
                {
                    Console.WriteLine("Strategy name not recognized");
                    return;
                }

                //invoke specified query
                if (query_name.Equals("query_1"))
                    strategy.runQuery(SoccerStrategy.QUERY_1, context);
                else if (query_name.Equals("query_3"))
                    strategy.runQuery(SoccerStrategy.QUERY_3, context);
                else
                {
                    Console.WriteLine("query name not recognized");
                    return;
                }                                    
               

            }catch(Exception e)
            {
                Console.WriteLine("ImperativeSoccer: " + e.ToString());
            }
            
                        
        }
        
    }
}
