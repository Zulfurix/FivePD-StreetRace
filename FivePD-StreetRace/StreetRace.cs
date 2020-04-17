using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using CalloutAPI;


namespace FivePD_StreetRace
{
    [CalloutProperties("StreetRace", "Zulfurix", "1.0", Probability.Medium)]
    public class StreetRace : CalloutAPI.Callout
    {
        static int amountOfRacers = 3;
        static int amountOfRaceCheckpoints = 10;
        Random random = new Random();

        TaskSequence raceSequence;
        Ped[] Suspects = new Ped[amountOfRacers];
        Vehicle[] SuspectVehicles = new Vehicle[amountOfRacers];
        VehicleHash[] VehicleModels = { VehicleHash.Sentinel, VehicleHash.Stinger, VehicleHash.Tampa, VehicleHash.Vigero, VehicleHash.SultanRS, VehicleHash.Schwarzer, VehicleHash.Bullet, VehicleHash.Buffalo, VehicleHash.Buffalo2 };
        PedHash[] PedModels = { PedHash.Bevhills01AMM, PedHash.Bevhills02AMM };
        
        // Locations for start / end of the street race
        Vector3[] StartLocations = {
            new Vector3(-934, -127, 37),
            new Vector3(-1924, -521, 11),
            new Vector3(-1782, 781, 138)
                };

        Vector3[] EndLocations = {
            new Vector3(1225, -2052, 44),
            new Vector3(-74, 874, 235),
            new Vector3(187, -3031, 5),
            new Vector3(-1340, -838, 17)
        };

        public StreetRace()
        {
            InitBase(World.GetNextPositionOnStreet(StartLocations[random.Next(0, StartLocations.Length)], true));

            // Callout details
            this.ShortName = "Street Race";
            this.CalloutDescription = "Local residents report acts of dangerous driving and street racing involving multiple vehicles.";
            this.ResponseCode = 3;
            this.StartDistance = 300f;
        }

        public async override Task Init()
        {
            OnAccept();

            float offset = 0f;

            // Set up suspects + their vehicles
            for (int i = 0; i < amountOfRacers; i++)
            {
                // Create the suspect poed
                Suspects[i] = await SpawnPed(PedModels[random.Next(0, PedModels.Length)], Location);
                Suspects[i].AlwaysKeepTask = true;
                Suspects[i].BlockPermanentEvents = true;
                
                // Attach blip to suspect
                Suspects[i].AttachBlip();
                Suspects[i].AttachedBlip.Color = BlipColor.Red;
                Suspects[i].AttachedBlip.IsShortRange = true;

                // Put suspect in their vehicle
                SuspectVehicles[i] = await World.CreateVehicle(VehicleModels[random.Next(0,VehicleModels.Length)], Location + offset);
                Suspects[i].SetIntoVehicle(SuspectVehicles[i], VehicleSeat.Driver);

                offset += 5f;
            }
        }

        public override void OnStart(Ped player)
        {
            base.OnStart(player);


            Vector3[] checkpoints = new Vector3[amountOfRaceCheckpoints];
            for (int i = 0; i < amountOfRaceCheckpoints; i++)
                checkpoints[i] = EndLocations[random.Next(0, EndLocations.Length)];


            // Initiate suspect ped tasks
            for (int i = 0; i < amountOfRacers; i++)
            {
                raceSequence = new TaskSequence();

                // Set up task sequence
                for (int j = 0; j < amountOfRaceCheckpoints; j++)
                    raceSequence.AddTask.DriveTo(SuspectVehicles[i], checkpoints[j], 100f, 1000f, 786956);

                // Initiate the task sequence for current ped
                Suspects[i].Task.PerformSequence(raceSequence);
                SetDriverAbility(Suspects[i].Handle, 1000f);
                SetDriverAggressiveness(Suspects[i].Handle, 1000f);
            }

            Tick += Update;
        }

        private async Task Update()
        {
            await BaseScript.Delay(100);
        }

        private void Notify(string message)
        {
            SetNotificationTextEntry("STRING");
            AddTextComponentString(message);
            DrawNotification(false, false);
        }
    }
}
