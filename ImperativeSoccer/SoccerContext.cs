using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace ImperativeSoccer
{
    class SoccerContext
    {
        private const int BUFFER_SIZE = 2 << 16;
        private const bool MY_PARSE = true;
        private const string SENSOR_DATA_FILE = @"E:\simulated_data\full-game";
        private StringBuilder sb = new StringBuilder();      
        
        public void readSensorDataDDSListener(Demultiplexer<SensorData> distributor)
        {
            DDS.DomainParticipant participant = DefaultParticipant.Instance;
            DefaultParticipant.RegisterType<SensorData, SensorDataTypeSupport>();
            ListenerSubscriber<SensorData> subscriber = 
                new ListenerSubscriber<SensorData>(participant, "Raw SensorData",null,distributor);
            subscriber.subscribe();
            /*DDS.DomainParticipant participant = DefaultParticipant.Instance;
            DefaultParticipant.RegisterType<SensorData, SensorDataTypeSupport>();
            DDS.Topic topic = participant.create_topic("Raw SensorData", typeof(SensorData).ToString(),
                  DDS.DomainParticipant.TOPIC_QOS_DEFAULT, null, DDS.StatusMask.STATUS_MASK_NONE);
            if (topic == null)
            {
                throw new ApplicationException("create_topic error");
            }
            ListenerSubscriber2<SensorData> subscriber_A = 
                new ListenerSubscriber2<SensorData>(participant, topic, distributor, QueryHelpers.getDefaultSensorData);
            subscriber_A.setContentFilter("filtered_topic_A",
                    "(sensor_id >= %0 and sensor_id <= %1) OR (sensor_id >= %2 and sensor_id <= %3) OR (sensor_id >= %4 and sensor_id <= %5)",
                    new DDS.StringWrapper[] { "10", "28", "47", "59", "88", "98" });
            var readerQoS_A = subscriber_A.getDataReaderQos();

            DDS.TransportUnicastSettings_t[] arr_A = new DDS.TransportUnicastSettings_t[1];
            arr_A[0] = new DDS.TransportUnicastSettings_t();
            readerQoS_A.unicast.value.loan(arr_A, 1);
            readerQoS_A.unicast.value.ensure_length(1, 1);
            readerQoS_A.unicast.value.get_at(0).receive_port = 9000;

            var subscriber_B = new ListenerSubscriber2<SensorData>(participant, topic, distributor, QueryHelpers.getDefaultSensorData);
            subscriber_B.setContentFilter("filtered_topic_B",
                "(sensor_id = %0) OR (sensor_id = %1) OR (sensor_id >= %2 and sensor_id <= %3) OR (sensor_id >= %4 and sensor_id <= %5) OR (sensor_id >= %6 and sensor_id <= %7)",
                new DDS.StringWrapper[] { "4", "8", "38", "44", "61", "75", "99", "106" });
            var readerQoS_B = subscriber_B.getDataReaderQos();

            DDS.TransportUnicastSettings_t[] arr_B = new DDS.TransportUnicastSettings_t[1];
            arr_B[0] = new DDS.TransportUnicastSettings_t();
            readerQoS_B.unicast.value.loan(arr_B, 1);
            readerQoS_B.unicast.value.ensure_length(1, 1);
            readerQoS_B.unicast.value.get_at(0).receive_port = 8000;  
            
            subscriber_A.subscribe();
            subscriber_B.subscribe();*/


        }
        public void readSensorDataDDSWaitset(Demultiplexer<SensorData> distributor)
        {
            DDS.Duration_t timeout;
            timeout.nanosec = 0;
            timeout.sec = 10; 
            DDS.DomainParticipant participant = DefaultParticipant.Instance;
            DefaultParticipant.RegisterType<SensorData, SensorDataTypeSupport>();
            WaitsetSubscriber<SensorData> subscriber =
                new WaitsetSubscriber<SensorData>(participant, "Raw SensorData", null,timeout,distributor);
            subscriber.subscribe();

        }
        public void readSensorDataSync(Demultiplexer<SensorData> distributor)
        {
            var read = -1;
            byte[] buffer = new byte[BUFFER_SIZE];
            Array.Clear(buffer, 0, BUFFER_SIZE);
            using (FileStream fs = new FileStream(SENSOR_DATA_FILE, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE))
            {
                while (read != 0)
                {
                    read = fs.Read(buffer, 0, BUFFER_SIZE);
                    
                    for (var i = 0; i < read; i++)
                    {
                        var c = (char)buffer[i];

                        sb.Append(c);

                        if (c == '\n')
                        {
                            SensorData data = produceSensorData();
                            if (data != null)
                                distributor(data);
                        }

                    }
                }
                Console.WriteLine("finished reading file.Sending end signal");
                distributor(null);
            }      
        } 
        public async Task readSensorDataAsync(Demultiplexer<SensorData> distributor)
        {            
            byte[] buffer = new byte[BUFFER_SIZE];
            Array.Clear(buffer, 0, BUFFER_SIZE);
            using (FileStream fs = new FileStream(SENSOR_DATA_FILE, FileMode.Open, FileAccess.Read, FileShare.Read, BUFFER_SIZE, true))
            {
                var read = -1;
                while (read != 0)
                {
                    read = await fs.ReadAsync(buffer, 0, BUFFER_SIZE);
                    //Console.WriteLine("read {0} bytes of data", read);
                    for (var i = 0; i < read; i++)
                    {
                        var c = (char)buffer[i];

                        sb.Append(c);

                        if (c == '\n')
                        {
                            SensorData data = produceSensorData();
                            if (data != null)
                                distributor(data);
                        }

                    }
                }
                Console.WriteLine("finished reading file.Sending end signal");
                distributor(null);
            }           

        }
        public Thread readSensorDataNewThread(Demultiplexer<SensorData> distributor)
        {
            Thread producer = new Thread(readSensorDataThreadStart);
            producer.Start(distributor);
            return producer;
        }
        private void readSensorDataThreadStart(object obj)
        {
            Demultiplexer<SensorData> distributor = (Demultiplexer<SensorData>)obj;
            readSensorDataSync(distributor);
        }

        private SensorData produceSensorData()
        {
           
            var line = sb.ToString();
            sb.Clear();

            if (String.IsNullOrWhiteSpace(line))
            {
                Console.WriteLine("empty string!");
                return null;
            }


            var instance = new SensorData();

            if (MY_PARSE)
            {
                int pos = 0;
                instance.sensor_id = myParseInt32(line, ref pos);
                
                instance.ts = myParseLong(line, ref pos);
                instance.pos_x = myParseInt32(line, ref pos);
                instance.pos_y = myParseInt32(line, ref pos);
                instance.pos_z = myParseInt32(line, ref pos);
                instance.vel = myParseInt32(line, ref pos);
                instance.accel = myParseInt32(line, ref pos);
                instance.vel_x = myParseInt32(line, ref pos);
                instance.vel_y = myParseInt32(line, ref pos);
                instance.vel_z = myParseInt32(line, ref pos);
                instance.accel_x = myParseInt32(line, ref pos);
                instance.accel_y = myParseInt32(line, ref pos);
                instance.accel_z = myParseInt32(line, ref pos);
                
                
            }
            else
            {
                String[] fields = line.Split(',');
                instance.sensor_id = Int32.Parse(fields[0]);
                instance.ts = Int64.Parse(fields[1]);
                instance.pos_x = Int32.Parse(fields[2]);
                instance.pos_y = Int32.Parse(fields[3]);
                instance.pos_z = Int32.Parse(fields[4]);
                instance.vel = Int32.Parse(fields[5]);
                instance.accel = Int32.Parse(fields[6]);
                instance.vel_x = Int32.Parse(fields[7]);
                instance.vel_y = Int32.Parse(fields[8]);
                instance.vel_z = Int32.Parse(fields[9]);
                instance.accel_x = Int32.Parse(fields[10]);
                instance.accel_y = Int32.Parse(fields[11]);
                instance.accel_z = Int32.Parse(fields[12]);
                
            }
            return instance;

        }
        private long myParseLong(string line, ref int pos)
        {
            int length = line.Length;
            long num = 0;
            bool neg = false;
            int i = pos;
            for (; i < length; ++i)
            {

                if (line[i] == ' ')
                    continue;
                else if (line[i] == ',')
                    break;
                else if (line[i] == '-')
                    neg = true;
                else
                {
                    num = num * 10 + ((int)line[i]) - 48;

                }
            }

            pos = ++i;
            if (neg)
                return -num;
            else
                return num;
        }
        private int myParseInt32(string line, ref int pos)
        {
            int length = line.Length;
            int num = 0;
            bool neg = false;
            int i = pos;


            for (; i < length; ++i)
            {
                if (line[i] == ' ')
                    continue;
                else if (line[i] == ',')
                    break;
                else if (line[i] == '-')
                    neg = true;
                else
                {
                    num = num * 10 + ((int)line[i]) - 48;

                }
            }

            pos = ++i;
            if (neg)
                return -num;
            else
                return num;
        }
       

    }
    public class PlayerState
    {
        private object state_lock;
        public ManualResetEvent signal;

        public PerformanceTest pt;
        public string player_name;
        public Dictionary<int, SensorData> sensorId_sensorData;

        //Query-1 state 
        public CurrentRunningState currentRunningState;
        public AggregateRunningData aggRunning_full;
        public TimeWindow<CurrentRunningData, AggregateRunningData> aggRunning_1min;
        public TimeWindow<CurrentRunningData, AggregateRunningData> aggRunning_5min;
        public TimeWindow<CurrentRunningData, AggregateRunningData> aggRunning_20min;


        //Query-3 state 
        public HeatmapState heatmap_full;
        public HeatmapState heatmap_1min;
        public HeatmapState heatmap_5min;
        public HeatmapState heatmap_10min; 


        public PlayerState(String player_name)
        {
            this.player_name = player_name;
            signal = new ManualResetEvent(false);
            state_lock = new object();
            pt = new PerformanceTest(player_name);
            sensorId_sensorData = new Dictionary<int, SensorData>();
            foreach (var sensor_id in MetaData.PLAYER_MAP[player_name])
            {
                sensorId_sensorData.Add(sensor_id, QueryHelpers.getDefaultSensorDataWithSensorID(sensor_id));
            }
            currentRunningState = new CurrentRunningState(-1, -1, -1);
            aggRunning_full = new AggregateRunningData();
            aggRunning_1min = new TimeWindow<CurrentRunningData, AggregateRunningData>
                (60 * MetaData.SECOND_TO_PICO, "ts_start", new AggregateRunningData(), QueryHelpers.updateAggregateRunningData);
            aggRunning_5min = new TimeWindow<CurrentRunningData, AggregateRunningData>
                (5 * 60 * MetaData.SECOND_TO_PICO, "ts_start", new AggregateRunningData(), QueryHelpers.updateAggregateRunningData);
            aggRunning_20min = new TimeWindow<CurrentRunningData, AggregateRunningData>
                (20 * 60 * MetaData.SECOND_TO_PICO, "ts_start", new AggregateRunningData(), QueryHelpers.updateAggregateRunningData);

            heatmap_full = new HeatmapState(player_name);
            heatmap_1min = new HeatmapState(player_name, 1 * 60);
            heatmap_5min = new HeatmapState(player_name, 5 * 60);
            heatmap_10min = new HeatmapState(player_name, 10 * 60); 

        }

        public void signalEnd(object _)
        {
            Console.WriteLine("{0} received end signal.", player_name);
            pt.postProcess();
            signal.Set();
        }
        public void incrementInputCount()
        {
            if (pt.InputCount == 0)
                pt.startSW();
            pt.InputCount++;
        }
        public PlayerData getPlayerData(SensorData msg)
        {           
            sensorId_sensorData[msg.sensor_id] = msg;
            return QueryHelpers.returnPlayerData(sensorId_sensorData.Values.ToList());
        }
        public void protected_query1_WithSensorData(object msg)
        {
            lock (state_lock)
            {
                query1((SensorData)msg);
            }
        }
        public void protected_query1_WithPlayerData(object msg)
        {
            lock (state_lock)
            {
                query1((PlayerData)msg);
            }

        }
        public void query1(SensorData msg)
        {
            var player = getPlayerData(msg);
            query1(player);
        }
        public void query1(PlayerData player)
        {
            incrementInputCount(); 
            CurrentRunningData currentRunning = QueryHelpers.updateCurrentRunningData(currentRunningState, player);
            if (currentRunning != null)
            {
                aggRunning_full = QueryHelpers.updateAggregateRunningData(aggRunning_full, currentRunning, new List<CurrentRunningData> { }, 0);
                pt.OutputCount++;
                aggRunning_1min.getUpdate(currentRunning);
                pt.OutputCount++;
                aggRunning_5min.getUpdate(currentRunning);
                pt.OutputCount++;
                aggRunning_20min.getUpdate(currentRunning);
                pt.OutputCount++;
            }
        }

        public void query3(SensorData msg)
        {
            var player = getPlayerData(msg);
            query3(player);
        }
        public void query3(PlayerData player)
        {
            incrementInputCount();
            var list=heatmap_full.update(player);
            pt.OutputCount += list.Count; 
            list=heatmap_1min.update(player);
            pt.OutputCount += list.Count; 
            list=heatmap_5min.update(player);
            pt.OutputCount += list.Count; 
            list=heatmap_10min.update(player);
            pt.OutputCount += list.Count; 
 
        }       
        public void protected_query3_WithSensorData(object msg)
        {
            lock (state_lock)
            {
                query3((SensorData)msg);
            }
        }
        public void protected_query3_WithPlayerData(object msg)
        {
            lock (state_lock)
            {
                query3((PlayerData)msg);
            }
        }


    }
    public class DefaultParticipant
    {
        public static int DomainId
        {
            get { return domainId; }
            set { domainId = value; }
        }

        public static DDS.DomainParticipant Instance
        {
            get
            {
                if (participant == null)
                {
                    //NDDS.ConfigLogger.get_instance().set_verbosity_by_category(NDDS.LogCategory.NDDS_CONFIG_LOG_CATEGORY_API,
                    //    NDDS.LogVerbosity.NDDS_CONFIG_LOG_VERBOSITY_STATUS_ALL);
                    participant =
                        DDS.DomainParticipantFactory.get_instance().create_participant(
                            domainId,
                            DDS.DomainParticipantFactory.PARTICIPANT_QOS_DEFAULT,
                            null /* listener */,
                            DDS.StatusMask.STATUS_MASK_NONE);
                    if (participant == null)
                    {
                        throw new ApplicationException("create_participant error");
                    }
                }

                return participant;
            }
        }

        public static void Shutdown()
        {
            if (Instance != null)
            {
                Instance.delete_contained_entities();
                DDS.DomainParticipantFactory.get_instance().delete_participant(
                    ref participant);
            }
        }

        public static void RegisterType<Type, TypeSupportClass>()
        {
            typeof(TypeSupportClass)
              .GetMethod("register_type",
                         System.Reflection.BindingFlags.Public |
                         System.Reflection.BindingFlags.Static)
             .Invoke(null, new Object[] { Instance, typeof(Type).ToString() });
        }

        public static DDS.TypedDataWriter<T> CreateDataWriter<T>(string topicName)
        {
            return CreateDataWriter<T>(topicName, typeof(T).ToString());
        }
        public static DDS.TypedDataWriter<T> CreateDataWriter<T>(string topicName,
                                                                 string typeName)
        {
            DDS.DomainParticipant participant = Instance;


            DDS.Publisher publisher = participant.create_publisher(
                DDS.DomainParticipant.PUBLISHER_QOS_DEFAULT,
                null /* listener */,
                DDS.StatusMask.STATUS_MASK_NONE);

            if (publisher == null)
            {
                throw new ApplicationException("create_publisher error");
            }

            DDS.Topic topic = participant.create_topic(
                topicName,
                typeName,
                DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
                null /* listener */,
                DDS.StatusMask.STATUS_MASK_NONE);

            if (topic == null)
            {
                throw new ApplicationException("create_topic error");
            }
            
            /* DDS.DataWriterQos dw_qos = new DDS.DataWriterQos();
            participant.get_default_datawriter_qos(dw_qos);
            dw_qos.reliability.kind = DDS.ReliabilityQosPolicyKind.RELIABLE_RELIABILITY_QOS;
            dw_qos.history.kind = DDS.HistoryQosPolicyKind.KEEP_ALL_HISTORY_QOS;*/
            DDS.DataWriterQos dw_qos = new DDS.DataWriterQos();
            participant.get_default_datawriter_qos(dw_qos);
            

            DDS.DataWriter writer = publisher.create_datawriter(
                topic,
                DDS.Publisher.DATAWRITER_QOS_DEFAULT,
                null /* listener */,
                DDS.StatusMask.STATUS_MASK_NONE);
            if (writer == null)
            {
                throw new ApplicationException("create_datawriter error");
            }

            return (DDS.TypedDataWriter<T>)writer;
        }

        private static DDS.DomainParticipant participant;
        private static int domainId = 0;
    }
    class DataReaderListenerAdapter : DDS.DataReaderListener
    {
        public override void on_requested_deadline_missed(
         DDS.DataReader reader,
         ref DDS.RequestedDeadlineMissedStatus status)
        {
            Console.WriteLine("Requested deadline missed {0} total_count.", status.total_count);
        }

        public override void on_requested_incompatible_qos(
            DDS.DataReader reader,
            DDS.RequestedIncompatibleQosStatus status)
        {
            Console.WriteLine("Requested incompatible qos {0} total_count.", status.total_count);
        }

        public override void on_sample_rejected(
          DDS.DataReader reader,
          ref DDS.SampleRejectedStatus status)
        {
            Console.WriteLine("Sample Rejected. Reason={0}", status.last_reason.ToString());
        }

        public override void on_liveliness_changed(
            DDS.DataReader reader,
            ref DDS.LivelinessChangedStatus status)
        {
            Console.WriteLine("Liveliness changed. {0} now alive.", status.alive_count);
        }

        public override void on_sample_lost(
            DDS.DataReader reader,
            ref DDS.SampleLostStatus status)
        {
            Console.WriteLine("Sample lost. Reason={0}", status.last_reason.ToString());
        }

        public override void on_subscription_matched(
            DDS.DataReader reader,
            ref DDS.SubscriptionMatchedStatus status)
        {
            Console.WriteLine("Subscription changed. {0} current count.", status.current_count);
        }

        public override void on_data_available(
            DDS.DataReader reader) { }
    }

    class ListenerSubscriber2<T> where T : class, DDS.ICopyable<T>, new()
    {
        private DDS.DomainParticipant participant;
        private DDS.Topic topic; 
        private DataReaderListener listener;
        private Demultiplexer<T> demultiplexer;        
        private DDS.DataReaderQos readerQoS;
        private DDS.ContentFilteredTopic cftTopic;
        private DDS.Subscriber subscriber; 

        public ListenerSubscriber2(DDS.DomainParticipant participant, DDS.Topic topic,
                              Demultiplexer<T> demux)
        {            
            this.topic=topic; 
            this.participant = participant;            
            demultiplexer = demux;            
            initialize();
        }
        private void initialize()
        {
             subscriber = participant.create_subscriber( DDS.DomainParticipant.SUBSCRIBER_QOS_DEFAULT,
             null /* listener */,
             DDS.StatusMask.STATUS_MASK_NONE);
            if (subscriber == null)
            {
                throw new ApplicationException("create_subscriber error");
            }  
            readerQoS = new DDS.DataReaderQos();
            try
            {
                subscriber.get_default_datareader_qos(readerQoS);
            }
            catch (DDS.Exception e)
            {
                Console.WriteLine("get_default_datareader_qos error {0}", e);
                throw e;
            }

           
        }
        public void subscribe()
        {
            listener = new DataReaderListener(demultiplexer);
            DDS.DataReader reader = null;
            if(cftTopic !=null)
            {
               reader = subscriber.create_datareader(cftTopic,
                    DDS.Subscriber.DATAREADER_QOS_DEFAULT,listener,DDS.StatusMask.STATUS_MASK_ALL);

            }
            else
            {
                reader = subscriber.create_datareader(topic,
                    DDS.Subscriber.DATAREADER_QOS_DEFAULT, listener, DDS.StatusMask.STATUS_MASK_ALL);

            }
           
            if (reader == null)
            {
                listener = null;
                throw new ApplicationException("create_datareader error");
            }


        }

        public void setContentFilter(string cft_name, string expression, DDS.StringWrapper[] param_list)
        {
            DDS.StringSeq parameters = new DDS.StringSeq(param_list.Length);
            parameters.from_array(param_list);
            cftTopic = participant.create_contentfilteredtopic(cft_name, topic, expression, parameters);
            if (cftTopic == null)
            {
                throw new ApplicationException(
                    "create_contentfilteredtopic error");
            }

        }
        public DDS.DataReaderQos getDataReaderQos()
        {
            return readerQoS;
        }
        private class DataReaderListener : DataReaderListenerAdapter
        {
            public DataReaderListener(Demultiplexer<T> demux)
            {
                demultiplexer = demux;                
                dataSeq = new DDS.UserRefSequence<T>();
                infoSeq = new DDS.SampleInfoSeq();
            }
            public override void on_liveliness_changed(DDS.DataReader reader, ref DDS.LivelinessChangedStatus status)
            {
                base.on_liveliness_changed(reader, ref status);
                if (status.alive_count == 0)
                {
                    Console.WriteLine("SENDING END SIGNAL");
                    demultiplexer(null);
                }
            }
            public override void on_data_available(DDS.DataReader reader)
            {
                try
                {
                    DDS.TypedDataReader<T> dataReader = (DDS.TypedDataReader<T>)reader;

                    dataReader.take(
                        dataSeq,
                        infoSeq,
                        DDS.ResourceLimitsQosPolicy.LENGTH_UNLIMITED,
                        DDS.SampleStateKind.ANY_SAMPLE_STATE,
                        DDS.ViewStateKind.ANY_VIEW_STATE,
                        DDS.InstanceStateKind.ANY_INSTANCE_STATE);

                    System.Int32 dataLength = dataSeq.length;

                    for (int i = 0; i < dataLength; ++i)
                    {
                        if (infoSeq.get_at(i).valid_data)
                        {
                            T temp = new T();
                            temp.copy_from(dataSeq.get_at(i));
                            //Console.WriteLine("on_receive_callback called");
                            demultiplexer(temp);
                        }
                       
                    }

                    dataReader.return_loan(dataSeq, infoSeq);
                }
                catch (DDS.Retcode_NoData)
                {
                    Console.WriteLine("SENDING END SIGNAL: RETCODE_NODATA");
                    demultiplexer(null);
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("DDS Subscriber: take error {0}", ex);
                }
            }
            private Demultiplexer<T> demultiplexer;           
            private DDS.UserRefSequence<T> dataSeq;
            private DDS.SampleInfoSeq infoSeq;            

        }

    }
    
    
    class ListenerSubscriber<T> where T:class, DDS.ICopyable<T>, new()
    {
        private DDS.DomainParticipant participant;
        private string topicName;
        private string typeName;
        private DataReaderListener listener;
        private Demultiplexer<T> demultiplexer;
       
 
        public ListenerSubscriber(DDS.DomainParticipant participant,
                             string topicName,
                             string typeName,Demultiplexer<T> demux)      
        {
          if (typeName == null)
              this.typeName = typeof(T).ToString();
          else
              this.typeName = typeName;

          this.participant = participant;
          this.topicName = topicName;
          demultiplexer = demux;
         
        }
        public void subscribe()
        {
            DDS.Subscriber subscriber = participant.create_subscriber(
             DDS.DomainParticipant.SUBSCRIBER_QOS_DEFAULT,
             null /* listener */,
             DDS.StatusMask.STATUS_MASK_NONE);
            if (subscriber == null)
            {
                throw new ApplicationException("create_subscriber error");
            }

            DDS.Topic topic = participant.create_topic(
                topicName,
                typeName,
                DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
                null /* listener */,
                DDS.StatusMask.STATUS_MASK_NONE);
            if (topic == null)
            {
                throw new ApplicationException("create_topic error");
            }

            listener = new DataReaderListener(demultiplexer);

            
            DDS.DataReader reader = subscriber.create_datareader(
                topic,
                DDS.Subscriber.DATAREADER_QOS_DEFAULT,
                listener,
                DDS.StatusMask.STATUS_MASK_ALL);

            if (reader == null)
            {
                listener = null;
                throw new ApplicationException("create_datareader error");
            }

        }
        private class DataReaderListener : DataReaderListenerAdapter
        {
            public DataReaderListener(Demultiplexer<T> demux)
            {
                demultiplexer = demux;                
                dataSeq = new DDS.UserRefSequence<T>();
                infoSeq = new DDS.SampleInfoSeq();
            }
            public override void on_liveliness_changed(DDS.DataReader reader, ref DDS.LivelinessChangedStatus status)
            {
                base.on_liveliness_changed(reader, ref status);
                if(status.alive_count==0)
                {
                    Console.WriteLine("SENDING END SIGNAL");                    
                    demultiplexer(null);
                }
            }
            public override void on_data_available(DDS.DataReader reader)
            {
                try
                {
                    DDS.TypedDataReader<T> dataReader = (DDS.TypedDataReader<T>)reader;

                    dataReader.take(
                        dataSeq,
                        infoSeq,
                        DDS.ResourceLimitsQosPolicy.LENGTH_UNLIMITED,
                        DDS.SampleStateKind.ANY_SAMPLE_STATE,
                        DDS.ViewStateKind.ANY_VIEW_STATE,
                        DDS.InstanceStateKind.ANY_INSTANCE_STATE);

                    System.Int32 dataLength = dataSeq.length;

                    for (int i = 0; i < dataLength; ++i)
                    {
                        if (infoSeq.get_at(i).valid_data)
                        {
                            T temp = new T();
                            temp.copy_from(dataSeq.get_at(i));                            
                            demultiplexer(temp);
                        }
                      
                    }

                    dataReader.return_loan(dataSeq, infoSeq);
                }
                catch (DDS.Retcode_NoData)
                {
                    Console.WriteLine("SENDING END SIGNAL: RETCODE_NODATA");                    
                    demultiplexer(null);
                    return;
                }
                catch (Exception ex)
                {                   
                    Console.WriteLine("DDS Subscriber: take error {0}", ex);
                }
            }
            private Demultiplexer<T> demultiplexer;            
            private DDS.UserRefSequence<T> dataSeq;
            private DDS.SampleInfoSeq infoSeq;
            
        }

    }
    class WaitsetSubscriber<T> where T:class, DDS.ICopyable<T>,new ()
    {
        private DDS.DomainParticipant participant;
        private string topicName;
        private string typeName;
        private DDS.Duration_t timeout;
        private DDS.DataReader reader;
        private DDS.StatusCondition status_condition;
        private DDS.WaitSet waitset;
        private DDS.UserRefSequence<T> dataSeq = new DDS.UserRefSequence<T>();
        private DDS.SampleInfoSeq infoSeq = new DDS.SampleInfoSeq();
        
        Demultiplexer<T> demultiplexer; 

        public WaitsetSubscriber(DDS.DomainParticipant participant, string topicName,string typeName,DDS.Duration_t timeout,Demultiplexer<T> demux)
        {
            this.participant = participant;
            this.topicName = topicName;
            if (typeName == null)
                this.typeName = typeof(T).ToString();
            else
                this.typeName = typeName;
            this.timeout = timeout;
            this.demultiplexer = demux;
           
        }
        private void initialize()
        {
            DDS.Subscriber subscriber = participant.create_subscriber(
              DDS.DomainParticipant.SUBSCRIBER_QOS_DEFAULT,
              null /* listener */,
              DDS.StatusMask.STATUS_MASK_NONE);
            if (subscriber == null)
            {
                throw new ApplicationException("create_subscriber error");
            }

            DDS.Topic topic = participant.create_topic(
            topicName,
            typeName,
            DDS.DomainParticipant.TOPIC_QOS_DEFAULT,
            null /* listener */,
            DDS.StatusMask.STATUS_MASK_NONE);
            if (topic == null)
            {
                throw new ApplicationException("create_topic error");
            }
            
            reader = subscriber.create_datareader(
             topic,
             DDS.Subscriber.DATAREADER_QOS_DEFAULT,
             null,
             DDS.StatusMask.STATUS_MASK_ALL);

            if (reader == null)
            {
                throw new ApplicationException("create_datareader error");
            }
            status_condition = reader.get_statuscondition();

            try
            {
                int mask =
                    (int)DDS.StatusKind.DATA_AVAILABLE_STATUS |
                    (int)DDS.StatusKind.SUBSCRIPTION_MATCHED_STATUS |
                    (int)DDS.StatusKind.LIVELINESS_CHANGED_STATUS |
                    (int)DDS.StatusKind.SAMPLE_LOST_STATUS |
                    (int)DDS.StatusKind.SAMPLE_REJECTED_STATUS;

                status_condition.set_enabled_statuses((DDS.StatusMask)mask);
            }
            catch (DDS.Exception e)
            {
                throw new ApplicationException("set_enabled_statuses error {0}", e);
            }

            waitset = new DDS.WaitSet();

            try
            {
                waitset.attach_condition(status_condition);
            }
            catch (DDS.Exception e)
            {
                throw new ApplicationException("attach_condition error {0}", e);
            }

        }
        private void receiveData()
        {
            int count = 0;
            DDS.ConditionSeq active_conditions = new DDS.ConditionSeq();
            while (true)
            {
                try
                {
                    waitset.wait(active_conditions, timeout);                    
                    for (int c = 0; c < active_conditions.length; ++c)
                    {
                        
                        if (active_conditions.get_at(c) == status_condition)
                        {
                            DDS.StatusMask triggeredmask =
                                reader.get_status_changes();

                            if ((triggeredmask &
                                (DDS.StatusMask)
                                 DDS.StatusKind.DATA_AVAILABLE_STATUS) != 0)
                            {
                                try
                                {
                                    DDS.TypedDataReader<T> dataReader
                                        = (DDS.TypedDataReader<T>)reader;

                                    dataReader.take(
                                        dataSeq,
                                        infoSeq,
                                        DDS.ResourceLimitsQosPolicy.LENGTH_UNLIMITED,
                                        DDS.SampleStateKind.ANY_SAMPLE_STATE,
                                        DDS.ViewStateKind.ANY_VIEW_STATE,
                                        DDS.InstanceStateKind.ANY_INSTANCE_STATE);

                                    System.Int32 dataLength = dataSeq.length;                                   
                                    for (int i = 0; i < dataLength; ++i)
                                    {
                                        if (infoSeq.get_at(i).valid_data)
                                        {
                                            T temp = new T();
                                            temp.copy_from(dataSeq.get_at(i));
                                            demultiplexer(temp);
                                        }
                                        else if (infoSeq.get_at(i).instance_state ==
                                                  DDS.InstanceStateKind.NOT_ALIVE_DISPOSED_INSTANCE_STATE)
                                        {
                                            Console.WriteLine("DDS INSTANCE NOT_ALIVE_DISPOSED_INSTANCE_STATE");
                                            demultiplexer(null);
                                            
                                        }
                                    }

                                    dataReader.return_loan(dataSeq, infoSeq);
                                }
                                catch (DDS.Retcode_NoData)
                                {
                                    Console.WriteLine("RETCODE_NODATA");
                                    demultiplexer(null);
                                    return;
                                }
                                catch (Exception ex)
                                {                                    
                                    Console.WriteLine("WaitsetSubscriber: take error {0}", ex);
                                }
                            }
                            else
                            {
                                StatusKindPrinter.print((int)triggeredmask);
                                if ((triggeredmask &
                                   (DDS.StatusMask)
                                    DDS.StatusKind.SUBSCRIPTION_MATCHED_STATUS) != 0)
                                {
                                    DDS.SubscriptionMatchedStatus status = new DDS.SubscriptionMatchedStatus();
                                    reader.get_subscription_matched_status(ref status);
                                    Console.WriteLine("Subscription matched. current_count = {0}", status.current_count);
                                }
                                if ((triggeredmask &
                                    (DDS.StatusMask)
                                     DDS.StatusKind.LIVELINESS_CHANGED_STATUS) != 0)
                                {
                                    DDS.LivelinessChangedStatus status = new DDS.LivelinessChangedStatus();
                                    reader.get_liveliness_changed_status(ref status);
                                    Console.WriteLine("Liveliness changed. alive_count = {0}", status.alive_count);
                                    if(status.alive_count==0)
                                    {
                                        Console.WriteLine("publisher disconnected");
                                        return;
                                    }
                                   
                                }
                                if ((triggeredmask &
                                   (DDS.StatusMask)
                                    DDS.StatusKind.SAMPLE_LOST_STATUS) != 0)
                                {
                                    DDS.SampleLostStatus status = new DDS.SampleLostStatus();
                                    reader.get_sample_lost_status(ref status);
                                    Console.WriteLine("Sample lost. Reason = {0}", status.last_reason.ToString());
                                   
                                }
                                if ((triggeredmask &
                                    (DDS.StatusMask)
                                     DDS.StatusKind.SAMPLE_REJECTED_STATUS) != 0)
                                {
                                    DDS.SampleRejectedStatus status = new DDS.SampleRejectedStatus();
                                    reader.get_sample_rejected_status(ref status);
                                    Console.WriteLine("Sample Rejected. Reason = {0}", status.last_reason.ToString());
                                    
                                }
                                
                            }
                        }
                    }
                }
                catch (DDS.Retcode_Timeout)
                {
                    Console.WriteLine("wait timed out");
                    count += 2;
                    continue;
                }
            }

        }
        public void subscribe()
        {
            initialize();
            receiveData();
        }

    }
    class StatusKindPrinter
    {
        public static void print(int kind)
        {
            if ((kind & 1) == 1)
                Console.WriteLine("INCONSISTENT_TOPIC_STATUS");
            if ((kind & 2) == 2)
                Console.WriteLine("OFFERED_DEADLINE_MISSED_STATUS");
            if ((kind & 4) == 4)
                Console.WriteLine("REQUESTED_DEADLINE_MISSED_STATUS");
            if ((kind & 32) == 32)
                Console.WriteLine("OFFERED_INCOMPATIBLE_QOS_STATUS");
            if ((kind & 64) == 64)
                Console.WriteLine("REQUESTED_INCOMPATIBLE_QOS_STATUS");
            if ((kind & 128) == 128)
                Console.WriteLine("SAMPLE_LOST_STATUS");
            if ((kind & 256) == 256)
                Console.WriteLine("SAMPLE_REJECTED_STATUS");
            if ((kind & 512) == 512)
                Console.WriteLine("DATA_ON_READERS_STATUS");
            if ((kind & 1024) == 1024)
                Console.WriteLine("DATA_AVAILABLE_STATUS");
            if ((kind & 2048) == 2048)
                Console.WriteLine("LIVELINESS_LOST_STATUS");
            if ((kind & 4096) == 4096)
                Console.WriteLine("LIVELINESS_CHANGED_STATUS");
            if ((kind & 8192) == 8192)
                Console.WriteLine("PUBLICATION_MATCHED_STATUS");
            if ((kind & 16384) == 16384)
                Console.WriteLine("SUBSCRIPTION_MATCHED_STATUS");
            if ((kind & 16777216) == 16777216)
                Console.WriteLine("RELIABLE_WRITER_CACHE_CHANGED_STATUS");
            if ((kind & 33554432) == 33554432)
                Console.WriteLine("RELIABLE_READER_ACTIVITY_CHANGED_STATUS");
            if ((kind & 67108864) == 67108864)
                Console.WriteLine("DATA_WRITER_CACHE_STATUS");
            if ((kind & 134217728) == 134217728)
                Console.WriteLine("DATA_WRITER_PROTOCOL_STATUS");
            if ((kind & 268435456) == 268435456)
                Console.WriteLine("DATA_READER_CACHE_STATUS");
            if ((kind & 536870912) == 536870912)
                Console.WriteLine("DATA_READER_PROTOCOL_STATUS");
            if ((kind & 1073741824) == 1073741824)
                Console.WriteLine("DATA_WRITER_DESTINATION_UNREACHABLE_STATUS");
            if ((kind & 2147483648) == 2147483648)
                Console.WriteLine("DATA_WRITER_SAMPLE_REMOVED_STATUS");
        }
    }
  
}
