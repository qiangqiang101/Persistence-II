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
    public static class VehicleEx
    {
        //Retractable Wheels
        public static bool HasRetractableWheels(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash.GET_HAS_RETRACTABLE_WHEELS, vehicle);
        }

        public static void RaiseRetractableWheels(this Vehicle vehicle)
        {
            Function.Call(Hash.SET_WHEELS_RETRACTED_INSTANTLY, vehicle);
        }

        public static void LowerRetractablewheels(this Vehicle vehicle)
        {
            Function.Call(Hash.SET_WHEELS_EXTENDED_INSTANTLY, vehicle);
        }

        //Rocket Boost
        public static bool HasRocketBoost(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash.GET_HAS_ROCKET_BOOST, vehicle.Handle);
        }

        public static bool IsRocketBoostActive(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash.IS_ROCKET_BOOST_ACTIVE, vehicle);
        }

        public static void SetRocketBoostActive(this Vehicle vehicle, bool active)
        {
            Function.Call(Hash.SET_ROCKET_BOOST_ACTIVE, vehicle, active);
        }

        public static void SetRocketBoostRefillTime(this Vehicle vehicle, float seconds)
        {
            Function.Call(Hash.SET_SCRIPT_ROCKET_BOOST_RECHARGE_TIME, vehicle, seconds);
        }

        public static void SetRocketBoostPercentage(this Vehicle vehicle, float percentage)
        {
            Function.Call(Hash.SET_ROCKET_BOOST_FILL, vehicle, percentage);
        }

        //Nitro Boost
        public static void SetBoostActiveSound(this Vehicle vehicle, bool toggle)
        {
            Function.Call(Hash.SET_VEHICLE_BOOST_ACTIVE, vehicle, toggle);
        }

        public static bool IsVehicleShuntBoostActive(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash.GET_IS_VEHICLE_SHUNTING, vehicle);
        }

        public static void SetNitroEnabled(this Vehicle vehicle, bool toggle, float level = 2.5f, float power = 1.1f, float rechargeTime = 4f, bool disableSound = false)
        {
            if (!Function.Call<bool>(Hash.HAS_NAMED_PTFX_ASSET_LOADED, "veh_xs_vehicle_mods"))
                Function.Call(Hash.REQUEST_NAMED_PTFX_ASSET, "veh_xs_vehicle_mods");

            if (toggle == true)
            {
                //if (!Function.Call<bool>(Hash.ANIMPOSTFX_IS_RUNNING, "CrossLine"))            
                //    Function.Call(Hash.ANIMPOSTFX_PLAY, "CrossLine", 0, true);
                Function.Call(Hash.SET_OVERRIDE_NITROUS_LEVEL, vehicle, toggle, level, power, rechargeTime, disableSound);        
            } 
            if (toggle == false)
            {
                //if (Function.Call<bool>(Hash.ANIMPOSTFX_IS_RUNNING, "CrossLine"))            
                //    Function.Call(Hash.ANIMPOSTFX_STOP, "CrossLine");
                Function.Call(Hash.SET_OVERRIDE_NITROUS_LEVEL, vehicle, toggle, level, power, rechargeTime, disableSound);
            }
        }

        public static void SetNitroHudActive(bool toggle)
        {
            if (toggle == true)
            {
                Function.Call(Hash.SET_PLAYER_IS_IN_DIRECTOR_MODE, true);
                Function.Call(Hash.SET_ABILITY_BAR_VISIBILITY, toggle);
            }
            else
            {
                Function.Call(Hash.SET_PLAYER_IS_IN_DIRECTOR_MODE, false);
            }
        }

        //Xenon Lights Color
        public static void XenonLightsColor(this Vehicle vehicle, eXenonColor colorIndex)
        {
            Function.Call(Hash.SET_VEHICLE_XENON_LIGHT_COLOR_INDEX, vehicle, (int)colorIndex);
        }

        public static eXenonColor XenonLightsColor(this Vehicle vehicle)
        {
            return Function.Call<eXenonColor>(Hash.GET_VEHICLE_XENON_LIGHT_COLOR_INDEX, vehicle);
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
            return Function.Call<bool>(Hash.GET_DOES_VEHICLE_HAVE_TOMBSTONE, vehicle);
        }

        public static void HideTombstone(this Vehicle vehicle, bool toggle)
        {
            Function.Call(Hash.HIDE_TOMBSTONE, vehicle, toggle);
        }

        //Deluxo Flight
        public static void SetSpecialFlightWingRatio(this Vehicle vehicle, float ratio)
        {
            Function.Call(Hash.SET_HOVER_MODE_WING_RATIO, vehicle, ratio);
        }

        public static void SetHoverTransformRatio(this Vehicle vehicle, float ratio)
        {
            Function.Call(Hash.SET_SPECIAL_FLIGHT_MODE_RATIO, vehicle, ratio);
        }

        public static void SetHoverTransformPercentage(this Vehicle vehicle, float percent)
        {
            Function.Call(Hash.SET_SPECIAL_FLIGHT_MODE_TARGET_RATIO, vehicle, percent);
        }

        public static bool CanTransformFlightMode(this Vehicle vehicle)
        {
            return vehicle.Bones.Contains("thrust");
            //return vehicle.HasBone("thrust");
        }

        public static void SetHoverTransformActive(this Vehicle vehicle, bool toggle)
        {
            Function.Call(Hash.SET_DISABLE_HOVER_MODE_FLIGHT, vehicle, toggle);
        }

        //Car to Submarine
        public static void TransformVehicleToSubmarine(this Vehicle vehicle, bool noAnimation)
        {
            Function.Call(Hash.TRANSFORM_TO_SUBMARINE, vehicle, noAnimation);
        }

        public static void TransformSubmarineToVehicle(this Vehicle vehicle, bool noAnimation)
        {
            Function.Call(Hash.TRANSFORM_TO_CAR, vehicle, noAnimation);
        }

        public static bool IsSubmarineVehicletransformed(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash.IS_VEHICLE_IN_SUBMARINE_MODE, vehicle);
        }

        public static bool CanTransformSubmarineMode(this Vehicle vehicle)
        {
            return vehicle.Bones.Contains("turbine_hatch");
            //return vehicle.HasBone("turbine_hatch");
        }

        //Amphibious Vehicle
        public static bool IsModelAnAmphibiousCar(this Model model)
        {
            return Function.Call<bool>(Hash.IS_THIS_MODEL_AN_AMPHIBIOUS_CAR, model);
        }

        public static bool IsModelAnAmphibiousQuadbike(this Model model)
        {
            return Function.Call<bool>(Hash.IS_THIS_MODEL_AN_AMPHIBIOUS_QUADBIKE, model);
        }

        //Parachute & Jump
        public static bool CanJump(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash.GET_CAR_HAS_JUMP, vehicle);
        }

        public static void SetParachuteModel(this Vehicle vehicle, string modelname)
        {
            //sr_prop_specraces_para_s_01, imp_prop_impexp_para_s (SecuroServ; Default)
            Function.Call(Hash.VEHICLE_SET_PARACHUTE_MODEL_OVERRIDE, vehicle, StringHash.AtStringHash(modelname));
        }

        public static void SetParachuteTextVariation(this Vehicle vehicle, eParachuteTextureVariation textureVariation)
        {
            Function.Call(Hash.VEHICLE_SET_PARACHUTE_MODEL_TINT_INDEX, vehicle, (int)textureVariation);
        }

        public enum eParachuteTextureVariation
        {
            Rainbow, 
            Red, 
            WhiteBlueYellow, 
            BlackRedWhite, 
            RedWhiteBlue, 
            Blue, 
            Black, 
            BlackYellow
        }

        public static bool HasParachute(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash.GET_VEHICLE_HAS_PARACHUTE, vehicle);
        }

        public static bool CanActivateParachute(this Vehicle vehicle)
        {
            return Function.Call<bool>(Hash.GET_VEHICLE_HAS_PARACHUTE, vehicle);
        }

        public static void SetParachuteActive(this Vehicle vehicle, bool active)
        {
            Function.Call(Hash.VEHICLE_START_PARACHUTING, vehicle, active);
        }

        //Others
        public static Vehicle GetLastRammedVehicle(this Vehicle vehicle)
        {
            return Function.Call<Vehicle>(Hash.GET_LAST_SHUNT_VEHICLE, vehicle);
        }

        public static int GetNumberOfVehicleDoors(this Vehicle vehicle)
        {
            return Function.Call<int>(Hash.GET_NUMBER_OF_VEHICLE_DOORS, vehicle);
        }

        public static bool HasRam(this Vehicle vehicle)
        {
            return vehicle.Bones.Contains("ram_1mod");
            //return vehicle.HasBone("ram_1mod");
        }

        public static bool HasScoop(this Vehicle vehicle)
        {
            return vehicle.Bones.Contains("scoop_1mod");
            //return vehicle.HasBone("scoop_1mod");
        }

        public static bool HasSpike(this Vehicle vehicle)
        {
            return vehicle.Bones.Contains("spike_1mod");
            //return vehicle.HasBone("spike_1mod");
        }

        public static bool IsCheating(string cheat)
        {
            return Function.Call<bool>(Hash.HAS_PC_CHEAT_WITH_HASH_BEEN_ACTIVATED, StringHash.AtStringHash(cheat));
        }

        public static bool HideHud = false;
    }
}
