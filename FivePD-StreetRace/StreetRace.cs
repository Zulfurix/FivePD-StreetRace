﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CitizenFX.Core;
using static CitizenFX.Core.Native.API;
using FivePD.API;


namespace FivePD_StreetRace
{
    [CalloutProperties("Street Race", "Zulfurix", "2.3")]
    public class StreetRace : FivePD.API.Callout
    {
        static int amountOfRacers = 3;
        static int amountOfRaceCheckpoints = 10;
        Random random = new Random();

        TaskSequence raceSequence;
        Ped[] Suspects = new Ped[amountOfRacers];
        Vehicle[] SuspectVehicles = new Vehicle[amountOfRacers];
        VehicleHash[] VehicleModels = { 
            VehicleHash.Blista2, 
            VehicleHash.Prairie, 
            VehicleHash.Vacca, 
            VehicleHash.Comet2, 
            VehicleHash.F620, 
            VehicleHash.Intruder, 
            VehicleHash.Warrener, 
            VehicleHash.Dukes, 
            VehicleHash.Phoenix, 
            VehicleHash.Sentinel, 
            VehicleHash.Stinger, 
            VehicleHash.Tampa, 
            VehicleHash.Vigero, 
            VehicleHash.SultanRS, 
            VehicleHash.Schwarzer, 
            VehicleHash.Bullet, 
            VehicleHash.Buffalo, 
            VehicleHash.Buffalo2, 
            VehicleHash.Oracle, 
            VehicleHash.Oracle2
        };

        PedHash[] PedModels = { PedHash.Bevhills01AMM, PedHash.Bevhills02AMM, PedHash.AfriAmer01AMM, PedHash.Eastsa01AMM };

        // Locations for start / end of the street race
        Vector3[] StartLocations = {
            new Vector3(-934, -127, 37),
            new Vector3(-1924, -521, 11),
            new Vector3(-1782, 781, 138),
            new Vector3(-507, -657, 33),
            new Vector3(118, -1441, 29),
            new Vector3(-3034, 1430, 24),
            new Vector3(263, -579, 43),
            new Vector3(-242, -2623, 5.66f),
            new Vector3(466, -1184, 41)
        };

        Vector3[] EndLocations = {
            new Vector3(1225, -2052, 44),
            new Vector3(-74, 874, 235),
            new Vector3(187, -3031, 5),
            new Vector3(-1340, -838, 17),
            new Vector3(-228, -674, 33),
            new Vector3(-369, -1831, 22),
            new Vector3(388, -1225, 40),
            new Vector3(798, -1433, 27),
            new Vector3(1030, -741, 57),
            new Vector3(62, -2036, 18)
        };

        public StreetRace()
        {
            InitInfo(World.GetNextPositionOnStreet(StartLocations[random.Next(0, StartLocations.Length)], true));

            // Callout details
            this.ShortName = "Street Race";
            this.CalloutDescription = "Local residents report acts of dangerous driving and street racing involving multiple vehicles.";
            this.ResponseCode = 3;
            this.StartDistance = 200f;
        }

        public async override Task OnAccept()
        {
            InitBlip();

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

                // Put suspect in their vehicle after calculating spawn position for their vehicle
                Vector3 placemenetPos = World.GetNextPositionOnStreet(Location, true);
                SuspectVehicles[i] = await World.CreateVehicle(VehicleModels[random.Next(0,VehicleModels.Length)], placemenetPos);
                Suspects[i].SetIntoVehicle(SuspectVehicles[i], VehicleSeat.Driver);

                // Randomly applied vehicle modifications
                if (random.Next(0, 10) == 5)
                {
                    bool turboEnabled = random.Next(0, 1) == 1;
                    ToggleVehicleMod(SuspectVehicles[i].Handle, 18, turboEnabled);          // Turbo
                    SetVehicleMod(SuspectVehicles[i].Handle, 11, random.Next(0,4), true);   // Engine
                    SetVehicleMod(SuspectVehicles[i].Handle, 12, random.Next(0, 4), true);  // Brakes
                    SetVehicleMod(SuspectVehicles[i].Handle, 13, random.Next(0, 4), true);  // Transmission
                }

                // Exclude suspect vehicles from traffic stop logic
                Utilities.ExcludeVehicleFromTrafficStop(SuspectVehicles[i].NetworkId, true);
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

                // Potential Driving Styles = 1074543228, 786956
                // Set up task sequence
                for (int j = 0; j < amountOfRaceCheckpoints; j++)
                    raceSequence.AddTask.DriveTo(SuspectVehicles[i], checkpoints[j], 100f, 1000f, 1074543228);

                // Initiate the task sequence for current ped
                SuspectVehicles[i].Speed = 20f;
                Suspects[i].Task.PerformSequence(raceSequence);
                SetDriverAbility(Suspects[i].Handle, 1.0f);
                SetDriverAggressiveness(Suspects[i].Handle, 1.0f);
                SetDriverRacingModifier(Suspects[i].Handle, 1.0f);

                // Calculate desired heading in the direction of the next path finding node. This is used to ensure that the suspect
                // vehicle is facing the right direction before initiating the task sequence
                Vector3 closestNode = new Vector3(0f,0f,0f);
                GetClosestMajorVehicleNode(Suspects[i].Position.X, Suspects[i].Position.Y, Suspects[i].Position.Z, ref closestNode, 3.0f, 0);
                float initialHeading = GetHeadingFromVector_2d(closestNode.X - Suspects[i].Position.X, closestNode.Y - Suspects[i].Position.Y);

                SuspectVehicles[i].Heading = initialHeading;
            }
        }

        private void Notify(string message)
        {
            SetNotificationTextEntry("STRING");
            AddTextComponentString(message);
            DrawNotification(false, false);
        }
    }
}
