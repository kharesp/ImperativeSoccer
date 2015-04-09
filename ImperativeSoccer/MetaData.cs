using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;

namespace ImperativeSoccer
{
    public sealed class MetaData
    {
        private MetaData() { }
        private static bool mInitialized = false;

        //list of player info
        private static List<PlayerInfo> PLAYER_DATA = new List<PlayerInfo>();

        //mapping of sensor_ids to player names
        public static ReadOnlyDictionary<int, string> SENSOR_MAP;

        //mapping of sensor_ids to player names
        public static ReadOnlyDictionary<string, List<int>> PLAYER_MAP;

        //mapping of player_name to team_id 
        public static ReadOnlyDictionary<string, string> PLAYER_TEAM_MAP;

        public static int PICO_TO_MILI = 1000000000;
        public static long SECOND_TO_PICO = 1000000000000;
        //Max Lengths on x and y axis
        public static int LEN_X = 52483;
        public static int LEN_Y = 67925;
        //Y_OFFSET
        public static int Y_OFFSET = 33960;

        //initializes the player meta data stored in PLAYER_DATA list
        public static void initializePlayerData()
        {
            Console.WriteLine("Initializing Player Information");
            /* players for team A */
            PLAYER_DATA.Add(new PlayerInfo("A", "Nick Gertje", 13));
            PLAYER_DATA.Add(new PlayerInfo("A", "Nick Gertje", 14));
            PLAYER_DATA.Add(new PlayerInfo("A", "Nick Gertje", 97));
            PLAYER_DATA.Add(new PlayerInfo("A", "Nick Gertje", 98));

            PLAYER_DATA.Add(new PlayerInfo("A", "Dennis Dotterweich", 47));
            PLAYER_DATA.Add(new PlayerInfo("A", "Dennis Dotterweich", 16));

            PLAYER_DATA.Add(new PlayerInfo("A", "Niklas Waelzlein", 49));
            PLAYER_DATA.Add(new PlayerInfo("A", "Niklas Waelzlein", 88));

            PLAYER_DATA.Add(new PlayerInfo("A", "Wili Sommer", 19));
            PLAYER_DATA.Add(new PlayerInfo("A", "Wili Sommer", 52));

            PLAYER_DATA.Add(new PlayerInfo("A", "Philipp Harlass", 53));
            PLAYER_DATA.Add(new PlayerInfo("A", "Philipp Harlass", 54));

            PLAYER_DATA.Add(new PlayerInfo("A", "Roman Hartleb", 23));
            PLAYER_DATA.Add(new PlayerInfo("A", "Roman Hartleb", 24));

            PLAYER_DATA.Add(new PlayerInfo("A", "Erik Engelhardt", 57));
            PLAYER_DATA.Add(new PlayerInfo("A", "Erik Engelhardt", 58));

            PLAYER_DATA.Add(new PlayerInfo("A", "Sandro Schneider", 59));
            PLAYER_DATA.Add(new PlayerInfo("A", "Sandro Schneider", 28));

            /*players for team B */

            PLAYER_DATA.Add(new PlayerInfo("B", "Leon Krapf", 61));
            PLAYER_DATA.Add(new PlayerInfo("B", "Leon Krapf", 62));
            PLAYER_DATA.Add(new PlayerInfo("B", "Leon Krapf", 99));
            PLAYER_DATA.Add(new PlayerInfo("B", "Leon Krapf", 100));

            PLAYER_DATA.Add(new PlayerInfo("B", "Kevin Baer", 63));
            PLAYER_DATA.Add(new PlayerInfo("B", "Kevin Baer", 64));

            PLAYER_DATA.Add(new PlayerInfo("B", "Luca Ziegler", 65));
            PLAYER_DATA.Add(new PlayerInfo("B", "Luca Ziegler", 66));

            PLAYER_DATA.Add(new PlayerInfo("B", "Ben Mueller", 67));
            PLAYER_DATA.Add(new PlayerInfo("B", "Ben Mueller", 68));

            PLAYER_DATA.Add(new PlayerInfo("B", "Vale Reitstetter", 69));
            PLAYER_DATA.Add(new PlayerInfo("B", "Vale Reitstetter", 38));

            PLAYER_DATA.Add(new PlayerInfo("B", "Christopher Lee", 71));
            PLAYER_DATA.Add(new PlayerInfo("B", "Christopher Lee", 40));

            PLAYER_DATA.Add(new PlayerInfo("B", "Leon Heinze", 73));
            PLAYER_DATA.Add(new PlayerInfo("B", "Leon Heinze", 74));

            PLAYER_DATA.Add(new PlayerInfo("B", "Leo Langhans", 75));
            PLAYER_DATA.Add(new PlayerInfo("B", "Leo Langhans", 44));

            /* sensors for Refree */
            PLAYER_DATA.Add(new PlayerInfo("", "Referee", 105));
            PLAYER_DATA.Add(new PlayerInfo("", "Referee", 106));

            mInitialized = true;
        }
        public static void createTeamMap()
        {
            Console.WriteLine("Creating player_name to team_id map");
            if (mInitialized)
            {
                Dictionary<string, string> player_team = new Dictionary<string, string>();
                foreach (var element in PLAYER_DATA)
                {
                    if (!player_team.ContainsKey(element.mName))
                        player_team.Add(element.mName, element.mTeamId);
                }
                PLAYER_TEAM_MAP = new ReadOnlyDictionary<string, string>(player_team);

            }
            else
                throw new ApplicationException("player information is not initialized");
        }

        public static void createSensorMap()
        {
            Console.WriteLine("Creating Sensor_id to Player_name map");
            if (mInitialized)
            {
                SENSOR_MAP = new ReadOnlyDictionary<int, string>(PLAYER_DATA
                    .ToDictionary(info => info.mSensorId, info => info.mName));
            }
            else
                throw new ApplicationException("player information is not initialized");
        }
        public static void createPlayerMap()
        {

            Console.WriteLine("Creating Player_name to sensor_ids map");
            if (mInitialized)
            {
                PLAYER_MAP = new ReadOnlyDictionary<string, List<int>>(
                    PLAYER_DATA.ToLookup(info => info.mName, info => info.mSensorId)
                    .ToDictionary(x => x.Key, x => x.ToList()));

            }
            else
                throw new ApplicationException("player information is not initialized");
        }


        /**
         * Encapsulates information about a player. 
         */
        public sealed class PlayerInfo
        {
            public string mTeamId { get; set; }
            public string mName { get; set; }
            public int mSensorId { get; set; }

            public PlayerInfo(string teamId, string playerName, int sensorId)
            {
                mTeamId = teamId;
                mName = playerName;
                mSensorId = sensorId;
            }

        }

    }
}
