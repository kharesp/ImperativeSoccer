using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
namespace ImperativeSoccer
{
    abstract class SoccerStrategy
    {
        public const string QUERY_1="query_1";
        public const string QUERY_3="query_3";

        
        protected Dictionary<string, PlayerState> playerId_playerState;
        protected IList<ManualResetEvent> playerSignals;
        protected string output_file;
        protected string current_query;

        public SoccerStrategy(string output_file)
        { 
            current_query = QUERY_1;
            this.output_file = output_file;
             
            playerId_playerState = new Dictionary<string, PlayerState>();
            playerSignals = new List<ManualResetEvent>();

            foreach(var player_name in MetaData.PLAYER_MAP.Keys)
            {
                PlayerState state=new PlayerState(player_name);
                playerId_playerState.Add(player_name,state);
                playerSignals.Add(state.signal);                
                var sensor_list = MetaData.PLAYER_MAP[player_name];               

            }
        }

        protected void writeQueryResults()
        {
            using (StreamWriter file = new StreamWriter(output_file))
            {
                foreach (var mapping in playerId_playerState)
                {
                    var pt = mapping.Value.pt;
                    file.WriteLine("{0},{1},{2},{3}",
                            pt.OutputCount,
                            pt.Throughput,
                            pt.InputCount,
                            pt.InpRate);
                }
            }           
            
        }
        protected void waitForResults()
        {
            WaitHandle.WaitAll(playerSignals.ToArray());                       
        }
      

        public void runQuery(String query_name,SoccerContext context)
        {
            current_query = query_name;
            initializeStrategy();
            context.readSensorDataDDSListener(demux);
            Console.WriteLine("waiting for results..");
            waitForResults();
            writeQueryResults();
        }
        protected abstract void initializeStrategy();
        protected abstract void demux(SensorData data); 

    }
}
