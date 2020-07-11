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

        public static void LoadConfig()
        {
            string jsonText = LoadResourceFile("fivepd", "callouts/FivePD-StreetRaceConfig.json");
            dynamic configFile = JsonConvert.DeserializeObject(jsonText);
            try
            {
                minAmountOfRacers = (int)configFile["config"]["minAmountOfRacers"];
                maxAmountOfRacers = (int)configFile["config"]["maxAmountOfRacers"];
            }
            catch (Exception ex)
            {
                minAmountOfRacers = 1;
                maxAmountOfRacers = 3;
            }

        }
    }
}
