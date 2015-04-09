using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;

namespace ImperativeSoccer
{
    class ThreadPoolStrategy<T>: SoccerStrategy
    {
        public ThreadPoolStrategy(string file_name)
            : base(file_name)
        { }
        protected override void demux(SensorData data)
        {
            if(data == null)
            {
                foreach (var player_state in playerId_playerState.Values)
                    ThreadPool.QueueUserWorkItem(new WaitCallback(player_state.signalEnd));
                return; 
            }
            if (MetaData.SENSOR_MAP.ContainsKey(data.sensor_id))
            {
                if (typeof(T) == typeof(SensorData))
                {
                    string player_name = MetaData.SENSOR_MAP[data.sensor_id];
                    PlayerState state = playerId_playerState[player_name];
                    if (current_query.Equals(QUERY_1))
                        ThreadPool.QueueUserWorkItem(new WaitCallback(state.protected_query1_WithSensorData), data);
                    else if (current_query.Equals(QUERY_3))
                        ThreadPool.QueueUserWorkItem(new WaitCallback(state.protected_query3_WithSensorData), data);
                }
                else if (typeof(T) == typeof(PlayerData))
                {
                    string player_name = MetaData.SENSOR_MAP[data.sensor_id];
                    PlayerState state = playerId_playerState[player_name];
                    PlayerData player_data = state.getPlayerData(data);
                    if (current_query.Equals(QUERY_1))
                        ThreadPool.QueueUserWorkItem(new WaitCallback(state.protected_query1_WithPlayerData), player_data);
                    else if (current_query.Equals(QUERY_3))
                        ThreadPool.QueueUserWorkItem(new WaitCallback(state.protected_query3_WithPlayerData), player_data);
                }
            }           
            
        }
        protected override void initializeStrategy()
        {
            //no initialization required for threadpool strategy 
        }
    }  

}
