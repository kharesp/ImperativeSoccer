using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImperativeSoccer
{
    
    class QueryHelpers
    {
        
        public static SensorData getDefaultSensorDataWithSensorID(int id)
        {
            return new SensorData
            {
                ts = -99999,
                sensor_id = id,
                pos_x = -99999,
                pos_y = -99999,
                pos_z = -99999,
                vel = -99999,
                accel = -99999,
                vel_x = -99999,
                vel_y = -99999,
                vel_z = -99999,
                accel_x = -99999,
                accel_y = -99999,
                accel_z = -99999
            };
        }
        public static SensorData getDefaultSensorData()
        {
            return new SensorData
            {
                ts = -99999,
                sensor_id = Int32.MinValue,
                pos_x = -99999,
                pos_y = -99999,
                pos_z = -99999,
                vel = -99999,
                accel = -99999,
                vel_x = -99999,
                vel_y = -99999,
                vel_z = -99999,
                accel_x = -99999,
                accel_y = -99999,
                accel_z = -99999
            };

        }
        public static PlayerData returnPlayerData(IList<SensorData> values)
        {
            double pos_x = 0, pos_y = 0, pos_z = 0, vel = 0, accel = 0,
                   vel_x = 0, vel_y = 0, vel_z = 0, accel_x = 0,
                   accel_y = 0, accel_z = 0;
            Int64 ts_max = 0;

            string player_name = MetaData.SENSOR_MAP[values.ElementAt(0).sensor_id];
            var count = values.Count;
            //Console.WriteLine("sensor list contains:");
            foreach (var value in values)
            {
                //Console.Write("\t {0},", value.ts);
                if (value.ts == -99999)
                {
                    count--;
                    continue;
                }

                if (value.ts >= ts_max)
                    ts_max = value.ts;

                pos_x += value.pos_x;
                pos_y += value.pos_y;
                pos_z += value.pos_z;
                vel += value.vel;
                accel += value.accel;
                vel_x += value.vel_x;
                vel_y += value.vel_y;
                vel_z += value.vel_z;
                accel_x += value.accel_x;
                accel_y += value.accel_y;
                accel_z += value.accel_z;
            }
            //Console.Write("\n");
            if (count != 0)
            {
                return new PlayerData
                {
                    player_name = player_name,
                    ts = ts_max,
                    pos_x = (pos_x / count),
                    pos_y = (pos_y / count),
                    pos_z = (pos_z / count),
                    vel = (vel / count),
                    accel = (accel / count),
                    vel_x = (vel_x / count),
                    vel_y = (vel_y / count),
                    vel_z = (vel_z / count),
                    accel_x = (accel_x / count),
                    accel_y = (accel_y / count),
                    accel_z = (accel_z / count)
                };
            }
            else
                return new PlayerData
                {
                    player_name = "",
                    ts = ts_max,
                    pos_x = (pos_x),
                    pos_y = (pos_y),
                    pos_z = (pos_z),
                    vel = (vel),
                    accel = (accel),
                    vel_x = (vel_x),
                    vel_y = (vel_y),
                    vel_z = (vel_z),
                    accel_x = (accel_x),
                    accel_y = (accel_y),
                    accel_z = (accel_z)
                };


        }
        public static double returnDistance(double prev_x, double prev_y, double curr_x, double curr_y)
        {
            return Math.Sqrt(Math.Pow(curr_x - prev_x, 2) + Math.Pow(curr_y - prev_y, 2));
        }
        public static Intensity returnRunningIntensity(double speed)
        {
            if (speed <= 1)
                return Intensity.STOP;
            else if (speed > 1 && speed <= 11)
                return Intensity.TROT;
            else if (speed > 11 && speed <= 14)
                return Intensity.LOW;
            else if (speed > 14 && speed <= 17)
                return Intensity.MEDIUM;
            else if (speed > 17 && speed <= 24)
                return Intensity.HIGH;
            else
                return Intensity.SPRINT;
        }      

        public static AggregateRunningData updateAggregateRunningData
           (AggregateRunningData seed, CurrentRunningData curValue,
           IList<CurrentRunningData> expiredList,long count)
        {           
           double standing_time = seed.standing_time,
               standing_dist = seed.standing_distance,
               trot_time = seed.trot_time,
               trot_dist = seed.trot_distance,
               low_time = seed.low_time,
               low_dist = seed.low_distance,
               medium_time = seed.medium_time,
               medium_dist = seed.medium_distance,
               high_time = seed.high_time,
               high_dist = seed.high_distance,
               sprint_time = seed.sprint_time,
               sprint_dist = seed.sprint_distance;        
               

           //for each expired element, remove its values from previous seed's values
           foreach (var element in expiredList)
           {               
               subtractCurrentRunningData(element, ref standing_time, ref standing_dist,
                   ref trot_time, ref trot_dist, ref low_time, ref low_dist, ref medium_time,
                   ref medium_dist, ref high_time, ref high_dist, ref sprint_time, ref sprint_dist);
           }

                     
           //add current values to previous seed values
           addCurrentRunningData(curValue, ref standing_time, ref standing_dist,
               ref trot_time, ref trot_dist, ref low_time, ref low_dist, ref medium_time,
               ref medium_dist, ref high_time, ref high_dist, ref sprint_time, ref sprint_dist);
                         
           
           return new AggregateRunningData
               {
                   player_id = curValue.player_id,
                   ts = curValue.ts_stop,
                   standing_time = standing_time,
                   standing_distance = standing_dist,
                   trot_time = trot_time,
                   trot_distance = trot_dist,
                   low_time = low_time,
                   low_distance = low_dist,
                   medium_time = medium_time,
                   medium_distance = medium_dist,
                   high_time = high_time,
                   high_distance = high_dist,
                   sprint_time = sprint_time,
                   sprint_distance = sprint_dist
               };
       }
        public static CurrentRunningData updateCurrentRunningData(CurrentRunningState state,PlayerData player)
        {
            CurrentRunningData currentRunning = null;
            if (state.prev_ts != -1 && player.ts != -1)
            {
                double avgSpeed = -1;
                //picoseconds 
                var timespan = player.ts - state.prev_ts;
                //distance travelled in mm
                var dist = QueryHelpers.returnDistance(state.prev_pos_x, state.prev_pos_y, player.pos_x, player.pos_y);
                //distnace travelled in km 
                var dist_km = dist / 1000000;
                //Average speed in Km/hr 
                if (timespan != 0)
                    avgSpeed = (dist / timespan) * 3.6 * 1000000000;
                currentRunning = new CurrentRunningData()
                {
                    player_id = player.player_name,
                    ts_start = state.prev_ts,
                    ts_stop = player.ts,
                    distance = dist_km,
                    speed = avgSpeed,
                    intensity = QueryHelpers.returnRunningIntensity(avgSpeed)
                };
            }
            state.prev_ts = player.ts;
            state.prev_pos_x = player.pos_x;
            state.prev_pos_y = player.pos_y;
            return currentRunning;

        }
        

        private static void addCurrentRunningData(CurrentRunningData element,
            ref double standing_time, ref double standing_dist,
            ref double trot_time, ref double trot_dist,
            ref double low_time, ref double low_dist,
            ref double medium_time, ref double medium_dist,
            ref double high_time, ref double high_dist,
            ref double sprint_time, ref double sprint_dist)
        {
            switch (element.intensity)
            {
                case Intensity.STOP:
                    standing_dist += element.distance;
                    standing_time +=
                        (element.ts_stop - element.ts_start) / MetaData.PICO_TO_MILI;
                    break;
                case Intensity.TROT:
                    trot_dist += element.distance;
                    trot_time +=
                        (element.ts_stop - element.ts_start) / MetaData.PICO_TO_MILI;
                    break;
                case Intensity.LOW:
                    low_dist += element.distance;
                    low_time +=
                        (element.ts_stop - element.ts_start) / MetaData.PICO_TO_MILI;
                    break;
                case Intensity.MEDIUM:
                    medium_dist += element.distance;
                    medium_time +=
                        (element.ts_stop - element.ts_start) / MetaData.PICO_TO_MILI;
                    break;
                case Intensity.HIGH:
                    high_dist += element.distance;
                    high_time +=
                        (element.ts_stop - element.ts_start) / MetaData.PICO_TO_MILI;
                    break;
                case Intensity.SPRINT:
                    sprint_dist += element.distance;
                    sprint_time +=
                        (element.ts_stop - element.ts_start) / MetaData.PICO_TO_MILI;
                    break;
                default:
                    throw new ApplicationException("Intensity value not recognized");
            }
        }


        private static void subtractCurrentRunningData(CurrentRunningData element,
             ref double standing_time, ref double standing_dist,
             ref double trot_time, ref double trot_dist,
             ref double low_time, ref double low_dist,
             ref double medium_time, ref double medium_dist,
             ref double high_time, ref double high_dist,
             ref double sprint_time, ref double sprint_dist)
        {
            switch (element.intensity)
            {
                case Intensity.STOP:
                    standing_dist -= element.distance;
                    standing_time -=
                        (element.ts_stop - element.ts_start) / MetaData.PICO_TO_MILI;
                    break;
                case Intensity.TROT:
                    trot_dist -= element.distance;
                    trot_time -=
                        (element.ts_stop - element.ts_start) / MetaData.PICO_TO_MILI;
                    break;
                case Intensity.LOW:
                    low_dist -= element.distance;
                    low_time -=
                        (element.ts_stop - element.ts_start) / MetaData.PICO_TO_MILI;
                    break;
                case Intensity.MEDIUM:
                    medium_dist -= element.distance;
                    medium_time -=
                        (element.ts_stop - element.ts_start) / MetaData.PICO_TO_MILI;
                    break;
                case Intensity.HIGH:
                    high_dist -= element.distance;
                    high_time -=
                        (element.ts_stop - element.ts_start) / MetaData.PICO_TO_MILI;
                    break;
                case Intensity.SPRINT:
                    sprint_dist -= element.distance;
                    sprint_time -=
                        (element.ts_stop - element.ts_start) / MetaData.PICO_TO_MILI;
                    break;
                default:
                    throw new ApplicationException("Intensity value not recognized");
            }

        }

      

    }
    public class CurrentRunningState
    {
        public long prev_ts;
        public double prev_pos_x;
        public double prev_pos_y;
        public CurrentRunningState(long ts, double pos_x, double pos_y) { prev_ts = ts; prev_pos_x = pos_x; prev_pos_y = pos_y; }
    }
    public class HeatmapState
    {
        public IList<double[,]> heatmaps; 
        public double sum;        
        public string player_name;        
        private long flush_ts;
        private long time_window_picosec;
        private TimeInCell seed;
        private LinkedList<TimeInCell> past_data; 
        
        public HeatmapState(string player_name, int time_window_secs = 0)
        {
            time_window_picosec = time_window_secs * MetaData.SECOND_TO_PICO; 
            this.player_name = player_name;
            heatmaps = new List<double[,]> { new double[8, 13], new double[16, 25], new double[32, 50], new double[64, 100] };
            sum = 0;
            seed = new TimeInCell(-1,-1,-1,-1,-1,0);
            past_data = new LinkedList<TimeInCell>(); 
        }
        public IList<HeatMapData> update(PlayerData pData)
        {            
            //correct x and y co-ordinates of playerdata. 
            double x = pData.pos_x < 0 ? 0 : pData.pos_x;
            if (x > MetaData.LEN_X)
                x = MetaData.LEN_X;

            double y = pData.pos_y + MetaData.Y_OFFSET;
            if (y < 0)
                y = y * -1;
            if (y > MetaData.LEN_Y)
                y = MetaData.LEN_Y;

            int x_index = ((int)Math.Floor(y / (MetaData.LEN_Y / 64)));
            if (x_index >= 64)
                x_index = 63;

            int y_index = ((int)Math.Floor(x / (MetaData.LEN_X / 100)));
            if (y_index >= 100)
                y_index = 99;

            TimeInCell new_seed = new TimeInCell(pData.ts, seed.curr_index_x,seed.curr_index_y,x_index,y_index,0);
            if(new_seed.prev_index_x==-1 && new_seed.prev_index_y==-1)
            {
                new_seed.prev_index_x = x_index;
                new_seed.prev_index_y = y_index;
                flush_ts = pData.ts; 
            }
            else
            {
                new_seed.time_in_cell = (pData.ts - seed.ts) / MetaData.PICO_TO_MILI;
            }                
            seed = new_seed; 
            return updateHeatmaps(new_seed);          

        }
        public IList<HeatMapData> updateHeatmaps(TimeInCell update)
        {
            IList<HeatMapData> output_list = new List<HeatMapData>();
            
            //bool toFlush = false; 
            if(time_window_picosec > 0)
            {
                while (true)
                {
                    var item = past_data.First;
                    if (item == null)
                        break;

                    if ((update.ts - item.Value.ts) > time_window_picosec)
                    {
                        for (int i = 0; i < 4; i++)
                        {
                            var heatmap = heatmaps[i];
                            int grid_factor = 1 << (4 - (i + 1));
                            heatmap[item.Value.prev_index_x / grid_factor, item.Value.prev_index_y / grid_factor] 
                                -= item.Value.time_in_cell;
                        }
                        
                        sum -= item.Value.time_in_cell;
                        past_data.RemoveFirst();
                    }
                    else
                        break;
                }
                past_data.AddLast(update);
            }
            
            for (int i = 0; i < 4; i++)
            {
                var heatmap = heatmaps[i];
                int grid_factor = 1 << (4 - (i + 1));
                heatmap[update.prev_index_x / grid_factor, update.prev_index_y / grid_factor] += update.time_in_cell;
            }
            sum += update.time_in_cell; 

            /*if ((update.ts - flush_ts) > MetaData.SECOND_TO_PICO)
            {
                toFlush = true;
                flush_ts = update.ts;
            }*/
            //if (toFlush)
            {
                for (int i = 0; i < 4; i++)
                {
                    HeatMapData data = new HeatMapData(i);
                    data.ts = update.ts;
                    data.player_name = player_name;
                    data.sum = sum;
                    data.gridType = i;
                    data.heatmap = (double[,])heatmaps[i].Clone();
                    output_list.Add(data);
                }

            }
            return output_list;
        }        
    }
    public class HeatMapData
    {
        public Int64 ts;
        public string player_name;
        //gridType is 0 for 8*13, 1 for 16*25, 2 for 32*50 and 3 for 64*100
        public int gridType;

        public double[,] heatmap;
        public double sum;
        public HeatMapData(int grid_type)
        {
            ts = -1; player_name = ""; gridType = grid_type; sum = -1;
            if (grid_type == 0)
                heatmap = new double[8, 13];
            else if (grid_type == 1)
                heatmap = new double[16, 25];
            else if (grid_type == 2)
                heatmap = new double[32, 50];
            else if (grid_type == 3)
                heatmap = new double[64, 100];
            else
                heatmap = new double[64, 100];
        }

    }
    public class TimeInCell
    {
        public long ts; 
        public int prev_index_x;
        public int prev_index_y;
        public int curr_index_x;
        public int curr_index_y; 

        public double time_in_cell;
        public TimeInCell(long ts, int prev_x, int prev_y, int curr_x, int curr_y, double time) 
        {
            this.ts = ts;
            prev_index_x = prev_x;
            prev_index_y = prev_y;
            curr_index_x = curr_x;
            curr_index_y = curr_y; 
            time_in_cell = time;             
        }

    }
}
