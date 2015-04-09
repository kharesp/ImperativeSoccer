using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Collections.ObjectModel;

namespace ImperativeSoccer
{ 
    class NewThreadStrategySensorData: SoccerStrategy
    {        
        private Dictionary<string, WorkerThread<SensorData>> playerId_playerThread;
        private ReadOnlyDictionary<string, WorkerThread<SensorData>> playerId_playerThread_ro; 

        public NewThreadStrategySensorData(string output_file):base(output_file)
        {          
            playerId_playerThread = new Dictionary<string, WorkerThread<SensorData>>();
           
        }
        private void createPlayerThreads(WorkerThreadStart<SensorData> threadStart)
        {
            foreach(var player in playerId_playerState)
            {
                WorkerThread<SensorData> worker = new WorkerThread<SensorData>(threadStart, player.Value);
                playerId_playerThread.Add(player.Key, worker);
                worker.Start();
            }
            playerId_playerThread_ro = new ReadOnlyDictionary<string, WorkerThread<SensorData>>(playerId_playerThread);
        }
        private void query1ThreadStart(WorkerThread<SensorData> thread,object state)
        {
            PlayerState player_state = (PlayerState)state;
            while(true)
            {
                SensorData msg = thread.extractMessage();               
                if(msg==null)
                {
                    player_state.signalEnd(null);
                    break;
                }
                player_state.query1(msg);
            }
        }
        private void query3ThreadStart(WorkerThread<SensorData> thread, object state)
        {
            PlayerState player_state = (PlayerState)state;
            while (true)
            {
                SensorData msg = thread.extractMessage();               
                if(msg==null)
                {
                    player_state.signalEnd(null);
                    break;
                }
                player_state.query3(msg);
            }
        }

        protected override void demux(SensorData data)
        {          
            if(data==null)
            {
                foreach (var player_thread in playerId_playerThread_ro.Values)
                    player_thread.postMessage(data);
                return;
            }
            if (MetaData.SENSOR_MAP.ContainsKey(data.sensor_id))
            {
                var player = MetaData.SENSOR_MAP[data.sensor_id];
                playerId_playerThread_ro[player].postMessage(data);
            }  
        }
        protected override void initializeStrategy()
        {
            if (current_query.Equals(QUERY_1))
                createPlayerThreads(query1ThreadStart);
            else if (current_query.Equals(QUERY_3))
                createPlayerThreads(query3ThreadStart);            
        }        
        
    }   
    
    
    class NewThreadStrategyPlayerData: SoccerStrategy
    {
        private Dictionary<string, WorkerThread<PlayerData>> playerId_playerThread;
        public NewThreadStrategyPlayerData(string file_name):base(file_name)
        {
            playerId_playerThread = new Dictionary<string, WorkerThread<PlayerData>>();
        }
       
        private void createPlayerThreads(WorkerThreadStart<PlayerData> threadStart)
        {
            foreach(var player in playerId_playerState)
            {
                WorkerThread<PlayerData> worker = 
                    new WorkerThread<PlayerData>(threadStart, player.Value);
                playerId_playerThread.Add(player.Key, worker);
                worker.Start();
            }
        }
        private void query1ThreadStart(WorkerThread<PlayerData> thread, object state)
        {
            PlayerState player_state = (PlayerState)state;            
            while(true)
            {
                PlayerData player_data=thread.extractMessage();
                if(player_data == null)
                {
                    player_state.signalEnd(null);
                    return;
                }                
                player_state.query1(player_data);
            }
        }
        private void query3ThreadStart(WorkerThread<PlayerData> thread, object state)
        {
            PlayerState player_state = (PlayerState)state;
            while (true)
            {
                PlayerData player_data = thread.extractMessage();
                if (player_data == null)
                {
                    player_state.signalEnd(null);
                    return;
                }
                player_state.query3(player_data);
            }
        }
        protected override void demux(SensorData data)
        {
            
            if(data ==null)
            {
                foreach (var player_thread in playerId_playerThread.Values)
                    player_thread.postMessage(null);
                return;
            }
            if (MetaData.SENSOR_MAP.ContainsKey(data.sensor_id))
            {
                string player = MetaData.SENSOR_MAP[data.sensor_id];
                PlayerData player_data=playerId_playerState[player].getPlayerData(data);
                playerId_playerThread[player_data.player_name].postMessage(player_data);
            } 
        }
        protected override void initializeStrategy()
        {
            if (current_query.Equals(QUERY_1))
                createPlayerThreads(query1ThreadStart);
            else if (current_query.Equals(QUERY_3))
                createPlayerThreads(query3ThreadStart);
            
        }
       

    }

    
   
   
}
