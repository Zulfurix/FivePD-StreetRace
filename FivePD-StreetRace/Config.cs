using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using Newtonsoft.Json;
using System.IO;
using Newtonsoft.Json.Linq;

namespace FivePD_StreetRace
{
    class Config : BaseScript
    {
        public static int minAmountOfRacers;
        public static int maxAmountOfRacers;
        public static string[] VehicleModels;

        public static void LoadConfig()
        {
            string jsonText = LoadResourceFile("fivepd", "callouts/FivePD-StreetRaceConfig.json");
            dynamic configFile = JsonConvert.DeserializeObject(jsonText);
            try
            {
                minAmountOfRacers = (int)configFile["config"]["minAmountOfRacers"];
                maxAmountOfRacers = (int)configFile["config"]["maxAmountOfRacers"];

                JArray jarr = (JArray)configFile["config"]["vehicles"];
                VehicleModels = jarr.ToObject<string[]>();
                Debug.WriteLine("[FivePD-StreetRace]: Parsed " + VehicleModels.Length + " vehicle names.");
            }
            catch (Exception ex)
            {
                Debug.WriteLine("[FivePD-StreetRace]: " + ex.Message);
                minAmountOfRacers = 1;
                maxAmountOfRacers = 3;
            }

        }
    }
}
