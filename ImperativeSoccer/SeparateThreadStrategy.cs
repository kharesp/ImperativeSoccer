using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;

namespace ImperativeSoccer
{
    
    class SeparateThreadStrategy: SoccerStrategy
    {
        WorkerThread<SensorData> worker;
        public SeparateThreadStrategy(string output_file): base(output_file)
        { }
        
        private void threadStart(WorkerThread<SensorData> worker,object obj)
        {            
            while (true)
            {
                SensorData data = worker.extractMessage();                
                if(data == null)
                {
                    foreach (var player in playerId_playerState.Values)
                        player.signalEnd(null);
                    break;
                }
                                
                if (MetaData.SENSOR_MAP.ContainsKey(data.sensor_id))
                {                       
                    string player_name = MetaData.SENSOR_MAP[data.sensor_id];                                                
                    PlayerState state = playerId_playerState[player_name];                    
                    if (current_query.Equals(QUERY_1))                    
                        state.query1(data);                        
                    else if (current_query.Equals(QUERY_3))                    
                        state.query3(data);
                }          

            }

        }
        protected override void demux(SensorData data)
        {
            worker.postMessage(data); 
        }
        protected override void initializeStrategy()
        {
            worker = new WorkerThread<SensorData>(this.threadStart, null);
            worker.Start();
        }
        

    }
}
