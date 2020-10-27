using GTA;
using GTA.Math;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metadata
{
    public static class NewFunc
    {
        //Retractable Wheels
        public static bool HasRetractableWheels(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash(0xDCA174A42133F08C), vehicle);
        }

        public static void RaiseRetractableWheels(this Vehicle vehicle)
        {
            Function.Call(Hash(0xF660602546D27BA8), vehicle);
        }

        public static void LowerRetractablewheels(this Vehicle vehicle)
        {
            Function.Call(Hash(0x5335BE58C083E74E), vehicle);
        }

        //Rocket Boost
        public static bool HasRocketBoost(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash(0x36D782F68B309BDA), vehicle.Handle);
        }

        public static bool IsRocketBoostActive(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash(0x3D34E80EED4AE3BE), vehicle);
        }

        public static void SetRocketBoostActive(this Vehicle vehicle, bool active)
        {
            Function.Call(Hash(0x81E1552E35DC3839), vehicle, active);
        }

        public static void SetRocketBoostRefillTime(this Vehicle vehicle, float seconds)
        {
            Function.Call(Hash(0xE00F2AB100B76E89), vehicle, seconds);
        }

        public static void SetRocketBoostPercentage(this Vehicle vehicle, float percentage)
        {
            Function.Call(Hash(0xFEB2DDED3509562E), vehicle, percentage);
        }

        //Nitro Boost
        public static void SetBoostActiveSound(this Vehicle vehicle, bool toggle)
        {
            Function.Call(GTA.Native.Hash.SET_VEHICLE_BOOST_ACTIVE, vehicle, toggle);
        }

        public static bool IsVehicleShuntBoostActive(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash(0xA2459F72C14E2E8D), vehicle);
        }

        public static void SetNitroEnabled(this Vehicle vehicle, bool toggle, float level = 2.5f, float power = 1.1f, float rechargeTime = 4f, bool disableSound = false)
        {
            if (!Function.Call<bool>(GTA.Native.Hash.HAS_NAMED_PTFX_ASSET_LOADED, "veh_xs_vehicle_mods"))
                Function.Call(GTA.Native.Hash.REQUEST_NAMED_PTFX_ASSET, "veh_xs_vehicle_mods");

            if (toggle == true)
            {
                //if (!Function.Call<bool>(GTA.Native.Hash._0x36AD3E690DA5ACEB, "CrossLine")) //ANIMPOSTFX_IS_RUNNING                
                //    Function.Call(GTA.Native.Hash._0x2206BF9A37B7F724, "CrossLine", 0, true); //ANIMPOSTFX_PLAY
                Function.Call(Hash(0xC8E9B6B71B8E660D), vehicle, toggle, level, power, rechargeTime, disableSound); //_SET_VEHICLE_NITRO_ENABLED           
            } 
            if (toggle == false)
            {
                //if (Function.Call<bool>(GTA.Native.Hash._0x36AD3E690DA5ACEB, "CrossLine")) //ANIMPOSTFX_IS_RUNNING               
                //    Function.Call(GTA.Native.Hash._0x068E835A1D0DC0E3, "CrossLine"); //ANIMPOSTFX_STOP
                Function.Call(Hash(0xC8E9B6B71B8E660D), vehicle, toggle, level, power, rechargeTime, disableSound); //_SET_VEHICLE_NITRO_ENABLED
            }
        }

        public static void SetNitroHudActive(bool toggle)
        {
            if (toggle == true)
            {
                Function.Call(GTA.Native.Hash._0x808519373FD336A3, true); //_SET_PLAYER_IS_IN_DIRECTOR_MODE
                Function.Call(Hash(0x1DFEDD15019315A9), toggle); //_SET_ABILITY_BAR_VISIBILITY_IN_MULTIPLAYER
            }
            else
            {
                Function.Call(GTA.Native.Hash._0x808519373FD336A3, false); //_SET_PLAYER_IS_IN_DIRECTOR_MODE
            }
        }

        //Xenon Lights Color
        public static void XenonLightsColor(this Vehicle vehicle, eXenonColor colorIndex)
        {
            Function.Call(Hash(0xE41033B25D003A07), vehicle, (int)colorIndex);
        }

        public static eXenonColor XenonLightsColor(this Vehicle vehicle)
        {
            return Function.Call<eXenonColor>(Hash(0x3DFF319A831E0CDB), vehicle);
        }

        public enum eXenonColor
        {
            White = 0,
            Blue,
            ElectricBlue,
            MintGreen,
            Limegreen,
            Yellow,
            GoldenShower,
            Orange,
            Red,
            PonyPink,
            HotPink,
            Blacklight,
            Purple
        }

        //Dominator Tombstone 
        public static bool IsVehicleHaveTombstone(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash(0x71AFB258CCED3A27), vehicle);
        }

        public static void HideTombstone(this Vehicle vehicle, bool toggle)
        {
            Function.Call(Hash(0xAE71FB656C600587), vehicle, toggle);
        }

        //Deluxo Flight
        public static void SetSpecialFlightWingRatio(this Vehicle vehicle, float ratio)
        {
            Function.Call(Hash(0x70A252F60A3E036B), vehicle, ratio);
        }

        public static void SetHoverTransformRatio(this Vehicle vehicle, float ratio)
        {
            Function.Call(Hash(0xD138FA15C9776837), vehicle, ratio);
        }

        public static void SetHoverTransformPercentage(this Vehicle vehicle, float percent)
        {
            Function.Call(Hash(0x438B3D7CA026FE91), vehicle, percent);
        }

        public static bool CanTransformFlightMode(this Vehicle vehicle)
        {
            return vehicle.HasBone("thrust");
        }

        public static void SetHoverTransformActive(this Vehicle vehicle, bool toggle)
        {
            Function.Call(Hash(0x2D55FE374D5FDB91), vehicle, toggle);
        }

        //Car to Submarine
        public static void TransformVehicleToSubmarine(this Vehicle vehicle, bool noAnimation)
        {
            Function.Call(Hash(0xBE4C854FFDB6EEBE), vehicle, noAnimation);
        }

        public static void TransformSubmarineToVehicle(this Vehicle vehicle, bool noAnimation)
        {
            Function.Call(Hash(0x2A69FFD1B42BFF9E), vehicle, noAnimation);
        }

        public static bool IsSubmarineVehicletransformed(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash(0xA77DC70BD689A1E5), vehicle);
        }

        public static bool CanTransformSubmarineMode(this Vehicle vehicle)
        {
            return vehicle.HasBone("turbine_hatch");
        }

        //Amphibious Vehicle
        public static bool IsModelAnAmphibiousCar(this Model model)
        {
            return Function.Call<bool>(Hash(0x633F6F44A537EBB6), model);
        }

        public static bool IsModelAnAmphibiousQuadbike(this Model model)
        {
            return Function.Call<bool>(Hash(0xA1A9FC1C76A6730D), model);
        }

        //Parachute & Jump
        public static bool CanJump(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash(0x9078C0C5EF8C19E9), vehicle);
        }

        public static void SetParachuteModel(this Vehicle vehicle, int modelhash)
        {
            //parachute model = 230075693
            Function.Call(Hash(0x4D610C6B56031351), vehicle, modelhash);
        }

        public static void SetParachuteTextVariation(this Vehicle vehicle, eTextureVariation textureVariation)
        {
            Function.Call(Hash(0xA74AD2439468C883), vehicle, (int)textureVariation);
        }

        public enum eTextureVariation
        {
            Unk0 = 0, Unk1, Unk2, Unk3, Unk4, Unk5, Unk6, Unk7
        }

        public static bool HasParachute(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash(0xBC9CFF381338CB4F), vehicle);
        }

        public static bool CanActivateParachute(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash(0xA916396DF4154EE3), vehicle);
        }

        public static void SetParachuteActive(this Vehicle vehicle, bool active)
        {
            Function.Call(Hash(0x0BFFB028B3DD0A97), vehicle, active);
        }

        //Others
        public static Vehicle GetLastRammedVehicle(this Vehicle vehicle)
        {
            return Function.Call<Vehicle>(Hash(0x04F2FA6E234162F7), vehicle);
        }

        public static int GetNumberOfVehicleDoors(this Vehicle vehicle)
        {
            return Function.Call<int>(Hash(0x92922A607497B14D), vehicle);
        }

        public static bool HasRam(this Vehicle vehicle)
        {
            return vehicle.HasBone("ram_1mod");
        }

        public static bool HasScoop(this Vehicle vehicle)
        {
            return vehicle.HasBone("scoop_1mod");
        }

        public static bool HasSpike(this Vehicle vehicle)
        {
            return vehicle.HasBone("spike_1mod");
        }

        public static bool IsCheating(string cheat)
        {
            return Function.Call<bool>(GTA.Native.Hash._0x557E43C447E700A8, Game.GenerateHash(cheat));
        }

        //Global
        static Hash Hash(ulong hash)
        {
            return (Hash)hash;
        }

        public static bool HideHud = false;
    }
}
