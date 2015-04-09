using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace ImperativeSoccer
{
    class SingleThreadStrategy: SoccerStrategy
    {   
        public SingleThreadStrategy(string output_file) : base(output_file) { }
        
        protected override void initializeStrategy()
        {
            //no initialization required for single thread strategy. 
        }
        protected override void demux(SensorData data)
        {           
            if(data==null)
            {
                foreach (var player_state in playerId_playerState.Values)
                    player_state.signalEnd(null);
                return; 
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
}
