using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Forms;
using GTA;
using GTA.Math;
using System.Runtime.Serialization;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

//todo
//landing gear
//deluxo state
//submarine car states

namespace AdvancedPersistence
{
    public class Constants
    {
        public const float Version = 1.61f;
        public const bool DebugMode = false;
        public const string SubFolder = "AdvancedPersistence";

        public const string ChangeLogs = @"=Version 1.61=
-Fix some bugs and re-compile for latest version.
";
    }

    public class Streamer
    {
        public static int CurrVehIndex = 0;
        public static Dictionary<Vehicle, VehicleDataV1> StreamedVehicles = new Dictionary<Vehicle, VehicleDataV1>();
        public static void HandleNextVehicle()
        {
            if(AdvancedPersistence.VehicleDatabase.Count > 0)
            {
                if (CurrVehIndex >= AdvancedPersistence.VehicleDatabase.Count)
                    CurrVehIndex = 0;

                VehicleDataV1 veh = AdvancedPersistence.VehicleDatabase[CurrVehIndex];
                if(veh != null)
                {
                    if (veh.Handle != null)
                    {
                        if (Game.Player.Character.Position.DistanceTo(veh.Handle.Position) > ModSettings.StreamOutDistance)
                        {
                            AdvancedPersistence.SaveVehicleData(veh.Handle, veh);
                            veh.WasUserDespawned = false;
                            AdvancedPersistence.DeleteBlipsOnCar(veh.Handle);
                            if (AdvancedPersistence.AttachedVehicles.ContainsKey(veh.Handle))
                            {
                                AdvancedPersistence.AttachedVehicles.Remove(veh.Handle);
                            }

                            StreamedVehicles.Remove(veh.Handle);
                            veh.Handle.IsPersistent = true;
                            GTA.Native.OutputArgument arg1 = new GTA.Native.OutputArgument(veh.Handle);
                            GTA.Native.Function.Call(GTA.Native.Hash.DELETE_VEHICLE, arg1);

                            if (AdvancedPersistence.AttachedTasks.ContainsKey(veh.Handle))
                            {
                                AdvancedPersistence.AttachedTasks[veh.Handle].Clean();
                                if (AdvancedPersistence.AttachedTasks[veh.Handle].Ped != null)
                                {
                                    GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS_IMMEDIATELY, AdvancedPersistence.AttachedTasks[veh.Handle].Ped.Handle);
                                    GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE, AdvancedPersistence.AttachedTasks[veh.Handle].Ped.Handle, AdvancedPersistence.AttachedTasks[veh.Handle].Handle, 16);

                                    if (AdvancedPersistence.AttachedTasks[veh.Handle].Ped.Exists())
                                    {
                                        AdvancedPersistence.AttachedTasks[veh.Handle].Ped.IsPersistent = true;
                                        AdvancedPersistence.AttachedTasks[veh.Handle].Ped.Delete();
                                    }
                                }
                                AdvancedPersistence.AttachedTasks.Remove(veh.Handle);
                            }

                            veh.Handle = null;
                        }
                    }
                    else
                    {
                        if (!veh.WasUserDespawned)
                        {
                            if (Game.Player.Character.Position.DistanceTo(veh.Position) < ModSettings.StreamInDistance)
                            {
                                if (StreamedVehicles.Count >= ModSettings.MaxNumberOfStreamedInCars)
                                {
                                    foreach (Vehicle v in StreamedVehicles.Keys)
                                    {
                                        if (Game.Player.Character.Position.DistanceTo(veh.Position) < Game.Player.Character.Position.DistanceTo(v.Position))
                                        {
                                            VehicleDataV1 v2 = StreamedVehicles[v];
                                            if (v2.Handle != null)
                                            {
                                                AdvancedPersistence.SaveVehicleData(v2.Handle, v2);
                                                v2.WasUserDespawned = false;
                                                AdvancedPersistence.DeleteBlipsOnCar(v2.Handle);
                                                if (AdvancedPersistence.AttachedVehicles.ContainsKey(v2.Handle))
                                                {
                                                    AdvancedPersistence.AttachedVehicles.Remove(v2.Handle);
                                                }

                                                StreamedVehicles.Remove(v2.Handle);
                                                v2.Handle.IsPersistent = true;
                                                GTA.Native.OutputArgument arg1 = new GTA.Native.OutputArgument(v2.Handle);
                                                GTA.Native.Function.Call(GTA.Native.Hash.DELETE_VEHICLE, arg1);

                                                if (AdvancedPersistence.AttachedTasks.ContainsKey(v2.Handle))
                                                {
                                                    AdvancedPersistence.AttachedTasks[v2.Handle].Clean();
                                                    if (AdvancedPersistence.AttachedTasks[v2.Handle].Ped != null)
                                                    {
                                                        GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS_IMMEDIATELY, AdvancedPersistence.AttachedTasks[v2.Handle].Ped.Handle);
                                                        GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE, AdvancedPersistence.AttachedTasks[v2.Handle].Ped.Handle, AdvancedPersistence.AttachedTasks[v2.Handle].Handle, 16);

                                                        if (AdvancedPersistence.AttachedTasks[v2.Handle].Ped.Exists())
                                                        {
                                                            AdvancedPersistence.AttachedTasks[v2.Handle].Ped.IsPersistent = true;
                                                            AdvancedPersistence.AttachedTasks[v2.Handle].Ped.Delete();
                                                        }
                                                    }
                                                    AdvancedPersistence.AttachedTasks.Remove(v2.Handle);
                                                }

                                                v2.Handle = null;
                                            }

                                            GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_AREA, veh.Position.X, veh.Position.Y, veh.Position.Z, 10f, false, false, false, false);
                                            Vehicle vvv = AdvancedPersistence.CreateVehicle(veh);
                                            if (vvv != null)
                                                StreamedVehicles.Add(vvv, veh);
                                            break;
                                        }
                                    }
                                }
                                else
                                {
                                    GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_AREA, veh.Position.X, veh.Position.Y, veh.Position.Z, 10f, false, false, false, false);
                                    Vehicle vvv = AdvancedPersistence.CreateVehicle(veh);
                                    if(vvv != null)
                                        StreamedVehicles.Add(vvv, veh);
                                }
                            }
                        }
                    }
                }

                CurrVehIndex++;
                if (CurrVehIndex >= AdvancedPersistence.VehicleDatabase.Count)
                    CurrVehIndex = 0;
            }
            else
            {
                CurrVehIndex = 0;
            }
        }
    }

    public class BoneHelper
    {
        public static int GetBoneIndex(Vehicle veh, string bone)
        {
            return GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_ENTITY_BONE_INDEX_BY_NAME, veh.Handle, bone);
        }

        public static Vector3 GetBonePositionWorld(Vehicle veh, int boneIndex)
        {
            return GTA.Native.Function.Call<Vector3>(GTA.Native.Hash.GET_WORLD_POSITION_OF_ENTITY_BONE, veh.Handle, boneIndex);
        }
    }

    public class MathHelper
    {
        public static Vector3 RotateX(Vector3 point, float angle)
        {
            Vector3 f1 = new Vector3(1, 0, 0);
            Vector3 f2 = new Vector3(0, (float)Math.Cos(DegreesToRad(angle)), (float)-Math.Sin(DegreesToRad(angle)));
            Vector3 f3 = new Vector3(0, (float)Math.Sin(DegreesToRad(angle)), (float)Math.Cos(DegreesToRad(angle)));

            Vector3 final = new Vector3();
            final.X = (f1.X * point.X + f1.Y * point.Y + f1.Z * point.Z);
            final.Y = (f2.X * point.X + f2.Y * point.Y + f2.Z * point.Z);
            final.Z = (f3.X * point.X + f3.Y * point.Y + f3.Z * point.Z);

            return final;
        }

        public static Vector3 RotateZ(Vector3 point, float angle)
        {
            Vector3 f7 = new Vector3((float)Math.Cos(DegreesToRad(angle)), (float)-Math.Sin(DegreesToRad(angle)), 0);
            Vector3 f8 = new Vector3((float)Math.Sin(DegreesToRad(angle)), (float)Math.Cos(DegreesToRad(angle)), 0);
            Vector3 f9 = new Vector3(0, 0, 1);

            Vector3 final = new Vector3();
            final.X = (f7.X * point.X + f7.Y * point.Y + f7.Z * point.Z);
            final.Y = (f8.X * point.X + f8.Y * point.Y + f8.Z * point.Z);
            final.Z = (f9.X * point.X + f9.Y * point.Y + f9.Z * point.Z);
            return final;
        }

        public static Vector3 RotateY(Vector3 point, float angle)
        {
            Vector3 f4 = new Vector3((float)Math.Cos(DegreesToRad(angle)), 0, (float)Math.Sin(DegreesToRad(angle)));
            Vector3 f5 = new Vector3(0, 1, 0);
            Vector3 f6 = new Vector3((float)-Math.Sin(DegreesToRad(angle)), 0, (float)Math.Cos(DegreesToRad(angle)));

            Vector3 final = new Vector3();
            final.X = (f4.X * point.X + f4.Y * point.Y + f4.Z * point.Z);
            final.Y = (f5.X * point.X + f5.Y * point.Y + f5.Z * point.Z);
            final.Z = (f6.X * point.X + f6.Y * point.Y + f6.Z * point.Z);
            return final;
        }

        public static float DegreesToRad(float deg)
        {
            return (float)Math.PI * deg / 180.0f;
        }

        public static float RadToDegrees(float rad)
        {
            return rad * (180.0f / (float)Math.PI);
        }

        public static Vector3 ScreenRelToWorld(Vector3 camPos, Vector3 camRot, Vector2 coord)
        {
            var camForward = RotationToDirection(camRot) * 10f;
            var rotUp = camRot;
            rotUp.X += 10f;
            var rotDown = camRot;
            rotDown.X -= 10f;
            var rotLeft = camRot;
            rotLeft.Z -= 10f;
            var rotRight = camRot;
            rotRight.Z += 10f;

            var camRight = RotationToDirection(rotRight) - RotationToDirection(rotLeft);
            var camUp = RotationToDirection(rotUp) - RotationToDirection(rotDown);

            var rollRad = -DegreesToRad(camRot.Y);

            var camRightRoll = camRight * (float)Math.Cos(rollRad) - camUp * (float)Math.Sin(rollRad);
            var camUpRoll = camRight * (float)Math.Sin(rollRad) + camUp * (float)Math.Cos(rollRad);

            var point3D = camPos + camForward + camRightRoll + camUpRoll;
            Vector2 point2D;
            if (!WorldToScreenRel(point3D, out point2D))
                return camPos + camForward;
            var point3DZero = camPos + camForward;
            Vector2 point2DZero;
            if (!WorldToScreenRel(point3DZero, out point2DZero))
                return camPos + camForward;

            const double eps = 0.001;
            if (Math.Abs(point2D.X - point2DZero.X) < eps || Math.Abs(point2D.Y - point2DZero.Y) < eps) return camPos + camForward;
            var scaleX = (coord.X - point2DZero.X) / (point2D.X - point2DZero.X);
            var scaleY = (coord.Y - point2DZero.Y) / (point2D.Y - point2DZero.Y);
            var point3Dret = camPos + camForward + camRightRoll * scaleX + camUpRoll * scaleY;
            return point3Dret;
        }

        public static bool WorldToScreenRel(Vector3 vec, out Vector2 vec2)
        {
            bool F = false;
            float X;
            float Y;
            unsafe
            {
                float y = 0f;
                float x = 0f;
                bool f = GTA.Native.Function.Call<bool>((GTA.Native.Hash)0x34E82F05DF2974F5, vec.X, vec.Y, vec.Z, &x, &y);
                F = f;
                X = x;
                Y = y;
            }

            if(F)
                vec2 = new Vector2(X, Y);
            else
                vec2 = Vector2.Zero;
            return F;
        }

        public static Vector3 RotationToDirection(Vector3 rotation)
        {
            var z = DegreesToRad(rotation.Z);
            var x = DegreesToRad(rotation.X);
            var num = Math.Abs(Math.Cos(x));
            return new Vector3
            {
                X = (float)(-Math.Sin(z) * num),
                Y = (float)(Math.Cos(z) * num),
                Z = (float)Math.Sin(x)
            };
        }
    }

    public class GraphicsManager
    {
        public static GUI.RGBA HighlightColor_Edge = new GUI.RGBA(255, 255, 255);
        public static GUI.RGBA HighlightColor_Full = new GUI.RGBA(255, 255, 255, 25);

        public static void DrawSkeleton(Vector3 pos, Vector3 size, Vector3 rot)
        {
            Vector3 p1 = pos + new Vector3(size.X / 2, size.Y / 2, size.Z / 2);
            Vector3 p2 = pos + new Vector3(size.X / 2, -size.Y / 2, size.Z / 2);
            Vector3 p3 = pos + new Vector3(size.X / 2, -size.Y / 2, -size.Z / 2);
            Vector3 p4 = pos + new Vector3(size.X / 2, size.Y / 2, -size.Z / 2);

            Vector3 p5 = pos + new Vector3(-size.X / 2, size.Y / 2, size.Z / 2);
            Vector3 p6 = pos + new Vector3(-size.X / 2, -size.Y / 2, size.Z / 2);
            Vector3 p7 = pos + new Vector3(-size.X / 2, -size.Y / 2, -size.Z / 2);
            Vector3 p8 = pos + new Vector3(-size.X / 2, size.Y / 2, -size.Z / 2);

            p1 -= pos;
            p1 = MathHelper.RotateY(p1, rot.Y);
            p1 = MathHelper.RotateX(p1, rot.X);
            p1 = MathHelper.RotateZ(p1, rot.Z);
            p1 += pos;

            p2 -= pos;
            p2 = MathHelper.RotateY(p2, rot.Y);
            p2 = MathHelper.RotateX(p2, rot.X);
            p2 = MathHelper.RotateZ(p2, rot.Z);
            p2 += pos;

            p3 -= pos;
            p3 = MathHelper.RotateY(p3, rot.Y);
            p3 = MathHelper.RotateX(p3, rot.X);
            p3 = MathHelper.RotateZ(p3, rot.Z);
            p3 += pos;

            p4 -= pos;
            p4 = MathHelper.RotateY(p4, rot.Y);
            p4 = MathHelper.RotateX(p4, rot.X);
            p4 = MathHelper.RotateZ(p4, rot.Z);
            p4 += pos;

            p5 -= pos;
            p5 = MathHelper.RotateY(p5, rot.Y);
            p5 = MathHelper.RotateX(p5, rot.X);
            p5 = MathHelper.RotateZ(p5, rot.Z);
            p5 += pos;

            p6 -= pos;
            p6 = MathHelper.RotateY(p6, rot.Y);
            p6 = MathHelper.RotateX(p6, rot.X);
            p6 = MathHelper.RotateZ(p6, rot.Z);
            p6 += pos;

            p7 -= pos;
            p7 = MathHelper.RotateY(p7, rot.Y);
            p7 = MathHelper.RotateX(p7, rot.X);
            p7 = MathHelper.RotateZ(p7, rot.Z);
            p7 += pos;

            p8 -= pos;
            p8 = MathHelper.RotateY(p8, rot.Y);
            p8 = MathHelper.RotateX(p8, rot.X);
            p8 = MathHelper.RotateZ(p8, rot.Z);
            p8 += pos;

            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, p1.X, p1.Y, p1.Z, p2.X, p2.Y, p2.Z, HighlightColor_Edge.Red, HighlightColor_Edge.Green, HighlightColor_Edge.Blue, 255);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, p2.X, p2.Y, p2.Z, p3.X, p3.Y, p3.Z, HighlightColor_Edge.Red, HighlightColor_Edge.Green, HighlightColor_Edge.Blue, 255);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, p3.X, p3.Y, p3.Z, p4.X, p4.Y, p4.Z, HighlightColor_Edge.Red, HighlightColor_Edge.Green, HighlightColor_Edge.Blue, 255);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, p4.X, p4.Y, p4.Z, p1.X, p1.Y, p1.Z, HighlightColor_Edge.Red, HighlightColor_Edge.Green, HighlightColor_Edge.Blue, 255);

            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_POLY, p3.X, p3.Y, p3.Z, p4.X, p4.Y, p4.Z, p1.X, p1.Y, p1.Z, HighlightColor_Full.Red, HighlightColor_Full.Green, HighlightColor_Full.Blue, HighlightColor_Full.Alpha);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_POLY, p2.X, p2.Y, p2.Z, p3.X, p3.Y, p3.Z, p1.X, p1.Y, p1.Z, HighlightColor_Full.Red, HighlightColor_Full.Green, HighlightColor_Full.Blue, HighlightColor_Full.Alpha);

            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, p5.X, p5.Y, p5.Z, p6.X, p6.Y, p6.Z, HighlightColor_Edge.Red, HighlightColor_Edge.Green, HighlightColor_Edge.Blue, 255);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, p6.X, p6.Y, p6.Z, p7.X, p7.Y, p7.Z, HighlightColor_Edge.Red, HighlightColor_Edge.Green, HighlightColor_Edge.Blue, 255);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, p7.X, p7.Y, p7.Z, p8.X, p8.Y, p8.Z, HighlightColor_Edge.Red, HighlightColor_Edge.Green, HighlightColor_Edge.Blue, 255);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, p8.X, p8.Y, p8.Z, p5.X, p5.Y, p5.Z, HighlightColor_Edge.Red, HighlightColor_Edge.Green, HighlightColor_Edge.Blue, 255);

            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_POLY, p8.X, p8.Y, p8.Z, p7.X, p7.Y, p7.Z, p5.X, p5.Y, p5.Z, HighlightColor_Full.Red, HighlightColor_Full.Green, HighlightColor_Full.Blue, HighlightColor_Full.Alpha);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_POLY, p7.X, p7.Y, p7.Z, p6.X, p6.Y, p6.Z, p5.X, p5.Y, p5.Z, HighlightColor_Full.Red, HighlightColor_Full.Green, HighlightColor_Full.Blue, HighlightColor_Full.Alpha);

            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, p1.X, p1.Y, p1.Z, p5.X, p5.Y, p5.Z, HighlightColor_Edge.Red, HighlightColor_Edge.Green, HighlightColor_Edge.Blue, 255);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, p2.X, p2.Y, p2.Z, p6.X, p6.Y, p6.Z, HighlightColor_Edge.Red, HighlightColor_Edge.Green, HighlightColor_Edge.Blue, 255);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, p3.X, p3.Y, p3.Z, p7.X, p7.Y, p7.Z, HighlightColor_Edge.Red, HighlightColor_Edge.Green, HighlightColor_Edge.Blue, 255);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, p4.X, p4.Y, p4.Z, p8.X, p8.Y, p8.Z, HighlightColor_Edge.Red, HighlightColor_Edge.Green, HighlightColor_Edge.Blue, 255);

            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_POLY, p1.X, p1.Y, p1.Z, p4.X, p4.Y, p4.Z, p5.X, p5.Y, p5.Z, HighlightColor_Full.Red, HighlightColor_Full.Green, HighlightColor_Full.Blue, HighlightColor_Full.Alpha);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_POLY, p5.X, p5.Y, p5.Z, p4.X, p4.Y, p4.Z, p8.X, p8.Y, p8.Z, HighlightColor_Full.Red, HighlightColor_Full.Green, HighlightColor_Full.Blue, HighlightColor_Full.Alpha);

            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_POLY, p2.X, p2.Y, p2.Z, p5.X, p5.Y, p5.Z, p6.X, p6.Y, p6.Z, HighlightColor_Full.Red, HighlightColor_Full.Green, HighlightColor_Full.Blue, HighlightColor_Full.Alpha);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_POLY, p2.X, p2.Y, p2.Z, p1.X, p1.Y, p1.Z, p5.X, p5.Y, p5.Z, HighlightColor_Full.Red, HighlightColor_Full.Green, HighlightColor_Full.Blue, HighlightColor_Full.Alpha);

            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_POLY, p3.X, p3.Y, p3.Z, p2.X, p2.Y, p2.Z, p6.X, p6.Y, p6.Z, HighlightColor_Full.Red, HighlightColor_Full.Green, HighlightColor_Full.Blue, HighlightColor_Full.Alpha);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_POLY, p3.X, p3.Y, p3.Z, p6.X, p6.Y, p6.Z, p7.X, p7.Y, p7.Z, HighlightColor_Full.Red, HighlightColor_Full.Green, HighlightColor_Full.Blue, HighlightColor_Full.Alpha);

            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_POLY, p3.X, p3.Y, p3.Z, p7.X, p7.Y, p7.Z, p8.X, p8.Y, p8.Z, HighlightColor_Full.Red, HighlightColor_Full.Green, HighlightColor_Full.Blue, HighlightColor_Full.Alpha);
            GTA.Native.Function.Call(GTA.Native.Hash.DRAW_POLY, p8.X, p8.Y, p8.Z, p4.X, p4.Y, p4.Z, p3.X, p3.Y, p3.Z, HighlightColor_Full.Red, HighlightColor_Full.Green, HighlightColor_Full.Blue, HighlightColor_Full.Alpha);
        }
    }

    public class WeaponManager
    {
        public static unsafe uint GetCurrentWeapon(int ped)
        {
            GTA.Native.OutputArgument arg1 = new GTA.Native.OutputArgument();
            GTA.Native.Function.Call<bool>(GTA.Native.Hash.GET_CURRENT_PED_WEAPON, ped, arg1, true);
            return arg1.GetResult<uint>();
        }

        public static uint GetWeaponTintIndex(int ped, WeaponHash wep)
        {
            return GTA.Native.Function.Call<uint>(GTA.Native.Hash.GET_PED_WEAPON_TINT_INDEX, ped, wep);
        }

        public static uint GetWeaponTintCount(WeaponHash wep)
        {
            return GTA.Native.Function.Call<uint>(GTA.Native.Hash.GET_WEAPON_TINT_COUNT, wep);
        }

        public static int GetAmmoInWeapon(int ped, WeaponHash wep)
        {
            return GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_AMMO_IN_PED_WEAPON, ped, wep);
        }

        public static bool DoesWeaponTakeComponent(WeaponHash wep, WeaponComponentHash comp)
        {
            return GTA.Native.Function.Call<bool>(GTA.Native.Hash.DOES_WEAPON_TAKE_WEAPON_COMPONENT, wep, comp);
        }

        public static bool HasGotWeaponComponent(int ped, WeaponHash wep, WeaponComponentHash comp)
        {
            return GTA.Native.Function.Call<bool>(GTA.Native.Hash.HAS_PED_GOT_WEAPON_COMPONENT, ped, wep, comp);
        }

        public static void GiveWeaponComponent(int ped, uint wep, uint comp)
        {
            GTA.Native.Function.Call(GTA.Native.Hash.GIVE_WEAPON_COMPONENT_TO_PED, ped, wep, comp);
        }

        public static bool HasWeapon(int ped, WeaponHash wep)
        {
            return GTA.Native.Function.Call<bool>(GTA.Native.Hash.HAS_PED_GOT_WEAPON, ped, wep);
        }
    }

    public class PedTask
    {
        public int Handle = 0;
        public Ped Ped;
        public int SequenceCount = -1;
        public PedTask(Ped ped)
        {
            Ped = ped;
        }

        public void Open()
        {
            GTA.Native.OutputArgument arg1 = new GTA.Native.OutputArgument();
            GTA.Native.Function.Call(GTA.Native.Hash.OPEN_SEQUENCE_TASK, arg1);
            Handle = arg1.GetResult<int>();
        }

        public void Close()
        {
            GTA.Native.Function.Call(GTA.Native.Hash.SET_SEQUENCE_TO_REPEAT, Handle, false);
            GTA.Native.Function.Call(GTA.Native.Hash.CLOSE_SEQUENCE_TASK, Handle);
        }

        public void DriveTo(Vehicle veh, Vector3 target, float radius, float speed, DrivingStyle style)
        {
            SequenceCount++;
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_DRIVE_TO_COORD_LONGRANGE, 0, veh.Handle, target.X, target.Y, target.Z, speed, style, radius);
        }

        public void WalkTo(Vector3 pos)
        {
            SequenceCount++;
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_GO_STRAIGHT_TO_COORD, 0, pos.X, pos.Y, pos.Z, 1f, 3000, 0f, 0f);
        }

        public void FaceCoord(Vector3 pos)
        {
            SequenceCount++;
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_TURN_PED_TO_FACE_COORD, 0, pos.X, pos.Y, pos.Z, 2000);
        }

        public void PlayAnimAdv(Vector3 pos, string animDict, string animName)
        {
            SequenceCount++;
            if(!GTA.Native.Function.Call<bool>(GTA.Native.Hash.HAS_ANIM_DICT_LOADED, animDict))
            {
                GTA.Native.Function.Call(GTA.Native.Hash.REQUEST_ANIM_DICT, animDict);
            }
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_PLAY_ANIM_ADVANCED, 0, animDict, animName, pos.X, pos.Y, pos.Z, 0f, 0f, -180f, 8f, 1f, 5000, 0, 0f, 0, 0);
        }

        public void Brake(Vehicle veh)
        {
            SequenceCount++;
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_TEMP_ACTION, 0, veh.Handle, 6, 5000);
        }

        public void ParkAt(Vehicle veh, Vector3 target, float radius)
        {
            SequenceCount++;
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_VEHICLE_PARK, 0, veh.Handle, target.X, target.Y, target.Z, 0f, 0, radius, false);
        }

        public void ExitVehicle(Vehicle veh, bool normal = false)
        {
            SequenceCount++;
            if (normal)
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE, 0, veh.Handle, 0);
            else
                GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE, 0, veh.Handle, 16);
        }

        public void FleeCoords(Vector3 pos)
        {
            SequenceCount++;
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_SMART_FLEE_COORD, 0, pos.X, pos.Y, pos.Z, 500f, 20000, false, false);
        }

        public void Wander()
        {
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_WANDER_STANDARD, 0, 10f, 10);
        }

        public void Run()
        {
            GTA.Native.Function.Call(GTA.Native.Hash.TASK_PERFORM_SEQUENCE, Ped.Handle, Handle);
        }

        public void Clean()
        {
            unsafe
            {
                int handle = Handle;
                GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_SEQUENCE_TASK, &handle);
            }
            Handle = 0;
        }

        public bool IsDoneSequence()
        {
            int seq = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_SEQUENCE_PROGRESS, Ped.Handle);
            return seq == -1 || seq >= SequenceCount - 1;
        }

        public int GetSequence()
        {
            return GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_SEQUENCE_PROGRESS, Ped.Handle);
        }
    }

    public static class GUI
    {
        public class RGBA
        {
            public int Red, Green, Blue, Alpha;
            public RGBA(int r, int g, int b, int a = 255)
            {
                Red = r;
                Green = g;
                Blue = b;
                Alpha = a;
            }
        }
        public static float GetDeltaTime()
        {
            return Game.LastFrameTime;
        }

        public static float Lerp(float a, float b, float t)
        {
            if (t > 1.0f)
                t = 1.0f;
            if (t < 0.0f)
                t = 0.0f;
            return (1f - t) * a + t * b;
        }

        public static bool doorState = true;
        public static bool windowState = true;
        public static bool neonState = true;
        public static class Phone
        {
            public static Theme ActiveTheme = Theme.Red;
            public static int PhoneColor = 0;
            public static int PhoneBrightness = 5;
            static AppObject ControlMain = null;
            static AppObject AppScroll = null;
            static AppObject AppScroll2 = null;
            static AppObject AppScroll3 = null;
            static AppObject Converter = null;
            static AppObject CurrentVehicleApp = null;
            static AppObject VehicleMarketApp = null;
            static AppObject VehicleTemplate = null;
            static AppObject VehicleTemplate2 = null;
            static AppObject VehicleTemplate3 = null;
            static AppObject VehicleTemplate4 = null;
            static AppObject VehicleTemplate_Doors = null;
            static AppObject VehicleTemplate_Engine = null;
            static AppObject VehicleTemplate_Alarm = null;
            static AppObject VehicleTemplate_Lights = null;
            static AppObject VehicleTemplate_Windows = null;
            static AppObject VehicleTemplate_Neons = null;
            static AppObject VehicleTemplate_Anchor = null;
            static AppObject ChangelogApp = null;
            static AppObject Settings = null;
            static AppObject About = null;
            static AppObject ThemeApp = null;
            static AppObject BackApp = null;
            static AppObject BrightnessApp = null;
            static AppObject ColorApp = null;
            static AppObject ToneApp = null;
            static AppObject ModelApp = null;
            static AppObject PhoneSettings = null;
            static AppObject BlipApp = null;
            public static VehicleDataV1 activatedCar = null;

            public static void RedoScroll()
            {
                try
                {
                    SetDataSlotEmpty((int)AppScroll.Container);
                    AppScroll.Items.Clear();
                    SetDataSlotEmpty((int)AppScroll2.Container);
                    AppScroll2.Items.Clear();
                    SetDataSlotEmpty((int)ControlMain.Container);
                    ControlMain.Items.Clear();
                    SetDataSlotEmpty((int)AppScroll3.Container);
                    AppScroll3.Items.Clear();
                    if (GetHomeObjectByIndex(0) != null)
                        GetHomeObjectByIndex(0).NotificationNumber = AdvancedPersistence.VehicleDatabase.Count;
                    int indx = 1;
                    foreach (VehicleDataV1 dat in AdvancedPersistence.VehicleDatabase)
                    {
                        string x = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, dat.Hash);
                        string xx = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_FILENAME_FOR_AUDIO_CONVERSATION, x);
                        if (xx == "NULL")
                            if (dat.Handle != null)
                                if (dat.Handle.Exists())
                                    xx = dat.Handle.Mods.LicensePlate;
                        string tagName = $"[{indx}]: {xx}";
                        if (dat.Tag != "" && !string.IsNullOrEmpty(dat.Tag) && !string.IsNullOrWhiteSpace(dat.Tag))
                            tagName = dat.Tag;
                        AppScroll.AddItem(new AppSettingItem(dat.Id, tagName, ListIcons.None, () =>
                        {
                            string insiderInfo = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, dat.Hash);
                            string insiderInfo2 = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_FILENAME_FOR_AUDIO_CONVERSATION, insiderInfo);
                            if (insiderInfo2 == "NULL")
                                if (dat.Handle != null)
                                    if (dat.Handle.Exists())
                                        insiderInfo2 = dat.Handle.Mods.LicensePlate;
                            VehicleTemplate2.Name = insiderInfo2;
                            if (dat.Tag != "")
                                VehicleTemplate2.Name = dat.Tag;
                            activatedCar = dat;
                            if (activatedCar != null)
                            {
                                if (activatedCar.Handle != null)
                                {
                                    AdvancedPersistence.DrawTrace = true;
                                    if (activatedCar.Handle.Exists())
                                    {
                                        if (activatedCar.Handle.AttachedBlip != null)
                                        {
                                            if(ModSettings.EnableBlips)
                                            {
                                                if (activatedCar.Handle.AttachedBlip.Exists())
                                                {
                                                    if (activatedCar.Handle.Model.IsHelicopter)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                                                    else if (activatedCar.Handle.Model.IsAmphibiousQuadBike || activatedCar.Handle.Model.IsBicycle || activatedCar.Handle.Model.IsBike || activatedCar.Handle.Model.IsQuadBike)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                                    else if (activatedCar.Handle.Model.IsJetSki)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Seashark;
                                                    else if (activatedCar.Handle.Model.IsBoat)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Boat;
                                                    else if (activatedCar.Handle.Model.IsPlane)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Plane;
                                                    else
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                                    activatedCar.Handle.AttachedBlip.IsShortRange = true;
                                                    activatedCar.Handle.AttachedBlip.Scale = 0.75f;
                                                    activatedCar.Handle.AttachedBlip.Name = "Saved Vehicle";
                                                    activatedCar.Handle.AttachedBlip.Priority = 255;
                                                    GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, true);
                                                    activatedCar.Handle.AttachedBlip.Color = (BlipColor)activatedCar.BlipColor;
                                                    GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            RefreshDisplay();
                        }, VehicleTemplate2));

                        ControlMain.AddItem(new AppSettingItem(dat.Id, tagName, ListIcons.None, () =>
                        {
                            string insiderInfo = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, dat.Hash);
                            string insiderInfo2 = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_FILENAME_FOR_AUDIO_CONVERSATION, insiderInfo);
                            if (insiderInfo2 == "NULL")
                                if (dat.Handle != null)
                                    if (dat.Handle.Exists())
                                        insiderInfo2 = dat.Handle.Mods.LicensePlate;
                            VehicleTemplate.Name = insiderInfo2;
                            if (dat.Tag != "" && !string.IsNullOrEmpty(dat.Tag) && !string.IsNullOrWhiteSpace(dat.Tag))
                                VehicleTemplate.Name = dat.Tag;
                            activatedCar = dat;
                            if (activatedCar != null)
                            {
                                if (activatedCar.Handle != null)
                                {
                                    VehicleTemplate.GetItemByID<AppSettingItem>("id_engine_stat").Name = activatedCar.Handle.IsEngineRunning ? "ENGINE: ON" : "ENGINE: OFF";
                                    VehicleTemplate.GetItemByID<AppSettingItem>("id_lights_stat").Name = (activatedCar.Handle.AreLightsOn || activatedCar.Handle.AreHighBeamsOn) ? "LIGHTS: ON" : "LIGHTS: OFF";
                                    VehicleTemplate.GetItemByID<AppSettingItem>("id_alarm_stat").Name = activatedCar.Handle.IsAlarmSet ? "ALARM: ON" : "ALARM: OFF";
                                    VehicleTemplate.GetItemByID<AppSettingItem>("id_locked_stat").Name = activatedCar.Handle.LockStatus == VehicleLockStatus.CannotEnter ? "LOCKED: TRUE" : "LOCKED: FALSE";
                                }
                                else
                                {
                                    VehicleTemplate.GetItemByID<AppSettingItem>("id_engine_stat").Name = activatedCar.EngineState ? "ENGINE: ON" : "ENGINE: OFF";
                                    VehicleTemplate.GetItemByID<AppSettingItem>("id_lights_stat").Name = (activatedCar.LightState2 == 1 || activatedCar.LightState2 == 2) ? "LIGHTS: ON" : "LIGHTS: OFF";
                                    VehicleTemplate.GetItemByID<AppSettingItem>("id_alarm_stat").Name = activatedCar.AlarmState ? "ALARM: ON" : "ALARM: OFF";
                                    VehicleTemplate.GetItemByID<AppSettingItem>("id_locked_stat").Name = activatedCar.LockState ? "LOCKED: TRUE" : "LOCKED: FALSE";
                                }

                                if (activatedCar.Handle != null)
                                {
                                    AdvancedPersistence.DrawTrace = true;
                                    if (activatedCar.Handle.Exists())
                                    {
                                        if (activatedCar.Handle.AttachedBlip != null)
                                        {
                                            if(ModSettings.EnableBlips)
                                            {
                                                if (activatedCar.Handle.AttachedBlip.Exists())
                                                {
                                                    if (activatedCar.Handle.Model.IsHelicopter)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                                                    else if (activatedCar.Handle.Model.IsAmphibiousQuadBike || activatedCar.Handle.Model.IsBicycle || activatedCar.Handle.Model.IsBike || activatedCar.Handle.Model.IsQuadBike)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                                    else if (activatedCar.Handle.Model.IsJetSki)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Seashark;
                                                    else if (activatedCar.Handle.Model.IsBoat)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Boat;
                                                    else if (activatedCar.Handle.Model.IsPlane)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Plane;
                                                    else
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                                    activatedCar.Handle.AttachedBlip.IsShortRange = true;
                                                    activatedCar.Handle.AttachedBlip.Scale = 0.75f;
                                                    activatedCar.Handle.AttachedBlip.Name = "Saved Vehicle";
                                                    activatedCar.Handle.AttachedBlip.Priority = 255;
                                                    GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, true);
                                                    activatedCar.Handle.AttachedBlip.Color = (BlipColor)activatedCar.BlipColor;
                                                    GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            RefreshDisplay();
                        }, VehicleTemplate));

                        AppScroll2.AddItem(new AppSettingItem(dat.Id, tagName, ListIcons.None, () =>
                        {
                            string insiderInfo = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, dat.Hash);
                            string insiderInfo2 = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_FILENAME_FOR_AUDIO_CONVERSATION, insiderInfo);
                            if (insiderInfo2 == "NULL")
                                if (dat.Handle != null)
                                    if (dat.Handle.Exists())
                                        insiderInfo2 = dat.Handle.Mods.LicensePlate;
                            VehicleTemplate3.Name = insiderInfo2;
                            if (dat.Tag != "" && !string.IsNullOrEmpty(dat.Tag) && !string.IsNullOrWhiteSpace(dat.Tag))
                                VehicleTemplate3.Name = dat.Tag;
                            activatedCar = dat;
                            if (activatedCar != null)
                            {
                                if (activatedCar.Handle != null)
                                {
                                    AdvancedPersistence.DrawTrace = true;
                                    if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_BOAT_ANCHORED, activatedCar.Handle.Handle))
                                        VehicleTemplate4.GetItemByID<AppSettingItem>("id_anchor_status").Name = "ANCHOR: TRUE";
                                    else
                                        VehicleTemplate4.GetItemByID<AppSettingItem>("id_anchor_status").Name = "ANCHOR: FALSE";
                                }
                                else
                                {
                                    VehicleTemplate4.GetItemByID<AppSettingItem>("id_anchor_status").Name = "ANCHOR: FALSE";
                                }

                                if (activatedCar.Handle != null)
                                {
                                    if (activatedCar.Handle.Exists())
                                    {
                                        if (activatedCar.Handle.AttachedBlip != null)
                                        {
                                            if(ModSettings.EnableBlips)
                                            {
                                                if (activatedCar.Handle.AttachedBlip.Exists())
                                                {
                                                    if (activatedCar.Handle.Model.IsHelicopter)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                                                    else if (activatedCar.Handle.Model.IsAmphibiousQuadBike || activatedCar.Handle.Model.IsBicycle || activatedCar.Handle.Model.IsBike || activatedCar.Handle.Model.IsQuadBike)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                                    else if (activatedCar.Handle.Model.IsJetSki)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Seashark;
                                                    else if (activatedCar.Handle.Model.IsBoat)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Boat;
                                                    else if (activatedCar.Handle.Model.IsPlane)
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Plane;
                                                    else
                                                        activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                                    activatedCar.Handle.AttachedBlip.IsShortRange = true;
                                                    activatedCar.Handle.AttachedBlip.Scale = 0.75f;
                                                    activatedCar.Handle.AttachedBlip.Name = "Saved Vehicle";
                                                    activatedCar.Handle.AttachedBlip.Priority = 255;
                                                    GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, true);
                                                    activatedCar.Handle.AttachedBlip.Color = (BlipColor)activatedCar.BlipColor;
                                                    GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                                }
                                            }
                                        }
                                    }
                                }
                            }
                            RefreshDisplay();
                        }, VehicleTemplate3));

                        Model mdl = new Model(dat.Hash);
                        if (mdl.IsBoat || mdl.IsAmphibiousVehicle || mdl.IsAmphibiousQuadBike || mdl.IsAmphibiousCar || mdl.IsSubmarineCar || mdl.IsJetSki || AdvancedPersistence.IsSubmarine(mdl) || AdvancedPersistence.IsAmphCar(mdl))
                        {
                            AppScroll3.AddItem(new AppSettingItem(dat.Id, tagName, ListIcons.None, () =>
                            {
                                string insiderInfo = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, dat.Hash);
                                string insiderInfo2 = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_FILENAME_FOR_AUDIO_CONVERSATION , insiderInfo);
                                if (insiderInfo2 == "NULL")
                                    if (dat.Handle != null)
                                        if (dat.Handle.Exists())
                                            insiderInfo2 = dat.Handle.Mods.LicensePlate;
                                VehicleTemplate4.Name = insiderInfo2;
                                if (dat.Tag != "" && !string.IsNullOrEmpty(dat.Tag) && !string.IsNullOrWhiteSpace(dat.Tag))
                                    VehicleTemplate4.Name = dat.Tag;
                                activatedCar = dat;
                                if (activatedCar != null)
                                {
                                    if (activatedCar.Handle != null)
                                    {
                                        AdvancedPersistence.DrawTrace = true;
                                        if (activatedCar.Handle.Exists())
                                        {
                                            if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_BOAT_ANCHORED, activatedCar.Handle.Handle))
                                                VehicleTemplate4.GetItemByID<AppSettingItem>("id_anchor_status").Name = "ANCHOR: TRUE";
                                            else
                                                VehicleTemplate4.GetItemByID<AppSettingItem>("id_anchor_status").Name = "ANCHOR: FALSE";
                                            if (activatedCar.Handle.AttachedBlip != null)
                                            {
                                                if(ModSettings.EnableBlips)
                                                {
                                                    if (activatedCar.Handle.AttachedBlip.Exists())
                                                    {
                                                        if (activatedCar.Handle.Model.IsHelicopter)
                                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                                                        else if (activatedCar.Handle.Model.IsAmphibiousQuadBike || activatedCar.Handle.Model.IsBicycle || activatedCar.Handle.Model.IsBike || activatedCar.Handle.Model.IsQuadBike)
                                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                                        else if (activatedCar.Handle.Model.IsJetSki)
                                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Seashark;
                                                        else if (activatedCar.Handle.Model.IsBoat)
                                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Boat;
                                                        else if (activatedCar.Handle.Model.IsPlane)
                                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Plane;
                                                        else
                                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                                        activatedCar.Handle.AttachedBlip.IsShortRange = true;
                                                        activatedCar.Handle.AttachedBlip.Scale = 0.75f;
                                                        activatedCar.Handle.AttachedBlip.Name = "Saved Vehicle";
                                                        activatedCar.Handle.AttachedBlip.Priority = 255;
                                                        GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, true);
                                                        activatedCar.Handle.AttachedBlip.Color = (BlipColor)activatedCar.BlipColor;
                                                        GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                                    }
                                                }
                                            }
                                        }
                                    }
                                }
                                RefreshDisplay();
                            }, VehicleTemplate4));
                        }
                        indx++;
                    }
                }
                catch(Exception e)
                {
                    GTA.UI.Screen.ShowSubtitle("ERROR: " + e.ToString());
                }
            }

            public static void Initialize()
            {
                //Create app containers and set links between them
                ControlMain = new AppObject("Settings", AppContainer.Settings);
                Converter = new AppObject("Converter", AppContainer.Settings);
                AppScroll = new AppObject("List", AppContainer.Settings);
                AppScroll2 = new AppObject("List", AppContainer.Settings);
                AppScroll3 = new AppObject("List", AppContainer.Settings);
                VehicleTemplate = new AppObject("CAR", AppContainer.Settings);
                VehicleTemplate2 = new AppObject("CAR", AppContainer.Settings);
                VehicleTemplate3 = new AppObject("CAR", AppContainer.Settings);
                VehicleTemplate4 = new AppObject("BOAT", AppContainer.Settings);
                Settings = new AppObject("Settings", AppContainer.Settings);
                PhoneSettings = new AppObject("Phone Settings", AppContainer.Settings);
                PhoneSettings.Backward = Settings;
                About = new AppObject("About", AppContainer.Settings);
                About.Backward = Settings;
                ThemeApp = new AppObject("Theme", AppContainer.Settings);
                ThemeApp.Backward = PhoneSettings;
                BackApp = new AppObject("Background", AppContainer.Settings);
                BackApp.Backward = PhoneSettings;
                ColorApp = new AppObject("Color", AppContainer.Settings);
                ColorApp.Backward = PhoneSettings;
                CurrentVehicleApp = new AppObject("Current Vehicle", AppContainer.Settings);
                ChangelogApp = new AppObject("Changelog", AppContainer.MessageView);
                ChangelogApp.Backward = About;
                BrightnessApp = new AppObject("Brightness", AppContainer.Settings);
                BrightnessApp.Backward = PhoneSettings;
                ModelApp = new AppObject("Model", AppContainer.Settings);
                ModelApp.Backward = PhoneSettings;
                ToneApp = new AppObject("Tone", AppContainer.Settings);
                ToneApp.Backward = PhoneSettings;
                BlipApp = new AppObject("Blip Color", AppContainer.Settings);
                BlipApp.Backward = VehicleTemplate2;
                VehicleTemplate2.Backward = AppScroll;
                VehicleTemplate3.Backward = AppScroll2;
                VehicleTemplate4.Backward = AppScroll3;
                VehicleTemplate.Backward = ControlMain;
                VehicleTemplate_Doors = new AppObject("Doors", AppContainer.Settings);
                VehicleTemplate_Doors.Backward = VehicleTemplate;
                VehicleTemplate_Engine = new AppObject("Engine", AppContainer.Settings);
                VehicleTemplate_Engine.Backward = VehicleTemplate;
                VehicleTemplate_Alarm = new AppObject("Alarm", AppContainer.Settings);
                VehicleTemplate_Alarm.Backward = VehicleTemplate;
                VehicleTemplate_Lights = new AppObject("Lights", AppContainer.Settings);
                VehicleTemplate_Lights.Backward = VehicleTemplate;
                VehicleTemplate_Windows = new AppObject("Windows", AppContainer.Settings);
                VehicleTemplate_Windows.Backward = VehicleTemplate;
                VehicleTemplate_Neons = new AppObject("Neons", AppContainer.Settings);
                VehicleTemplate_Neons.Backward = VehicleTemplate;
                VehicleTemplate_Anchor = new AppObject("Anchoring", AppContainer.Settings);
                VehicleTemplate_Anchor.Backward = VehicleTemplate4;

                VehicleMarketApp = new AppObject("Vehicle Market", AppContainer.Settings);
                VehicleMarketApp.AddItem(new AppSettingItem("id_sell", "Sell Vehicle", ListIcons.Attachment));
                VehicleMarketApp.AddItem(new AppSettingItem("id_buy", "Buy Vehicle", ListIcons.Checklist));
                RedoScroll();

                VehicleTemplate.OnBack = () =>
                {
                    AdvancedPersistence.DrawTrace = false;
                    if (activatedCar.Handle != null)
                    {
                        if (activatedCar.Handle.Exists())
                        {
                            if (activatedCar.Handle.AttachedBlip != null)
                            {
                                if(ModSettings.EnableBlips)
                                {
                                    if (activatedCar.Handle.AttachedBlip.Exists())
                                    {
                                        if (activatedCar.Handle.Model.IsHelicopter)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                                        else if (activatedCar.Handle.Model.IsAmphibiousQuadBike || activatedCar.Handle.Model.IsBicycle || activatedCar.Handle.Model.IsBike || activatedCar.Handle.Model.IsQuadBike)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                        else if (activatedCar.Handle.Model.IsJetSki)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Seashark;
                                        else if (activatedCar.Handle.Model.IsBoat)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Boat;
                                        else if (activatedCar.Handle.Model.IsPlane)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Plane;
                                        else
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                        activatedCar.Handle.AttachedBlip.IsShortRange = true;
                                        activatedCar.Handle.AttachedBlip.Color = (BlipColor)activatedCar.BlipColor;
                                        activatedCar.Handle.AttachedBlip.Scale = 0.75f;
                                        activatedCar.Handle.AttachedBlip.Name = "Saved Vehicle";
                                        activatedCar.Handle.AttachedBlip.Priority = 0;
                                        GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                        GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                    }
                                }
                            }
                        }
                    }
                };

                VehicleTemplate2.OnBack = () =>
                {
                    AdvancedPersistence.DrawTrace = false;
                    if (activatedCar.Handle != null)
                    {
                        if (activatedCar.Handle.Exists())
                        {
                            if (activatedCar.Handle.AttachedBlip != null)
                            {
                                if(ModSettings.EnableBlips)
                                {
                                    if (activatedCar.Handle.AttachedBlip.Exists())
                                    {
                                        if (activatedCar.Handle.Model.IsHelicopter)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                                        else if (activatedCar.Handle.Model.IsAmphibiousQuadBike || activatedCar.Handle.Model.IsBicycle || activatedCar.Handle.Model.IsBike || activatedCar.Handle.Model.IsQuadBike)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                        else if (activatedCar.Handle.Model.IsJetSki)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Seashark;
                                        else if (activatedCar.Handle.Model.IsBoat)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Boat;
                                        else if (activatedCar.Handle.Model.IsPlane)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Plane;
                                        else
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                        activatedCar.Handle.AttachedBlip.IsShortRange = true;
                                        activatedCar.Handle.AttachedBlip.Color = (BlipColor)activatedCar.BlipColor;
                                        activatedCar.Handle.AttachedBlip.Scale = 0.75f;
                                        activatedCar.Handle.AttachedBlip.Name = "Saved Vehicle";
                                        activatedCar.Handle.AttachedBlip.Priority = 0;
                                        GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                        GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                    }
                                }
                            }
                        }
                    }
                };

                VehicleTemplate3.OnBack = () =>
                {
                    AdvancedPersistence.DrawTrace = false;
                    if (activatedCar.Handle != null)
                    {
                        if (activatedCar.Handle.Exists())
                        {
                            if (activatedCar.Handle.AttachedBlip != null)
                            {
                                if(ModSettings.EnableBlips)
                                {
                                    if (activatedCar.Handle.AttachedBlip.Exists())
                                    {
                                        if (activatedCar.Handle.Model.IsHelicopter)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                                        else if (activatedCar.Handle.Model.IsAmphibiousQuadBike || activatedCar.Handle.Model.IsBicycle || activatedCar.Handle.Model.IsBike || activatedCar.Handle.Model.IsQuadBike)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                        else if (activatedCar.Handle.Model.IsJetSki)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Seashark;
                                        else if (activatedCar.Handle.Model.IsBoat)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Boat;
                                        else if (activatedCar.Handle.Model.IsPlane)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Plane;
                                        else
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                        activatedCar.Handle.AttachedBlip.IsShortRange = true;
                                        activatedCar.Handle.AttachedBlip.Color = (BlipColor)activatedCar.BlipColor;
                                        activatedCar.Handle.AttachedBlip.Scale = 0.75f;
                                        activatedCar.Handle.AttachedBlip.Name = "Saved Vehicle";
                                        activatedCar.Handle.AttachedBlip.Priority = 0;
                                        GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                        GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                    }
                                }
                            }
                        }
                    }
                };

                VehicleTemplate4.OnBack = () =>
                {
                    AdvancedPersistence.DrawTrace = false;
                    if (activatedCar.Handle != null)
                    {
                        if (activatedCar.Handle.Exists())
                        {
                            if (activatedCar.Handle.AttachedBlip != null)
                            {
                                if(ModSettings.EnableBlips)
                                {
                                    if (activatedCar.Handle.AttachedBlip.Exists())
                                    {
                                        if (activatedCar.Handle.Model.IsHelicopter)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                                        else if (activatedCar.Handle.Model.IsAmphibiousQuadBike || activatedCar.Handle.Model.IsBicycle || activatedCar.Handle.Model.IsBike || activatedCar.Handle.Model.IsQuadBike)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                        else if (activatedCar.Handle.Model.IsJetSki)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Seashark;
                                        else if (activatedCar.Handle.Model.IsBoat)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Boat;
                                        else if (activatedCar.Handle.Model.IsPlane)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Plane;
                                        else
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                        activatedCar.Handle.AttachedBlip.IsShortRange = true;
                                        activatedCar.Handle.AttachedBlip.Color = (BlipColor)activatedCar.BlipColor;
                                        activatedCar.Handle.AttachedBlip.Scale = 0.75f;
                                        activatedCar.Handle.AttachedBlip.Name = "Saved Vehicle";
                                        activatedCar.Handle.AttachedBlip.Priority = 0;
                                        GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                        GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                    }
                                }
                            }
                        }
                    }
                };

                PhoneSettings.AddItem(new AppSettingItem("id_model", "Phone Model", ListIcons.Settings1, null, ModelApp));
                PhoneSettings.AddItem(new AppSettingItem("id_color", "Phone Tone", ListIcons.Settings1, null, ToneApp));
                PhoneSettings.AddItem(new AppSettingItem("id_color", "Phone Color", ListIcons.Settings1, null, ColorApp));
                PhoneSettings.AddItem(new AppSettingItem("id_theme", "Phone Theme", ListIcons.Settings1, null, ThemeApp));
                PhoneSettings.AddItem(new AppSettingItem("id_back", "Phone Background", ListIcons.Settings1, null, BackApp));
                PhoneSettings.AddItem(new AppSettingItem("id_bright", "Phone Brightness", ListIcons.Settings1, null, BrightnessApp));

                ChangelogApp.AddItem(new AppMessageViewItem("Current Version: " + Constants.Version.ToString("0.00"), Constants.ChangeLogs, "CHAR_HUMANDEFAULT"));
                Settings.AddItem(new AppSettingItem("id_phsettings", "Phone Settings", ListIcons.Settings1, null, PhoneSettings));
                Settings.AddItem(new AppSettingItem("id_doors", "Fix Garage Doors", ListIcons.Ticked, () =>
                {
                    if (!GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DOOR_REGISTERED_WITH_SYSTEM, 6969))
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.ADD_DOOR_TO_SYSTEM, 6969, Game.GenerateHash("prop_ch_025c_g_door_01"), 18.65038f, 546.3401f, 176.3448f, true, false, true);
                    }
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x9BA001CB45CBF627), 6969, 10f, false, true);
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x03C27E13B42A0E82), 6969, 1f, false, true);

                    if (!GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DOOR_REGISTERED_WITH_SYSTEM, 696969))
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.ADD_DOOR_TO_SYSTEM, 696969, Game.GenerateHash("prop_ld_garaged_01"), -815.2816f, 185.975f, 72.99993f, true, false, true);
                    }
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x9BA001CB45CBF627), 696969, 10f, false, true);
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x03C27E13B42A0E82), 696969, 1f, false, true);

                    if (!GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DOOR_REGISTERED_WITH_SYSTEM, 69696969))
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.ADD_DOOR_TO_SYSTEM, 69696969, Game.GenerateHash("prop_cs4_10_tr_gd_01"), 1972.787f, 3824.554f, 32.65174f, true, false, true);
                    }
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x9BA001CB45CBF627), 69696969, 10f, false, true);
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x03C27E13B42A0E82), 69696969, 1f, false, true);

                    GTA.Native.Function.Call((GTA.Native.Hash)(0x6BAB9442830C7F53), 6969, 0, true, true);
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x6BAB9442830C7F53), 696969, 0, true, true);
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x6BAB9442830C7F53), 69696969, 0, true, true);

                    GTA.UI.Screen.ShowSubtitle("Attempted fix. Check doors.", 3000);
                }));
                Settings.AddItem(new AppSettingItem("id_about", "About", ListIcons.Profile, null, About));
                About.AddItem(new AppSettingItem("id_mod", "Advanced Persistence", ListIcons.None, null));
                About.AddItem(new AppSettingItem("id_version", "Version: " + Constants.Version.ToString("0.00"), ListIcons.Attachment, null));
                About.AddItem(new AppSettingItem("id_changelog", "Changelog", ListIcons.Checklist, null, ChangelogApp));
                About.AddItem(new AppSettingItem("id_author", "Author: systematic", ListIcons.Profile, null));

                ModelApp.AddItem(new AppSettingItem("id_ifruit", "iFruit Model", ListIcons.Settings1, () =>
                {
                    PhoneModel = 0;
                    GTA.Native.Function.Call(GTA.Native.Hash.DESTROY_MOBILE_PHONE);
                    GTA.Native.Function.Call(GTA.Native.Hash.CREATE_MOBILE_PHONE, PhoneModel);
                    GTA.Native.Function.Call(GTA.Native.Hash.SCRIPT_IS_MOVING_MOBILE_PHONE_OFFSCREEN, true);
                    Script.Wait(1000);
                    GTA.Native.Function.Call(GTA.Native.Hash.SCRIPT_IS_MOVING_MOBILE_PHONE_OFFSCREEN, false);
                    Script.Wait(500);
                    SetPhoneColor(PhoneColor);
                }));

                ModelApp.AddItem(new AppSettingItem("id_facade", "Facade Model", ListIcons.Settings1, () =>
                {
                    PhoneModel = 1;
                    GTA.Native.Function.Call(GTA.Native.Hash.DESTROY_MOBILE_PHONE);
                    GTA.Native.Function.Call(GTA.Native.Hash.CREATE_MOBILE_PHONE, PhoneModel);
                    GTA.Native.Function.Call(GTA.Native.Hash.SCRIPT_IS_MOVING_MOBILE_PHONE_OFFSCREEN, true);
                    Script.Wait(1000);
                    GTA.Native.Function.Call(GTA.Native.Hash.SCRIPT_IS_MOVING_MOBILE_PHONE_OFFSCREEN, false);
                    Script.Wait(500);
                    SetPhoneColor(PhoneColor);
                }));

                ModelApp.AddItem(new AppSettingItem("id_badger", "Badger Model", ListIcons.Settings1, () =>
                {
                    PhoneModel = 2;
                    GTA.Native.Function.Call(GTA.Native.Hash.DESTROY_MOBILE_PHONE);
                    GTA.Native.Function.Call(GTA.Native.Hash.CREATE_MOBILE_PHONE, PhoneModel);
                    GTA.Native.Function.Call(GTA.Native.Hash.SCRIPT_IS_MOVING_MOBILE_PHONE_OFFSCREEN, true);
                    Script.Wait(1000);
                    GTA.Native.Function.Call(GTA.Native.Hash.SCRIPT_IS_MOVING_MOBILE_PHONE_OFFSCREEN, false);
                    Script.Wait(500);
                    SetPhoneColor(PhoneColor);
                }));

                ToneApp.AddItem(new AppSettingItem("id_ifruit", "iFruit Tone", ListIcons.Settings1, () =>
                {
                    PhoneTone = 0;
                }));

                ToneApp.AddItem(new AppSettingItem("id_facade", "Facade Tone", ListIcons.Settings1, () =>
                {
                    PhoneTone = 1;
                }));

                ToneApp.AddItem(new AppSettingItem("id_badger", "Badger Tone", ListIcons.Settings1, () =>
                {
                    PhoneTone = 2;
                }));

                BackApp.AddItem(new AppSettingItem("id_back1", "Blue Angles", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.BlueAngles);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back2", "Blue Circles", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.BlueCircles);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back3", "Blue Shards", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.BlueShards);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back4", "Default", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.Default);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back5", "Diamonds", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.Diamonds);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back6", "Green Glow", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.GreenGlow);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back7", "Green Shards", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.GreenShards);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back8", "Green Squares", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.GreenSquares);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back9", "Green Triangles", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.GreenTriangles);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back10", "None", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.None);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back11", "Orange 8Bit", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.Orange8Bit);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back12", "Orange Half Tone", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.OrangeHalfTone);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back13", "Orange Herring Bone", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.OrangeHerringBone);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back14", "Orange Triangles", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.OrangeTriangles);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back15", "Purple Glow", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.PurpleGlow);
                    RefreshDisplay();
                }));

                BackApp.AddItem(new AppSettingItem("id_back16", "Purple Tartan", ListIcons.Settings1, () =>
                {
                    Phone.SetBackgroundImage(BackgroundImage.PurpleTartan);
                    RefreshDisplay();
                }));

                ThemeApp.AddItem(new AppSettingItem("id_red", "Red", ListIcons.Profile, () =>
                {
                    SetTheme(Theme.Red);
                    RefreshDisplay();
                }));
                ThemeApp.AddItem(new AppSettingItem("id_green", "Green", ListIcons.Profile, () =>
                {
                    SetTheme(Theme.Green);
                    RefreshDisplay();
                }));
                ThemeApp.AddItem(new AppSettingItem("id_blue", "Blue", ListIcons.Profile, () =>
                {
                    SetTheme(Theme.LightBlue);
                    RefreshDisplay();
                }));
                ThemeApp.AddItem(new AppSettingItem("id_orange", "Orange", ListIcons.Profile, () =>
                {
                    SetTheme(Theme.Orange);
                    RefreshDisplay();
                }));
                ThemeApp.AddItem(new AppSettingItem("id_pink", "Pink", ListIcons.Profile, () =>
                {
                    SetTheme(Theme.Pink);
                    RefreshDisplay();
                }));
                ThemeApp.AddItem(new AppSettingItem("id_purple", "Purple", ListIcons.Profile, () =>
                {
                    SetTheme(Theme.Purple);
                    RefreshDisplay();
                }));
                ThemeApp.AddItem(new AppSettingItem("id_grey", "Grey", ListIcons.Profile, () =>
                {
                    SetTheme(Theme.Grey);
                    RefreshDisplay();
                }));

                BrightnessApp.AddItem(new AppSettingItem("id_full", "Maximum", ListIcons.Profile, () =>
                {
                    PhoneBrightness = 5;
                }));
                BrightnessApp.AddItem(new AppSettingItem("id_full", "4", ListIcons.Profile, () =>
                {
                    PhoneBrightness = 4;
                }));
                BrightnessApp.AddItem(new AppSettingItem("id_full", "3", ListIcons.Profile, () =>
                {
                    PhoneBrightness = 3;
                }));
                BrightnessApp.AddItem(new AppSettingItem("id_full", "2", ListIcons.Profile, () =>
                {
                    PhoneBrightness = 2;
                }));
                BrightnessApp.AddItem(new AppSettingItem("id_full", "Minimum", ListIcons.Profile, () =>
                {
                    PhoneBrightness = 1;
                }));
                //

                ColorApp.AddItem(new AppSettingItem("id_red", "Red", ListIcons.Profile, () =>
                {
                    SetPhoneColor((int)Theme.Red - 1);
                    RefreshDisplay();
                }));
                ColorApp.AddItem(new AppSettingItem("id_green", "Green", ListIcons.Profile, () =>
                {
                    SetPhoneColor((int)Theme.Green - 1);
                    RefreshDisplay();
                }));
                ColorApp.AddItem(new AppSettingItem("id_blue", "Blue", ListIcons.Profile, () =>
                {
                    SetPhoneColor((int)Theme.LightBlue - 1);
                    RefreshDisplay();
                }));
                ColorApp.AddItem(new AppSettingItem("id_orange", "Orange", ListIcons.Profile, () =>
                {
                    SetPhoneColor((int)Theme.Orange - 1);
                    RefreshDisplay();
                }));
                ColorApp.AddItem(new AppSettingItem("id_pink", "Pink", ListIcons.Profile, () =>
                {
                    SetPhoneColor((int)Theme.Pink - 1);
                    RefreshDisplay();
                }));
                ColorApp.AddItem(new AppSettingItem("id_purple", "Purple", ListIcons.Profile, () =>
                {
                    SetPhoneColor((int)Theme.Purple - 1);
                    RefreshDisplay();
                }));
                ColorApp.AddItem(new AppSettingItem("id_grey", "Grey", ListIcons.Profile, () =>
                {
                    SetPhoneColor((int)Theme.Grey - 1);
                    RefreshDisplay();
                }));

                VehicleTemplate.AddItem(new AppSettingItem("id_engine_stat", "ENGINE: OFF", ListIcons.None, null));
                VehicleTemplate.AddItem(new AppSettingItem("id_lights_stat", "LIGHTS: OFF", ListIcons.None, null));
                VehicleTemplate.AddItem(new AppSettingItem("id_locked_stat", "LOCKED: FALSE", ListIcons.None, null));
                VehicleTemplate.AddItem(new AppSettingItem("id_alarm_stat", "ALARM: OFF", ListIcons.None, null));
                VehicleTemplate.AddItem(new AppSettingItem("id_engine", "Engine", ListIcons.None, null, VehicleTemplate_Engine));
                VehicleTemplate.AddItem(new AppSettingItem("id_alarm", "Alarm", ListIcons.None, null, VehicleTemplate_Alarm));
                VehicleTemplate.AddItem(new AppSettingItem("id_doors", "Doors", ListIcons.None, null, VehicleTemplate_Doors));
                VehicleTemplate.AddItem(new AppSettingItem("id_lights", "Lights", ListIcons.None, null, VehicleTemplate_Lights));
                VehicleTemplate.AddItem(new AppSettingItem("id_windows", "Windows", ListIcons.None, null, VehicleTemplate_Windows));
                VehicleTemplate.AddItem(new AppSettingItem("id_neons", "Neons", ListIcons.None, null, VehicleTemplate_Neons));

                VehicleTemplate3.AddItem(new AppSettingItem("id_waypoint", "Set Waypoint", ListIcons.None, () =>
                {
                    if (activatedCar != null)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_WAYPOINT_OFF);
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_NEW_WAYPOINT, activatedCar.Position.X, activatedCar.Position.Y);

                    }
                }));

                VehicleTemplate3.AddItem(new AppSettingItem("id_request", "Request Vehicle", ListIcons.None, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (activatedCar.Handle.ClassType == VehicleClass.Helicopters || activatedCar.Handle.ClassType == VehicleClass.Boats || activatedCar.Handle.ClassType == VehicleClass.Planes
                                || activatedCar.Handle.ClassType == VehicleClass.Trains)
                                {
                                    GTA.UI.Screen.ShowSubtitle("Cannot request obscure vehicles.", 3000);
                                }
                                else
                                {

                                    if (activatedCar.Handle == Game.Player.Character.CurrentVehicle)
                                    {
                                        GTA.UI.Screen.ShowSubtitle("You are already in that vehicle.", 3000);
                                        return;
                                    }
                                    GTA.Native.OutputArgument arg1 = new GTA.Native.OutputArgument();
                                    GTA.Native.OutputArgument arg2 = new GTA.Native.OutputArgument();
                                    GTA.Native.OutputArgument arg3 = new GTA.Native.OutputArgument();
                                    Random r = new Random();
                                    bool gotPos = GTA.Native.Function.Call<bool>(GTA.Native.Hash.GET_NTH_CLOSEST_VEHICLE_NODE_WITH_HEADING, Game.Player.Character.Position.X, Game.Player.Character.Position.Y, Game.Player.Character.Position.Z, r.Next(30, 125), arg1, arg2, arg3, 1, 3f, 0f);
                                    Vector3 pos = arg1.GetResult<Vector3>();
                                    float head = arg2.GetResult<float>();
                                    int x = arg3.GetResult<int>();
                                    if (gotPos)
                                    {
                                        activatedCar.Handle.Position = pos;
                                        activatedCar.Handle.Heading = head;
                                        activatedCar.Handle.SteeringAngle = 0f;
                                        //activatedCar.Handle.IsEngineRunning = true;
                                        GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_ENGINE_ON, activatedCar.Handle.Handle, true, true, false);
                                        activatedCar.Handle.IsHandbrakeForcedOn = false;
                                        activatedCar.Handle.IsInvincible = true;
                                        activatedCar.Handle.CanBeVisiblyDamaged = false;
                                        activatedCar.Handle.LockStatus = VehicleLockStatus.Unlocked;
                                        GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_DOORS_SHUT, activatedCar.Handle, true);
                                        activatedCar.Handle.PlaceOnGround();
                                        Ped ped = World.CreatePed(PedHash.Xmech01SMY, pos);
                                        ped.IsPersistent = true;
                                        ped.SetIntoVehicle(activatedCar.Handle, VehicleSeat.Driver);
                                        ped.IsInvincible = true;
                                        //GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_ALPHA, ped.Handle, 0, false);
                                        if (AdvancedPersistence.AttachedTasks.ContainsKey(activatedCar.Handle))
                                        {
                                            PedTask t = AdvancedPersistence.AttachedTasks[activatedCar.Handle];
                                            t.Clean();
                                            if (t.Ped != null)
                                            {
                                                GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS_IMMEDIATELY, t.Ped.Handle);
                                                GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE, t.Ped.Handle, activatedCar.Handle.Handle, 16);
                                                //t.Ped.IsPersistent = false;
                                                t.Ped.IsPersistent = true;
                                                while (t.Ped.Exists())
                                                {
                                                    t.Ped.Delete();
                                                }
                                            }
                                        }
                                        PedTask tsk = new PedTask(ped);
                                        tsk.Open();
                                        tsk.DriveTo(activatedCar.Handle, Game.Player.Character.Position + Game.Player.Character.ForwardVector * 4, 5f, 20f, DrivingStyle.AvoidTraffic);
                                        tsk.Brake(activatedCar.Handle);
                                        tsk.ExitVehicle(activatedCar.Handle, true);
                                        tsk.FleeCoords(activatedCar.Handle.Position);
                                        tsk.Wander();
                                        tsk.Close();
                                        AdvancedPersistence.AttachedTasks[activatedCar.Handle] = tsk;
                                        ped.AlwaysKeepTask = true;
                                        tsk.Run();
                                        GTA.UI.Screen.ShowSubtitle("Your mechanic is on his way! Be patient!\n(Request again if spawn position is undesired)", 7500);
                                    }
                                }
                            }
                        }
                    }
                }));
                VehicleTemplate2.AddItem(new AppSettingItem("id_spawn", "Spawn Vehicle", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                    {
                        activatedCar.WasUserDespawned = false;
                        if (activatedCar.Handle == null)
                        {
                            AdvancedPersistence.CreateVehicle(activatedCar);
                        }
                        else if (activatedCar.Handle != null)
                        {
                            if (!activatedCar.Handle.Exists())
                            {
                                AdvancedPersistence.CreateVehicle(activatedCar);
                            }
                        }
                    }
                }));
                VehicleTemplate2.AddItem(new AppSettingItem("id_despawn", "Despawn Vehicle", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            AdvancedPersistence.SaveVehicleData(activatedCar.Handle, activatedCar);
                            activatedCar.WasUserDespawned = true;
                            AdvancedPersistence.DeleteBlipsOnCar(activatedCar.Handle);
                            if (AdvancedPersistence.AttachedVehicles.ContainsKey(activatedCar.Handle))
                            {
                                AdvancedPersistence.AttachedVehicles.Remove(activatedCar.Handle);
                            }


                            if (activatedCar.Handle.Exists())
                            {
                                GTA.Native.OutputArgument arg1 = new GTA.Native.OutputArgument(activatedCar.Handle);
                                GTA.Native.Function.Call(GTA.Native.Hash.DELETE_VEHICLE, arg1);
                            }

                            if (AdvancedPersistence.AttachedTasks.ContainsKey(activatedCar.Handle))
                            {
                                AdvancedPersistence.AttachedTasks[activatedCar.Handle].Clean();
                                if (AdvancedPersistence.AttachedTasks[activatedCar.Handle].Ped != null)
                                {
                                    GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS_IMMEDIATELY, AdvancedPersistence.AttachedTasks[activatedCar.Handle].Ped.Handle);
                                    GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE, AdvancedPersistence.AttachedTasks[activatedCar.Handle].Ped.Handle, AdvancedPersistence.AttachedTasks[activatedCar.Handle].Handle, 16);

                                    if (AdvancedPersistence.AttachedTasks[activatedCar.Handle].Ped.Exists())
                                    {
                                        AdvancedPersistence.AttachedTasks[activatedCar.Handle].Ped.IsPersistent = true;
                                        AdvancedPersistence.AttachedTasks[activatedCar.Handle].Ped.Delete();
                                    }
                                }
                                AdvancedPersistence.AttachedTasks.Remove(activatedCar.Handle);
                            }

                            activatedCar.Handle = null;
                        }
                    }
                }));
                VehicleTemplate2.AddItem(new AppSettingItem("id_spawn", "Move To Safe Spawn", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if(activatedCar.SafeSpawnSet)
                                {
                                    activatedCar.Handle.Position = activatedCar.SafeSpawn;
                                    activatedCar.Handle.Rotation = activatedCar.SafeRotation;
                                    GTA.UI.Screen.ShowSubtitle("Moved!");
                                }
                                else
                                {
                                    GTA.UI.Screen.ShowSubtitle("Safe Spawn Not Set!");
                                }
                            }
                            else
                            {
                                GTA.UI.Screen.ShowSubtitle("Vehicle Not Spawned!");
                            }
                        }
                        else
                        {
                            GTA.UI.Screen.ShowSubtitle("Vehicle Not Spawned!");
                        }
                    }
                }));
                VehicleTemplate2.AddItem(new AppSettingItem("id_setspawn", "Set Safe Spawn", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                activatedCar.SafeSpawn = activatedCar.Handle.Position;
                                activatedCar.SafeSpawnSet = true;
                                activatedCar.SafeRotation = activatedCar.Handle.Rotation;
                                GTA.UI.Screen.ShowSubtitle("Vehicle Spawn Set!");
                            }
                            else
                            {
                                GTA.UI.Screen.ShowSubtitle("Vehicle Does Not Exist!");
                            }
                        }
                        else
                        {
                            GTA.UI.Screen.ShowSubtitle("Vehicle Does Not Exist!");
                        }
                    }
                    else
                    {
                        GTA.UI.Screen.ShowSubtitle("Vehicle Does Not Exist!");
                    }

                }));
                VehicleTemplate4.AddItem(new AppSettingItem("id_anchor_status", "ANCHOR: FALSE", ListIcons.None));
                VehicleTemplate4.AddItem(new AppSettingItem("id_anchoring", "Anchoring", ListIcons.None, null, VehicleTemplate_Anchor));
                VehicleTemplate_Anchor.AddItem(new AppSettingItem("id_anchor", "Anchor Boat", ListIcons.None, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            GTA.Native.Function.Call(GTA.Native.Hash.SET_BOAT_ANCHOR, activatedCar.Handle.Handle, true);
                            VehicleTemplate4.GetItemByID<AppSettingItem>("id_anchor_status").Name = "ANCHOR: TRUE";
                        }
                    }
                }));

                VehicleTemplate_Anchor.AddItem(new AppSettingItem("id_unanchor", "Unanchor Boat", ListIcons.None, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            GTA.Native.Function.Call(GTA.Native.Hash.SET_BOAT_ANCHOR, activatedCar.Handle.Handle, false);
                            VehicleTemplate4.GetItemByID<AppSettingItem>("id_anchor_status").Name = "ANCHOR: FALSE";
                        }
                    }
                }));

                VehicleTemplate2.AddItem(new AppSettingItem("id_fix", "Fix Vehicle", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                activatedCar.Handle.Repair();
                            }
                        }
                    }
                }));
                VehicleTemplate2.AddItem(new AppSettingItem("id_sogp", "Set On Ground Properly", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                activatedCar.Handle.PlaceOnGround();
                            }
                        }
                    }
                }));
                VehicleTemplate2.AddItem(new AppSettingItem("id_blip", "Set Blip Color", ListIcons.Checklist, null, BlipApp));

                VehicleTemplate2.AddItem(new AppSettingItem("id_tags", "Set Vehicle Name", ListIcons.Checklist, () =>
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.DISPLAY_ONSCREEN_KEYBOARD, 1, "", "", "New Name", "", "", "", 20);
                    while (GTA.Native.Function.Call<int>(GTA.Native.Hash.UPDATE_ONSCREEN_KEYBOARD) == 0)
                    {
                        Script.Yield();
                    }
                    if (GTA.Native.Function.Call<int>(GTA.Native.Hash.UPDATE_ONSCREEN_KEYBOARD) == 2)
                    {

                    }
                    else
                    {
                        string x = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_ONSCREEN_KEYBOARD_RESULT);
                        if (x != null)
                        {
                            if (x != "")
                            {
                                if (activatedCar != null)
                                    activatedCar.Tag = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_ONSCREEN_KEYBOARD_RESULT);
                                RedoScroll();
                                OpenApp(VehicleTemplate2, 5);
                            }
                        }
                    }
                }));

                BlipApp.AddItem(new AppSettingItem("id_red", "Red", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                        activatedCar.BlipColor = 1;
                    if (activatedCar.Handle != null)
                    {
                        if(ModSettings.EnableBlips)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (activatedCar.Handle.AttachedBlip != null)
                                {
                                    activatedCar.Handle.AttachedBlip.Color = BlipColor.Red;
                                }
                            }
                        }
                    }
                }));
                BlipApp.AddItem(new AppSettingItem("id_green", "Green", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                        activatedCar.BlipColor = 2;
                    if (activatedCar.Handle != null)
                    {
                        if (ModSettings.EnableBlips)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (activatedCar.Handle.AttachedBlip != null)
                                {
                                    activatedCar.Handle.AttachedBlip.Color = BlipColor.Green;
                                }
                            }
                        }
                    }
                }));
                BlipApp.AddItem(new AppSettingItem("id_blue", "Blue", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                        activatedCar.BlipColor = 3;
                    if (activatedCar.Handle != null)
                    {
                        if (ModSettings.EnableBlips)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (activatedCar.Handle.AttachedBlip != null)
                                {
                                    activatedCar.Handle.AttachedBlip.Color = BlipColor.Blue;
                                }
                            }
                        }
                    }
                }));
                BlipApp.AddItem(new AppSettingItem("id_white", "White", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                        activatedCar.BlipColor = 0;
                    if (activatedCar.Handle != null)
                    {
                        if (ModSettings.EnableBlips)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (activatedCar.Handle.AttachedBlip != null)
                                {
                                    activatedCar.Handle.AttachedBlip.Color = BlipColor.White;
                                }
                            }
                        }
                    }
                }));
                BlipApp.AddItem(new AppSettingItem("id_yellow", "Yellow", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                        activatedCar.BlipColor = 66;
                    if (activatedCar.Handle != null)
                    {
                        if (ModSettings.EnableBlips)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (activatedCar.Handle.AttachedBlip != null)
                                {
                                    activatedCar.Handle.AttachedBlip.Color = BlipColor.Yellow;
                                }
                            }
                        }
                    }
                }));
                BlipApp.AddItem(new AppSettingItem("id_orange", "Orange", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                        activatedCar.BlipColor = 51;
                    if (activatedCar.Handle != null)
                    {
                        if (ModSettings.EnableBlips)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (activatedCar.Handle.AttachedBlip != null)
                                {
                                    activatedCar.Handle.AttachedBlip.Color = BlipColor.Orange;
                                }
                            }
                        }
                    }
                }));
                BlipApp.AddItem(new AppSettingItem("id_pink", "Pink", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                        activatedCar.BlipColor = 8;
                    if (activatedCar.Handle != null)
                    {
                        if(ModSettings.EnableBlips)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (activatedCar.Handle.AttachedBlip != null)
                                {
                                    activatedCar.Handle.AttachedBlip.Color = BlipColor.NetPlayer3;
                                }
                            }
                        }
                    }
                }));
                BlipApp.AddItem(new AppSettingItem("id_purple", "Purple", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                        activatedCar.BlipColor = 50;
                    if (activatedCar.Handle != null)
                    {
                        if (ModSettings.EnableBlips)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (activatedCar.Handle.AttachedBlip != null)
                                {
                                    activatedCar.Handle.AttachedBlip.Color = BlipColor.Purple;
                                }
                            }
                        }
                    }
                }));
                BlipApp.AddItem(new AppSettingItem("id_brown", "Brown", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                        activatedCar.BlipColor = 31;
                    if (activatedCar.Handle != null)
                    {
                        if (ModSettings.EnableBlips)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (activatedCar.Handle.AttachedBlip != null)
                                {
                                    activatedCar.Handle.AttachedBlip.Color = BlipColor.NetPlayer26;
                                }
                            }
                        }
                    }
                }));
                BlipApp.AddItem(new AppSettingItem("id_grey", "Grey", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                        activatedCar.BlipColor = 55;
                    if (activatedCar.Handle != null)
                    {
                        if (ModSettings.EnableBlips)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (activatedCar.Handle.AttachedBlip != null)
                                {
                                    activatedCar.Handle.AttachedBlip.Color = BlipColor.Grey;
                                }
                            }
                        }
                    }
                }));
                BlipApp.AddItem(new AppSettingItem("id_dgrey", "Dark Grey", ListIcons.Attachment, () =>
                {
                    if (activatedCar != null)
                        activatedCar.BlipColor = 40;
                    if (activatedCar.Handle != null)
                    {
                        if (ModSettings.EnableBlips)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (activatedCar.Handle.AttachedBlip != null)
                                {
                                    activatedCar.Handle.AttachedBlip.Color = BlipColor.GreyDark;
                                }
                            }
                        }
                    }
                }));
                CurrentVehicleApp.AddItem(new AppSettingItem("id_add", "Track Vehicle", ListIcons.None, () =>
                {
                    if (Game.Player.Character.CurrentVehicle != null)
                    {
                        if (AdvancedPersistence.AttachedVehicles.ContainsKey(Game.Player.Character.CurrentVehicle))
                        {
                            GTA.UI.Notification.Show($"ERROR: Vehicle already tracked");
                        }
                        else
                        {
                            if (AdvancedPersistence.VehicleDatabase.Count >= ModSettings.MaxNumberOfCars)
                            {
                                GTA.UI.Notification.Show($"ERROR: Max Vehicles Reached [{ModSettings.MaxNumberOfCars}]");
                            }
                            else
                            {
                                Vehicle veh = Game.Player.Character.CurrentVehicle;
                                VehicleDataV1 dat = new VehicleDataV1();

                                AdvancedPersistence.SaveVehicleData(veh, dat);
                                dat.SafeSpawn = veh.Position;
                                dat.SafeSpawnSet = true;
                                dat.SafeRotation = veh.Rotation;
                                if(ModSettings.EnableBlips)
                                {
                                    if (veh.AttachedBlip == null)
                                        veh.AddBlip();
                                    if (veh.Model.IsHelicopter)
                                        veh.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                                    else if (veh.Model.IsAmphibiousQuadBike || veh.Model.IsBicycle || veh.Model.IsBike || veh.Model.IsQuadBike)
                                        veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                    else if (veh.Model.IsJetSki)
                                        veh.AttachedBlip.Sprite = BlipSprite.Seashark;
                                    else if (veh.Model.IsBoat)
                                        veh.AttachedBlip.Sprite = BlipSprite.Boat;
                                    else if (veh.Model.IsPlane)
                                        veh.AttachedBlip.Sprite = BlipSprite.Plane;
                                    else
                                        veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                    veh.AttachedBlip.IsShortRange = true;
                                    veh.AttachedBlip.Scale = 0.75f;
                                    veh.AttachedBlip.Name = "Saved Vehicle";
                                    veh.AttachedBlip.Alpha = 255;
                                    veh.AttachedBlip.Priority = 0;
                                    GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, veh.AttachedBlip.Handle, false);
                                    veh.AttachedBlip.Color = (BlipColor)dat.BlipColor;
                                    GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, veh.AttachedBlip.Handle, false);
                                }
                                AdvancedPersistence.MainCharacter.CarAttach = dat.Id;
                                AdvancedPersistence.AttachedVehicles[veh] = dat;
                                AdvancedPersistence.VehicleDatabase.Add(dat);
                                AdvancedPersistence.VehicleMetabase.Add(dat.Meta);
                                GTA.UI.Notification.Show($"Vehicle Added [{AdvancedPersistence.VehicleDatabase.Count}]");
                                GUI.Phone.RedoScroll();
                                if (GUI.Phone.GetHomeObjectByIndex(0) != null)
                                {
                                    GUI.Phone.GetHomeObjectByIndex(0).NotificationNumber = AdvancedPersistence.VehicleDatabase.Count;
                                }
                                RefreshDisplay(true);
                            }
                        }
                    }
                    else
                    {
                        GTA.UI.Notification.Show("ERROR: Not in vehicle");
                    }
                }));

                CurrentVehicleApp.AddItem(new AppSettingItem("id_remove", "Untrack Vehicle", ListIcons.None, () =>
                {
                    if (Game.Player.Character.CurrentVehicle != null)
                    {
                        if (AdvancedPersistence.AttachedVehicles.ContainsKey(Game.Player.Character.CurrentVehicle))
                        {
                            Vehicle veh = Game.Player.Character.CurrentVehicle;
                            VehicleDataV1 dat = AdvancedPersistence.AttachedVehicles[veh];
                            AdvancedPersistence.AttachedVehicles.Remove(veh);
                            AdvancedPersistence.VehicleDatabase.Remove(dat);
                            AdvancedPersistence.VehicleMetabase.Remove(dat.Meta);
                            if (veh.AttachedBlip != null)
                                veh.AttachedBlip.Delete();
                            foreach (Blip blip in veh.AttachedBlips)
                            {
                                blip.Delete();
                            }
                            veh.IsPersistent = false;
                            GTA.UI.Notification.Show($"Vehicle Removed [{AdvancedPersistence.VehicleDatabase.Count}]");
                            GUI.Phone.RedoScroll();
                            if (GUI.Phone.GetHomeObjectByIndex(0) != null)
                            {
                                GUI.Phone.GetHomeObjectByIndex(0).NotificationNumber = AdvancedPersistence.VehicleDatabase.Count;
                            }
                            RefreshDisplay(true);
                        }
                        else
                        {
                            GTA.UI.Notification.Show($"ERROR: Vehicle already untracked");
                        }
                    }
                    else
                    {
                        GTA.UI.Notification.Show("ERROR: Not in vehicle");
                    }
                }));

                VehicleTemplate2.AddItem(new AppSettingItem("id_warn1", "----------WARNING----------", ListIcons.None));
                VehicleTemplate2.AddItem(new AppSettingItem("id_delete", "Delete Vehicle", ListIcons.Attachment, () =>
                {
                    AdvancedPersistence.VehicleDatabase.Remove(activatedCar);
                    AdvancedPersistence.VehicleMetabase.Remove(activatedCar.Meta);
                    if(activatedCar.Handle != null)
                    {
                        AdvancedPersistence.AttachedVehicles.Remove(activatedCar.Handle);
                        if (AdvancedPersistence.AttachedTasks.ContainsKey(activatedCar.Handle))
                        {
                            AdvancedPersistence.AttachedTasks[activatedCar.Handle].Clean();
                            if (AdvancedPersistence.AttachedTasks[activatedCar.Handle].Ped != null)
                            {
                                GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS_IMMEDIATELY, AdvancedPersistence.AttachedTasks[activatedCar.Handle].Ped.Handle);
                                GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE, AdvancedPersistence.AttachedTasks[activatedCar.Handle].Ped.Handle, AdvancedPersistence.AttachedTasks[activatedCar.Handle].Handle, 16);
                                if(AdvancedPersistence.AttachedTasks[activatedCar.Handle].Ped.Exists())
                                {
                                    AdvancedPersistence.AttachedTasks[activatedCar.Handle].Ped.IsPersistent = true;
                                    AdvancedPersistence.AttachedTasks[activatedCar.Handle].Ped.Delete();
                                }
                            }
                            AdvancedPersistence.AttachedTasks.Remove(activatedCar.Handle);
                        }
                        if (activatedCar.Handle.AttachedBlip != null)
                            activatedCar.Handle.AttachedBlip.Delete();
                        foreach (Blip blip in activatedCar.Handle.AttachedBlips)
                        {
                            blip.Delete();
                        }
                        GTA.Native.OutputArgument arg1 = new GTA.Native.OutputArgument(activatedCar.Handle);
                        GTA.Native.Function.Call(GTA.Native.Hash.DELETE_VEHICLE, arg1);
                        activatedCar.Handle = null;
                        activatedCar = null;
                        AdvancedPersistence.DrawTrace = false;
                    }
                    GTA.UI.Notification.Show($"Vehicle Deleted [{AdvancedPersistence.VehicleDatabase.Count}]");
                    GUI.Phone.RedoScroll();
                    if (GUI.Phone.GetHomeObjectByIndex(0) != null)
                    {
                        GUI.Phone.GetHomeObjectByIndex(0).NotificationNumber = AdvancedPersistence.VehicleDatabase.Count;
                    }
                }, AppScroll));
                VehicleTemplate2.AddItem(new AppSettingItem("id_warn2", "----------WARNING----------", ListIcons.None));
                VehicleTemplate_Doors.AddItem(new AppSettingItem("id_door_state", "[OPEN] | CLOSE", ListIcons.Settings1, () =>
                {
                    
                    if (doorState)
                        VehicleTemplate_Doors.GetItemByID<AppSettingItem>("id_door_state").Name = "OPEN | [CLOSE]";
                    else
                        VehicleTemplate_Doors.GetItemByID<AppSettingItem>("id_door_state").Name = "[OPEN] | CLOSE";
                    doorState = !doorState;
                    RefreshDisplay();
                }));

                VehicleTemplate_Doors.AddItem(new AppSettingItem("id_hood", "Hood", ListIcons.Checklist, () =>
                {
                    if(activatedCar != null)
                    {
                        if(activatedCar.Handle != null)
                        {
                            if(activatedCar.Handle.Exists())
                            {
                                if (doorState)
                                    activatedCar.Handle.Doors[VehicleDoorIndex.Hood].Open();
                                else
                                    activatedCar.Handle.Doors[VehicleDoorIndex.Hood].Close();
                            }
                        }
                    }
                }));

                VehicleTemplate_Doors.AddItem(new AppSettingItem("id_trunk", "Trunk", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (doorState)
                                    activatedCar.Handle.Doors[VehicleDoorIndex.Trunk].Open();
                                else
                                    activatedCar.Handle.Doors[VehicleDoorIndex.Trunk].Close();
                            }
                        }
                    }
                }));

                VehicleTemplate_Doors.AddItem(new AppSettingItem("id_lfd", "Left Front Door", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (doorState)
                                    activatedCar.Handle.Doors[VehicleDoorIndex.FrontLeftDoor].Open();
                                else
                                    activatedCar.Handle.Doors[VehicleDoorIndex.FrontLeftDoor].Close();
                            }
                        }
                    }
                }));

                VehicleTemplate_Doors.AddItem(new AppSettingItem("id_rfd", "Right Front Door", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (doorState)
                                    activatedCar.Handle.Doors[VehicleDoorIndex.FrontRightDoor].Open();
                                else
                                    activatedCar.Handle.Doors[VehicleDoorIndex.FrontRightDoor].Close();
                            }
                        }
                    }
                }));

                VehicleTemplate_Doors.AddItem(new AppSettingItem("id_lbd", "Left Back Door", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (doorState)
                                    activatedCar.Handle.Doors[VehicleDoorIndex.BackLeftDoor].Open();
                                else
                                    activatedCar.Handle.Doors[VehicleDoorIndex.BackLeftDoor].Close();
                            }
                        }
                    }
                }));

                VehicleTemplate_Doors.AddItem(new AppSettingItem("id_rbd", "Right Back Door", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (doorState)
                                    activatedCar.Handle.Doors[VehicleDoorIndex.BackRightDoor].Open();
                                else
                                    activatedCar.Handle.Doors[VehicleDoorIndex.BackRightDoor].Close();
                            }
                        }
                    }
                }));

                VehicleTemplate_Engine.AddItem(new AppSettingItem("id_engine_on", "Turn Engine ON", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                //activatedCar.Handle.IsEngineRunning = true;
                                GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_ENGINE_ON, activatedCar.Handle.Handle, true, true, false);
                                VehicleTemplate.GetItemByID<AppSettingItem>("id_engine_stat").Name = "ENGINE: ON";
                            }
                        }
                    }
                }));

                VehicleTemplate_Engine.AddItem(new AppSettingItem("id_engine_off", "Turn Engine OFF", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                //activatedCar.Handle.IsEngineRunning = false;
                                GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_ENGINE_ON, activatedCar.Handle.Handle, false, true, false);
                                VehicleTemplate.GetItemByID<AppSettingItem>("id_engine_stat").Name = "ENGINE: OFF";
                            }
                        }
                    }
                }));

                VehicleTemplate_Lights.AddItem(new AppSettingItem("id_lights_on", "Turn Lights ON", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                activatedCar.Handle.AreLightsOn = true;
                                VehicleTemplate.GetItemByID<AppSettingItem>("id_lights_stat").Name = "LIGHTS: ON";
                            }
                        }
                    }
                }));

                VehicleTemplate_Lights.AddItem(new AppSettingItem("id_lights_off", "Turn Lights OFF", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                activatedCar.Handle.AreLightsOn = false;
                                VehicleTemplate.GetItemByID<AppSettingItem>("id_lights_stat").Name = "LIGHTS: OFF";
                            }
                        }
                    }
                }));

                VehicleTemplate_Alarm.AddItem(new AppSettingItem("id_lock_on", "Lock Vehicle", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                activatedCar.Handle.LockStatus = VehicleLockStatus.CannotEnter;
                                VehicleTemplate.GetItemByID<AppSettingItem>("id_locked_stat").Name = "LOCKED: TRUE";
                            }
                        }
                    }
                }));

                VehicleTemplate_Alarm.AddItem(new AppSettingItem("id_lock_off", "Unlock Vehicle", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                activatedCar.Handle.LockStatus = VehicleLockStatus.Unlocked;
                                VehicleTemplate.GetItemByID<AppSettingItem>("id_locked_stat").Name = "LOCKED: FALSE";
                            }
                        }
                    }
                }));

                VehicleTemplate_Alarm.AddItem(new AppSettingItem("id_alarm_on", "Enable Alarm", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                activatedCar.Handle.IsAlarmSet = true;
                                VehicleTemplate.GetItemByID<AppSettingItem>("id_alarm_stat").Name = "ALARM: ON";
                            }
                        }
                    }
                }));

                VehicleTemplate_Alarm.AddItem(new AppSettingItem("id_alarm_off", "Disable Alarm", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                activatedCar.Handle.IsAlarmSet = false;
                                VehicleTemplate.GetItemByID<AppSettingItem>("id_alarm_stat").Name = "ALARM: OFF";
                            }
                        }
                    }
                }));

                VehicleTemplate_Windows.AddItem(new AppSettingItem("id_window_state", "[OPEN] | CLOSE", ListIcons.Settings1, () =>
                {
                    if (windowState)
                        VehicleTemplate_Windows.GetItemByID<AppSettingItem>("id_window_state").Name = "OPEN | [CLOSE]";
                    else
                        VehicleTemplate_Windows.GetItemByID<AppSettingItem>("id_window_state").Name = "[OPEN] | CLOSE";
                    windowState = !windowState;
                    RefreshDisplay();
                }));

                VehicleTemplate_Windows.AddItem(new AppSettingItem("id_flw", "Front Left Window", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (windowState)
                                    activatedCar.Handle.Windows[VehicleWindowIndex.FrontLeftWindow].RollDown();
                                else
                                    activatedCar.Handle.Windows[VehicleWindowIndex.FrontLeftWindow].RollUp();
                            }
                        }
                    }
                }));

                VehicleTemplate_Windows.AddItem(new AppSettingItem("id_frw", "Front Right Window", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (windowState)
                                    activatedCar.Handle.Windows[VehicleWindowIndex.FrontRightWindow].RollUp();
                                else
                                    activatedCar.Handle.Windows[VehicleWindowIndex.FrontRightWindow].RollDown();
                            }
                        }
                    }
                }));

                VehicleTemplate_Windows.AddItem(new AppSettingItem("id_blw", "Back Left Window", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (windowState)
                                    activatedCar.Handle.Windows[VehicleWindowIndex.BackLeftWindow].RollUp();
                                else
                                    activatedCar.Handle.Windows[VehicleWindowIndex.BackLeftWindow].RollDown();
                            }
                        }
                    }
                }));

                VehicleTemplate_Windows.AddItem(new AppSettingItem("id_brw", "Back Right Window", ListIcons.Checklist, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                if (windowState)
                                    activatedCar.Handle.Windows[VehicleWindowIndex.BackRightWindow].RollUp();
                                else
                                    activatedCar.Handle.Windows[VehicleWindowIndex.BackRightWindow].RollDown();
                            }
                        }
                    }
                }));

                VehicleTemplate_Neons.AddItem(new AppSettingItem("id_state", "[ON] | OFF", ListIcons.None, () =>
                {
                    neonState = !neonState;
                    if (neonState)
                        VehicleTemplate_Neons.GetItemByID<AppSettingItem>("id_state").Name = "[ON] | OFF";
                    else
                        VehicleTemplate_Neons.GetItemByID<AppSettingItem>("id_state").Name = "ON | [OFF]";
                    RefreshDisplay();
                }));

                VehicleTemplate_Neons.AddItem(new AppSettingItem("id_ln", "Left Neon", ListIcons.None, () =>
                {
                    if(activatedCar != null)
                    {
                        if(activatedCar.Handle != null)
                        {
                            if(activatedCar.Handle.Exists())
                            {
                                activatedCar.Handle.Mods.SetNeonLightsOn(VehicleNeonLight.Left, neonState);
                            }
                        }
                    }
                }));

                VehicleTemplate_Neons.AddItem(new AppSettingItem("id_rn", "Right Neon", ListIcons.None, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                activatedCar.Handle.Mods.SetNeonLightsOn(VehicleNeonLight.Right, neonState);
                            }
                        }
                    }
                }));

                VehicleTemplate_Neons.AddItem(new AppSettingItem("id_lf", "Front Neon", ListIcons.None, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                activatedCar.Handle.Mods.SetNeonLightsOn(VehicleNeonLight.Front, neonState);
                            }
                        }
                    }
                }));

                VehicleTemplate_Neons.AddItem(new AppSettingItem("id_bn", "Back Neon", ListIcons.None, () =>
                {
                    if (activatedCar != null)
                    {
                        if (activatedCar.Handle != null)
                        {
                            if (activatedCar.Handle.Exists())
                            {
                                activatedCar.Handle.Mods.SetNeonLightsOn(VehicleNeonLight.Back, neonState);
                            }
                        }
                    }
                }));

                int nonMod = 0;
                foreach(Vehicle veh in World.GetAllVehicles())
                {
                    if(veh.IsPersistent)
                    {
                        if(!AdvancedPersistence.AttachedVehicles.ContainsKey(veh))
                        {
                            nonMod++;
                        }
                    }
                }

                Converter.AddItem(new AppSettingItem("id_num", "Found cars: " + nonMod.ToString(), ListIcons.Attachment));
                Converter.AddItem(new AppSettingItem("id_conv", "Convert All", ListIcons.Ticked, () =>
                {
                    int mm = 0;
                    foreach (Vehicle veh in World.GetAllVehicles())
                    {
                        if (veh.IsPersistent)
                        {
                            if (!AdvancedPersistence.AttachedVehicles.ContainsKey(veh))
                            {
                                VehicleDataV1 dat = new VehicleDataV1();
                                AdvancedPersistence.SaveVehicleData(veh, dat);
                                dat.SafeSpawn = veh.Position;
                                dat.SafeSpawnSet = true;
                                dat.SafeRotation = veh.Rotation;
                                if(ModSettings.EnableBlips)
                                {
                                    if (veh.AttachedBlip == null)
                                        veh.AddBlip();
                                    if (veh.Model.IsHelicopter)
                                        veh.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                                    else if (veh.Model.IsAmphibiousQuadBike || veh.Model.IsBicycle || veh.Model.IsBike || veh.Model.IsQuadBike)
                                        veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                    else if (veh.Model.IsJetSki)
                                        veh.AttachedBlip.Sprite = BlipSprite.Seashark;
                                    else if (veh.Model.IsBoat)
                                        veh.AttachedBlip.Sprite = BlipSprite.Boat;
                                    else if (veh.Model.IsPlane)
                                        veh.AttachedBlip.Sprite = BlipSprite.Plane;
                                    else
                                        veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                    veh.AttachedBlip.IsShortRange = true;
                                    veh.AttachedBlip.Scale = 0.75f;
                                    veh.AttachedBlip.Name = "Saved Vehicle";
                                    veh.AttachedBlip.Alpha = 255;
                                    veh.AttachedBlip.Priority = 0;
                                    GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, veh.AttachedBlip.Handle, false);
                                    veh.AttachedBlip.Color = (BlipColor)dat.BlipColor;
                                    GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, veh.AttachedBlip.Handle, false);
                                }
                                AdvancedPersistence.AttachedVehicles[veh] = dat;
                                AdvancedPersistence.VehicleDatabase.Add(dat);
                                AdvancedPersistence.VehicleMetabase.Add(dat.Meta);
                                mm++;
                            }
                        }
                    }

                    if(GetHomeObjectByIndex(6) != null)
                    {
                        GetHomeObjectByIndex(6).NotificationNumber = 0;
                    }

                    if(mm > 0)
                        GTA.UI.Screen.ShowSubtitle($"({mm}) Vehicles Converted, PLEASE RESTART YOUR GAME AND REMOVE THE OTHER MOD", 10000);
                    else
                        GTA.UI.Screen.ShowSubtitle($"No Vehicles Found", 5000);
                    Converter.GetItemByID<AppSettingItem>("id_num").Name = "Found cars: 0";
                    RefreshDisplay();
                }));

                SetHomeObjectsData(
                    new HomeObject("My Vehicles", HomeIcon.PlayerList, HomescreenLocation.TopLeft, AdvancedPersistence.VehicleDatabase.Count, AppScroll), //0
                    new HomeObject("Vehicle Control", HomeIcon.SightSeer, HomescreenLocation.TopMiddle, 0, ControlMain), //1
                    new HomeObject("Tracker", HomeIcon.Tracker, HomescreenLocation.TopRight, 0, AppScroll2), //2
                    new HomeObject("Settings", HomeIcon.Settings2, HomescreenLocation.BottomRight, 0, Settings), //3
                    new HomeObject("Converter", HomeIcon.Multiplayer, HomescreenLocation.BottomLeft, nonMod, Converter), //4
                    new HomeObject("Boat Control", HomeIcon.Broadcast, HomescreenLocation.MiddleLeft, 0, AppScroll3), //5
                    new HomeObject("Current Vehicle", HomeIcon.Sniper, HomescreenLocation.Middle, 0, CurrentVehicleApp), //6
                    new HomeObject("[Coming Soon]", HomeIcon.Bennys, HomescreenLocation.MiddleRight, 0, null, 50) //7
                );

                SetTheme(ActiveTheme);
                SetPhoneColor(PhoneColor);
                RefreshDisplay();
            }

            public static void ClearHomescreen()
            {
                for (int i = 0; i < 9; i++)
                    SetDataSlotForHome(1, i, 23, 0, "");
            }

            public static void HideCurrentSelection()
            {
                DisplayView(1, -1);
            }

            public enum HomescreenLocation
            {
                TopLeft,
                TopMiddle,
                TopRight,
                MiddleLeft,
                Middle,
                MiddleRight,
                BottomLeft,
                BottomMiddle,
                BottomRight
            }
            public enum HomeIcon
            {
                Camera = 1,
                TextMessage,
                Calendar,
                Email,
                Call,
                Eyefind,
                Map,
                Apps,
                Media = 9,
                NewContact = 11,
                BAWSAQ = 13,
                Multiplayer,
                Music,
                GPS,
                Spare = 17,
                Settings2 = 24,
                MissedCall = 27,
                UnreadEmail,
                ReadEmail,
                ReplyEmail,
                ReplayMission,
                ShitSkip,
                UnreadSMS,
                ReadSMS,
                PlayerList,
                CopBackup,
                GangTaxi,
                RepeatPlay = 38,
                Sniper = 40,
                ZitIT,
                Trackify,
                Save,
                AddTag,
                RemoveTag,
                Location,
                Party = 47,
                Broadcast = 49,
                Gamepad = 50,
                Silent = 51,
                InvitesPending = 52,
                OnCall,
                HLock,
                PushToTalk,
                Bennys,
                Gang,
                Tracker,
                SightSeer,
                Beacon = 60
            }

            public enum ListIcons
            {
                None = 0,
                Attachment = 10,
                SideTasks = 12,
                RingTone = 18,
                TextTone = 19,
                VibrateOn = 20,
                VibrateOff = 21,
                Volume = 22,
                Settings1 = 23,
                Profile = 25,
                SleepMode = 26,
                Checklist = 39,
                Ticked = 48,
                Silent = 51
            }

            public enum Theme
            {
                LightBlue = 1,
                Green,
                Red,
                Orange,
                Grey,
                Purple,
                Pink,
                Black
            }

            public enum Direction
            {
                Up = 1,
                Right,
                Down,
                Left,
            }
            public enum BackgroundImage
            {
                Default = 0,
                None = 1, //2, 3
                PurpleGlow = 4,
                GreenSquares = 5,
                OrangeHerringBone = 6,
                OrangeHalfTone = 7,
                GreenTriangles = 8,
                GreenShards = 9,
                BlueAngles = 10,
                BlueShards = 11,
                BlueCircles = 12,
                Diamonds = 13,
                GreenGlow = 14,
                Orange8Bit = 15,
                OrangeTriangles = 16,
                PurpleTartan = 17
            }

            public enum SoftKey
            {
                Left = 1,
                Middle,
                Right
            }

            public enum SoftkeyIcon
            {
                Blank = 1,
                Select = 2,
                Pages = 3,
                Back = 4,
                Call = 5,
                Hangup = 6,
                Hangup_Human = 7,
                Hide_Phone = 8,
                Keypad = 9,
                Open = 10,
                Reply = 11,
                Delete = 12,
                Yes = 13,
                No = 14,
                Sort = 15,
                Website = 16,
                Police = 17,
                Ambulance = 18,
                Fire = 19,
                Pages2 = 20
            }

            public static int PhoneTone = 0;
            public static int PhoneModel = 0;
            public static int Interface = 0;
            private static Vector3 PhonePosition_Final = new Vector3(99.62f, -51f, -113f);
            private static Vector3 PhoneRotation_Final = new Vector3(-90f, 0f, 0f);
            private static Vector3 PhonePosition_Start = new Vector3(99.62f, -150f, -113f);
            private static Vector3 PhoneRotation_Start = new Vector3(-90f, 180f, 0f);
            private static Vector3 PhonePosition_Current = new Vector3(99.62f, -180f, -113f);
            private static Vector3 PhoneRotation_Current = new Vector3(-90f, 180f, 0f);
            private static float PhoneScale = 525f;
            private static int PhoneRenderID = -1;
            public static GTA.Scaleform PhoneScaleform;
            public static bool IsOn { get; private set; } = false;
            public static bool RawOn = false;
            private static int HomescreenSelection = 0;
            private static int CurrentAppSelection = 0;
            public static AppObject CurrentApp = null;
            public static BackgroundImage HomescreenImage = BackgroundImage.BlueAngles;
            public static bool IsOnHomeScreen()
            {
                return CurrentApp == null;
            }

            public static int GetCurrentIndex()
            {
                if (IsOnHomeScreen())
                    return HomescreenSelection;
                else
                    return 0;
            }

            public enum AppContainer
            {
                HomeMenu = 1,
                Contacts = 2,
                CallScreen = 4,
                MessageList = 6,
                MessageView = 7,
                EmailList = 8,
                EmailView = 9,
                Settings = 22,
                ToDoList = 17,
                TodoView = 15,
                MissionRepeat = 18,
                MissionStats = 19,
                JobList = 20,
                EmailResponse = 21
            }

            public interface AppItem
            {
                string Type { get; set; }
                AppObject Forward { get; set; }
                Action Invoker { get; set; }
                string Id { get; set; }
                Action OnSoftkey_Left { get; set; }
                Action OnSoftkey_Right { get; set; }
                Action OnSoftkey_Middle { get; set; }
            }

            public class AppSettingItem : AppItem
            {
                public string Type { get; set; } = "Setting";
                public string Name;
                public string Id { get; set; }
                public ListIcons Icon;
                public AppObject Forward { get; set; }
                public Action Invoker { get; set; }
                public Action OnSoftkey_Left { get; set; } = null;
                public Action OnSoftkey_Right { get; set; } = null;
                public Action OnSoftkey_Middle { get; set; } = null;
                public AppSettingItem(string id, string name, ListIcons icon, Action invoke = null, AppObject forward = null, Action onleft = null, Action onright = null, Action onmiddle = null)
                {
                    Id = id;
                    Name = name;
                    Icon = icon;
                    Forward = forward;
                    Invoker = invoke;
                    OnSoftkey_Left = onleft;
                    OnSoftkey_Middle = onmiddle;
                    OnSoftkey_Right = onright;
                }
            }

            public class AppMessageItem : AppItem
            {
                public string Type { get; set; } = "Message";
                public string Id { get; set; }
                public string Hour;
                public string Minute;
                public bool Seen;
                public string FromAddress;
                public string SubjectTitle;
                public AppObject Forward { get; set; }
                public Action Invoker { get; set; }
                public Action OnSoftkey_Left { get; set; } = null;
                public Action OnSoftkey_Right { get; set; } = null;
                public Action OnSoftkey_Middle { get; set; } = null;
                public AppMessageItem(string id, string hour, string minute, bool seen, string from, string subject, Action act = null, AppObject forward = null, Action onleft = null, Action onright = null, Action onmiddle = null)
                {
                    Id = id;
                    Hour = hour;
                    Minute = minute;
                    Seen = seen;
                    FromAddress = from;
                    SubjectTitle = subject;
                    Forward = forward;
                    Invoker = act;
                    OnSoftkey_Left = onleft;
                    OnSoftkey_Middle = onmiddle;
                    OnSoftkey_Right = onright;
                }
            }

            public class AppCallscreenItem : AppItem
            {
                public string Type { get; set; } = "Message";
                public string Id { get; set; }
                public AppObject Forward { get; set; }
                public Action Invoker { get; set; }
                public string FromAddress;
                public string JobTitle;
                public string Icon;
                public Action OnSoftkey_Left { get; set; } = null;
                public Action OnSoftkey_Right { get; set; } = null;
                public Action OnSoftkey_Middle { get; set; } = null;
                public AppCallscreenItem(string id, string from, string title, string icon, Action act = null, AppObject forward = null, Action onleft = null, Action onright = null, Action onmiddle = null)
                {
                    Id = id;
                    FromAddress = from;
                    JobTitle = title;
                    Icon = icon;
                    Forward = forward;
                    Invoker = act;
                    OnSoftkey_Left = onleft;
                    OnSoftkey_Middle = onmiddle;
                    OnSoftkey_Right = onright;
                }
            }

            public class AppMessageViewItem : AppItem
            {
                public string Type { get; set; } = "Message";
                public string FromAddress;
                public string Id { get; set; }
                public string Message;
                public string Icon = "CHAR_HUMANDEFAULT";
                public AppObject Forward { get; set; }
                public Action Invoker { get; set; }
                public Action OnSoftkey_Left { get; set; } = null;
                public Action OnSoftkey_Right { get; set; } = null;
                public Action OnSoftkey_Middle { get; set; } = null;
                public AppMessageViewItem(string from, string msg, string icon, Action act = null, AppObject forward = null, Action onleft = null, Action onright = null, Action onmiddle = null)
                {
                    FromAddress = from;
                    Message = msg;
                    Icon = icon;
                    Invoker = act;
                    Forward = forward;
                    OnSoftkey_Left = onleft;
                    OnSoftkey_Middle = onmiddle;
                    OnSoftkey_Right = onright;
                }
            }

            public class AppContactItem : AppItem
            {
                public string Type { get; set; } = "Message";
                public AppObject Forward { get; set; }
                public string Id { get; set; }
                public Action Invoker { get; set; }
                public bool MissedCall;
                public string Name;
                public string Icon;
                public Action OnSoftkey_Left { get; set; } = null;
                public Action OnSoftkey_Right { get; set; } = null;
                public Action OnSoftkey_Middle { get; set; } = null;
                public AppContactItem(string id, bool missedcall, string name, string icon, Action act = null, AppObject forward = null, Action onleft = null, Action onright = null, Action onmiddle = null)
                {
                    Id = id;
                    MissedCall = missedcall;
                    Name = name;
                    Icon = icon;
                    Invoker = act;
                    Forward = forward;
                    OnSoftkey_Left = onleft;
                    OnSoftkey_Middle = onmiddle;
                    OnSoftkey_Right = onright;
                }
            }

            public class AppObject
            {
                public string Name;
                public AppContainer Container;

                public List<AppItem> Items = new List<AppItem>();
                public AppObject Backward;
                public Action OnBack = null;
                public Action Invoker = null;
                public SoftkeyObject SoftKey_Left = new SoftkeyObject(SoftkeyIcon.Blank, false, new RGBA(0, 0, 0));
                public SoftkeyObject SoftKey_Right = new SoftkeyObject(SoftkeyIcon.Blank, false, new RGBA(0, 0, 0));
                public SoftkeyObject SoftKey_Middle = new SoftkeyObject(SoftkeyIcon.Blank, false, new RGBA(0, 0, 0));
                public Action OnSoftKey_Right = null;
                public Action OnSoftKey_Left = null;
                public Action OnSoftKey_Middle = null;

                public int Selection = 0;

                public AppObject(string name, AppContainer cont)
                {
                    SoftKey_Left = new SoftkeyObject(SoftkeyIcon.Select, true, new RGBA(46, 204, 113));
                    SoftKey_Right = new SoftkeyObject(SoftkeyIcon.Back, true, new RGBA(255, 255, 255));
                    Name = name;
                    Container = cont;
                }

                public T GetItemByID<T>(string id)
                {
                    return (T)Items.FirstOrDefault(x => x.Id == id);
                }

                public void AddItem(object item)
                {
                    if (Container == AppContainer.Settings)
                    {
                        if (item is AppSettingItem)
                        {
                            Items.Add((AppItem)item);
                        }
                        else
                            return;
                    }
                    else if (Container == AppContainer.MessageList)
                    {
                        if (item is AppMessageItem)
                        {
                            Items.Add((AppItem)item);
                        }
                        else
                            return;
                    }
                    else if (Container == AppContainer.MessageView)
                    {
                        if (item is AppMessageViewItem)
                        {
                            Items.Add((AppItem)item);
                        }
                        else
                            return;
                    }
                    else if (Container == AppContainer.Contacts)
                    {
                        if (item is AppContactItem)
                        {
                            Items.Add((AppItem)item);
                        }
                        else
                            return;
                    }
                    else if (Container == AppContainer.CallScreen)
                    {
                        if (item is AppCallscreenItem)
                        {
                            Items.Add((AppItem)item);
                        }
                        else
                            return;
                    }
                }
            }

            public static void OpenApp(AppObject app, int LastSelection)
            {
                if (app == null)
                    return;
                CurrentApp = app;
                CurrentAppSelection = LastSelection;
                app.Selection = LastSelection;
                SetDataSlotEmpty((int)app.Container);
                for (int i = 0; i < app.Items.Count; i++)
                {
                    if (app.Container == AppContainer.Settings)
                    {
                        SetDataSlotForSetting((int)AppContainer.Settings, i, (int)(app.Items[i] as AppSettingItem).Icon, (app.Items[i] as AppSettingItem).Name);
                    }
                    else if (app.Container == AppContainer.MessageList)
                    {
                        SetDataSlotForMessageList((int)AppContainer.MessageList, i, (app.Items[i] as AppMessageItem).Hour, (app.Items[i] as AppMessageItem).Minute, (app.Items[i] as AppMessageItem).Seen, (app.Items[i] as AppMessageItem).FromAddress, (app.Items[i] as AppMessageItem).SubjectTitle);
                    }
                    else if (app.Container == AppContainer.MessageView)
                    {
                        SetDataSlotForMessageView((int)AppContainer.MessageView, i, (app.Items[i] as AppMessageViewItem).FromAddress, (app.Items[i] as AppMessageViewItem).Message, (app.Items[i] as AppMessageViewItem).Icon);
                    }
                    else if (app.Container == AppContainer.Contacts)
                    {
                        SetDataSlotForContactList((int)AppContainer.Contacts, i, (app.Items[i] as AppContactItem).MissedCall, (app.Items[i] as AppContactItem).Name, (app.Items[i] as AppContactItem).Icon);
                    }
                    else if (app.Container == AppContainer.CallScreen)
                    {
                        SetDataSlotForCallscreen((int)AppContainer.CallScreen, i, (app.Items[i] as AppCallscreenItem).FromAddress, (app.Items[i] as AppCallscreenItem).JobTitle, (app.Items[i] as AppCallscreenItem).Icon);
                    }
                }
                if (app.Invoker != null)
                    app.Invoker.Invoke();
                SetSoftKey_Data(SoftKey.Left, app.SoftKey_Left);
                SetSoftKey_Data(SoftKey.Right, app.SoftKey_Right);
                SetSoftKey_Data(SoftKey.Middle, app.SoftKey_Middle);
                SetHeaderText(app.Name);
                DisplayView((int)app.Container, CurrentAppSelection);
            }

            public static void CloseApp(AppObject app)
            {
                if (app == null)
                    return;

                if (app.Backward == null)
                    GoHome();
                else
                    OpenApp(app.Backward, app.Backward.Selection);
            }

            public class HomeObject
            {
                public int NotificationNumber = 5;
                public AppObject Forward;
                public HomeIcon Icon = 0;
                public string Name = "NULL";
                public HomescreenLocation Location;
                public int Alpha = 100;
                public HomeObject(string name, HomeIcon icon, HomescreenLocation location, int notifications, AppObject link = null, int alpha = 100)
                {
                    Name = name;
                    NotificationNumber = notifications;
                    Icon = icon;
                    Location = location;
                    Forward = link;
                    Alpha = alpha;
                }
            }

            public static HomeObject GetHomeObjectByIndex(int index)
            {
                if (index < 0)
                    index = 0;
                if (index > 8)
                    index = 8;
                return HomeObjects_Stored[0][index];
            }

            static List<List<HomeObject>> HomeObjects_Stored = new List<List<HomeObject>>()
            {
                new List<HomeObject>()
                {
                    null, null, null, null, null, null, null, null, null
                },
                new List<HomeObject>()
                {
                    null, null, null, null, null, null, null, null, null
                },
                new List<HomeObject>()
                {
                    null, null, null, null, null, null, null, null, null
                }
            };

            public static void SetHomeObjectsData(params HomeObject[] objs)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    //SetDataSlotForHome(1, (int)objs[i].Location, (int)objs[i].Icon, objs[i].NotificationNumber, objs[i].Name);
                    HomeObjects_Stored[0][(int)objs[i].Location] = objs[i];
                }

                for (int i = 0; i < 9; i++)
                {
                    if (HomeObjects_Stored[0][i] == null)
                    {
                        HomeObjects_Stored[0][i] = new HomeObject(" ", HomeIcon.Spare, (HomescreenLocation)i, 0);
                        //SetDataSlotForHome(1, (int)HomeObjects[i].Location, (int)HomeObjects[i].Icon, HomeObjects[i].NotificationNumber, HomeObjects[i].Name);
                    }
                }
            }

            public static void SetHomeObjectsTemp(params HomeObject[] objs)
            {
                for (int i = 0; i < objs.Length; i++)
                {
                    SetDataSlotForHome(1, (int)objs[i].Location, (int)objs[i].Icon, objs[i].NotificationNumber, objs[i].Name, objs[i].Alpha);
                }
            }

            public static void SwitchHomeScreen(int index)
            {
                SetHomeObjectsTemp(HomeObjects_Stored[index].ToArray());
                DisplayView((int)AppContainer.Settings, 0);
            }

            public class SoftkeyObject
            {
                public SoftkeyIcon Icon = SoftkeyIcon.Blank;
                public bool Visible = false;
                public RGBA RGBA = new RGBA(255, 255, 255);

                public SoftkeyObject(SoftkeyIcon icon, bool vis, RGBA col)
                {
                    Icon = icon;
                    Visible = vis;
                    RGBA = col;
                }
            }

            private static SoftkeyObject SoftkeyLeft = new SoftkeyObject(SoftkeyIcon.Blank, false, new RGBA(0, 0, 0));
            private static SoftkeyObject SoftkeyRight = new SoftkeyObject(SoftkeyIcon.Blank, false, new RGBA(0, 0, 0));
            private static SoftkeyObject SoftkeyMiddle = new SoftkeyObject(SoftkeyIcon.Blank, false, new RGBA(0, 0, 0));

            public static SoftkeyObject Home_SoftkeyLeft = new SoftkeyObject(SoftkeyIcon.Select, true, new RGBA(46, 204, 113));
            public static SoftkeyObject Home_SoftkeyRight = new SoftkeyObject(SoftkeyIcon.Blank, false, new RGBA(0, 0, 0));
            public static SoftkeyObject Home_SoftkeyMiddle = new SoftkeyObject(SoftkeyIcon.Blank, false, new RGBA(0, 0, 0));

            private static bool IsInvalid()
            {
                if(PhoneScaleform != null)
                {
                    return !PhoneScaleform.IsValid;
                }
                return true;
            }

            public static int GetCurrentSelection()
            {
                if (CurrentApp == null)
                    return HomescreenSelection;

                return CurrentAppSelection;
            }

            public static bool SleepMode = false;
            public static void SetSleepMode(bool active)
            {
                if (IsInvalid())
                    return;

                SleepMode = active;
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_SLEEP_MODE");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_BOOL, active);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            public static void SetBackgroundImage(BackgroundImage img)
            {
                if (IsInvalid())
                    return;
                HomescreenImage = img;
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_BACKGROUND_IMAGE");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, (int)img);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            public static void RefreshDisplay(bool forced = false)
            {
                if (IsOnHomeScreen())
                {
                    DisplayView(1, HomescreenSelection);
                }
                else
                {
                    int select = CurrentAppSelection;
                    if (CurrentApp.Container == AppContainer.Settings)
                    {
                        CurrentApp.Selection = select;
                        AppSettingItem id = CurrentApp.Items[select] as AppSettingItem;
                        SetDataSlotForSetting((int)AppContainer.Settings, select, (int)id.Icon, id.Name);
                    }
                    if(forced)
                    {
                        OpenApp(CurrentApp, CurrentAppSelection);
                    }
                    else
                    {
                        DisplayView((int)CurrentApp.Container, CurrentAppSelection);
                    }
                }
            }

            public static void SetHeaderText(string text)
            {
                if (IsInvalid())
                    return;

                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_HEADER");
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_TEXT_COMMAND_SCALEFORM_STRING, "STRING");
                GTA.Native.Function.Call(GTA.Native.Hash.ADD_TEXT_COMPONENT_SUBSTRING_PHONE_NUMBER, text, -1);
                GTA.Native.Function.Call(GTA.Native.Hash.END_TEXT_COMMAND_SCALEFORM_STRING);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            public static void SetPhoneColor(int theme)
            {
                PhoneColor = theme;
                int cln = theme;

                if(PhoneModel == 1)
                {
                    if (cln == (int)Theme.Red - 1)
                        cln = (int)Theme.Green - 1;
                    else if (cln == (int)Theme.Green - 1)
                        cln = (int)Theme.Pink - 1;
                    else if (cln == (int)Theme.LightBlue - 1)
                        cln = (int)Theme.Purple - 1;
                    else if (cln == (int)Theme.Orange - 1)
                        cln = (int)Theme.LightBlue - 1;
                    else if (cln == (int)Theme.Pink - 1)
                        cln = (int)Theme.Grey - 1;
                    else if (cln == (int)Theme.Purple - 1)
                        cln = (int)Theme.Orange - 1;
                    else if (cln == (int)Theme.Grey - 1)
                        cln = (int)Theme.Red - 1;
                }
                else if(PhoneModel == 2)
                {
                    if (cln == (int)Theme.Red - 1)
                        cln = (int)Theme.Green - 1;
                    else if (cln == (int)Theme.Green - 1)
                        cln = (int)Theme.LightBlue - 1;
                    else if (cln == (int)Theme.LightBlue - 1)
                        cln = (int)Theme.Pink - 1;
                    else if (cln == (int)Theme.Orange - 1)
                        cln = (int)Theme.Red - 1;
                    else if (cln == (int)Theme.Pink - 1)
                        cln = (int)Theme.Purple - 1;
                    else if (cln == (int)Theme.Purple - 1)
                        cln = (int)Theme.Grey - 1;
                    else if (cln == (int)Theme.Grey - 1)
                        cln = (int)Theme.Orange - 1;
                }

                GTA.Native.Function.Call(GTA.Native.Hash.SET_PLAYER_PHONE_PALETTE_IDX, Game.Player, cln);

                //ChangePhysicalColor(cln);
            }

            public static void SetTitlebarTime(int hour, int minute, string day)
            {
                if (IsInvalid())
                    return;

                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_TITLEBAR_TIME");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, hour);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, minute);
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_TEXT_COMMAND_SCALEFORM_STRING, "STRING");
                GTA.Native.Function.Call(GTA.Native.Hash.ADD_TEXT_COMPONENT_SUBSTRING_PHONE_NUMBER, day, -1);
                GTA.Native.Function.Call(GTA.Native.Hash.END_TEXT_COMMAND_SCALEFORM_STRING);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            public static void SetTitlebarTimeEx(string hour, string minute, string day)
            {
                if (IsInvalid())
                    return;

                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_TITLEBAR_TIME");
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_TEXT_COMMAND_SCALEFORM_STRING, "STRING");
                GTA.Native.Function.Call(GTA.Native.Hash.ADD_TEXT_COMPONENT_SUBSTRING_PHONE_NUMBER, hour, -1);
                GTA.Native.Function.Call(GTA.Native.Hash.END_TEXT_COMMAND_SCALEFORM_STRING);
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_TEXT_COMMAND_SCALEFORM_STRING, "STRING");
                GTA.Native.Function.Call(GTA.Native.Hash.ADD_TEXT_COMPONENT_SUBSTRING_PHONE_NUMBER, minute, -1);
                GTA.Native.Function.Call(GTA.Native.Hash.END_TEXT_COMMAND_SCALEFORM_STRING);
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_TEXT_COMMAND_SCALEFORM_STRING, "STRING");
                GTA.Native.Function.Call(GTA.Native.Hash.ADD_TEXT_COMPONENT_SUBSTRING_PHONE_NUMBER, day, -1);
                GTA.Native.Function.Call(GTA.Native.Hash.END_TEXT_COMMAND_SCALEFORM_STRING);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            public static void Click()
            {
                if (IsInvalid())
                    return;

                if (IsOnHomeScreen())
                {
                    OpenApp(HomeObjects_Stored[0][GetCurrentSelection()].Forward, 0);
                }
                else
                {
                    int v = GetCurrentSelection();
                    if (CurrentApp.Items.Count > 0)
                    {
                        if (CurrentApp.Items[v].Invoker != null)
                            CurrentApp.Items[v].Invoker.Invoke();
                        if (CurrentApp.Items[v].OnSoftkey_Left != null)
                            CurrentApp.Items[v].OnSoftkey_Left.Invoke();
                    }

                    if (CurrentApp.OnSoftKey_Left != null)
                        CurrentApp.OnSoftKey_Left.Invoke();

                    if (CurrentApp.Items.Count > 0)
                        if (CurrentApp.Items[v].Forward != null)
                            OpenApp(CurrentApp.Items[v].Forward, 0);
                }

                GTA.Native.Function.Call(GTA.Native.Hash.CELL_SET_INPUT, 1);
                AdvancedPersistence.PlayFrontendAudio("Menu_Accept", PhoneTone);
            }

            public static void Back()
            {
                if (IsInvalid())
                    return;

                if (!IsOnHomeScreen())
                {
                    if (GetCurrentSelection() < CurrentApp.Items.Count)
                    {
                        if (CurrentApp.Items[GetCurrentSelection()].OnSoftkey_Right != null)
                            CurrentApp.Items[GetCurrentSelection()].OnSoftkey_Right.Invoke();
                    }

                    if (CurrentApp.OnSoftKey_Right != null)
                        CurrentApp.OnSoftKey_Right.Invoke();

                    if (CurrentApp.OnBack != null)
                        CurrentApp.OnBack.Invoke();
                    CloseApp(CurrentApp);
                }
                else
                {
                    TurnOff();
                }

                GTA.Native.Function.Call(GTA.Native.Hash.CELL_SET_INPUT, 2);
                AdvancedPersistence.PlayFrontendAudio("Menu_Back", PhoneTone);
            }

            public static void Middle()
            {
                if (IsInvalid())
                    return;

                if (!IsOnHomeScreen())
                {
                    if (CurrentApp.Items[GetCurrentSelection()].OnSoftkey_Middle != null)
                        CurrentApp.Items[GetCurrentSelection()].OnSoftkey_Middle.Invoke();

                    if (CurrentApp.OnSoftKey_Middle != null)
                        CurrentApp.OnSoftKey_Middle.Invoke();
                }

                GTA.Native.Function.Call(GTA.Native.Hash.CELL_SET_INPUT, 5);
            }

            public static void SetInputEvent(Direction dir)
            {
                if (IsInvalid())
                    return;

                if (dir == Direction.Left)
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.CELL_SET_INPUT, 3);
                    AdvancedPersistence.PlayFrontendAudio("Menu_Navigate", PhoneTone);
                }
                else if (dir == Direction.Right)
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.CELL_SET_INPUT, 4);
                    AdvancedPersistence.PlayFrontendAudio("Menu_Navigate", PhoneTone);
                }
                else if (dir == Direction.Down)
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.CELL_SET_INPUT, 2);
                    AdvancedPersistence.PlayFrontendAudio("Menu_Navigate", PhoneTone);
                }
                else if (dir == Direction.Up)
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.CELL_SET_INPUT, 1);
                    AdvancedPersistence.PlayFrontendAudio("Menu_Navigate", PhoneTone);
                }

                if (CurrentApp == null)
                {
                    if (dir == Direction.Left)
                        HomescreenSelection--;
                    else if (dir == Direction.Right)
                        HomescreenSelection++;
                    else if (dir == Direction.Up)
                        HomescreenSelection -= 3;
                    else if (dir == Direction.Down)
                        HomescreenSelection += 3;

                    if (HomescreenSelection > 8)
                    {
                        HomescreenSelection -= 9;
                    }
                    else if (HomescreenSelection < 0)
                    {
                        HomescreenSelection += 9;
                    }

                    DisplayView(1, HomescreenSelection);
                }
                else
                {
                    if (CurrentApp.Items.Count == 1)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_INPUT_EVENT");
                        GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, (int)dir);
                        GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
                        return;
                    }
                    else if (CurrentApp.Items.Count == 0)
                    {
                        return;
                    }
                    if (dir == Direction.Down)
                        CurrentAppSelection++;
                    else if (dir == Direction.Up)
                        CurrentAppSelection--;

                    if (CurrentAppSelection > CurrentApp.Items.Count - 1)
                        CurrentAppSelection = 0;
                    if (CurrentAppSelection < 0)
                        CurrentAppSelection = CurrentApp.Items.Count - 1;
                    CurrentApp.Selection = CurrentAppSelection;
                    DisplayView((int)CurrentApp.Container, CurrentAppSelection);
                }
            }

            public static void SetSignalStrength(int strength)
            {
                if (IsInvalid())
                    return;

                if (strength < 0)
                    strength = 0;
                if (strength > 5)
                    strength = 5;

                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_SIGNAL_STRENGTH");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, strength);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            public static void SetTheme(Theme th)
            {
                if (IsInvalid())
                    return;

                ActiveTheme = th;
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_THEME");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, (int)th);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            public static void SetSoftKey_Data(SoftKey key, SoftkeyObject obj)
            {
                SetSoftKey_Visible(key, obj.Visible);
                SetSoftKey_Color(key, obj.RGBA);
                SetSoftKey_Icon(key, obj.Icon);
            }

            public static void SetSoftKey_Visible(SoftKey key, bool visible)
            {
                int icon = 0;
                if (key == SoftKey.Left)
                {
                    SoftkeyLeft.Visible = visible;
                    icon = (int)SoftkeyLeft.Icon;
                }
                else if (key == SoftKey.Middle)
                {
                    SoftkeyMiddle.Visible = visible;
                    icon = (int)SoftkeyMiddle.Icon;
                }
                else
                {
                    SoftkeyRight.Visible = visible;
                    icon = (int)SoftkeyRight.Icon;
                }

                if (IsInvalid())
                    return;

                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_SOFT_KEYS");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, (int)key);//key
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_BOOL, visible);//visible
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, (int)icon);//icon
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            public static void SetSoftKey_Icon(SoftKey key, SoftkeyIcon icon)
            {
                bool visible = false;
                if (key == SoftKey.Left)
                {
                    SoftkeyLeft.Icon = icon;
                    visible = SoftkeyLeft.Visible;
                }
                else if (key == SoftKey.Middle)
                {
                    SoftkeyMiddle.Icon = icon;
                    visible = SoftkeyMiddle.Visible;
                }
                else
                {
                    SoftkeyRight.Icon = icon;
                    visible = SoftkeyRight.Visible;
                }

                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_SOFT_KEYS");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, (int)key);//key
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_BOOL, visible);//visible
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, (int)icon);//icon
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            public static void SetSoftKey_Color(SoftKey key, RGBA rgba)
            {
                if (key == SoftKey.Left)
                {
                    SoftkeyLeft.RGBA = rgba;
                }
                else if (key == SoftKey.Middle)
                {
                    SoftkeyMiddle.RGBA = rgba;
                }
                else
                {
                    SoftkeyRight.RGBA = rgba;
                }

                if (IsInvalid())
                    return;

                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_SOFT_KEYS_COLOUR");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, (int)key);//key
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, rgba.Red);//r
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, rgba.Green);//g
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, rgba.Blue);//b
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            private static void DisplayView(int view, int select)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "DISPLAY_VIEW");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, view);//view
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, select);//slot
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            private static void SetDataSlotForHome(int view, int slot, int icon, int notf, string name, int alpha = 100)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_DATA_SLOT");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, view);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, slot);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, icon);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, notf);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, name);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, alpha);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            private static void SetDataSlotForSetting(int view, int slot, int icon, string name)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_DATA_SLOT");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, view);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, slot);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, icon);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, name);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            //s1 = 33 - unread sms, 34 - readsms or anything else invisible
            //s2 = string, icon? name?
            //s3 = <FONT COLOr=" support actual text?
            private static void SetDataSlotForMessageList(int view, int slot, string hour, string minute, bool seen, string fromAddress, string subjectTitle)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_DATA_SLOT");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, view);//view
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, slot);//slot
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, hour);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, minute);
                if (seen)
                    GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, 34);
                else
                    GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, 33);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, fromAddress);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, subjectTitle);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            //7
            private static void SetDataSlotForMessageView(int view, int slot, string from, string message, string icon)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_DATA_SLOT");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, view);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, slot);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, from);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, message);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, icon);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            private static void SetDataSlotForContactList(int view, int slot, bool missedcall, string name, string icon)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_DATA_SLOT");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, view);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, slot);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_BOOL, missedcall);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, name);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, "");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, icon);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            private static void SetDataSlotForCallscreen(int view, int slot, string from, string title, string icon)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_DATA_SLOT");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, view);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, slot);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, "");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, from);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, icon);
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_PLAYER_NAME_STRING, title);
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            private static void SetDataSlotEmpty(int view)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.BEGIN_SCALEFORM_MOVIE_METHOD, PhoneScaleform.Handle, "SET_DATA_SLOT_EMPTY");
                GTA.Native.Function.Call(GTA.Native.Hash.SCALEFORM_MOVIE_METHOD_ADD_PARAM_INT, view);//view
                GTA.Native.Function.Call(GTA.Native.Hash.END_SCALEFORM_MOVIE_METHOD);
            }

            public static void GoHome()
            {
                SetSoftKey_Data(SoftKey.Left, Home_SoftkeyLeft);
                SetSoftKey_Data(SoftKey.Right, Home_SoftkeyRight);
                SetSoftKey_Data(SoftKey.Middle, Home_SoftkeyMiddle);
                SetBackgroundImage(HomescreenImage);

                foreach (HomeObject obj in HomeObjects_Stored[0])
                    SetDataSlotForHome(1, (int)obj.Location, (int)obj.Icon, obj.NotificationNumber, obj.Name, obj.Alpha);

                DisplayView(1, HomescreenSelection);
                CurrentAppSelection = 0;
                CurrentApp = null;
            }

            public static bool TriggerLoaded = false;
            public static void TurnOn()
            {
                if (IsOn)
                    return;
                IsOn = true;
                RawOn = true;
                GTA.Native.Function.Call(GTA.Native.Hash.CREATE_MOBILE_PHONE, PhoneModel);
                GTA.Native.Function.Call(GTA.Native.Hash.SET_MOBILE_PHONE_POSITION, PhonePosition_Start.X, PhonePosition_Start.Y, PhonePosition_Start.Z);
                GTA.Native.Function.Call(GTA.Native.Hash.SET_MOBILE_PHONE_ROTATION, PhoneRotation_Start.X, PhoneRotation_Start.Y, PhoneRotation_Start.Z, 0);
                GTA.Native.Function.Call(GTA.Native.Hash.SET_MOBILE_PHONE_SCALE, PhoneScale);
                GTA.Native.OutputArgument arg1 = new GTA.Native.OutputArgument();
                GTA.Native.Function.Call(GTA.Native.Hash.GET_MOBILE_PHONE_RENDER_ID, arg1);
                PhoneRenderID = arg1.GetResult<int>();

                GTA.Native.Function.Call(GTA.Native.Hash.SCRIPT_IS_MOVING_MOBILE_PHONE_OFFSCREEN, false);
                AdvancedPersistence.PlayFrontendAudio("Pull_Out", PhoneTone);
                SetDataSlotEmpty(1);

                SetSleepMode(false);
                SetSignalStrength(5);
                SetSoftKey_Icon(SoftKey.Left, SoftkeyIcon.Select);
                SetSoftKey_Color(SoftKey.Left, new RGBA(46, 204, 113));
                SetSoftKey_Visible(SoftKey.Left, true);
                SetSoftKey_Icon(SoftKey.Middle, SoftkeyIcon.Keypad);
                SetSoftKey_Color(SoftKey.Middle, new RGBA(149, 165, 166));
                SetSoftKey_Visible(SoftKey.Middle, true);
                SetSoftKey_Icon(SoftKey.Right, SoftkeyIcon.Website);
                SetSoftKey_Color(SoftKey.Right, new RGBA(52, 152, 219));
                SetSoftKey_Visible(SoftKey.Right, true);
                SetTitlebarTimeEx("----------", "--------------", "");

                Initialize();

                GoHome();

                int viewType = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CAM_ACTIVE_VIEW_MODE_CONTEXT);
                int viewMode = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CAM_VIEW_MODE_FOR_CONTEXT, viewType);
                if(viewMode != 4)
                {
                    DoBlackLerp = true;
                    CurBlackLerp = 0f;
                    CurLerp = 0f;
                    DoLerpDown = false;
                    DoLerpUp = true;
                    DoBlackLerpInverse = false;
                }

                CurBlackLerp = 0f;
                DoBlackLerp = true;
                DoBlackLerpInverse = false;
            }

            public static void ChangePhysicalColor(int col)
            {
                if (!IsOn)
                    return;
                int objHash = 0;

                if (PhoneModel == 0)
                    objHash = Game.GenerateHash("prop_phone_ing");
                else if (PhoneModel == 1)
                    objHash = Game.GenerateHash("prop_phone_ing_02");
                else if (PhoneModel == 2)
                    objHash = Game.GenerateHash("prop_phone_ing_03");
                else
                    return;

                Vector3 pos = Game.Player.Character.Position;
                int objMy = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CLOSEST_OBJECT_OF_TYPE, pos.X, pos.Y, pos.Z, 3f, objHash, false, false, false);
                if(objMy != 0 && objMy != -1)
                    GTA.Native.Function.Call(GTA.Native.Hash.SET_OBJECT_TINT_INDEX, objMy, col);
            }

            public static void BringUp()
            {
                if (!IsOn)
                    return;

                //DoBlackLerp = true;
                //CurBlackLerp = 0f;
                CurLerp = 0f;
                DoLerpDown = false;
                DoLerpUp = true;
                //DoBlackLerpInverse = false;
            }

            public static void BringDown()
            {
                CurLerp = 0f;
                DoLerpUp = false;
                DoLerpDown = true;
                //DoBlackLerp = true;
                //DoBlackLerpInverse = true;
                //CurBlackLerp = 0f;
            }

            public static void TurnOff()
            {
                if (!IsOn)
                    return;
                IsOn = false;
                GTA.Native.Function.Call(GTA.Native.Hash.SCRIPT_IS_MOVING_MOBILE_PHONE_OFFSCREEN, true);
                AdvancedPersistence.PlayFrontendAudio("Put_Away", PhoneTone);
                //DisplayView(0, 0);
                CurLerp = 0f;
                DoLerpUp = false;
                DoLerpDown = true;
                DoBlackLerp = true;
                DoBlackLerpInverse = true;
                CurBlackLerp = 0f;

                if(activatedCar != null)
                {
                    if(activatedCar.Handle != null)
                    {
                        if(activatedCar.Handle.Exists())
                        {
                            if(activatedCar.Handle.AttachedBlip != null)
                            {
                                if(ModSettings.EnableBlips)
                                {
                                    if (activatedCar.Handle.AttachedBlip.Exists())
                                    {
                                        if (activatedCar.Handle.Model.IsHelicopter)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                                        else if (activatedCar.Handle.Model.IsAmphibiousQuadBike || activatedCar.Handle.Model.IsBicycle || activatedCar.Handle.Model.IsBike || activatedCar.Handle.Model.IsQuadBike)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                        else if (activatedCar.Handle.Model.IsJetSki)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Seashark;
                                        else if (activatedCar.Handle.Model.IsBoat)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Boat;
                                        else if (activatedCar.Handle.Model.IsPlane)
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.Plane;
                                        else
                                            activatedCar.Handle.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                        activatedCar.Handle.AttachedBlip.IsShortRange = true;
                                        activatedCar.Handle.AttachedBlip.Color = (BlipColor)activatedCar.BlipColor;
                                        activatedCar.Handle.AttachedBlip.Scale = 0.75f;
                                        activatedCar.Handle.AttachedBlip.Name = "Saved Vehicle";
                                        activatedCar.Handle.AttachedBlip.Priority = 0;
                                        GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                        GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, activatedCar.Handle.AttachedBlip.Handle, false);
                                    }
                                }
                            }
                        }
                    }
                }
                activatedCar = null;
            }

            private static float CurLerp = 0f;
            private static float TimeLerp = 1.5f;
            private static float BlackLerp = 1f;
            private static float BlackLerpInverse = 0.125f;
            private static bool DoLerpUp = false;
            private static float BlackValue = 255f;
            private static bool DoLerpDown = false;
            private static bool DoBlackLerp = false;
            private static bool DoBlackLerpInverse = false;
            private static float CurBlackLerp = 0f;
            public static void Draw()
            {
                if(RawOn)
                    SetTitlebarTimeEx(World.CurrentTimeOfDay.Hours.ToString(), World.CurrentTimeOfDay.Minutes.ToString(), World.CurrentDate.DayOfWeek.ToString().ToUpper().Substring(0, 3));
                float f = GUI.GetDeltaTime();
                if (DoBlackLerp)
                {
                    if (CurBlackLerp >= BlackLerp)
                    {
                        DoBlackLerp = false;
                    }
                    else
                    {
                        CurBlackLerp += f;
                        if (DoBlackLerpInverse)
                            BlackValue = GUI.Lerp(0f, 255f, (CurBlackLerp / BlackLerpInverse));
                        else
                            BlackValue = GUI.Lerp(255f, 0f, (CurBlackLerp / BlackLerp));
                    }
                }
                if (DoLerpUp)
                {
                    if (CurLerp >= TimeLerp)
                    {
                        DoLerpUp = false;
                    }
                    else
                    {
                        CurLerp += f;
                        PhonePosition_Current.Y = GUI.Lerp(PhonePosition_Current.Y, PhonePosition_Final.Y, (CurLerp / TimeLerp));
                        PhoneRotation_Current.Y = GUI.Lerp(PhoneRotation_Current.Y, PhoneRotation_Final.Y, (CurLerp / TimeLerp));
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_MOBILE_PHONE_POSITION, PhonePosition_Current.X, PhonePosition_Current.Y, PhonePosition_Current.Z);
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_MOBILE_PHONE_ROTATION, PhoneRotation_Current.X, PhoneRotation_Current.Y, PhoneRotation_Current.Z, 0);
                    }
                }
                else if (DoLerpDown)
                {
                    if (CurLerp >= TimeLerp)
                    {
                        DoLerpDown = false;
                        if (!IsOn)
                        {
                            GTA.Native.Function.Call(GTA.Native.Hash.DESTROY_MOBILE_PHONE);
                            PhoneRenderID = -1;
                            PhoneScaleform.Dispose();
                            RawOn = false;
                        }
                    }
                    else
                    {
                        CurLerp += f;
                        PhonePosition_Current.Y = GUI.Lerp(PhonePosition_Current.Y, PhonePosition_Start.Y, (CurLerp / TimeLerp));
                        PhoneRotation_Current.Y = GUI.Lerp(PhoneRotation_Current.Y, PhoneRotation_Start.Y, (CurLerp / TimeLerp));
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_MOBILE_PHONE_POSITION, PhonePosition_Current.X, PhonePosition_Current.Y, PhonePosition_Current.Z);
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_MOBILE_PHONE_ROTATION, PhoneRotation_Current.X, PhoneRotation_Current.Y, PhoneRotation_Current.Z, 0);
                    }
                }

                if (!IsInvalid() && PhoneRenderID != -1)
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.SET_TEXT_RENDER_ID, PhoneRenderID);
                    GTA.Native.Function.Call(GTA.Native.Hash.SET_SCRIPT_GFX_DRAW_ORDER, 4);
                    GTA.Native.Function.Call(GTA.Native.Hash.DRAW_SCALEFORM_MOVIE, PhoneScaleform.Handle, 0.1f, 0.179f, 0.2f, 0.356f, 255, 0, 255, 255, 0);
                    if(PhoneBrightness == 5)
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_RECT, 0.5f, 0.5f, 1.0f, 1.0f, 0, 0, 0, 0, 0);
                    else if(PhoneBrightness == 4)
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_RECT, 0.5f, 0.5f, 1.0f, 1.0f, 0, 0, 0, 50, 0);
                    else if (PhoneBrightness == 3)
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_RECT, 0.5f, 0.5f, 1.0f, 1.0f, 0, 0, 0, 100, 0);
                    else if (PhoneBrightness == 2)
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_RECT, 0.5f, 0.5f, 1.0f, 1.0f, 0, 0, 0, 175, 0);
                    else if (PhoneBrightness == 1)
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_RECT, 0.5f, 0.5f, 1.0f, 1.0f, 0, 0, 0, 220, 0);
                    GTA.Native.Function.Call(GTA.Native.Hash.DRAW_RECT, 0.5f, 0.5f, 1.0f, 1.0f, 0, 0, 0, (int)BlackValue, 0);
                    GTA.Native.Function.Call(GTA.Native.Hash.SET_SCRIPT_GFX_DRAW_ORDER, 1);
                }
            }
        }
    }

    [Serializable]
    public class VehicleDataMeta
    {
        public int Version { get; set; } = 2;
        public string Id { get; set; }
    }

    [Serializable]
    public class CharacterDataMeta
    {
        public int Version { get; set; } = 2;
        public string Id { get; set; } = "one";
    }

    [Serializable]
    public class VehicleDataV1
    {
        public string Id { get; set; }
        public string LicensePlate { get; set; } = "";

        public VehicleHash Hash { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }

        public bool ConvertibleState { get; set; } = false;
        public bool EngineState { get; set; } = false;
        public bool LockState { get; set; } = false;
        public bool LightState { get; set; } = false;
        public bool AlarmState { get; set; } = false;
        public int Boost { get; set; } = -1;

        public bool Turbo { get; set; } = false;
        public bool TireSmoke { get; set; } = false;
        public bool XenonHeadlights { get; set; } = false;
        public int XenonHeadlightsColor { get; set; } = -1;

        public int Spoiler { get; set; } = -1;
        public bool SpoilerVar { get; set; } = false;
        public int FrontBumper { get; set; } = -1;
        public bool FrontBumperVar { get; set; } = false;
        public int RearBumper { get; set; } = -1;
        public bool RearBumperVar { get; set; } = false;
        public int SideSkirt { get; set; } = -1;
        public bool SideSkirtVar { get; set; } = false;
        public int Exhaust { get; set; } = -1;
        public bool ExhaustVar { get; set; } = false;
        public int Frame { get; set; } = -1;
        public bool FrameVar { get; set; } = false;
        public int Grille { get; set; } = -1;
        public bool GrilleVar { get; set; } = false;
        public int Hood { get; set; } = -1;
        public bool HoodVar { get; set; } = false;
        public int Fender { get; set; } = -1;
        public bool FenderVar { get; set; } = false;
        public int RightFender { get; set; } = -1;
        public bool RightFenderVar { get; set; } = false;
        public int Roof { get; set; } = -1;
        public bool RoofVar { get; set; } = false;
        public int Engine { get; set; } = -1;
        public bool EngineVar { get; set; } = false;
        public int Brakes { get; set; } = -1;
        public bool BrakesVar { get; set; } = false;
        public int Transmission { get; set; } = -1;
        public bool TransmissionVar { get; set; } = false;
        public int Horns { get; set; } = -1;
        public bool HornsVar { get; set; } = false;
        public int Suspension { get; set; } = -1;
        public bool SuspensionVar { get; set; } = false;
        public int Armor { get; set; } = -1;
        public bool ArmorVar { get; set; } = false;
        public int FrontWheel { get; set; } = -1;
        public bool FrontWheelVar { get; set; } = false;
        public int RearWheel { get; set; } = -1;
        public bool RearWheelVar { get; set; } = false;
        public int PlateHolder { get; set; } = -1;
        public bool PlateHolderVar { get; set; } = false;
        public int VanityPlates { get; set; } = -1;
        public bool VanityPlatesVar { get; set; } = false;
        public int TrimDesign { get; set; } = -1;
        public bool TrimDesignVar { get; set; } = false;
        public int Ornaments { get; set; } = -1;
        public bool OrnamentsVar { get; set; } = false;
        public int Dashboard { get; set; } = -1;
        public bool DashboardVar { get; set; } = false;
        public int DialDesign { get; set; } = -1;
        public bool DialDesignVar { get; set; } = false;
        public int DoorSpeakers { get; set; } = -1;
        public bool DoorSpeakersVar { get; set; } = false;
        public int Seats { get; set; } = -1;
        public bool SeatsVar { get; set; } = false;
        public int SteeringWheels { get; set; } = -1;
        public bool SteeringWheelsVar { get; set; } = false;
        public int ColumnShifterLevers { get; set; } = -1;
        public bool ColumnShifterLeversVar { get; set; } = false;
        public int Plaques { get; set; } = -1;
        public bool PlaquesVar { get; set; } = false;
        public int Speakers { get; set; } = -1;
        public bool SpeakersVar { get; set; } = false;
        public int Trunk { get; set; } = -1;
        public bool TrunkVar { get; set; } = false;
        public int Hydraulics { get; set; } = -1;
        public bool HydraulicsVar { get; set; } = false;
        public int EngineBlock { get; set; } = -1;
        public bool EngineBlockVar { get; set; } = false;
        public int AirFilter { get; set; } = -1;
        public bool AirFilterVar { get; set; } = false;
        public int Struts { get; set; } = -1;
        public bool StrutsVar { get; set; } = false;
        public int ArchCover { get; set; } = -1;
        public bool ArchCoverVar { get; set; } = false;
        public int Aerials { get; set; } = -1;
        public bool AerialsVar { get; set; } = false;
        public int Trim { get; set; } = -1;
        public bool TrimVar { get; set; } = false;
        public int Tank { get; set; } = -1;
        public bool TankVar { get; set; } = false;
        public int Windows { get; set; } = -1;
        public bool WindowsVar { get; set; } = false;
        public int Livery { get; set; } = -1;
        public bool LiveryVar { get; set; } = false;

        public float SteeringAngle { get; set; } = 0f;
        public float DirtLevel { get; set; } = 0f;

        public int FrontLeftDoorState { get; set; } = 0;
        public int FrontRightDoorState { get; set; } = 0;
        public int BackLeftDoorState { get; set; } = 0;
        public int BackRightDoorState { get; set; } = 0;
        public int HoodState { get; set; } = 0;
        public int TrunkState { get; set; } = 0;

        public VehicleColor PrimaryColor { get; set; }
        public VehicleColor SecondaryColor { get; set; }
        public VehicleColor RimColor { get; set; }
        public VehicleColor PearlescentColor { get; set; }
        public VehicleColor TrimColor { get; set; }
        public VehicleColor DashboardColor { get; set; }
        public bool IsPrimaryCustom { get; set; }
        public bool IsSecondaryCustom { get; set; }
        public System.Drawing.Color CustomPrimaryColor { get; set; }
        public System.Drawing.Color CustomSecondaryColor { get; set; }
        public System.Drawing.Color NeonLightColor { get; set; }
        public System.Drawing.Color TireSmokeColor { get; set; }
        public VehicleWindowTint WindowTint { get; set; }
        public LicensePlateStyle LicensePlateStyle { get; set; }
        public VehicleWheelType WheelType { get; set; }

        public bool NeonLightLeft { get; set; } = false;
        public bool NeonLightRight { get; set; } = false;
        public bool NeonLightFront { get; set; } = false;
        public bool NeonLightBack { get; set; } = false;

        public int[] WindowStates { get; set; } = new int[8];
        public int[] WheelStates { get; set; } = new int[10];
        public bool[] Extras { get; set; } = new bool[15];

        public VehicleDataV1()
        {
            Id = Guid.NewGuid().ToString();
            Meta = new VehicleDataMeta();
            Meta.Id = Id;
        }

        [NonSerialized]
        public Vehicle Handle = null;

        [NonSerialized]
        public VehicleDataMeta Meta = null;

        [NonSerialized]
        public bool WasUserDespawned = false;

        [OptionalField(VersionAdded = 2)]
        public int BlipColor = 0;
        [OptionalField(VersionAdded = 2)]
        public string Tag = "";
        [OptionalField(VersionAdded = 2)]
        public float FrontLeftDoorAngle = 0f;
        [OptionalField(VersionAdded = 2)]
        public float FrontRightDoorAngle = 0f;
        [OptionalField(VersionAdded = 2)]
        public float BackLeftDoorAngle = 0f;
        [OptionalField(VersionAdded = 2)]
        public float BackRightDoorAngle = 0f;
        [OptionalField(VersionAdded = 2)]
        public float HoodAngle = 0f;
        [OptionalField(VersionAdded = 2)]
        public float TrunkAngle = 0f;
        [OptionalField(VersionAdded = 2)]
        public int SirenState = 0;
        [OptionalField(VersionAdded = 2)]
        public int LightState2 = 0;
        [OptionalField(VersionAdded = 2)]
        public bool BulletProofTires = false;
        [OptionalField(VersionAdded = 2)]
        public Vector3 SafeSpawn = new Vector3();
        [OptionalField(VersionAdded = 2)]
        public bool SafeSpawnSet = false;
        [OptionalField(VersionAdded = 2)]
        public Vector3 SafeRotation = new Vector3();
        [OptionalField(VersionAdded = 2)]
        public int IndicatorState = 0;
    }

    [Serializable]
    public class CharacterDataV1
    {
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public string CarAttach { get; set; } = null;
        public int PhoneTheme { get; set; } = 1;
        public int PhoneColor { get; set; } = 0;
        public int PhoneBackground { get; set; } = 0;

        public int Health { get; set; } = 200;
        public int Armor { get; set; } = 100;

        public PedHash PedSkin { get; set; } = PedHash.Franklin;

        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public Weather Weather { get; set; }
        public Weather WeatherNext { get; set; }

        public int[] ClothesVariant { get; set; } = new int[12];
        public int[] ClothesTexture { get; set; } = new int[12];
        public int[] ClothesPalette { get; set; } = new int[12];
        public int[] PropsVariant { get; set; } = new int[7];
        public int[] PropsTexture { get; set; } = new int[7];
    }

    [Serializable]
    public class CharacterDataV2
    {
        public Vector3 Position { get; set; }
        public float Heading { get; set; }
        public string CarAttach { get; set; } = null;
        public int PhoneTheme { get; set; } = 1;
        public int PhoneColor { get; set; } = 0;
        public int PhoneBackground { get; set; } = 0;
        public int PhoneBrightness { get; set; } = 5;

        public bool WearingDropHelmet { get; set; } = false;

        public uint ActiveWeapon { get; set; } = (uint)WeaponHash.Unarmed;

        public int Health { get; set; } = 200;
        public int Armor { get; set; } = 100;

        public int PedSkin { get; set; } = unchecked((int)PedHash.Franklin);

        public DateTime Date { get; set; }
        public TimeSpan Time { get; set; }
        public Weather Weather { get; set; }
        public Weather WeatherNext { get; set; }

        public int[] ClothesVariant { get; set; } = new int[12];
        public int[] ClothesTexture { get; set; } = new int[12];
        public int[] ClothesPalette { get; set; } = new int[12];
        public int[] PropsVariant { get; set; } = new int[7];
        public int[] PropsTexture { get; set; } = new int[7];
        public Dictionary<uint, List<uint>> Weapons { get; set; } = new Dictionary<uint, List<uint>>();
        public Dictionary<uint, uint> Tints { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, int> Ammo { get; set; } = new Dictionary<uint, int>();

        [OptionalField(VersionAdded = 2)]
        public int PhoneBody = 0;
        [OptionalField(VersionAdded = 2)]
        public int PhoneTone = 0;
    }

    public class AdvancedPersistence : Script
    {
        public static List<int> SoundIdBank = new List<int>();
        public static List<VehicleDataV1> VehicleDatabase = new List<VehicleDataV1>();
        public static List<VehicleDataMeta> VehicleMetabase = new List<VehicleDataMeta>();
        public static Dictionary<Vehicle, PedTask> AttachedTasks = new Dictionary<Vehicle, PedTask>();
        public static Dictionary<Vehicle, VehicleDataV1> AttachedVehicles = new Dictionary<Vehicle, VehicleDataV1>();
        public static CharacterDataV2 MainCharacter = new CharacterDataV2();
        public static CharacterDataMeta MainCharacterMeta = new CharacterDataMeta();
        public static Dictionary<int, string> SwitchedCars = new Dictionary<int, string>();

        public static readonly int[] TireIndices = new int[10] { 0, 1, 2, 3, 4, 5, 45, 46, 47, 48 };
        public static readonly string VehicleFilename = "scripts/AdvancedPersistence/AdvPer_VEH_DB.bin";
        public static readonly string VehicleMetaname = "scripts/AdvancedPersistence/AdvPer_VEH_META.bin";
        public static readonly string CharacterFilename = "scripts/AdvancedPersistence/AdvPer_CHR_DB.bin";
        public static readonly string CharacterMetaname = "scripts/AdvancedPersistence/AdvPer_CHR_META.bin";

        public bool StoppedInitial = true;
        public int StartedTime = Game.GameTime;

        public static void PlayFrontendAudio(string sound, int phone)
        {
            if(phone == 0)
            {
                int val = GTA.Audio.PlaySoundFrontend(sound, "Phone_SoundSet_Michael");
                SoundIdBank.Add(val);
            }
            else if(phone == 1)
            {
                int val = GTA.Audio.PlaySoundFrontend(sound, "Phone_SoundSet_Trevor");
                SoundIdBank.Add(val);
            }
            else
            {
                int val = GTA.Audio.PlaySoundFrontend(sound, "Phone_SoundSet_Franklin");
                SoundIdBank.Add(val);
            }
        }

        public AdvancedPersistence()
        {
            LoadCnt = 0;
            LoadMax = 0;
            LoadTick = Game.GameTime + 1500;
            LoadCharacter = true;
            LoadVehicles = false;

            if (Directory.Exists("scripts/" + Constants.SubFolder))
            {
                File.WriteAllText("scripts/" + Constants.SubFolder + "/" + Logging.Filename, string.Empty);
            }
            else
            {
                File.WriteAllText("scripts/" + Logging.Filename, string.Empty);
            }

            Logging.Log("Booting up...");
            try
            {
                bool dirExists = Directory.Exists("scripts/AdvancedPersistence");
                if (!dirExists)
                    Directory.CreateDirectory("scripts/AdvancedPersistence");
            }
            catch(Exception e)
            {
                Logging.Log("Couldn't create sub-folder. File permissions.");
                Logging.Log("ERROR: " + e.ToString());
                Logging.Log("ABORTING");
                Abort();
                return;
            }

            if (ModSettings.LoadSettings())
                Logging.Log("Loaded settings");
            else
                Logging.Log("WARN: Could not find settings, using defaults");

            try
            {
                if (File.Exists(VehicleMetaname))
                {
                    Logging.Log("Found vehicle metabase, loading...");
                    try
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        using (Stream f = File.OpenRead(VehicleMetaname))
                        {
                            VehicleMetabase = (List<VehicleDataMeta>)bin.Deserialize(f);
                        }
                        Logging.Log($"Meta-loaded [{VehicleMetabase.Count}] vehicles");
                    }
                    catch (Exception e)
                    {
                        Logging.Log("ERROR: " + e.ToString());
                    }
                }
                else
                {
                    Logging.Log("WARN: Could not find vehicle metabase");
                }

                if (File.Exists(VehicleFilename))
                {
                    Logging.Log("Found vehicle database, loading...");
                    try
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        using (Stream f = File.OpenRead(VehicleFilename))
                        {
                            VehicleDatabase = (List<VehicleDataV1>)bin.Deserialize(f);
                        }
                        GTA.UI.Notification.Show($"Loaded [{VehicleDatabase.Count}] vehicles");
                        Logging.Log($"Loaded [{VehicleDatabase.Count}] vehicles");
                        LoadMax = VehicleDatabase.Count;
                        LoadCnt = 0;
                    }
                    catch (Exception e)
                    {
                        Logging.Log("ERROR: " + e.ToString());
                    }
                }
                else
                {
                    Logging.Log("WARN: Could not find vehicle database");
                }

                if (File.Exists(CharacterMetaname))
                {
                    Logging.Log("Found character metabase, loading...");
                    try
                    {
                        BinaryFormatter bin = new BinaryFormatter();
                        using (Stream f = File.OpenRead(CharacterMetaname))
                        {
                            MainCharacterMeta = (CharacterDataMeta)bin.Deserialize(f);
                        }
                        Logging.Log("Loaded character meta VERSION: " + MainCharacterMeta.Version);
                    }
                    catch (Exception e)
                    {
                        Logging.Log("ERROR: " + e.ToString());
                    }
                }
                else
                {
                    LoadCharacter = false;
                    IsModLoading = false;
                    //GTA.UI.Screen.ShowSubtitle("Advanced Persistence Loaded!", 3000);
                    Logging.Log("WARN: Could not find character database");
                }

                if (File.Exists(CharacterFilename))
                {
                    Logging.Log("Found character database, loading...");
                    try
                    {
                        if(MainCharacterMeta != null)
                        {
                            if(MainCharacterMeta.Version == 1)
                            {
                                Logging.Log("WARN: DETECTED OLD CHARACTER DATA (Version 1)");
                                Logging.Log("Converting to newer data model...");
                                BinaryFormatter bin = new BinaryFormatter();
                                CharacterDataV1 char1 = new CharacterDataV1();
                                using (Stream f = File.OpenRead(CharacterFilename))
                                {
                                    char1 = (CharacterDataV1)bin.Deserialize(f);
                                }
                                if(char1 != null)
                                {
                                    MainCharacter.Position = char1.Position;
                                    MainCharacter.Heading = char1.Heading;
                                    MainCharacter.CarAttach = char1.CarAttach;
                                    MainCharacter.PhoneTheme = char1.PhoneTheme;
                                    MainCharacter.PhoneColor = char1.PhoneColor;
                                    MainCharacter.PhoneBackground = char1.PhoneBackground;
                                    MainCharacter.PhoneBrightness = 5;
                                    MainCharacter.PhoneBody = 0;
                                    MainCharacter.PhoneTone = 0;
                                    MainCharacter.Health = char1.Health;
                                    MainCharacter.Armor = char1.Armor;
                                    MainCharacter.PedSkin = (int)char1.PedSkin;
                                    MainCharacter.Date = char1.Date;
                                    MainCharacter.Time = char1.Time;
                                    MainCharacter.Weather = char1.Weather;
                                    MainCharacter.WeatherNext = char1.WeatherNext;
                                    for(int i = 0; i < 12; i++)
                                    {
                                        MainCharacter.ClothesVariant[i] = char1.ClothesVariant[i];
                                        MainCharacter.ClothesTexture[i] = char1.ClothesTexture[i];
                                        MainCharacter.ClothesPalette[i] = char1.ClothesPalette[i];
                                    }

                                    for(int i = 0; i < 7; i++)
                                    {
                                        MainCharacter.PropsVariant[i] = char1.PropsVariant[i];
                                        MainCharacter.PropsTexture[i] = char1.PropsTexture[i];
                                    }
                                    MainCharacterMeta.Version = 2;
                                    Logging.Log("SUCCESS: Done conversion");
                                }
                                else
                                {
                                    Logging.Log("Couldn't load meta for conversion");
                                }
                            }
                            else
                            {
                                BinaryFormatter bin = new BinaryFormatter();
                                using (Stream f = File.OpenRead(CharacterFilename))
                                {
                                    MainCharacter = (CharacterDataV2)bin.Deserialize(f);
                                }
                            }
                        }
                        Logging.Log("Loaded character data");
                    }
                    catch (Exception e)
                    {
                        Logging.Log("ERROR: " + e.ToString());
                    }
                }
                else
                {
                    LoadCharacter = false;
                    IsModLoading = false;
                    //GTA.UI.Screen.ShowSubtitle("Advanced Persistence Loaded!", 3000);
                    Logging.Log("WARN: Could not find character database");
                }
            }
            catch(Exception e)
            {
                GTA.UI.Screen.ShowSubtitle("[Advanced Persistence] BOOT UP ERROR. CHECK LOGS.", 10000);
                Logging.Log("ERROR LOADING: " + e.ToString());
                Logging.Log("ABORTING");
                Abort();
                return;
            }

            UpdateTick = Game.GameTime + ModSettings.DataSavingTime;
            if (!LoadCharacter)
            {
                GTA.UI.Screen.FadeIn(2000);
                SaveOne = true;
                UpdateTick = Game.GameTime + 2000;
                IsModLoading = false;
                IsInitializing = false;
            }
            else
                SaveOne = false;
            SaveTwo = false;
            Stream1 = false;
            Stream2 = false;
            Stream3 = false;
            Stream4 = false;
            BlipTick = Game.GameTime + 3000;

            int viewType = GTA.Native.Function.Call<int>((GTA.Native.Hash)(0x19CAFA3C87F7C2FF));
            int viewMode = GTA.Native.Function.Call<int>((GTA.Native.Hash)(0xEE778F8C7E1142E2), viewType);
            if (viewMode == 4)
                LastFirstPerson = true;
            if(ModSettings.RemovePersonalVehicles)
                DeletePersonalCars = true;

            GTA.Native.Function.Call(GTA.Native.Hash.DESTROY_MOBILE_PHONE);
            GTA.Native.Function.Call(GTA.Native.Hash.SCRIPT_IS_MOVING_MOBILE_PHONE_OFFSCREEN, true);

            Logging.Log("Attaching events...");
            Tick += OnTick;
            KeyDown += OnKeyDown;
            KeyUp += OnKeyUp;
            Aborted += OnAborted;
            Logging.Log("Attached");
            Logging.Log("Beginning load...");
        }

        public static void DeleteBlipsOnCar(Vehicle veh)
        {
            if(veh != null)
            {
                if(veh.Exists())
                {
                    foreach (Blip blip in veh.AttachedBlips)
                    {
                        if (blip != null)
                        {
                            if (blip.Exists())
                            {
                                blip.Delete();
                            }
                        }
                    }
                }
            }
        }

        public static bool IsAmphCar(VehicleHash hash)
        {
            return (hash == VehicleHash.SeaSparrow ||
                hash == VehicleHash.SeaSparrow2 ||
                hash == VehicleHash.SeaSparrow3 ||
                hash == VehicleHash.Dodo ||
                hash == VehicleHash.Tula ||
                hash == VehicleHash.Stromberg ||
                hash == VehicleHash.Apc ||
                hash == VehicleHash.Zhaba ||
                hash == VehicleHash.Toreador ||
                hash == VehicleHash.Seabreeze);
        }

        public static bool IsSubmarine(VehicleHash hash)
        {
            return (hash == VehicleHash.Submersible || hash == VehicleHash.Submersible2 || hash == VehicleHash.Avisa);
        }

        public static Vehicle CreateVehicle(VehicleDataV1 vdata, Vehicle bypassVeh = null, bool byPassed = false, bool unfreeze = true, bool atSafeSpot = false)
        {
            if (vdata == null)
                return null;

            if(!atSafeSpot || !vdata.SafeSpawnSet)
                GTA.Native.Function.Call(GTA.Native.Hash.REQUEST_COLLISION_AT_COORD, vdata.Position.X, vdata.Position.Y, vdata.Position.Z);
            else
                GTA.Native.Function.Call(GTA.Native.Hash.REQUEST_COLLISION_AT_COORD, vdata.SafeSpawn.X, vdata.SafeSpawn.Y, vdata.SafeSpawn.Z);

            Vehicle veh = null;
            if (bypassVeh != null)
                veh = bypassVeh;
            else
            {
                if(!atSafeSpot || !vdata.SafeSpawnSet)
                    veh = World.CreateVehicle(vdata.Hash, vdata.Position);
                else
                    veh = World.CreateVehicle(vdata.Hash, vdata.SafeSpawn);
            }
            if (veh == null)
                return null;

            veh.Model.RequestCollision(1000);
            GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_LOAD_COLLISION_FLAG, veh.Handle, true, 1);
            GTA.Native.Function.Call(GTA.Native.Hash.SET_ENTITY_SHOULD_FREEZE_WAITING_ON_COLLISION, veh.Handle, false);
            try
            {
                vdata.WasUserDespawned = false;
                if(!byPassed)
                {
                    if(!atSafeSpot || !vdata.SafeSpawnSet)
                        veh.PositionNoOffset = vdata.Position;
                    else
                        veh.PositionNoOffset = vdata.SafeSpawn;
                    veh.IsPositionFrozen = true;
                    veh.IsInvincible = true;
                }
                veh.Mods.InstallModKit();
                veh.IsPersistent = true;
                if(!byPassed)
                {
                    if (!atSafeSpot || !vdata.SafeSpawnSet)
                        veh.Rotation = vdata.Rotation;
                    else
                        veh.Rotation = vdata.SafeRotation;
                    veh.PlaceOnGround();
                }

                if (veh.Model.IsBoat || veh.Model.IsAmphibiousVehicle || veh.Model.IsAmphibiousQuadBike || veh.Model.IsAmphibiousCar || veh.Model.IsSubmarineCar || veh.Model.IsJetSki || IsSubmarine(veh.Model) || IsAmphCar(veh.Model))
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.SET_BOAT_ANCHOR, veh.Handle, true);
                }

                if (ModSettings.SaveVehicleExtras)
                {
                    for (int i = 0; i < 15; i++)
                    {
                        veh.ToggleExtra(i, vdata.Extras[i]);
                    }
                }

                if (ModSettings.SaveVehicleDoorState)
                {
                    if (vdata.FrontLeftDoorState == 0)
                    {
                        veh.Doors[VehicleDoorIndex.FrontLeftDoor].Close(true);
                    }
                    else if (vdata.FrontLeftDoorState == 1)
                    {
                        veh.Doors[VehicleDoorIndex.FrontLeftDoor].Open(false, false);
                    }
                    else if (vdata.FrontLeftDoorState == 2)
                    {
                        veh.Doors[VehicleDoorIndex.FrontLeftDoor].Break(false);
                    }

                    if (vdata.FrontRightDoorState == 0)
                    {
                        veh.Doors[VehicleDoorIndex.FrontRightDoor].Close(true);
                    }
                    else if (vdata.FrontRightDoorState == 1)
                    {
                        veh.Doors[VehicleDoorIndex.FrontRightDoor].Open(false, false);
                    }
                    else if (vdata.FrontRightDoorState == 2)
                    {
                        veh.Doors[VehicleDoorIndex.FrontRightDoor].Break(false);
                    }

                    if (vdata.BackLeftDoorState == 0)
                    {
                        veh.Doors[VehicleDoorIndex.BackLeftDoor].Close(true);
                    }
                    else if (vdata.BackLeftDoorState == 1)
                    {
                        veh.Doors[VehicleDoorIndex.BackLeftDoor].Open(false, false);
                    }
                    else if (vdata.BackLeftDoorState == 2)
                    {
                        veh.Doors[VehicleDoorIndex.BackLeftDoor].Break(false);
                    }

                    if (vdata.BackRightDoorState == 0)
                    {
                        veh.Doors[VehicleDoorIndex.BackRightDoor].Close(true);
                    }
                    else if (vdata.BackRightDoorState == 1)
                    {
                        veh.Doors[VehicleDoorIndex.BackRightDoor].Open(false, false);
                    }
                    else if (vdata.BackRightDoorState == 2)
                    {
                        veh.Doors[VehicleDoorIndex.BackRightDoor].Break(false);
                    }

                    if (vdata.TrunkState == 0)
                    {
                        veh.Doors[VehicleDoorIndex.Trunk].Close(true);
                    }
                    else if (vdata.TrunkState == 1)
                    {
                        veh.Doors[VehicleDoorIndex.Trunk].Open(false, false);
                    }
                    else if (vdata.TrunkState == 2)
                    {
                        veh.Doors[VehicleDoorIndex.Trunk].Break(false);
                    }

                    if (vdata.HoodState == 0)
                    {
                        veh.Doors[VehicleDoorIndex.Hood].Close(true);
                    }
                    else if (vdata.HoodState == 1)
                    {
                        veh.Doors[VehicleDoorIndex.Hood].Open(false, false);
                    }
                    else if (vdata.HoodState == 2)
                    {
                        veh.Doors[VehicleDoorIndex.Hood].Break(false);
                    }
                }

                if (ModSettings.SaveVehicleWindowState)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        if (vdata.WheelStates[i] == 1)
                            GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_TYRE_BURST, veh.Handle, TireIndices[i], false, 100.0f);
                        else if (vdata.WheelStates[i] == 2)
                            GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_TYRE_BURST, veh.Handle, TireIndices[i], true, 1000.0f);
                    }
                }

                if (ModSettings.SaveVehicleConvertibleState)
                {
                    if (veh.IsConvertible)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.ROLL_DOWN_WINDOWS, veh.Handle);

                        if (vdata.ConvertibleState)
                            GTA.Native.Function.Call(GTA.Native.Hash.RAISE_CONVERTIBLE_ROOF, veh.Handle, true);
                        else
                            GTA.Native.Function.Call(GTA.Native.Hash.LOWER_CONVERTIBLE_ROOF, veh.Handle, true);
                    }
                }

                if(!byPassed)
                    Wait(1);

                if (ModSettings.SaveVehicleWindowState)
                {
                    for (int i = 0; i < 8; i++)
                    {
                        if (vdata.WindowStates[i] == 1)
                            GTA.Native.Function.Call(GTA.Native.Hash.SMASH_VEHICLE_WINDOW, veh.Handle, i);
                        else
                        {
                            if (!veh.IsConvertible)
                                GTA.Native.Function.Call(GTA.Native.Hash.ROLL_UP_WINDOW, veh.Handle, i);
                            GTA.Native.Function.Call(GTA.Native.Hash.FIX_VEHICLE_WINDOW, veh.Handle, i);
                        }
                    }
                }

                if(!byPassed)
                {
                    if(ModSettings.SaveCharacterPosition)
                    {
                        if (vdata.Id == AdvancedPersistence.MainCharacter.CarAttach)
                        {
                            Game.Player.Character.SetIntoVehicle(veh, VehicleSeat.Driver);
                        }
                    }
                }
                

                veh.DirtLevel = vdata.DirtLevel;
                //veh.IsEngineRunning = false;
                if(veh.Model.IsPlane)
                {

                }
                else
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_ENGINE_ON, veh.Handle, false, true, false);
                    if (ModSettings.SaveVehicleEngineState)
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_ENGINE_ON, veh.Handle, vdata.EngineState, true, false);
                }

                if (ModSettings.SaveVehicleLightState)
                {
                    if (vdata.LightState2 == 0)
                    {
                        veh.AreHighBeamsOn = false;
                        veh.AreLightsOn = false;
                    }
                    else if(vdata.LightState2 == 1)
                    {
                        veh.AreHighBeamsOn = false;
                        veh.AreLightsOn = true;
                    }
                    else
                    {
                        veh.AreHighBeamsOn = true;
                        veh.AreLightsOn = true;
                    }

                    if (vdata.SirenState == 1)
                        veh.IsSirenActive = true;
                }
                veh.IsAlarmSet = vdata.AlarmState;
                if (vdata.LockState)
                    veh.LockStatus = VehicleLockStatus.CannotEnter;
                AttachedVehicles[veh] = vdata;
                vdata.Handle = veh;



                if (ModSettings.SaveVehicleMods)
                {
                    veh.CanTiresBurst = !vdata.BulletProofTires;
                    veh.Mods.NeonLightsColor = vdata.NeonLightColor;
                    veh.Mods.WindowTint = vdata.WindowTint;
                    veh.Mods.LicensePlateStyle = vdata.LicensePlateStyle;
                    veh.Mods.Livery = vdata.Livery;
                    veh.Mods.WheelType = vdata.WheelType;
                    veh.Mods.TireSmokeColor = vdata.TireSmokeColor;

                    if (vdata.Turbo)
                        veh.Mods[VehicleToggleModType.Turbo].IsInstalled = true;
                    if (vdata.XenonHeadlights)
                        veh.Mods[VehicleToggleModType.XenonHeadlights].IsInstalled = true;
                    if (vdata.TireSmoke)
                        veh.Mods[VehicleToggleModType.TireSmoke].IsInstalled = true;

                    veh.Mods[VehicleModType.Spoilers].Index = vdata.Spoiler;
                    veh.Mods[VehicleModType.Spoilers].Variation = vdata.SpoilerVar;

                    veh.Mods[VehicleModType.FrontBumper].Index = vdata.FrontBumper;
                    veh.Mods[VehicleModType.FrontBumper].Variation = vdata.FrontBumperVar;

                    veh.Mods[VehicleModType.RearBumper].Index = vdata.RearBumper;
                    veh.Mods[VehicleModType.RearBumper].Variation = vdata.RearBumperVar;

                    veh.Mods[VehicleModType.SideSkirt].Index = vdata.SideSkirt;
                    veh.Mods[VehicleModType.SideSkirt].Variation = vdata.SideSkirtVar;

                    veh.Mods[VehicleModType.Exhaust].Index = vdata.Exhaust;
                    veh.Mods[VehicleModType.Exhaust].Variation = vdata.ExhaustVar;

                    veh.Mods[VehicleModType.Frame].Index = vdata.Frame;
                    veh.Mods[VehicleModType.Frame].Variation = vdata.FrameVar;

                    veh.Mods[VehicleModType.Grille].Index = vdata.Grille;
                    veh.Mods[VehicleModType.Grille].Variation = vdata.GrilleVar;

                    veh.Mods[VehicleModType.Hood].Index = vdata.Hood;
                    veh.Mods[VehicleModType.Hood].Variation = vdata.HoodVar;

                    veh.Mods[VehicleModType.Fender].Index = vdata.Fender;
                    veh.Mods[VehicleModType.Fender].Variation = vdata.FenderVar;

                    veh.Mods[VehicleModType.RightFender].Index = vdata.RightFender;
                    veh.Mods[VehicleModType.RightFender].Variation = vdata.RightFenderVar;

                    veh.Mods[VehicleModType.Roof].Index = vdata.Roof;
                    veh.Mods[VehicleModType.Roof].Variation = vdata.RoofVar;

                    veh.Mods[VehicleModType.Engine].Index = vdata.Engine;
                    veh.Mods[VehicleModType.Engine].Variation = vdata.EngineVar;

                    veh.Mods[VehicleModType.Brakes].Index = vdata.Brakes;
                    veh.Mods[VehicleModType.Brakes].Variation = vdata.BrakesVar;

                    veh.Mods[VehicleModType.Transmission].Index = vdata.Transmission;
                    veh.Mods[VehicleModType.Transmission].Variation = vdata.TransmissionVar;

                    veh.Mods[VehicleModType.Horns].Index = vdata.Horns;
                    veh.Mods[VehicleModType.Horns].Variation = vdata.HornsVar;

                    veh.Mods[VehicleModType.Suspension].Index = vdata.Suspension;
                    veh.Mods[VehicleModType.Suspension].Variation = vdata.SuspensionVar;

                    veh.Mods[VehicleModType.Armor].Index = vdata.Armor;
                    veh.Mods[VehicleModType.Armor].Variation = vdata.ArmorVar;

                    veh.Mods[VehicleModType.FrontWheel].Index = vdata.FrontWheel;
                    veh.Mods[VehicleModType.FrontWheel].Variation = vdata.FrontWheelVar;

                    veh.Mods[VehicleModType.RearWheel].Index = vdata.RearWheel;
                    veh.Mods[VehicleModType.RearWheel].Variation = vdata.RearWheelVar;

                    veh.Mods[VehicleModType.PlateHolder].Index = vdata.PlateHolder;
                    veh.Mods[VehicleModType.PlateHolder].Variation = vdata.PlateHolderVar;

                    veh.Mods[VehicleModType.VanityPlates].Index = vdata.VanityPlates;
                    veh.Mods[VehicleModType.VanityPlates].Variation = vdata.VanityPlatesVar;

                    veh.Mods[VehicleModType.TrimDesign].Index = vdata.TrimDesign;
                    veh.Mods[VehicleModType.TrimDesign].Variation = vdata.TrimDesignVar;

                    veh.Mods[VehicleModType.Ornaments].Index = vdata.Ornaments;
                    veh.Mods[VehicleModType.Ornaments].Variation = vdata.OrnamentsVar;

                    veh.Mods[VehicleModType.Dashboard].Index = vdata.Dashboard;
                    veh.Mods[VehicleModType.Dashboard].Variation = vdata.DashboardVar;

                    veh.Mods[VehicleModType.DialDesign].Index = vdata.DialDesign;
                    veh.Mods[VehicleModType.DialDesign].Variation = vdata.DialDesignVar;

                    veh.Mods[VehicleModType.DoorSpeakers].Index = vdata.DoorSpeakers;
                    veh.Mods[VehicleModType.DoorSpeakers].Variation = vdata.DoorSpeakersVar;

                    veh.Mods[VehicleModType.Seats].Index = vdata.Seats;
                    veh.Mods[VehicleModType.Seats].Variation = vdata.SeatsVar;

                    veh.Mods[VehicleModType.SteeringWheels].Index = vdata.SteeringWheels;
                    veh.Mods[VehicleModType.SteeringWheels].Variation = vdata.SteeringWheelsVar;

                    veh.Mods[VehicleModType.ColumnShifterLevers].Index = vdata.ColumnShifterLevers;
                    veh.Mods[VehicleModType.ColumnShifterLevers].Variation = vdata.ColumnShifterLeversVar;

                    veh.Mods[VehicleModType.Plaques].Index = vdata.Plaques;
                    veh.Mods[VehicleModType.Plaques].Variation = vdata.PlaquesVar;

                    veh.Mods[VehicleModType.Speakers].Index = vdata.Speakers;
                    veh.Mods[VehicleModType.Speakers].Variation = vdata.SpeakersVar;

                    veh.Mods[VehicleModType.Trunk].Index = vdata.Trunk;
                    veh.Mods[VehicleModType.Trunk].Variation = vdata.TrunkVar;

                    veh.Mods[VehicleModType.Hydraulics].Index = vdata.Hydraulics;
                    veh.Mods[VehicleModType.Hydraulics].Variation = vdata.HydraulicsVar;

                    veh.Mods[VehicleModType.EngineBlock].Index = vdata.EngineBlock;
                    veh.Mods[VehicleModType.EngineBlock].Variation = vdata.EngineBlockVar;

                    veh.Mods[VehicleModType.AirFilter].Index = vdata.AirFilter;
                    veh.Mods[VehicleModType.AirFilter].Variation = vdata.AirFilterVar;

                    veh.Mods[VehicleModType.Struts].Index = vdata.Struts;
                    veh.Mods[VehicleModType.Struts].Variation = vdata.StrutsVar;

                    veh.Mods[VehicleModType.ArchCover].Index = vdata.ArchCover;
                    veh.Mods[VehicleModType.ArchCover].Variation = vdata.ArchCoverVar;

                    veh.Mods[VehicleModType.Aerials].Index = vdata.Aerials;
                    veh.Mods[VehicleModType.Aerials].Variation = vdata.AerialsVar;

                    veh.Mods[VehicleModType.Trim].Index = vdata.Trim;
                    veh.Mods[VehicleModType.Trim].Variation = vdata.TrimVar;

                    veh.Mods[VehicleModType.Tank].Index = vdata.Tank;
                    veh.Mods[VehicleModType.Tank].Variation = vdata.TankVar;

                    veh.Mods[VehicleModType.Windows].Index = vdata.Windows;
                    veh.Mods[VehicleModType.Windows].Variation = vdata.WindowsVar;

                    veh.Mods[VehicleModType.Livery].Index = vdata.Livery;
                    veh.Mods[VehicleModType.Livery].Variation = vdata.LiveryVar;

                    veh.Mods.SetNeonLightsOn(VehicleNeonLight.Back, vdata.NeonLightBack);
                    veh.Mods.SetNeonLightsOn(VehicleNeonLight.Front, vdata.NeonLightFront);
                    veh.Mods.SetNeonLightsOn(VehicleNeonLight.Left, vdata.NeonLightLeft);
                    veh.Mods.SetNeonLightsOn(VehicleNeonLight.Right, vdata.NeonLightRight);

                    veh.Mods.PrimaryColor = vdata.PrimaryColor;
                    veh.Mods.SecondaryColor = vdata.SecondaryColor;
                    veh.Mods.PearlescentColor = vdata.PearlescentColor;
                    veh.Mods.DashboardColor = vdata.DashboardColor;
                    //veh.Mods.ColorCombination = vdata.ColorCombination;
                    if (vdata.IsPrimaryCustom)
                        veh.Mods.CustomPrimaryColor = vdata.CustomPrimaryColor;
                    if (vdata.IsSecondaryCustom)
                        veh.Mods.CustomSecondaryColor = vdata.CustomSecondaryColor;
                    veh.Mods.LicensePlate = vdata.LicensePlate;

                    veh.Mods.TrimColor = vdata.TrimColor;
                    veh.Mods.RimColor = vdata.RimColor;

                    GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_MOD, veh.Handle, 40, vdata.Boost, vdata.RearWheelVar);


                    GTA.Native.Function.Call((GTA.Native.Hash)0xE41033B25D003A07, veh.Handle, vdata.XenonHeadlightsColor);
                }

                if (ModSettings.SaveVehicleWheelTurn)
                {
                    if (vdata.SteeringAngle >= -10f && vdata.SteeringAngle <= 10f)
                    {

                    }
                    else
                    {
                        if (vdata.SteeringAngle > 0f)
                            veh.SteeringAngle = vdata.SteeringAngle - 10f;
                        else
                            veh.SteeringAngle = vdata.SteeringAngle + 10f;
                    }
                }

                if(!byPassed)
                {
                    veh.IsHandbrakeForcedOn = true;
                    Wait(1);
                    veh.IsHandbrakeForcedOn = false;
                }

                if(ModSettings.EnableBlips)
                {
                    if (veh.AttachedBlip == null)
                        veh.AddBlip();
                    if (veh.Model.IsHelicopter)
                        veh.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                    else if (veh.Model.IsAmphibiousQuadBike || veh.Model.IsBicycle || veh.Model.IsBike || veh.Model.IsQuadBike)
                        veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                    else if (veh.Model.IsJetSki)
                        veh.AttachedBlip.Sprite = BlipSprite.Seashark;
                    else if (veh.Model.IsBoat)
                        veh.AttachedBlip.Sprite = BlipSprite.Boat;
                    else if (veh.Model.IsPlane)
                        veh.AttachedBlip.Sprite = BlipSprite.Plane;
                    else
                        veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                    veh.AttachedBlip.IsShortRange = true;
                    veh.AttachedBlip.Scale = 0.75f;
                    veh.AttachedBlip.Alpha = 255;
                    veh.AttachedBlip.Name = "Saved Vehicle";
                    veh.AttachedBlip.Priority = 0;
                    GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, veh.AttachedBlip.Handle, false);
                    veh.AttachedBlip.Color = (BlipColor)vdata.BlipColor;
                    GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, veh.AttachedBlip.Handle, false);
                }

                if(!byPassed)
                {
                    veh.IsInvincible = false;
                    if(!veh.Model.IsHelicopter)
                    {
                        if(unfreeze)
                        {
                            veh.IsPositionFrozen = false;
                        }
                    }
                }
                return veh;
            }
            catch(Exception e)
            {
                Logging.Log("ERROR VEH: " + e.ToString());
                return null;
            }
        }

        private void OnAborted(object sender, EventArgs e)
        {
            Logging.Log("ABORTED Cleaning up...");
            try
            {
                if (GTA.UI.Screen.IsFadedOut)
                    GTA.UI.Screen.FadeIn(2000);
                int cleanCount = 0;
                foreach (int x in SoundIdBank)
                {
                    GTA.Audio.StopSound(x);
                    GTA.Audio.ReleaseSound(x);
                }

                if (FobObj != null)
                {
                    FobObj.IsPersistent = true;
                    FobObj.Delete();
                    FobObj = null;
                }

                if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DOOR_REGISTERED_WITH_SYSTEM, 69696969))
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.REMOVE_DOOR_FROM_SYSTEM, 69696969);
                }

                if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DOOR_REGISTERED_WITH_SYSTEM, 696969))
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.REMOVE_DOOR_FROM_SYSTEM, 696969);
                }

                if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DOOR_REGISTERED_WITH_SYSTEM, 6969))
                {
                    GTA.Native.Function.Call(GTA.Native.Hash.REMOVE_DOOR_FROM_SYSTEM, 6969);
                }

                foreach (VehicleDataV1 dat in VehicleDatabase)
                {
                    if (dat.Handle != null)
                    {
                        DeleteBlipsOnCar(dat.Handle);
                        if (dat.Handle.Exists())
                        {
                            GTA.Native.OutputArgument arg1 = new GTA.Native.OutputArgument(dat.Handle);
                            GTA.Native.Function.Call(GTA.Native.Hash.DELETE_VEHICLE, arg1);
                        }
                        cleanCount++;
                        dat.Handle = null;
                    }
                }
                foreach (var task in AttachedTasks)
                {
                    task.Value.Clean();
                    if (task.Value.Ped != null)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.CLEAR_PED_TASKS_IMMEDIATELY, task.Value.Ped.Handle);
                        GTA.Native.Function.Call(GTA.Native.Hash.TASK_LEAVE_VEHICLE, task.Value.Ped.Handle, task.Key.Handle, 16);
                        if (task.Value.Ped.Exists())
                        {
                            task.Value.Ped.IsPersistent = true;
                            task.Value.Ped.Delete();
                        }
                    }
                }
                Logging.Log($"Cleaned up [{cleanCount}] vehicles");
            }
            catch(Exception ex)
            {
                Logging.Log("ABORT FAILED: " + ex.ToString());
            }
        }

        public int UpdateTick = 0;
        public int BlipTick = 0;
        public int LoadTick = 0;
        public int SaveTick = 0;
        public int StreamTick = 0;
        public bool LoadCharacter = false;
        public bool LoadVehicles = false;
        public int LoadMax = 0;
        public int LoadCnt = 0;
        public static bool SaveVehicle = false;

        public bool SaveOne = false;
        public int SaveTimed = 0;
        public bool SaveTwo = false;
        public bool SaveThree = false;
        public bool DeletePersonalCars = false;

        public static void SaveVehicleData(Vehicle veh, VehicleDataV1 dat)
        {
            dat.Hash = veh.Model;
            if(!veh.IsDead)
                dat.Position = veh.Position;
            dat.Rotation = veh.Rotation;
            dat.SteeringAngle = veh.SteeringAngle;
            dat.Handle = veh;
            veh.IsPersistent = true;

            if (dat.Handle.Doors[VehicleDoorIndex.FrontLeftDoor].IsBroken)
            {
                dat.FrontLeftDoorState = 2;
            }
            else if (dat.Handle.Doors[VehicleDoorIndex.FrontLeftDoor].AngleRatio > 0.1f)
            {
                dat.FrontLeftDoorState = 1;
            }
            else
            {
                dat.FrontLeftDoorState = 0;
            }

            if (dat.Handle.Doors[VehicleDoorIndex.FrontRightDoor].IsBroken)
            {
                dat.FrontRightDoorState = 2;
            }
            else if (dat.Handle.Doors[VehicleDoorIndex.FrontRightDoor].AngleRatio > 0.1f)
            {
                dat.FrontRightDoorState = 1;
            }
            else
            {
                dat.FrontRightDoorState = 0;
            }

            if (dat.Handle.Doors[VehicleDoorIndex.BackLeftDoor].IsBroken)
            {
                dat.BackLeftDoorState = 2;
            }
            else if (dat.Handle.Doors[VehicleDoorIndex.BackLeftDoor].AngleRatio > 0.1f)
            {
                dat.BackLeftDoorState = 1;
            }
            else
            {
                dat.BackLeftDoorState = 0;
            }

            if (dat.Handle.Doors[VehicleDoorIndex.BackRightDoor].IsBroken)
            {
                dat.BackRightDoorState = 2;
            }
            else if (dat.Handle.Doors[VehicleDoorIndex.BackRightDoor].AngleRatio > 0.1f)
            {
                dat.BackRightDoorState = 1;
            }
            else
            {
                dat.BackRightDoorState = 0;
            }

            if (dat.Handle.Doors[VehicleDoorIndex.Trunk].IsBroken)
            {
                dat.TrunkState = 2;
            }
            else if (dat.Handle.Doors[VehicleDoorIndex.Trunk].AngleRatio > 0.1f)
            {
                dat.TrunkState = 1;
            }
            else
            {
                dat.TrunkState = 0;
            }

            if (dat.Handle.Doors[VehicleDoorIndex.Hood].IsBroken)
            {
                dat.HoodState = 2;
            }
            else if (dat.Handle.Doors[VehicleDoorIndex.Hood].AngleRatio > 0.1f)
            {
                dat.HoodState = 1;
            }
            else
            {
                dat.HoodState = 0;
            }

            for(int i = 0; i < 10; i++)
            {
                bool val = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_VEHICLE_TYRE_BURST, veh.Handle, TireIndices[i], true);
                if(!val)
                {
                    val = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_VEHICLE_TYRE_BURST, veh.Handle, TireIndices[i], false);
                    if(!val)
                    {
                        dat.WheelStates[i] = 0;
                    }
                    else
                    {
                        dat.WheelStates[i] = 1;
                    }
                }
                else
                {
                    dat.WheelStates[i] = 2;
                }
            }

            for(int i = 0; i < 8; i++)
            {
                bool val = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_VEHICLE_WINDOW_INTACT, veh.Handle, i);
                if (val)
                    dat.WindowStates[i] = 0;
                else
                    dat.WindowStates[i] = 1;
            }

            int convertibleVal = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CONVERTIBLE_ROOF_STATE, veh.Handle);
            if (convertibleVal == 0)
                dat.ConvertibleState = true;
            else
                dat.ConvertibleState = false;

            dat.DirtLevel = veh.DirtLevel;
            dat.EngineState = veh.IsEngineRunning;
            dat.LockState = veh.LockStatus == VehicleLockStatus.CannotEnter;
            dat.AlarmState = veh.IsAlarmSet;
            dat.LightState = veh.AreLightsOn;
            dat.PrimaryColor = veh.Mods.PrimaryColor;
            dat.SecondaryColor = veh.Mods.SecondaryColor;
            dat.PearlescentColor = veh.Mods.PearlescentColor;
            dat.DashboardColor = veh.Mods.DashboardColor;
            dat.IsPrimaryCustom = veh.Mods.IsPrimaryColorCustom;
            dat.IsSecondaryCustom = veh.Mods.IsSecondaryColorCustom;
            dat.CustomPrimaryColor = veh.Mods.CustomPrimaryColor;
            dat.CustomSecondaryColor = veh.Mods.CustomSecondaryColor;
            dat.LicensePlate = veh.Mods.LicensePlate;

            dat.LightState2 = 0;
            if(veh.IsEngineRunning)
            {
                if (veh.AreLightsOn)
                {
                    if (veh.AreHighBeamsOn)
                        dat.LightState2 = 2;
                    else
                        dat.LightState2 = 1;
                }
            }

            dat.SirenState = 0;
            if (veh.IsSirenActive)
                dat.SirenState = 1;

            dat.BulletProofTires = !veh.CanTiresBurst;

            if (veh.Mods[VehicleToggleModType.Turbo].IsInstalled)
            {
                dat.Turbo = true;
            }
            else
            {
                dat.Turbo = false;
            }

            if(veh.Mods[VehicleToggleModType.TireSmoke].IsInstalled)
            {
                dat.TireSmoke = true;
            }
            else
            {
                dat.TireSmoke = false;
            }

            if(veh.Mods[VehicleToggleModType.XenonHeadlights].IsInstalled)
            {
                dat.XenonHeadlights = true;
            }
            else
            {
                dat.XenonHeadlights = false;
            }

            dat.SpoilerVar = veh.Mods[VehicleModType.Spoilers].Variation;
            dat.Spoiler = veh.Mods[VehicleModType.Spoilers].Index;

            dat.FrontBumperVar = veh.Mods[VehicleModType.FrontBumper].Variation;
            dat.FrontBumper = veh.Mods[VehicleModType.FrontBumper].Index;

            dat.RearBumperVar = veh.Mods[VehicleModType.RearBumper].Variation;
            dat.RearBumper = veh.Mods[VehicleModType.RearBumper].Index;

            dat.SideSkirtVar = veh.Mods[VehicleModType.SideSkirt].Variation;
            dat.SideSkirt = veh.Mods[VehicleModType.SideSkirt].Index;

            dat.ExhaustVar = veh.Mods[VehicleModType.Exhaust].Variation;
            dat.Exhaust = veh.Mods[VehicleModType.Exhaust].Index;

            dat.FrameVar = veh.Mods[VehicleModType.Frame].Variation;
            dat.Frame = veh.Mods[VehicleModType.Frame].Index;

            dat.GrilleVar = veh.Mods[VehicleModType.Grille].Variation;
            dat.Grille = veh.Mods[VehicleModType.Grille].Index;

            dat.HoodVar = veh.Mods[VehicleModType.Hood].Variation;
            dat.Hood = veh.Mods[VehicleModType.Hood].Index;

            dat.FenderVar = veh.Mods[VehicleModType.Fender].Variation;
            dat.Fender = veh.Mods[VehicleModType.Fender].Index;

            dat.RightFenderVar = veh.Mods[VehicleModType.RightFender].Variation;
            dat.RightFender = veh.Mods[VehicleModType.RightFender].Index;

            dat.RoofVar = veh.Mods[VehicleModType.Roof].Variation;
            dat.Roof = veh.Mods[VehicleModType.Roof].Index;

            dat.EngineVar = veh.Mods[VehicleModType.Engine].Variation;
            dat.Engine = veh.Mods[VehicleModType.Engine].Index;

            dat.BrakesVar = veh.Mods[VehicleModType.Brakes].Variation;
            dat.Brakes = veh.Mods[VehicleModType.Brakes].Index;

            dat.TransmissionVar = veh.Mods[VehicleModType.Transmission].Variation;
            dat.Transmission = veh.Mods[VehicleModType.Transmission].Index;

            dat.HornsVar = veh.Mods[VehicleModType.Horns].Variation;
            dat.Horns = veh.Mods[VehicleModType.Horns].Index;

            dat.SuspensionVar = veh.Mods[VehicleModType.Suspension].Variation;
            dat.Suspension = veh.Mods[VehicleModType.Suspension].Index;

            dat.ArmorVar = veh.Mods[VehicleModType.Armor].Variation;
            dat.Armor = veh.Mods[VehicleModType.Armor].Index;

            dat.FrontWheelVar = veh.Mods[VehicleModType.FrontWheel].Variation;
            dat.FrontWheel = veh.Mods[VehicleModType.FrontWheel].Index;

            dat.RearWheelVar = veh.Mods[VehicleModType.RearWheel].Variation;
            dat.RearWheel = veh.Mods[VehicleModType.RearWheel].Index;

            dat.PlateHolderVar = veh.Mods[VehicleModType.PlateHolder].Variation;
            dat.PlateHolder = veh.Mods[VehicleModType.PlateHolder].Index;

            dat.VanityPlatesVar = veh.Mods[VehicleModType.VanityPlates].Variation;
            dat.VanityPlates = veh.Mods[VehicleModType.VanityPlates].Index;

            dat.TrimDesignVar = veh.Mods[VehicleModType.TrimDesign].Variation;
            dat.TrimDesign = veh.Mods[VehicleModType.TrimDesign].Index;

            dat.OrnamentsVar = veh.Mods[VehicleModType.Ornaments].Variation;
            dat.Ornaments = veh.Mods[VehicleModType.Ornaments].Index;

            dat.DashboardVar = veh.Mods[VehicleModType.Dashboard].Variation;
            dat.Dashboard = veh.Mods[VehicleModType.Dashboard].Index;

            dat.DialDesignVar = veh.Mods[VehicleModType.DialDesign].Variation;
            dat.DialDesign = veh.Mods[VehicleModType.DialDesign].Index;

            dat.DoorSpeakersVar = veh.Mods[VehicleModType.DoorSpeakers].Variation;
            dat.DoorSpeakers = veh.Mods[VehicleModType.DoorSpeakers].Index;

            dat.SeatsVar = veh.Mods[VehicleModType.Seats].Variation;
            dat.Seats = veh.Mods[VehicleModType.Seats].Index;

            dat.SteeringWheelsVar = veh.Mods[VehicleModType.SteeringWheels].Variation;
            dat.SteeringWheels = veh.Mods[VehicleModType.SteeringWheels].Index;

            dat.ColumnShifterLeversVar = veh.Mods[VehicleModType.ColumnShifterLevers].Variation;
            dat.ColumnShifterLevers = veh.Mods[VehicleModType.ColumnShifterLevers].Index;

            dat.PlaquesVar = veh.Mods[VehicleModType.Plaques].Variation;
            dat.Plaques = veh.Mods[VehicleModType.Plaques].Index;

            dat.SpeakersVar = veh.Mods[VehicleModType.Speakers].Variation;
            dat.Speakers = veh.Mods[VehicleModType.Speakers].Index;

            dat.TrunkVar = veh.Mods[VehicleModType.Trunk].Variation;
            dat.Trunk = veh.Mods[VehicleModType.Trunk].Index;

            dat.HydraulicsVar = veh.Mods[VehicleModType.Hydraulics].Variation;
            dat.Hydraulics = veh.Mods[VehicleModType.Hydraulics].Index;

            dat.EngineBlockVar = veh.Mods[VehicleModType.EngineBlock].Variation;
            dat.EngineBlock = veh.Mods[VehicleModType.EngineBlock].Index;

            dat.AirFilterVar = veh.Mods[VehicleModType.AirFilter].Variation;
            dat.AirFilter = veh.Mods[VehicleModType.AirFilter].Index;

            dat.StrutsVar = veh.Mods[VehicleModType.Struts].Variation;
            dat.Struts = veh.Mods[VehicleModType.Struts].Index;

            dat.ArchCoverVar = veh.Mods[VehicleModType.ArchCover].Variation;
            dat.ArchCover = veh.Mods[VehicleModType.ArchCover].Index;

            dat.AerialsVar = veh.Mods[VehicleModType.Aerials].Variation;
            dat.Aerials = veh.Mods[VehicleModType.Aerials].Index;

            dat.TrimVar = veh.Mods[VehicleModType.Trim].Variation;
            dat.Trim = veh.Mods[VehicleModType.Trim].Index;

            dat.TankVar = veh.Mods[VehicleModType.Tank].Variation;
            dat.Tank = veh.Mods[VehicleModType.Tank].Index;

            dat.WindowsVar = veh.Mods[VehicleModType.Windows].Variation;
            dat.Windows = veh.Mods[VehicleModType.Windows].Index;

            dat.LiveryVar = veh.Mods[VehicleModType.Livery].Variation;
            dat.Livery = veh.Mods[VehicleModType.Livery].Index;

            dat.Boost = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_VEHICLE_MOD, veh.Handle, 40);

            dat.NeonLightBack = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Back);
            dat.NeonLightFront = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Front);
            dat.NeonLightLeft = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Left);
            dat.NeonLightRight = veh.Mods.IsNeonLightsOn(VehicleNeonLight.Right);

            dat.NeonLightColor = veh.Mods.NeonLightsColor;
            dat.WindowTint = veh.Mods.WindowTint;
            dat.LicensePlateStyle = veh.Mods.LicensePlateStyle;
            dat.Livery = veh.Mods.Livery;
            dat.WheelType = veh.Mods.WheelType;
            dat.TireSmokeColor = veh.Mods.TireSmokeColor;
            dat.XenonHeadlightsColor = GTA.Native.Function.Call<int>((GTA.Native.Hash)0x3DFF319A831E0CDB, veh.Handle);
            dat.RimColor = veh.Mods.RimColor;
            dat.TrimColor = veh.Mods.TrimColor;

            for(int i = 0; i < 15; i++)
            {
                dat.Extras[i] = veh.IsExtraOn(i);
            }
        }

        public bool LastViewChanged = false;
        public bool LastFirstPerson = false;
        public bool Stream1 = false;
        public bool Stream2 = false;
        public bool Stream3 = false;
        public bool Stream4 = false;
        public bool DoWeapons = false;
        public bool DoVehicles = false;
        public List<VehicleDataV1> CloneList = null;
        public WeaponHash[] Weapons = (WeaponHash[])Enum.GetValues(typeof(WeaponHash));
        public int WeaponIndex = 0;
        public int VehicleIndex = 0;
        public WeaponComponentHash[] WeaponComponents = (WeaponComponentHash[])Enum.GetValues(typeof(WeaponComponentHash));
        public BinaryFormatter form = new BinaryFormatter();

        public bool IsControlJustReleased(int num, GTA.Control ctrl)
        {
            return GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_JUST_RELEASED, num, (int)ctrl);
        }

        public bool IsControlPressed(int num, GTA.Control ctrl)
        {
            return GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_PRESSED, num, (int)ctrl);
        }

        public bool IsControlJustPressed(int num, GTA.Control ctrl)
        {
            return GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_JUST_PRESSED, num, (int)ctrl);
        }

        public bool IsDisabledControlJustReleased(int num, GTA.Control ctrl)
        {
            return GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DISABLED_CONTROL_JUST_RELEASED, num, (int)ctrl);
        } 

        public bool debouncer = false;
        public bool switchDetect = true;
        public bool waitForSwitch = false;
        public static int AudioTick = Game.GameTime;
        public bool ActivateThings = false;
        public bool IsModLoading = true;
        public bool IsInitializing = true;
        public int InitTime = Game.GameTime;
        public static bool DrawTrace = false;
        public static Vector3 StartTrace = new Vector3();
        public static Vector3 EndTrace = new Vector3();
        public Dictionary<uint, List<uint>> CWeapons { get; set; } = new Dictionary<uint, List<uint>>();
        public Dictionary<uint, uint> CTints { get; set; } = new Dictionary<uint, uint>();
        public Dictionary<uint, int> CAmmo { get; set; } = new Dictionary<uint, int>();

        public int TimeDiffToSec(int before, int now)
        {
            return Math.Max(0, (int)(4 - ((now - before)/1000f)));
        }

        public int RaycastTick = Game.GameTime;
        Vector3 ep_priv;
        Vector3 ep2_priv;
        Vector3 ep3_priv;
        Vector3 from_priv;
        Vector3 to_priv;
        Vector3 up_priv;
        Vector3 right_priv;
        public int GetOutHint = Game.GameTime;
        public bool DoGetOutHint = false;
        public VehicleDataV1 PressToSaveVeh = null;
        public Vehicle LastHinted = null;
        public int WaitTime = 0;

        public bool DoXThing = false;
        public int XTime = 0;
        public string SaveCarString = "";
        public float SteeringAngleRemote = 0f;
        public bool CanDie = true;
        public bool CanArrest = true;
        public bool GotArrested = false;
        public bool HasDied = false;
        public int PromptTime = 0;
        public bool ResetCars = false;
        public Vector3 DieArrestedAt = new Vector3();
        public Dictionary<Vehicle, int> DrawCarBlinkers = new Dictionary<Vehicle, int>();

        private void OnTick(object sender, EventArgs e)
        {
            if (Game.IsLoading)
                return;

            if (IsInitializing)
            {
                if (Game.GameTime - InitTime >= 4500)
                {
                    IsInitializing = false;
                    GTA.UI.Screen.FadeOut(100);
                }
                else
                {
                    return;
                }
            }

            foreach (Vehicle v in DrawCarBlinkers.Keys.ToList())
            {
                if(Game.GameTime - DrawCarBlinkers[v] > 500)
                {
                    DrawCarBlinkers.Remove(v);
                }
                else
                {
                    int indc1 = BoneHelper.GetBoneIndex(v, "indicator_lf");
                    int indc2 = BoneHelper.GetBoneIndex(v, "indicator_rf");
                    int indc3 = BoneHelper.GetBoneIndex(v, "indicator_lr");
                    int indc4 = BoneHelper.GetBoneIndex(v, "indicator_rr");

                    int indc5 = BoneHelper.GetBoneIndex(v, "headlight_l");
                    int indc6 = BoneHelper.GetBoneIndex(v, "headlight_r");
                    int indc7 = BoneHelper.GetBoneIndex(v, "taillight_l");
                    int indc8 = BoneHelper.GetBoneIndex(v, "taillight_r");
                    if (indc1 != -1)
                    {
                        //World.DrawLightWithRange(BoneHelper.GetBonePositionWorld(v, indc1), System.Drawing.Color.Orange, 1f, 1f);
                        Vector3 vec = BoneHelper.GetBonePositionWorld(v, indc1);
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LIGHT_WITH_RANGEEX, vec.X, vec.Y, vec.Z, System.Drawing.Color.Orange.R, System.Drawing.Color.Orange.G, System.Drawing.Color.Orange.B, 1f, 1f, 5f);
                    }
                    else if(indc5 != -1)
                    {
                        //World.DrawLightWithRange(BoneHelper.GetBonePositionWorld(v, indc5), System.Drawing.Color.Orange, 1f, 1f);
                        Vector3 vec = BoneHelper.GetBonePositionWorld(v, indc5);
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LIGHT_WITH_RANGEEX, vec.X, vec.Y, vec.Z, System.Drawing.Color.Orange.R, System.Drawing.Color.Orange.G, System.Drawing.Color.Orange.B, 1f, 1f, 5f);
                    }

                    if(indc2 != -1)
                    {
                        //World.DrawLightWithRange(BoneHelper.GetBonePositionWorld(v, indc2), System.Drawing.Color.Orange, 1f, 1f);
                        Vector3 vec = BoneHelper.GetBonePositionWorld(v, indc2);
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LIGHT_WITH_RANGEEX, vec.X, vec.Y, vec.Z, System.Drawing.Color.Orange.R, System.Drawing.Color.Orange.G, System.Drawing.Color.Orange.B, 1f, 1f, 5f);
                    }
                    else if(indc6 != -1)
                    {
                        //World.DrawLightWithRange(BoneHelper.GetBonePositionWorld(v, indc6), System.Drawing.Color.Orange, 1f, 1f);
                        Vector3 vec = BoneHelper.GetBonePositionWorld(v, indc6);
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LIGHT_WITH_RANGEEX, vec.X, vec.Y, vec.Z, System.Drawing.Color.Orange.R, System.Drawing.Color.Orange.G, System.Drawing.Color.Orange.B, 1f, 1f, 5f);
                    }

                    if (indc3 != -1)
                    {
                        //World.DrawLightWithRange(BoneHelper.GetBonePositionWorld(v, indc3), System.Drawing.Color.Orange, 1f, 1f);
                        Vector3 vec = BoneHelper.GetBonePositionWorld(v, indc3);
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LIGHT_WITH_RANGEEX, vec.X, vec.Y, vec.Z, System.Drawing.Color.Orange.R, System.Drawing.Color.Orange.G, System.Drawing.Color.Orange.B, 1f, 1f, 5f);
                    }
                    else if (indc7 != -1)
                    {
                        //World.DrawLightWithRange(BoneHelper.GetBonePositionWorld(v, indc7), System.Drawing.Color.Orange, 1f, 1f);
                        Vector3 vec = BoneHelper.GetBonePositionWorld(v, indc7);
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LIGHT_WITH_RANGEEX, vec.X, vec.Y, vec.Z, System.Drawing.Color.Orange.R, System.Drawing.Color.Orange.G, System.Drawing.Color.Orange.B, 1f, 1f, 5f);
                    }

                    if (indc4 != -1)
                    {
                        //World.DrawLightWithRange(BoneHelper.GetBonePositionWorld(v, indc4), System.Drawing.Color.Orange, 1f, 1f);
                        Vector3 vec = BoneHelper.GetBonePositionWorld(v, indc4);
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LIGHT_WITH_RANGEEX, vec.X, vec.Y, vec.Z, System.Drawing.Color.Orange.R, System.Drawing.Color.Orange.G, System.Drawing.Color.Orange.B, 1f, 1f, 5f);
                    }
                    else if (indc8 != -1)
                    {
                        //World.DrawLightWithRange(BoneHelper.GetBonePositionWorld(v, indc8), System.Drawing.Color.Orange, 1f, 1f);
                        Vector3 vec = BoneHelper.GetBonePositionWorld(v, indc8);
                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LIGHT_WITH_RANGEEX, vec.X, vec.Y, vec.Z, System.Drawing.Color.Orange.R, System.Drawing.Color.Orange.G, System.Drawing.Color.Orange.B, 1f, 1f, 5f);
                    }
                }
            }

            if(ModSettings.ReturnVehiclesToSafeLocation)
            {
                if (Game.Player.Character.IsDead && CanDie)
                {
                    CanDie = false;
                    HasDied = true;
                    PromptTime = Game.GameTime;
                    ResetCars = true;
                    DieArrestedAt = Game.Player.Character.Position;
                }

                if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PLAYER_BEING_ARRESTED, Game.Player, false) && CanArrest)
                {
                    CanArrest = false;
                    GotArrested = true;
                    PromptTime = Game.GameTime;
                    ResetCars = true;
                    DieArrestedAt = Game.Player.Character.Position;
                }

                if (Game.Player.Character.IsAlive)
                {
                    CanDie = true;
                    if (HasDied)
                    {
                        if ((Game.GameTime - PromptTime > 5000) && ResetCars)
                        {
                            Vehicle[] vehs = World.GetNearbyVehicles(DieArrestedAt, 100f);
                            foreach (Vehicle v in vehs)
                            {
                                if (AttachedVehicles.ContainsKey(v))
                                {
                                    if(v.IsAlive && !v.IsDead)
                                    {
                                        if (AttachedVehicles[v].SafeSpawnSet)
                                        {
                                            v.Position = AttachedVehicles[v].SafeSpawn;
                                            v.Rotation = AttachedVehicles[v].SafeRotation;
                                        }
                                    }
                                    else
                                    {
                                        VehicleDataV1 dat = AttachedVehicles[v];
                                        AttachedVehicles.Remove(v);
                                        v.Delete();
                                        CreateVehicle(dat, null, false, true, true);
                                    }
                                }
                            }
                            ResetCars = false;
                        }
                        if (Game.GameTime - PromptTime > 5000)
                            GTA.UI.Screen.ShowHelpTextThisFrame("Your vehicles have been returned to their set spawn.", false);
                        if (Game.GameTime - PromptTime > 10000)
                        {
                            HasDied = false;
                        }
                    }
                }

                if (!GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PLAYER_BEING_ARRESTED, Game.Player, false))
                {
                    CanArrest = true;
                    if (GotArrested)
                    {
                        if ((Game.GameTime - PromptTime > 5000) && ResetCars)
                        {
                            Vehicle[] vehs = World.GetNearbyVehicles(DieArrestedAt, 100f);
                            foreach (Vehicle v in vehs)
                            {
                                if (AttachedVehicles.ContainsKey(v))
                                {
                                    if (v.IsAlive && !v.IsDead)
                                    {
                                        if (AttachedVehicles[v].SafeSpawnSet)
                                        {
                                            v.Position = AttachedVehicles[v].SafeSpawn;
                                            v.Rotation = AttachedVehicles[v].SafeRotation;
                                        }
                                    }
                                    else
                                    {
                                        VehicleDataV1 dat = AttachedVehicles[v];
                                        AttachedVehicles.Remove(v);
                                        v.Delete();
                                        CreateVehicle(dat, null, false, true, true);
                                    }
                                }
                            }
                            ResetCars = false;
                        }
                        if (Game.GameTime - PromptTime > 5000)
                            GTA.UI.Screen.ShowHelpTextThisFrame("Your vehicles have been returned to their set spawn.", false);
                        if (Game.GameTime - PromptTime > 10000)
                        {
                            GotArrested = false;
                        }
                    }
                }
            }

            if(ModSettings.EnableRemoteSystem)
            {
                if (DoingFobAnim && (Game.GameTime - WaitTime > 1750))
                {
                    if (FobObj != null)
                    {
                        FobObj.IsPersistent = true;
                        FobObj.Delete();
                        FobObj = null;
                    }
                    if(StoredWeapon != WeaponHash.Unarmed)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_CURRENT_PED_WEAPON, Game.Player.Character.Handle, StoredWeapon, true);
                    }
                    DoingFobAnim = false;
                }
            }

            bool UsingKeyboard = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_USING_KEYBOARD_AND_MOUSE, 0);

            if(ModSettings.EnableRemoteSystem)
            {
                if(!Game.IsCutsceneActive)
                {
                    if (!DoingFobAnim)
                    {
                        if (Game.Player.Character.CurrentVehicle == null)
                        {
                            if (PressToSaveVeh != null)
                            {
                                if (!UsingKeyboard)
                                {
                                    if (Game.IsControlPressed(GTA.Control.ScriptPadRight))
                                    {
                                        if (!GUI.Phone.IsOn)
                                        {
                                            if (!DoXThing)
                                            {
                                                DoXThing = true;
                                                XTime = Game.GameTime;
                                            }
                                        }

                                    }
                                    if (Game.IsControlJustReleased(GTA.Control.ScriptPadRight))
                                    {
                                        if (!GUI.Phone.IsOn)
                                        {
                                            DoXThing = false;
                                            if (PressToSaveVeh != null)
                                            {
                                                if (PressToSaveVeh.Handle != null)
                                                {
                                                    if (PressToSaveVeh.Handle.LockStatus == VehicleLockStatus.CannotEnter)
                                                        PressToSaveVeh.Handle.LockStatus = VehicleLockStatus.Unlocked;
                                                    else
                                                    {
                                                        PressToSaveVeh.Handle.LockStatus = VehicleLockStatus.CannotEnter;
                                                        PressToSaveVeh.SafeRotation = PressToSaveVeh.Handle.Rotation;
                                                        PressToSaveVeh.SafeSpawn = PressToSaveVeh.Handle.Position;
                                                        PressToSaveVeh.SafeSpawnSet = true;
                                                    }
                                                    GTA.Native.Function.Call(GTA.Native.Hash.PLAY_SOUND_FROM_ENTITY, -1, "CONFIRM_BEEP", PressToSaveVeh.Handle.Handle, "HUD_MINI_GAME_SOUNDSET", 1, 0);
                                                    if (!DoingFobAnim)
                                                    {
                                                        int vType = GTA.Native.Function.Call<int>((GTA.Native.Hash)(0x19CAFA3C87F7C2FF));
                                                        int vMode = GTA.Native.Function.Call<int>((GTA.Native.Hash)(0xEE778F8C7E1142E2), vType);
                                                        if (vMode == 4)
                                                            Game.Player.Character.Task.PlayAnimation("ANIM@MP_PLAYER_INTMENU@KEY_FOB@", "FOB_CLICK_FP", 8f, -2f, -1, AnimationFlags.UpperBodyOnly | AnimationFlags.Secondary, 0f);
                                                        else
                                                            Game.Player.Character.Task.PlayAnimation("ANIM@MP_PLAYER_INTMENU@KEY_FOB@", "FOB_CLICK", 8f, -2f, -1, AnimationFlags.UpperBodyOnly | AnimationFlags.Secondary, 0f);

                                                        Model m = new Model("lr_prop_carkey_fob");
                                                        m.Request(5000);
                                                        FobObj = World.CreateProp(m, Game.Player.Character.Position, false, false);
                                                        int x = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_PED_BONE_INDEX, Game.Player.Character.Handle, Bone.PHRightHand);
                                                        if (FobObj != null)
                                                            GTA.Native.Function.Call(GTA.Native.Hash.ATTACH_ENTITY_TO_ENTITY, FobObj.Handle, Game.Player.Character.Handle, x, 0f, 0f, 0f, 0f, 0f, 0f, true, false, false, false, 2, true);
                                                        WaitTime = Game.GameTime;

                                                        StoredWeapon = Game.Player.Character.Weapons.Current.Hash;
                                                        GTA.Native.Function.Call(GTA.Native.Hash.SET_CURRENT_PED_WEAPON, Game.Player.Character.Handle, WeaponHash.Unarmed, true);
                                                        DoingFobAnim = true;
                                                    }
                                                    DrawCarBlinkers[PressToSaveVeh.Handle] = Game.GameTime;
                                                }
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (Game.IsControlPressed(GTA.Control.Context))
                                    {
                                        if (!GUI.Phone.IsOn)
                                        {
                                            if (!DoXThing)
                                            {
                                                DoXThing = true;
                                                XTime = Game.GameTime;
                                            }
                                        }

                                    }
                                    if (Game.IsControlJustReleased(GTA.Control.Context))
                                    {
                                        if (!GUI.Phone.IsOn)
                                        {
                                            DoXThing = false;
                                            if (PressToSaveVeh != null)
                                            {
                                                if (PressToSaveVeh.Handle != null)
                                                {
                                                    if (PressToSaveVeh.Handle.LockStatus == VehicleLockStatus.CannotEnter)
                                                        PressToSaveVeh.Handle.LockStatus = VehicleLockStatus.Unlocked;
                                                    else
                                                    {
                                                        PressToSaveVeh.Handle.LockStatus = VehicleLockStatus.CannotEnter;
                                                        PressToSaveVeh.SafeRotation = PressToSaveVeh.Handle.Rotation;
                                                        PressToSaveVeh.SafeSpawn = PressToSaveVeh.Handle.Position;
                                                        PressToSaveVeh.SafeSpawnSet = true;
                                                    }
                                                    GTA.Native.Function.Call(GTA.Native.Hash.PLAY_SOUND_FROM_ENTITY, -1, "CONFIRM_BEEP", PressToSaveVeh.Handle.Handle, "HUD_MINI_GAME_SOUNDSET", 1, 0);
                                                    if (!DoingFobAnim)
                                                    {
                                                        int vType = GTA.Native.Function.Call<int>((GTA.Native.Hash)(0x19CAFA3C87F7C2FF));
                                                        int vMode = GTA.Native.Function.Call<int>((GTA.Native.Hash)(0xEE778F8C7E1142E2), vType);
                                                        if (vMode == 4)
                                                            Game.Player.Character.Task.PlayAnimation("ANIM@MP_PLAYER_INTMENU@KEY_FOB@", "FOB_CLICK_FP", 8f, -2f, -1, AnimationFlags.UpperBodyOnly | AnimationFlags.Secondary, 0f);
                                                        else
                                                            Game.Player.Character.Task.PlayAnimation("ANIM@MP_PLAYER_INTMENU@KEY_FOB@", "FOB_CLICK", 8f, -2f, -1, AnimationFlags.UpperBodyOnly | AnimationFlags.Secondary, 0f);

                                                        Model m = new Model("lr_prop_carkey_fob");
                                                        m.Request(5000);
                                                        FobObj = World.CreateProp(m, Game.Player.Character.Position, false, false);
                                                        int x = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_PED_BONE_INDEX, Game.Player.Character.Handle, Bone.PHRightHand);
                                                        if (FobObj != null)
                                                            GTA.Native.Function.Call(GTA.Native.Hash.ATTACH_ENTITY_TO_ENTITY, FobObj.Handle, Game.Player.Character.Handle, x, 0f, 0f, 0f, 0f, 0f, 0f, true, false, false, false, 2, true);
                                                        WaitTime = Game.GameTime;
                                                        StoredWeapon = Game.Player.Character.Weapons.Current.Hash;
                                                        GTA.Native.Function.Call(GTA.Native.Hash.SET_CURRENT_PED_WEAPON, Game.Player.Character.Handle, WeaponHash.Unarmed, true);
                                                        DoingFobAnim = true;
                                                    }
                                                    DrawCarBlinkers[PressToSaveVeh.Handle] = Game.GameTime;
                                                }
                                            }
                                        }
                                    }
                                }

                                if (DoXThing && (Game.GameTime - XTime > 1250))
                                {
                                    if (PressToSaveVeh != null)
                                    {
                                        if (PressToSaveVeh.Handle != null)
                                        {
                                            if (PressToSaveVeh.Handle.IsEngineRunning)
                                            {
                                                GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_ENGINE_ON, PressToSaveVeh.Handle.Handle, false, true, false);
                                            }
                                            else
                                            {
                                                GTA.Native.Function.Call(GTA.Native.Hash.SET_VEHICLE_ENGINE_ON, PressToSaveVeh.Handle.Handle, true, true, false);
                                            }
                                            GTA.Native.Function.Call(GTA.Native.Hash.PLAY_SOUND_FROM_ENTITY, -1, "CONFIRM_BEEP", PressToSaveVeh.Handle.Handle, "HUD_MINI_GAME_SOUNDSET", 1, 0);
                                            if (!DoingFobAnim)
                                            {
                                                int vType = GTA.Native.Function.Call<int>((GTA.Native.Hash)(0x19CAFA3C87F7C2FF));
                                                int vMode = GTA.Native.Function.Call<int>((GTA.Native.Hash)(0xEE778F8C7E1142E2), vType);
                                                if (vMode == 4)
                                                    Game.Player.Character.Task.PlayAnimation("ANIM@MP_PLAYER_INTMENU@KEY_FOB@", "FOB_CLICK_FP", 8f, -2f, -1, AnimationFlags.UpperBodyOnly | AnimationFlags.Secondary, 0f);
                                                else
                                                    Game.Player.Character.Task.PlayAnimation("ANIM@MP_PLAYER_INTMENU@KEY_FOB@", "FOB_CLICK", 8f, -2f, -1, AnimationFlags.UpperBodyOnly | AnimationFlags.Secondary, 0f);

                                                Model m = new Model("lr_prop_carkey_fob");
                                                m.Request(5000);
                                                FobObj = World.CreateProp(m, Game.Player.Character.Position, false, false);
                                                int x = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_PED_BONE_INDEX, Game.Player.Character.Handle, Bone.PHRightHand);
                                                if (FobObj != null)
                                                    GTA.Native.Function.Call(GTA.Native.Hash.ATTACH_ENTITY_TO_ENTITY, FobObj.Handle, Game.Player.Character.Handle, x, 0f, 0f, 0f, 0f, 0f, 0f, true, false, false, false, 2, true);
                                                WaitTime = Game.GameTime;
                                                StoredWeapon = Game.Player.Character.Weapons.Current.Hash;
                                                GTA.Native.Function.Call(GTA.Native.Hash.SET_CURRENT_PED_WEAPON, Game.Player.Character.Handle, WeaponHash.Unarmed, true);
                                                DoingFobAnim = true;
                                            }
                                            DrawCarBlinkers[PressToSaveVeh.Handle] = Game.GameTime;
                                        }
                                    }
                                    DoXThing = false;
                                }
                            }
                        }
                    }
                }
            }

            if(!Game.IsCutsceneActive)
            {
                if (!GUI.Phone.IsOn)
                {
                    if (Game.GameTime - RaycastTick > 450)
                    {
                        PressToSaveVeh = null;
                        if (Game.Player.Character.CurrentVehicle == null)
                        {
                            ep_priv = GameplayCamera.Position;
                            ep2_priv = MathHelper.ScreenRelToWorld(GameplayCamera.Position, GameplayCamera.Rotation, new Vector2(0.5f, 0.5f));
                            ep3_priv = ep2_priv - ep_priv;
                            from_priv = ep_priv + ep3_priv * 0.05f;
                            to_priv = ep_priv + ep3_priv * 4f;
                            //GTA.UI.Screen.ShowSubtitle("From: " + from + " To: " + to);
                            up_priv = GameplayCamera.UpVector.Normalized;
                            right_priv = GameplayCamera.RightVector.Normalized;
                            Ped p = Game.Player.Character;
                            RaycastResult ray = World.Raycast(from_priv, to_priv, IntersectFlags.Map | IntersectFlags.Vehicles, p);
                            RaycastResult ray2 = World.Raycast(from_priv + up_priv * 0.5f, to_priv + up_priv * 0.2f, IntersectFlags.Map | IntersectFlags.Vehicles, p);
                            RaycastResult ray3 = World.Raycast(from_priv + up_priv * -0.5f, to_priv + up_priv * -0.2f, IntersectFlags.Map | IntersectFlags.Vehicles, p);
                            RaycastResult ray4 = World.Raycast(from_priv + right_priv * 0.5f, to_priv + right_priv * 0.2f, IntersectFlags.Map | IntersectFlags.Vehicles, p);
                            RaycastResult ray5 = World.Raycast(from_priv + right_priv * -0.5f, to_priv + right_priv * -0.2f, IntersectFlags.Map | IntersectFlags.Vehicles, p);
                            /*World.DrawLine(from, to, System.Drawing.Color.Red);
                            World.DrawLine(from + GameplayCamera.UpVector.Normalized * 0.5f, to + GameplayCamera.UpVector.Normalized * 0.2f, System.Drawing.Color.Red);
                            World.DrawLine(from + GameplayCamera.UpVector.Normalized * -0.5f, to + GameplayCamera.UpVector.Normalized * -0.2f, System.Drawing.Color.Red);
                            World.DrawLine(from + GameplayCamera.RightVector.Normalized * 0.5f, to + GameplayCamera.RightVector.Normalized * 0.2f, System.Drawing.Color.Red);
                            World.DrawLine(from + GameplayCamera.RightVector.Normalized * -0.5f, to + GameplayCamera.RightVector.Normalized * -0.2f, System.Drawing.Color.Red);*/
                            Vehicle closest = null;
                            if (ray.HitEntity != null)
                            {
                                foreach (VehicleDataV1 vdata in VehicleDatabase)
                                {
                                    if (vdata.Handle == ray.HitEntity)
                                    {
                                        if (closest == null)
                                        {
                                            if (Game.Player.Character.Position.DistanceTo(vdata.Handle.Position) < 50f)
                                            {
                                                closest = (Vehicle)ray.HitEntity;
                                            }
                                        }
                                        else
                                        {
                                            if (Game.Player.Character.Position.DistanceTo(vdata.Handle.Position) < Game.Player.Character.Position.DistanceTo(closest.Position))
                                            {
                                                closest = (Vehicle)ray.HitEntity;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            if (ray2.HitEntity != null)
                            {
                                foreach (VehicleDataV1 vdata in VehicleDatabase)
                                {
                                    if (vdata.Handle == ray2.HitEntity)
                                    {
                                        if (closest == null)
                                        {
                                            if (Game.Player.Character.Position.DistanceTo(vdata.Handle.Position) < 50f)
                                            {
                                                closest = (Vehicle)ray2.HitEntity;
                                            }
                                        }
                                        else
                                        {
                                            if (Game.Player.Character.Position.DistanceTo(vdata.Handle.Position) < Game.Player.Character.Position.DistanceTo(closest.Position))
                                            {
                                                closest = (Vehicle)ray2.HitEntity;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            if (ray3.HitEntity != null)
                            {
                                foreach (VehicleDataV1 vdata in VehicleDatabase)
                                {
                                    if (vdata.Handle == ray3.HitEntity)
                                    {
                                        if (closest == null)
                                        {
                                            if (Game.Player.Character.Position.DistanceTo(vdata.Handle.Position) < 50f)
                                            {
                                                closest = (Vehicle)ray3.HitEntity;
                                            }
                                        }
                                        else
                                        {
                                            if (Game.Player.Character.Position.DistanceTo(vdata.Handle.Position) < Game.Player.Character.Position.DistanceTo(closest.Position))
                                            {
                                                closest = (Vehicle)ray3.HitEntity;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            if (ray4.HitEntity != null)
                            {
                                foreach (VehicleDataV1 vdata in VehicleDatabase)
                                {
                                    if (vdata.Handle == ray4.HitEntity)
                                    {
                                        if (closest == null)
                                        {
                                            if (Game.Player.Character.Position.DistanceTo(vdata.Handle.Position) < 50f)
                                            {
                                                closest = (Vehicle)ray4.HitEntity;
                                            }
                                        }
                                        else
                                        {
                                            if (Game.Player.Character.Position.DistanceTo(vdata.Handle.Position) < Game.Player.Character.Position.DistanceTo(closest.Position))
                                            {
                                                closest = (Vehicle)ray4.HitEntity;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }
                            if (ray5.HitEntity != null)
                            {
                                foreach (VehicleDataV1 vdata in VehicleDatabase)
                                {
                                    if (vdata.Handle == ray5.HitEntity)
                                    {
                                        if (closest == null)
                                        {
                                            if (Game.Player.Character.Position.DistanceTo(vdata.Handle.Position) < 50f)
                                            {
                                                closest = (Vehicle)ray5.HitEntity;
                                            }
                                        }
                                        else
                                        {
                                            if (Game.Player.Character.Position.DistanceTo(vdata.Handle.Position) < Game.Player.Character.Position.DistanceTo(closest.Position))
                                            {
                                                closest = (Vehicle)ray5.HitEntity;
                                            }
                                        }
                                        break;
                                    }
                                }
                            }

                            if (closest != null)
                            {
                                PressToSaveVeh = AttachedVehicles[closest];
                                string insiderInfo = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_DISPLAY_NAME_FROM_VEHICLE_MODEL, PressToSaveVeh.Hash);
                                SaveCarString = GTA.Native.Function.Call<string>(GTA.Native.Hash.GET_FILENAME_FOR_AUDIO_CONVERSATION , insiderInfo);
                                if (SaveCarString == "NULL")
                                    SaveCarString = PressToSaveVeh.Handle.Mods.LicensePlate;
                            }
                            else
                            {
                                SteeringAngleRemote = 0f;
                            }

                            if (PressToSaveVeh == null)
                            {
                                if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_GAMEPLAY_HINT_ACTIVE))
                                    GTA.Native.Function.Call(GTA.Native.Hash.STOP_GAMEPLAY_HINT, false);
                            }
                            RaycastTick = Game.GameTime;
                        }
                        else
                        {
                            PressToSaveVeh = null;
                            if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_GAMEPLAY_HINT_ACTIVE))
                                GTA.Native.Function.Call(GTA.Native.Hash.STOP_GAMEPLAY_HINT, false);
                        }
                    }

                    if (DoGetOutHint && (Game.GameTime - GetOutHint > 1500))
                    {
                        if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_GAMEPLAY_HINT_ACTIVE))
                            GTA.Native.Function.Call(GTA.Native.Hash.STOP_GAMEPLAY_HINT, false);
                        DoGetOutHint = false;
                    }

                    if (ModSettings.EnableRemoteSystem)
                    {
                        if (PressToSaveVeh != null)
                        {
                            if (PressToSaveVeh.Handle != null)
                            {
                                Game.DisableControlThisFrame(GTA.Control.Context);
                                Game.DisableControlThisFrame(GTA.Control.ContextSecondary);
                                Game.DisableControlThisFrame(GTA.Control.Phone);
                                Game.DisableControlThisFrame(GTA.Control.PhoneDown);
                                Game.DisableControlThisFrame(GTA.Control.FrontendDown);
                                Game.DisableControlThisFrame(GTA.Control.FrontendUp);
                                Game.DisableControlThisFrame(GTA.Control.FrontendLeft);
                                Game.DisableControlThisFrame(GTA.Control.FrontendRight);
                                Game.DisableControlThisFrame(GTA.Control.CharacterWheel);
                                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.ReplayStartStopRecording, true);
                                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.ReplayStartStopRecordingSecondary, true);
                                if (ModSettings.EnableRemoteMovement)
                                {
                                    if (!UsingKeyboard)
                                        GTA.UI.Screen.ShowHelpTextThisFrame($"~b~[{SaveCarString}]~s~~n~Press ~INPUT_SCRIPT_PAD_RIGHT~ to " + (PressToSaveVeh.Handle.LockStatus == VehicleLockStatus.CannotEnter ? "~g~unlock~s~" : "~r~lock~s~") + "~n~Hold  ~INPUT_SCRIPT_PAD_RIGHT~ to remote " + (PressToSaveVeh.Handle.IsEngineRunning ? "~r~stop~s~" : "~g~start~s~") + "~n~Hold ~INPUTGROUP_FRONTEND_DPAD_UD~ ~INPUT_SPRINT~ ~INPUT_JUMP~ to move", false);
                                    else
                                        GTA.UI.Screen.ShowHelpTextThisFrame($"~b~[{SaveCarString}]~s~~n~Press ~INPUT_CONTEXT~ to " + (PressToSaveVeh.Handle.LockStatus == VehicleLockStatus.CannotEnter ? "~g~unlock~s~" : "~r~lock~s~") + "~n~Hold  ~INPUT_CONTEXT~ to remote " + (PressToSaveVeh.Handle.IsEngineRunning ? "~r~stop~s~" : "~g~start~s~") + "~n~Hold ~INPUTGROUP_FRONTEND_DPAD_ALL~ to move", false);

                                }
                                else
                                {
                                    if (!UsingKeyboard)
                                        GTA.UI.Screen.ShowHelpTextThisFrame($"~b~[{SaveCarString}]~s~~n~Press ~INPUT_SCRIPT_PAD_RIGHT~ to " + (PressToSaveVeh.Handle.LockStatus == VehicleLockStatus.CannotEnter ? "~g~unlock~s~" : "~r~lock~s~") + "~n~Hold  ~INPUT_SCRIPT_PAD_RIGHT~ to remote " + (PressToSaveVeh.Handle.IsEngineRunning ? "~r~stop~s~" : "~g~start~s~"), false);
                                    else
                                        GTA.UI.Screen.ShowHelpTextThisFrame($"~b~[{SaveCarString}]~s~~n~Press ~INPUT_CONTEXT~ to " + (PressToSaveVeh.Handle.LockStatus == VehicleLockStatus.CannotEnter ? "~g~unlock~s~" : "~r~lock~s~") + "~n~Hold  ~INPUT_CONTEXT~ to remote " + (PressToSaveVeh.Handle.IsEngineRunning ? "~r~stop~s~" : "~g~start~s~"), false);

                                }
                                if (ModSettings.EnableRemoteMovement)
                                {
                                    if (PressToSaveVeh.Handle.IsEngineRunning)
                                    {
                                        if (Game.IsControlPressed(GTA.Control.FrontendDown))
                                        {
                                            if (!UsingKeyboard)
                                            {
                                                if (SteeringAngleRemote < 20f)
                                                {
                                                    SteeringAngleRemote += 1f;
                                                }
                                                PressToSaveVeh.Handle.SteeringAngle = SteeringAngleRemote;
                                                if (PressToSaveVeh.Handle != LastHinted)
                                                    GTA.Native.Function.Call(GTA.Native.Hash.STOP_GAMEPLAY_HINT, false);
                                                GTA.Native.Function.Call(GTA.Native.Hash.SET_GAMEPLAY_VEHICLE_HINT, PressToSaveVeh.Handle, 0f, 0f, 0.5f, true, -1, 1000, 1000);
                                                GetOutHint = Game.GameTime;
                                                DoGetOutHint = true;
                                            }
                                            else
                                            {
                                                PressToSaveVeh.Handle.ForwardSpeed = -1f;
                                                if (PressToSaveVeh.Handle != LastHinted)
                                                    GTA.Native.Function.Call(GTA.Native.Hash.STOP_GAMEPLAY_HINT, false);
                                                GTA.Native.Function.Call(GTA.Native.Hash.SET_GAMEPLAY_VEHICLE_HINT, PressToSaveVeh.Handle, 0f, 0f, 0.5f, true, -1, 1000, 1000);
                                                GetOutHint = Game.GameTime;
                                                DoGetOutHint = true;
                                            }
                                        }
                                        if (Game.IsControlPressed(GTA.Control.FrontendUp))
                                        {
                                            if (!UsingKeyboard)
                                            {
                                                if (SteeringAngleRemote > -20f)
                                                {
                                                    SteeringAngleRemote -= 1f;
                                                }
                                                PressToSaveVeh.Handle.SteeringAngle = SteeringAngleRemote;
                                                if (PressToSaveVeh.Handle != LastHinted)
                                                    GTA.Native.Function.Call(GTA.Native.Hash.STOP_GAMEPLAY_HINT, false);
                                                GTA.Native.Function.Call(GTA.Native.Hash.SET_GAMEPLAY_VEHICLE_HINT, PressToSaveVeh.Handle, 0f, 0f, 0.5f, true, -1, 1000, 1000);
                                                GetOutHint = Game.GameTime;
                                                DoGetOutHint = true;
                                            }
                                            else
                                            {
                                                PressToSaveVeh.Handle.ForwardSpeed = 1f;
                                                if (PressToSaveVeh.Handle != LastHinted)
                                                    GTA.Native.Function.Call(GTA.Native.Hash.STOP_GAMEPLAY_HINT, false);
                                                GTA.Native.Function.Call(GTA.Native.Hash.SET_GAMEPLAY_VEHICLE_HINT, PressToSaveVeh.Handle, 0f, 0f, 0.5f, true, -1, 1000, 1000);
                                                GetOutHint = Game.GameTime;
                                                DoGetOutHint = true;
                                            }
                                        }
                                        if (Game.IsControlPressed(GTA.Control.FrontendLeft))
                                        {
                                            if (!UsingKeyboard)
                                            {

                                            }
                                            else
                                            {
                                                if (SteeringAngleRemote < 20f)
                                                {
                                                    SteeringAngleRemote += 1f;
                                                }
                                                PressToSaveVeh.Handle.SteeringAngle = SteeringAngleRemote;
                                                if (PressToSaveVeh.Handle != LastHinted)
                                                    GTA.Native.Function.Call(GTA.Native.Hash.STOP_GAMEPLAY_HINT, false);
                                                GTA.Native.Function.Call(GTA.Native.Hash.SET_GAMEPLAY_VEHICLE_HINT, PressToSaveVeh.Handle, 0f, 0f, 0.5f, true, -1, 1000, 1000);
                                                GetOutHint = Game.GameTime;
                                                DoGetOutHint = true;
                                            }

                                        }
                                        if (Game.IsControlPressed(GTA.Control.FrontendRight))
                                        {
                                            if (!UsingKeyboard)
                                            {

                                            }
                                            else
                                            {
                                                if (SteeringAngleRemote > -20f)
                                                {
                                                    SteeringAngleRemote -= 1f;
                                                }
                                                PressToSaveVeh.Handle.SteeringAngle = SteeringAngleRemote;
                                                if (PressToSaveVeh.Handle != LastHinted)
                                                    GTA.Native.Function.Call(GTA.Native.Hash.STOP_GAMEPLAY_HINT, false);
                                                GTA.Native.Function.Call(GTA.Native.Hash.SET_GAMEPLAY_VEHICLE_HINT, PressToSaveVeh.Handle, 0f, 0f, 0.5f, true, -1, 1000, 1000);
                                                GetOutHint = Game.GameTime;
                                                DoGetOutHint = true;
                                            }
                                        }
                                        if (Game.IsControlPressed(GTA.Control.Sprint))
                                        {
                                            if (!UsingKeyboard)
                                            {
                                                PressToSaveVeh.Handle.ForwardSpeed = 1f;
                                                if (PressToSaveVeh.Handle != LastHinted)
                                                    GTA.Native.Function.Call(GTA.Native.Hash.STOP_GAMEPLAY_HINT, false);
                                                GTA.Native.Function.Call(GTA.Native.Hash.SET_GAMEPLAY_VEHICLE_HINT, PressToSaveVeh.Handle, 0f, 0f, 0.5f, true, -1, 1000, 1000);
                                                GetOutHint = Game.GameTime;
                                                DoGetOutHint = true;
                                            }
                                        }
                                        if (Game.IsControlPressed(GTA.Control.Jump))
                                        {
                                            if (!UsingKeyboard)
                                            {
                                                PressToSaveVeh.Handle.ForwardSpeed = -1f;
                                                if (PressToSaveVeh.Handle != LastHinted)
                                                    GTA.Native.Function.Call(GTA.Native.Hash.STOP_GAMEPLAY_HINT, false);
                                                GTA.Native.Function.Call(GTA.Native.Hash.SET_GAMEPLAY_VEHICLE_HINT, PressToSaveVeh.Handle, 0f, 0f, 0.5f, true, -1, 1000, 1000);
                                                GetOutHint = Game.GameTime;
                                                DoGetOutHint = true;
                                            }
                                        }
                                    }

                                    if (PressToSaveVeh.Handle != LastHinted)
                                        GTA.Native.Function.Call(GTA.Native.Hash.STOP_GAMEPLAY_HINT, false);
                                    LastHinted = PressToSaveVeh.Handle;
                                }
                            }
                        }
                    }
                }
            }

            if (DrawTrace && ModSettings.ShowVehicleOutlines)
            {
                if(GUI.Phone.activatedCar != null)
                {
                    if(GUI.Phone.activatedCar.Handle != null)
                    {
                        if(GUI.Phone.activatedCar.Handle != Game.Player.Character.CurrentVehicle)
                        {
                            StartTrace = Game.Player.Character.Position;
                            GTA.Native.OutputArgument arg1 = new GTA.Native.OutputArgument();
                            GTA.Native.OutputArgument arg2 = new GTA.Native.OutputArgument();
                            (Vector3 bottomLeft, Vector3 topRight) = GUI.Phone.activatedCar.Handle.Model.Dimensions;
                            Vector3 size = topRight - bottomLeft;
                            EndTrace = GUI.Phone.activatedCar.Handle.Position;
                            Vector3 pos = Game.Player.Character.Position;
                            int objMy = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CLOSEST_OBJECT_OF_TYPE, pos.X, pos.Y, pos.Z, 3f, Game.GenerateHash("prop_phone_ing"), false, false, false);
                            if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, objMy))
                            {
                                StartTrace = GTA.Native.Function.Call<Vector3>(GTA.Native.Hash.GET_ENTITY_COORDS, objMy, false);
                                GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, StartTrace.X, StartTrace.Y, StartTrace.Z, EndTrace.X, EndTrace.Y, EndTrace.Z, 255, 255, 255, 255);
                            }
                            else
                            {
                                objMy = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CLOSEST_OBJECT_OF_TYPE, pos.X, pos.Y, pos.Z, 3f, Game.GenerateHash("prop_phone_ing_02"), false, false, false);
                                if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, objMy))
                                {
                                    StartTrace = GTA.Native.Function.Call<Vector3>(GTA.Native.Hash.GET_ENTITY_COORDS, objMy, false);
                                    GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, StartTrace.X, StartTrace.Y, StartTrace.Z, EndTrace.X, EndTrace.Y, EndTrace.Z, 255, 255, 255, 255);
                                }
                                else
                                {
                                    objMy = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_CLOSEST_OBJECT_OF_TYPE, pos.X, pos.Y, pos.Z, 3f, Game.GenerateHash("prop_phone_ing_03"), false, false, false);
                                    if (GTA.Native.Function.Call<bool>(GTA.Native.Hash.DOES_ENTITY_EXIST, objMy))
                                    {
                                        StartTrace = GTA.Native.Function.Call<Vector3>(GTA.Native.Hash.GET_ENTITY_COORDS, objMy, false);
                                        GTA.Native.Function.Call(GTA.Native.Hash.DRAW_LINE, StartTrace.X, StartTrace.Y, StartTrace.Z, EndTrace.X, EndTrace.Y, EndTrace.Z, 255, 255, 255, 255);
                                    }
                                }
                            }
                            GraphicsManager.DrawSkeleton(EndTrace, size, GUI.Phone.activatedCar.Handle.Rotation);
                        }
                    }
                }
            }

            if(IsModLoading)
                GTA.UI.Screen.ShowSubtitle($"Loading Advanced Persistence... [{(Game.GameTime - StartedTime)}ms]", 10000);

            if(!Game.IsMissionActive)
                GTA.Native.Function.Call(GTA.Native.Hash.SET_THIS_SCRIPT_CAN_REMOVE_BLIPS_CREATED_BY_ANY_SCRIPT, true);

            if (Game.GameTime - AudioTick > 1000)
            {
                if(SoundIdBank.Count > 0)
                {
                    foreach (int x in SoundIdBank.ToList())
                    {
                        if (GTA.Audio.HasSoundFinished(x))
                        {
                            GTA.Audio.ReleaseSound(x);
                            SoundIdBank.RemoveAll(y => y == x);
                        }
                    }
                }
                AudioTick = Game.GameTime;
            }

            if(switchDetect && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PLAYER_SWITCH_IN_PROGRESS))
            {
                Vehicle veh = Game.Player.Character.CurrentVehicle;
                if (veh != null)
                {
                    if(AttachedVehicles.ContainsKey(veh))
                    {
                        if(Game.Player.Character.Model == PedHash.Franklin)
                            SwitchedCars[1] = AttachedVehicles[veh].Id;
                        else if(Game.Player.Character.Model == PedHash.Michael)
                            SwitchedCars[2] = AttachedVehicles[veh].Id;
                        else if(Game.Player.Character.Model == PedHash.Trevor)
                            SwitchedCars[3] = AttachedVehicles[veh].Id;
                    }
                }
                waitForSwitch = true;
                switchDetect = false;
            }

            if(waitForSwitch)
            {
                if(!GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_PLAYER_SWITCH_IN_PROGRESS))
                {
                    Vehicle veh = Game.Player.Character.CurrentVehicle;
                    if (veh != null)
                    {
                        string id = "null";
                        if (Game.Player.Character.Model == PedHash.Franklin)
                        {
                            if (SwitchedCars.ContainsKey(1))
                            {
                                id = SwitchedCars[1];
                                SwitchedCars.Remove(1);

                                VehicleDataV1 findData = VehicleDatabase.FirstOrDefault(x => x.Id == id);
                                if (findData != null)
                                {
                                    if (veh == findData.Handle)
                                    {
                                        CreateVehicle(findData, veh, true);
                                    }
                                    else
                                    {
                                        if (findData.Handle != null)
                                        {
                                            if (findData.Handle.Model != veh.Model)
                                            {
                                                AttachedVehicles.Remove(findData.Handle);
                                                if (findData.Handle.Exists())
                                                {
                                                    DeleteBlipsOnCar(findData.Handle);
                                                    findData.Handle.Delete();
                                                }
                                                CreateVehicle(findData, veh);
                                            }
                                            else
                                            {
                                                AttachedVehicles.Remove(findData.Handle);
                                                if (findData.Handle.Exists())
                                                {
                                                    DeleteBlipsOnCar(findData.Handle);
                                                    findData.Handle.Delete();
                                                }
                                                CreateVehicle(findData, veh, true);
                                            }
                                        }
                                        else
                                        {
                                            CreateVehicle(findData, veh);
                                        }
                                    }
                                }
                            }
                        }
                        else if (Game.Player.Character.Model == PedHash.Michael)
                        {
                            if (SwitchedCars.ContainsKey(2))
                            {
                                id = SwitchedCars[2];
                                SwitchedCars.Remove(2);

                                VehicleDataV1 findData = VehicleDatabase.FirstOrDefault(x => x.Id == id);
                                if (findData != null)
                                {
                                    if (veh == findData.Handle)
                                    {
                                        CreateVehicle(findData, veh, true);
                                    }
                                    else
                                    {
                                        if (findData.Handle != null)
                                        {
                                            if (findData.Handle.Model != veh.Model)
                                            {
                                                AttachedVehicles.Remove(findData.Handle);
                                                if (findData.Handle.Exists())
                                                {
                                                    DeleteBlipsOnCar(findData.Handle);
                                                    findData.Handle.Delete();
                                                }
                                                CreateVehicle(findData, veh);
                                            }
                                            else
                                            {
                                                AttachedVehicles.Remove(findData.Handle);
                                                if (findData.Handle.Exists())
                                                {
                                                    DeleteBlipsOnCar(findData.Handle);
                                                    findData.Handle.Delete();
                                                }
                                                CreateVehicle(findData, veh, true);
                                            }
                                        }
                                        else
                                        {
                                            CreateVehicle(findData, veh);
                                        }
                                    }
                                }
                            }
                        }
                        else if (Game.Player.Character.Model == PedHash.Trevor)
                        {
                            if (SwitchedCars.ContainsKey(3))
                            {
                                id = SwitchedCars[3];
                                SwitchedCars.Remove(3);
                                VehicleDataV1 findData = VehicleDatabase.FirstOrDefault(x => x.Id == id);
                                if (findData != null)
                                {
                                    if (veh == findData.Handle)
                                    {
                                        CreateVehicle(findData, veh, true);
                                    }
                                    else
                                    {
                                        if (findData.Handle != null)
                                        {
                                            if (findData.Handle.Model != veh.Model)
                                            {
                                                AttachedVehicles.Remove(findData.Handle);
                                                if (findData.Handle.Exists())
                                                {
                                                    DeleteBlipsOnCar(findData.Handle);
                                                    findData.Handle.Delete();
                                                }
                                                CreateVehicle(findData, veh);
                                            }
                                            else
                                            {
                                                AttachedVehicles.Remove(findData.Handle);
                                                if (findData.Handle.Exists())
                                                {
                                                    DeleteBlipsOnCar(findData.Handle);
                                                    findData.Handle.Delete();
                                                }
                                                CreateVehicle(findData, veh, true);
                                            }
                                        }
                                        else
                                        {
                                            CreateVehicle(findData, veh);
                                        }
                                    }
                                }
                            }
                        }
                    }
                    else
                    {
                        if (Game.Player.Character.Model == PedHash.Franklin)
                        {
                            if (SwitchedCars.ContainsKey(1))
                            {
                                string id = SwitchedCars[1];
                                SwitchedCars.Remove(1);
                                VehicleDataV1 findData = VehicleDatabase.FirstOrDefault(x => x.Id == id);
                                if (findData != null)
                                {
                                    if (findData.Handle == null)
                                    {
                                        CreateVehicle(findData);
                                    }
                                    else
                                    {
                                        if (!findData.Handle.Exists())
                                        {
                                            CreateVehicle(findData);
                                        }
                                    }
                                }
                            }
                        }
                        else if(Game.Player.Character.Model == PedHash.Michael)
                        {
                            if (SwitchedCars.ContainsKey(2))
                            {
                                string id = SwitchedCars[2];
                                SwitchedCars.Remove(2);
                                VehicleDataV1 findData = VehicleDatabase.FirstOrDefault(x => x.Id == id);
                                if (findData != null)
                                {
                                    if (findData.Handle == null)
                                    {
                                        CreateVehicle(findData);
                                    }
                                    else
                                    {
                                        if (!findData.Handle.Exists())
                                        {
                                            CreateVehicle(findData);
                                        }
                                    }
                                }
                            }
                        }
                        else if(Game.Player.Character.Model == PedHash.Trevor)
                        {
                            if (SwitchedCars.ContainsKey(3))
                            {
                                string id = SwitchedCars[3];
                                SwitchedCars.Remove(3);
                                VehicleDataV1 findData = VehicleDatabase.FirstOrDefault(x => x.Id == id);
                                if (findData != null)
                                {
                                    if (findData.Handle == null)
                                    {
                                        CreateVehicle(findData);
                                    }
                                    else
                                    {
                                        if (!findData.Handle.Exists())
                                        {
                                            CreateVehicle(findData);
                                        }
                                    }
                                }
                            }
                        }

                        foreach (VehicleDataV1 v in VehicleDatabase)
                        {
                            if (v.Handle != null)
                            {
                                if (!v.Handle.Exists())
                                {
                                    if (!v.WasUserDespawned)
                                    {
                                        CreateVehicle(v);
                                        if (v.SafeSpawnSet)
                                        {
                                            if (v.Handle != null)
                                            {
                                                v.Position = v.SafeSpawn;
                                                v.Rotation = v.SafeRotation;
                                            }
                                        }
                                    }
                                }
                            }
                            else
                            {
                                if (!v.WasUserDespawned)
                                {
                                    CreateVehicle(v);
                                    if (v.SafeSpawnSet)
                                    {
                                        if (v.Handle != null)
                                        {
                                            v.Position = v.SafeSpawn;
                                            v.Rotation = v.SafeRotation;
                                        }
                                    }
                                }
                            }
                        }
                    }
                    switchDetect = true;
                    waitForSwitch = false;
                }
            }

            if(StoppedInitial)
            {
                if(Game.GameTime - StartedTime > 4000)
                {
                    StoppedInitial = false;
                    if (!GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DOOR_REGISTERED_WITH_SYSTEM, 6969))
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.ADD_DOOR_TO_SYSTEM, 6969, Game.GenerateHash("prop_ch_025c_g_door_01"), 18.65038f, 546.3401f, 176.3448f, true, false, true);
                    }
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x9BA001CB45CBF627), 6969, 10f, false, true);
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x03C27E13B42A0E82), 6969, 1f, false, true);

                    if (!GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DOOR_REGISTERED_WITH_SYSTEM, 696969))
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.ADD_DOOR_TO_SYSTEM, 696969, Game.GenerateHash("prop_ld_garaged_01"), -815.2816f, 185.975f, 72.99993f, true, false, true);
                    }
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x9BA001CB45CBF627), 696969, 10f, false, true);
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x03C27E13B42A0E82), 696969, 1f, false, true);

                    if (!GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_DOOR_REGISTERED_WITH_SYSTEM, 69696969))
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.ADD_DOOR_TO_SYSTEM, 69696969, Game.GenerateHash("prop_cs4_10_tr_gd_01"), 1972.787f, 3824.554f, 32.65174f, true, false, true);
                    }
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x9BA001CB45CBF627), 69696969, 10f, false, true);
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x03C27E13B42A0E82), 69696969, 1f, false, true);

                    GTA.Native.Function.Call((GTA.Native.Hash)(0x6BAB9442830C7F53), 6969, 0, true, true);
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x6BAB9442830C7F53), 696969, 0, true, true);
                    GTA.Native.Function.Call((GTA.Native.Hash)(0x6BAB9442830C7F53), 69696969, 0, true, true);
                }
            }

            if(!SaveVehicle)
            {
                if(ModSettings.EnablePhoneTurnOnByController)
                {
                    if (IsControlJustReleased(2, GTA.Control.PhoneDown) && !UsingKeyboard)
                    {
                        bool ctrl = GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_ENABLED, 0, GTA.Control.PhoneDown);
                        ctrl = ctrl && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_ENABLED, 1, GTA.Control.PhoneDown);
                        ctrl = ctrl && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_ENABLED, 2, GTA.Control.PhoneDown);
                        ctrl = ctrl && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_ENABLED, 0, GTA.Control.ScriptPadDown);
                        ctrl = ctrl && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_ENABLED, 1, GTA.Control.ScriptPadDown);
                        ctrl = ctrl && GTA.Native.Function.Call<bool>(GTA.Native.Hash.IS_CONTROL_ENABLED, 2, GTA.Control.ScriptPadDown);
                        if(ctrl)
                        {
                            if (!GUI.Phone.IsOn && !GUI.Phone.TriggerLoaded)
                            {
                                GTA.Native.OutputArgument arg1 = new GTA.Native.OutputArgument();
                                GTA.Native.Function.Call(GTA.Native.Hash.GET_MOBILE_PHONE_POSITION, arg1);
                                Vector3 pos = arg1.GetResult<Vector3>();
                                if (pos != null)
                                {
                                    if (pos.Y < -70f || pos.Y > 0f)
                                        GUI.Phone.TriggerLoaded = true;
                                }
                            }
                        }
                    }
                }

                if ((IsControlPressed(2, GTA.Control.ScriptLS) && IsControlPressed(2, GTA.Control.VehicleDuck)) && !debouncer)
                {
                    debouncer = true;
                    if (!GUI.Phone.IsOn)
                    {
                        SaveVehicle = true;
                    }
                    else
                    {
                        GTA.UI.Screen.ShowSubtitle("Please close your phone first.", 3000);
                    }
                }

                if(IsControlJustReleased(2, GTA.Control.ScriptLS))
                {
                    debouncer = false;
                }

                if(IsControlJustReleased(2, GTA.Control.VehicleDuck))
                {
                    debouncer = false;
                }
            }

            if(AttachedTasks.Count > 0)
            {
                foreach (var veh in new Dictionary<Vehicle, PedTask>(AttachedTasks))
                {
                    if(veh.Value.IsDoneSequence())
                    {
                        veh.Value.Clean();
                        veh.Value.Ped.IsPersistent = false;
                        veh.Value.Ped.IsInvincible = false;
                        veh.Key.IsInvincible = false;
                        veh.Key.CanBeVisiblyDamaged = true;
                        AttachedTasks.Remove(veh.Key);
                        if(ModSettings.PlaySpeechOnCarDelivery)
                            GTA.Native.Function.Call((GTA.Native.Hash)0x8E04FEDD28D42462, Game.Player.Character, "GENERIC_THANKS", "SPEECH_PARAMS_FORCE_SHOUTED", 0);
                        if(ModSettings.FocusOnCarDelivery)
                            GTA.Native.Function.Call(GTA.Native.Hash.SET_GAMEPLAY_VEHICLE_HINT, veh.Key.Handle, 0f, 0f, 0.5f, true, 3000, 1000, 1000);
                        GTA.UI.Screen.ShowSubtitle("Your vehicle has arrived!", 3000);
                    }
                }
            }

            
            int viewType = GTA.Native.Function.Call<int>((GTA.Native.Hash)(0x19CAFA3C87F7C2FF));
            int viewMode = GTA.Native.Function.Call<int>((GTA.Native.Hash)(0xEE778F8C7E1142E2), viewType);
            if(viewMode == 4)
            {
                if(!LastFirstPerson)
                {
                    LastFirstPerson = true;
                    LastViewChanged = true;
                }
            }
            else
            {
                if(LastFirstPerson)
                {
                    LastFirstPerson = false;
                    LastViewChanged = true;
                }
            }

            if(LastViewChanged)
            {
                if(GUI.Phone.IsOn)
                {
                    if (LastFirstPerson)
                        GUI.Phone.BringDown();
                    else
                        GUI.Phone.BringUp();
                }
                LastViewChanged = false;
            }

            if(ModSettings.RemovePersonalVehicles && !Game.IsMissionActive)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.TERMINATE_ALL_SCRIPTS_WITH_THIS_NAME, "vehicle_gen_controller");
            }

            if (GUI.Phone.TriggerLoaded)
            {
                if(GUI.Phone.Interface == 0)
                    GUI.Phone.PhoneScaleform = new GTA.Scaleform("cellphone_ifruit");
                else if(GUI.Phone.Interface == 1)
                    GUI.Phone.PhoneScaleform = new GTA.Scaleform("cellphone_facade");
                else
                    GUI.Phone.PhoneScaleform = new GTA.Scaleform("cellphone_badger");

                int timr = Game.GameTime;
                while (!GTA.Native.Function.Call<bool>(GTA.Native.Hash.HAS_SCALEFORM_MOVIE_LOADED, GUI.Phone.PhoneScaleform.Handle))
                {
                    Yield();
                    if (Game.GameTime - timr > 1000)
                    {
                        GTA.UI.Screen.ShowSubtitle("Failed to load phone. Try again.");
                        return;
                    }
                }
                GUI.Phone.TurnOn();
                GUI.Phone.TriggerLoaded = false;
            }

            if(GUI.Phone.RawOn)
            {
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.Phone, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleRadioWheel, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleHeadlight, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleCinCam, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.Attack, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.Aim, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleDuck, true);
                if(!UsingKeyboard)
                    GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.Sprint, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.Context, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.CharacterWheel, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleMouseControlOverride, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleSelectNextWeapon, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleSelectPrevWeapon, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleAim, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleAttack, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleAttack2, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleFlyAttack, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleFlyAttack2, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.VehicleFlyMouseControlOverride, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.ScriptPadRight, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.ReplayStartStopRecording, true);
                GTA.Native.Function.Call(GTA.Native.Hash.DISABLE_CONTROL_ACTION, 0, (int)GTA.Control.ReplayStartStopRecordingSecondary, true);
            }

            if (GUI.Phone.IsOn)
            {
                if(Game.IsControlJustPressed(GTA.Control.PhoneRight))
                {
                    if(GUI.Phone.IsOnHomeScreen())
                        GUI.Phone.SetInputEvent(GUI.Phone.Direction.Right);
                }
                if (Game.IsControlJustPressed(GTA.Control.PhoneLeft))
                {
                    if (GUI.Phone.IsOnHomeScreen())
                        GUI.Phone.SetInputEvent(GUI.Phone.Direction.Left);
                }
                if(GUI.Phone.CurrentApp != null)
                {
                    if(GUI.Phone.CurrentApp.Name == "Changelog")
                    {
                        if (Game.IsControlPressed(GTA.Control.PhoneDown))
                        {
                            GUI.Phone.SetInputEvent(GUI.Phone.Direction.Down);
                        }
                        if (Game.IsControlPressed(GTA.Control.PhoneUp))
                        {
                            GUI.Phone.SetInputEvent(GUI.Phone.Direction.Up);
                        }
                    }
                    else
                    {
                        if (Game.IsControlJustPressed(GTA.Control.PhoneDown))
                        {
                            if (GUI.Phone.IsOnHomeScreen())
                                GUI.Phone.SetInputEvent(GUI.Phone.Direction.Down);
                            else
                                GUI.Phone.SetInputEvent(GUI.Phone.Direction.Down);
                        }
                        if (Game.IsControlJustPressed(GTA.Control.PhoneUp))
                        {
                            if (GUI.Phone.IsOnHomeScreen())
                                GUI.Phone.SetInputEvent(GUI.Phone.Direction.Up);
                            else
                                GUI.Phone.SetInputEvent(GUI.Phone.Direction.Up);
                        }
                    }
                }
                else
                {
                    if (Game.IsControlJustPressed(GTA.Control.PhoneDown))
                    {
                        if (GUI.Phone.IsOnHomeScreen())
                            GUI.Phone.SetInputEvent(GUI.Phone.Direction.Down);
                        else
                            GUI.Phone.SetInputEvent(GUI.Phone.Direction.Down);
                    }
                    if (Game.IsControlJustPressed(GTA.Control.PhoneUp))
                    {
                        if (GUI.Phone.IsOnHomeScreen())
                            GUI.Phone.SetInputEvent(GUI.Phone.Direction.Up);
                        else
                            GUI.Phone.SetInputEvent(GUI.Phone.Direction.Up);
                    }
                }
                if (Game.IsControlJustPressed(GTA.Control.CursorScrollDown))
                {
                    if (GUI.Phone.IsOnHomeScreen())
                        GUI.Phone.SetInputEvent(GUI.Phone.Direction.Right);
                    else
                        GUI.Phone.SetInputEvent(GUI.Phone.Direction.Down);
                }
                if (Game.IsControlJustPressed(GTA.Control.CursorScrollUp))
                {
                    if (GUI.Phone.IsOnHomeScreen())
                        GUI.Phone.SetInputEvent(GUI.Phone.Direction.Left);
                    else
                        GUI.Phone.SetInputEvent(GUI.Phone.Direction.Up);
                }
                if (Game.IsControlJustPressed(GTA.Control.PhoneSelect))
                {
                    if (GUI.Phone.IsOn)
                    {
                        GUI.Phone.Click();
                    }
                }
                if (Game.IsControlJustPressed(GTA.Control.PhoneCancel))
                {
                    if (GUI.Phone.IsOn)
                    {
                        GUI.Phone.Back();
                    }
                }
            }

            GUI.Phone.Draw();

            if (DeletePersonalCars)
            {
                Vehicle[] vehs = World.GetAllVehicles();
                foreach (Vehicle veh in vehs)
                {
                    if (veh != null)
                    {
                        if (veh.Exists())
                        {
                            if (veh.Model == VehicleHash.Buffalo2 || veh.Model == VehicleHash.Bagger || veh.Model == VehicleHash.Tailgater || veh.Model == VehicleHash.Bodhi2 || veh.Model == VehicleHash.Issi2 || veh.Model == VehicleHash.Sentinel2 || veh.Model == VehicleHash.Blazer3)
                            {
                                while (veh.Exists())
                                {
                                    veh.Delete();
                                }
                            }
                        }
                    }
                }
                DeletePersonalCars = false;
                return;
            }

            if(SaveVehicle)
            {
                if(Game.Player.Character.CurrentVehicle != null)
                {
                    if(AttachedVehicles.ContainsKey(Game.Player.Character.CurrentVehicle))
                    {
                        Vehicle veh = Game.Player.Character.CurrentVehicle;
                        VehicleDataV1 dat = AttachedVehicles[veh];
                        AttachedVehicles.Remove(veh);
                        VehicleDatabase.Remove(dat);
                        VehicleMetabase.Remove(dat.Meta);
                        if(veh.AttachedBlip != null)
                            veh.AttachedBlip.Delete();
                        foreach(Blip blip in veh.AttachedBlips)
                        {
                            blip.Delete();
                        }
                        veh.IsPersistent = false;
                        GTA.UI.Notification.Show($"Vehicle Removed [{VehicleDatabase.Count}]");
                        SaveVehicle = false;
                        return;
                    }
                    else
                    {
                        if(VehicleDatabase.Count >= ModSettings.MaxNumberOfCars)
                        {
                            GTA.UI.Notification.Show($"ERROR: Max Vehicles Reached [{ModSettings.MaxNumberOfCars}]");
                        }
                        else
                        {
                            Vehicle veh = Game.Player.Character.CurrentVehicle;
                            VehicleDataV1 dat = new VehicleDataV1();

                            SaveVehicleData(veh, dat);
                            dat.SafeSpawn = veh.Position;
                            dat.SafeSpawnSet = true;
                            dat.SafeRotation = veh.Rotation;
                            if(ModSettings.EnableBlips)
                            {
                                if (veh.AttachedBlip == null)
                                    veh.AddBlip();
                                if (veh.Model.IsHelicopter)
                                    veh.AttachedBlip.Sprite = BlipSprite.PoliceHelicopter;
                                else if (veh.Model.IsAmphibiousQuadBike || veh.Model.IsBicycle || veh.Model.IsBike || veh.Model.IsQuadBike)
                                    veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleBike;
                                else if (veh.Model.IsJetSki)
                                    veh.AttachedBlip.Sprite = BlipSprite.Seashark;
                                else if (veh.Model.IsBoat)
                                    veh.AttachedBlip.Sprite = BlipSprite.Boat;
                                else if (veh.Model.IsPlane)
                                    veh.AttachedBlip.Sprite = BlipSprite.Plane;
                                else
                                    veh.AttachedBlip.Sprite = BlipSprite.PersonalVehicleCar;
                                veh.AttachedBlip.IsShortRange = true;
                                veh.AttachedBlip.Scale = 0.75f;
                                veh.AttachedBlip.Name = "Saved Vehicle";
                                veh.AttachedBlip.Alpha = 255;
                                GTA.Native.Function.Call(GTA.Native.Hash.SHOW_TICK_ON_BLIP, veh.AttachedBlip.Handle, false);
                                veh.AttachedBlip.Priority = 0;
                                veh.AttachedBlip.Color = (BlipColor)dat.BlipColor;
                                GTA.Native.Function.Call(GTA.Native.Hash.SHOW_HEADING_INDICATOR_ON_BLIP, veh.AttachedBlip.Handle, false);
                            }
                            MainCharacter.CarAttach = dat.Id;
                            AttachedVehicles[veh] = dat;
                            VehicleDatabase.Add(dat);
                            VehicleMetabase.Add(dat.Meta);
                            GTA.UI.Notification.Show($"Vehicle Added [{VehicleDatabase.Count}]");
                        }
                        SaveVehicle = false;
                        return;
                    }
                }
                else
                {
                    GTA.UI.Notification.Show("ERROR: Not in vehicle");
                }
                SaveVehicle = false;
                return;
            }

            if (Game.GameTime >= LoadTick && LoadCharacter && !Game.IsMissionActive && !Game.IsCutsceneActive)
            {
                if (ModSettings.SaveCharacterSkin)
                {
                    Model md = new Model(MainCharacter.PedSkin);
                    if (md.IsInCdImage)
                    {
                        md.Request(10000);
                        Game.Player.ChangeModel(md);
                        Game.Player.ChangeModel(md);

                        GTA.Native.Function.Call(GTA.Native.Hash.SET_PED_DEFAULT_COMPONENT_VARIATION, Game.Player.Character.Handle);
                    }
                }

                if (ModSettings.SaveGameTime)
                {
                    World.CurrentTimeOfDay = MainCharacter.Time;
                    World.CurrentDate = MainCharacter.Date;
                }

                if(ModSettings.SaveGameWeather)
                {
                    World.Weather = MainCharacter.Weather;
                    World.NextWeather = MainCharacter.WeatherNext;
                }

                if(ModSettings.SaveCharacterPosition)
                {
                    Game.Player.Character.PositionNoOffset = MainCharacter.Position;
                    Game.Player.Character.Heading = MainCharacter.Heading;
                }

                if (ModSettings.SaveCharacterHealthArmor)
                {
                    Game.Player.Character.Health = MainCharacter.Health;
                    Game.Player.Character.Armor = MainCharacter.Armor;
                }
                
                if(ModSettings.SaveCharacterClothes && ModSettings.SaveCharacterSkin)
                {
                    for (int i = 0; i < 12; i++)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_PED_COMPONENT_VARIATION, Game.Player.Character.Handle, i, MainCharacter.ClothesVariant[i], MainCharacter.ClothesTexture[i], MainCharacter.ClothesPalette[i]);
                    }

                    for (int i = 0; i < 7; i++)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_PED_PROP_INDEX, Game.Player.Character.Handle, i, MainCharacter.PropsVariant[i], MainCharacter.PropsTexture[i], false);
                    }
                }

                GUI.Phone.HomescreenImage = ((GUI.Phone.BackgroundImage)MainCharacter.PhoneBackground);
                GUI.Phone.PhoneColor = MainCharacter.PhoneColor;
                GUI.Phone.PhoneBrightness = MainCharacter.PhoneBrightness;
                GUI.Phone.ActiveTheme = ((GUI.Phone.Theme)MainCharacter.PhoneTheme);
                GUI.Phone.PhoneTone = MainCharacter.PhoneTone;
                GUI.Phone.PhoneModel = MainCharacter.PhoneBody;

                if(ModSettings.SaveCharacterWeapons)
                {
                    Game.Player.Character.Weapons.RemoveAll();
                    foreach (var dat in MainCharacter.Weapons)
                    {
                        GTA.Native.Function.Call(GTA.Native.Hash.GIVE_WEAPON_TO_PED, Game.Player.Character.Handle, dat.Key, MainCharacter.Ammo[dat.Key], false, false);
                    }
                    foreach (var dat in MainCharacter.Weapons)
                    {
                        if (WeaponManager.HasWeapon(Game.Player.Character.Handle, (WeaponHash)dat.Key))
                        {
                            foreach (var x in dat.Value)
                            {
                                if (WeaponManager.DoesWeaponTakeComponent((WeaponHash)dat.Key, (WeaponComponentHash)x))
                                {
                                    WeaponManager.GiveWeaponComponent(Game.Player.Character.Handle, dat.Key, x);
                                }
                            }
                            GTA.Native.Function.Call(GTA.Native.Hash.SET_AMMO_IN_CLIP, Game.Player.Character.Handle, dat.Key, MainCharacter.Ammo[dat.Key]);
                            GTA.Native.Function.Call(GTA.Native.Hash.SET_PED_AMMO, Game.Player.Character.Handle, dat.Key, MainCharacter.Ammo[dat.Key], true);
                            if (WeaponManager.GetWeaponTintCount((WeaponHash)dat.Key) >= MainCharacter.Tints[dat.Key])
                            {
                                GTA.Native.Function.Call(GTA.Native.Hash.SET_PED_WEAPON_TINT_INDEX, Game.Player.Character.Handle, dat.Key, MainCharacter.Tints[dat.Key]);
                            }
                        }
                    }
                }
                Wait(500);
                if(ModSettings.SaveCharacterWeapons)
                {
                    if (MainCharacter.ActiveWeapon != (uint)WeaponHash.Unarmed)
                        GTA.Native.Function.Call(GTA.Native.Hash.SET_CURRENT_PED_WEAPON, Game.Player.Character.Handle, MainCharacter.ActiveWeapon, true);
                }

                if (LoadMax > 0)
                {
                    if(ModSettings.SpawnAllVehiclesOnStartup && !ModSettings.EnableVehicleStreamer)
                    {
                        LoadVehicles = true;
                    }
                    else
                    {
                        if(!ModSettings.EnableVehicleStreamer)
                        {
                            foreach (VehicleDataV1 v in VehicleDatabase)
                                v.WasUserDespawned = true;
                        }

                        SaveOne = true;
                        UpdateTick = Game.GameTime + 2000;
                        IsModLoading = false;
                        GTA.UI.Screen.FadeIn(2000);
                        GTA.UI.Screen.ShowSubtitle("Advanced Persistence Loaded!", 3000);
                        Logging.Log("Advanced Persistence Loaded! (" + (Game.GameTime - StartedTime) + "ms)");
                    }
                }
                else
                {
                    SaveOne = true;
                    UpdateTick = Game.GameTime + 2000;
                    IsModLoading = false;
                    GTA.UI.Screen.FadeIn(2000);
                    GTA.UI.Screen.ShowSubtitle("Advanced Persistence Loaded!", 3000);
                    Logging.Log("Advanced Persistence Loaded! (" + (Game.GameTime - StartedTime) + "ms)");
                }
                LoadCharacter = false;
                return;
            }

            if(LoadVehicles)
            {
                VehicleDataV1 vdata = VehicleDatabase[LoadCnt];
                Vehicle veh = CreateVehicle(vdata, null, false, false, ModSettings.SpawnAllVehiclesAtSafeSpawn);
                if (LoadCnt == LoadMax - 1)
                {
                    foreach(Vehicle v in AttachedVehicles.Keys)
                    {
                        if(v != null)
                        {
                            if(v.Exists())
                            {
                                if(!v.Model.IsHelicopter)
                                {
                                    v.IsPositionFrozen = false;
                                }
                            }
                        }
                    }
                    IsModLoading = false;
                    GTA.UI.Screen.FadeIn(2000);
                    GTA.UI.Screen.ShowSubtitle("Advanced Persistence Loaded!", 3000);
                    LoadVehicles = false;
                    SaveOne = true;
                    UpdateTick = Game.GameTime + 2000;
                    Logging.Log("Advanced Persistence Loaded! (" + (Game.GameTime - StartedTime) + "ms)");
                }
                else
                {
                    LoadCnt++;
                }
                return;
            }

            if(SaveTwo)
            {
                if(SaveThree)
                {
                    if (Game.GameTime >= SaveTick)
                    {
                        StreamTick = Game.GameTime;
                        Stream1 = true;
                        SaveThree = false;
                    }
                }

                if (Stream1 && (Game.GameTime - StreamTick) > 50)
                {
                    using (Stream fstream = new FileStream(CharacterMetaname, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        form.Serialize(fstream, MainCharacterMeta);
                    }
                    Stream1 = false;
                    Stream2 = true;
                    StreamTick = Game.GameTime;
                }
                else if(Stream2 && (Game.GameTime - StreamTick) > 50)
                {
                    using (Stream fstream = new FileStream(CharacterFilename, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        form.Serialize(fstream, MainCharacter);
                    }
                    Stream2 = false;
                    Stream3 = true;
                    StreamTick = Game.GameTime;
                }
                else if(Stream3 && (Game.GameTime - StreamTick) > 50)
                {
                    using (Stream fstream = new FileStream(VehicleMetaname, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        form.Serialize(fstream, VehicleMetabase);
                    }
                    Stream3 = false;
                    Stream4 = true;
                    StreamTick = Game.GameTime;
                }
                else if(Stream4 && (Game.GameTime - StreamTick) > 50)
                {
                    using (Stream fstream = new FileStream(VehicleFilename, FileMode.Create, FileAccess.Write, FileShare.None))
                    {
                        form.Serialize(fstream, VehicleDatabase);
                    }
                    Stream4 = false;
                    SaveTwo = false;
                    SaveOne = true;
                    SaveThree = false;
                    UpdateTick = Game.GameTime + ModSettings.DataSavingTime;
                    //GTA.UI.Screen.ShowSubtitle("Time To Save: " + (Game.GameTime - SaveTimed));
                }
            }

            bool shouldSave = SaveOne;
            if (!ModSettings.SaveDuringMissions)
                shouldSave = shouldSave && !Game.IsMissionActive;
            if(shouldSave)
            {
                if (Game.GameTime >= UpdateTick)
                {
                    SaveTimed = Game.GameTime;
                    Ped pped = Game.Player.Character;
                    MainCharacter.Position = pped.Position;
                    MainCharacter.Heading = pped.Heading;
                    MainCharacter.PedSkin = pped.Model.Hash;
                    MainCharacter.Health = pped.Health;
                    MainCharacter.Armor = pped.Armor;
                    MainCharacter.PhoneBackground = (int)GUI.Phone.HomescreenImage;
                    MainCharacter.PhoneTheme = (int)GUI.Phone.ActiveTheme;
                    MainCharacter.PhoneColor = GUI.Phone.PhoneColor;
                    MainCharacter.PhoneBrightness = GUI.Phone.PhoneBrightness;
                    MainCharacter.Time = World.CurrentTimeOfDay;
                    MainCharacter.Date = World.CurrentDate;
                    MainCharacter.Weather = World.Weather;
                    MainCharacter.WeatherNext = World.NextWeather;
                    MainCharacter.PhoneBody = GUI.Phone.PhoneModel;
                    MainCharacter.PhoneTone = GUI.Phone.PhoneTone;

                    if(ModSettings.SaveCharacterClothes)
                    {
                        for (int i = 0; i < 12; i++)
                        {
                            MainCharacter.ClothesVariant[i] = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_PED_DRAWABLE_VARIATION, pped.Handle, i);
                            MainCharacter.ClothesTexture[i] = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_PED_TEXTURE_VARIATION, pped.Handle, i);
                            MainCharacter.ClothesPalette[i] = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_PED_PALETTE_VARIATION, pped.Handle, i);
                        }

                        for (int i = 0; i < 7; i++)
                        {
                            MainCharacter.PropsVariant[i] = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_PED_PROP_INDEX, pped.Handle, i);
                            MainCharacter.PropsTexture[i] = GTA.Native.Function.Call<int>(GTA.Native.Hash.GET_PED_PROP_TEXTURE_INDEX, pped.Handle, i);
                        }
                    }

                    MainCharacter.CarAttach = null;
                    if(ModSettings.SaveCharacterPosition)
                    {
                        if (pped.CurrentVehicle != null)
                        {
                            if (AttachedVehicles.ContainsKey(pped.CurrentVehicle))
                            {
                                MainCharacter.CarAttach = AttachedVehicles[pped.CurrentVehicle].Id;
                            }
                        }
                    }

                    if(ModSettings.SaveCharacterWeapons)
                    {
                        CWeapons = new Dictionary<uint, List<uint>>();
                        CTints = new Dictionary<uint, uint>();
                        CAmmo = new Dictionary<uint, int>();
                    }

                    SaveOne = false;
                    DoWeapons = true;
                }
            }

            if (DoVehicles)
            {
                if (ModSettings.SaveCharacterWeapons)
                    MainCharacter.ActiveWeapon = WeaponManager.GetCurrentWeapon(Game.Player.Character.Handle);

                if(CloneList != null)
                {
                    if(CloneList.Count > 0)
                    {
                        VehicleDataV1 dat = CloneList[VehicleIndex];
                        if (dat.Handle != null)
                        {
                            if (dat.Handle.Exists())
                            {
                                SaveVehicleData(dat.Handle, dat);
                            }
                            else
                            {
                                if(!dat.WasUserDespawned)
                                {
                                    if(!ModSettings.EnableVehicleStreamer)
                                        CreateVehicle(dat);
                                }
                            }

                            if (dat.Handle.Model.IsHelicopter)
                                dat.Handle.IsPositionFrozen = false;
                        }
                        else
                        {
                            if(!dat.WasUserDespawned)
                            {
                                if(!ModSettings.EnableVehicleStreamer)
                                    CreateVehicle(dat);
                            }
                            if(dat.Handle != null)
                                if (dat.Handle.Model.IsHelicopter)
                                    dat.Handle.IsPositionFrozen = false;
                        }
                    }
                }

                VehicleIndex++;
                if(CloneList != null)
                {
                    if(VehicleIndex >= CloneList.Count)
                    {
                        DoVehicles = false;
                        SaveThree = true;
                        SaveTwo = true;
                        SaveTick = Game.GameTime + 500;
                    }
                }
                else
                {
                    DoVehicles = false;
                    SaveThree = true;
                    SaveTwo = true;
                    SaveTick = Game.GameTime + 500;
                }
            }

            if (DoWeapons)
            {
                if (ModSettings.SaveCharacterWeapons)
                {
                    int ped = Game.Player.Character.Handle;
                    WeaponHash wep = Weapons[WeaponIndex];
                    if(wep != WeaponHash.Unarmed)
                    {
                        if (WeaponManager.HasWeapon(ped, wep))
                        {
                            CAmmo[(uint)wep] = WeaponManager.GetAmmoInWeapon(ped, wep);
                            CTints[(uint)wep] = WeaponManager.GetWeaponTintIndex(ped, wep);
                            CWeapons[(uint)wep] = new List<uint>();
                            foreach (WeaponComponentHash cmp in WeaponComponents)
                            {
                                if (WeaponManager.DoesWeaponTakeComponent(wep, cmp))
                                {
                                    if (WeaponManager.HasGotWeaponComponent(ped, wep, cmp))
                                    {
                                        CWeapons[(uint)wep].Add((uint)cmp);
                                    }
                                }
                            }
                        }
                    }
                }

                WeaponIndex++;
                if(WeaponIndex >= Weapons.Length)
                {
                    if(ModSettings.SaveCharacterWeapons)
                    {
                        MainCharacter.Weapons = CWeapons;
                        MainCharacter.Ammo = CAmmo;
                        MainCharacter.Tints = CTints;
                    }
                    WeaponIndex = 0;
                    DoWeapons = false;
                    DoVehicles = true;
                    VehicleIndex = 0;
                    CloneList = VehicleDatabase.ToList();
                }
            }

            if(ModSettings.EnableVehicleStreamer)
            {
                Streamer.HandleNextVehicle();
            }
        }

        public int PressDownETime = Game.GameTime;
        public bool initialPress = true;
        public bool DidTheThing = false;
        private void OnKeyDown(object sender, KeyEventArgs e)
        {

        }

        public static Prop FobObj = null;
        public static bool DoingFobAnim = false;
        public WeaponHash StoredWeapon = WeaponHash.Unarmed;
        delegate ulong GetHandleAddressFuncDelegate(int handle);
        private void OnKeyUp(object sender, KeyEventArgs e)
        {
         
            if(Constants.DebugMode)
            {

            }

            if(e.KeyCode == Keys.J)
            {

            }

            if (!SaveVehicle)
            {
                if (e.Modifiers == (Keys)Enum.Parse(typeof(Keys), ModSettings.SaveModifier, true) && e.KeyCode == (Keys)Enum.Parse(typeof(Keys), ModSettings.SaveKey, true))
                {
                    if (!GUI.Phone.IsOn)
                    {
                        SaveVehicle = true;
                    }
                    else
                    {
                        GTA.UI.Screen.ShowSubtitle("Please close your phone first.", 3000);
                    }
                }

                if (e.Modifiers == (Keys)Enum.Parse(typeof(Keys), ModSettings.PhoneModifier, true) && e.KeyCode == (Keys)Enum.Parse(typeof(Keys), ModSettings.PhoneKey, true))
                {
                    if (!GUI.Phone.IsOn && !GUI.Phone.TriggerLoaded)
                    {
                        GTA.Native.OutputArgument arg1 = new GTA.Native.OutputArgument();
                        GTA.Native.Function.Call(GTA.Native.Hash.GET_MOBILE_PHONE_POSITION, arg1);
                        Vector3 pos = arg1.GetResult<Vector3>();
                        if (pos != null)
                        {
                            if (pos.Y < -70f || pos.Y > 0f)
                                GUI.Phone.TriggerLoaded = true;
                        }
                    }
                }
            }
        }
    }

    public static class Logging
    {
        public static readonly string Filename = "AdvPer_LOGS.txt";

        public static void Log(string s)
        {
            try
            {
                if(Directory.Exists("scripts/" + Constants.SubFolder))
                {
                    StreamWriter f = new StreamWriter("scripts/" + Constants.SubFolder + "/" + Filename, true);
                    f.WriteLine($"[{DateTime.Now}] {s}");
                    f.Close();
                }
                else
                {
                    StreamWriter f = new StreamWriter("scripts/" + Filename, true);
                    f.WriteLine($"[{DateTime.Now}] {s}");
                    f.Close();
                }
            }
            catch(Exception e)
            {

            }
        }

        public static void Reset()
        {

        }
    }

    public static class ModSettings
    {
        public static readonly string Filename = "AdvPer_SETTINGS.txt";

        //Mod
        public static int MaxNumberOfStreamedInCars = 16;
        public static float StreamInDistance = 150f;
        public static float StreamOutDistance = 160f;
        public static bool EnableVehicleStreamer = true;
        public static int MaxNumberOfCars = 32;
        public static bool RemovePersonalVehicles = true;
        public static int DataSavingTime = 3000;
        public static bool SpawnAllVehiclesOnStartup = true;
        public static bool SpawnAllVehiclesAtSafeSpawn = false;
        public static bool ShowVehicleOutlines = true;
        public static bool SaveDuringMissions = false;
        public static bool EnablePhoneTurnOnByController = true;
        public static bool EnableBlips = true;
        public static bool PlaySpeechOnCarDelivery = true;
        public static bool FocusOnCarDelivery = true;
        public static bool EnableRemoteSystem = true;
        public static bool EnableRemoteMovement = true;
        public static bool ReturnVehiclesToSafeLocation = true;

        //Keys
        public static string SaveKey = "T";
        public static string SaveModifier = "SHIFT";

        public static string PhoneKey = "Z";
        public static string PhoneModifier = "SHIFT";

        //Character
        public static bool SaveCharacterPosition = true;
        public static bool SaveCharacterSkin = true;
        public static bool SaveCharacterClothes = true;
        public static bool SaveCharacterHealthArmor = true;
        public static bool SaveCharacterWeapons = true;

        //Vehicles
        public static bool SaveVehicleMods = true;
        public static bool SaveVehicleExtras = true;
        public static bool SaveVehicleDoorState = true;
        public static bool SaveVehicleWindowState = true;
        public static bool SaveVehicleWheelState = true;
        public static bool SaveVehicleLightState = true;
        public static bool SaveVehicleEngineState = true;
        public static bool SaveVehicleConvertibleState = true;
        public static bool SaveVehicleWheelTurn = true;

        //World
        public static bool SaveGameTime = true;
        public static bool SaveGameWeather = true;

        public static bool LoadSettings()
        {
            string filePath;
            if(File.Exists("scripts/" + Filename))
            {
                filePath = "scripts/" + Filename;
            }
            else
            {
                filePath = "scripts/" + Constants.SubFolder + "/" + Filename;
            }

            if(File.Exists(filePath))
            {
                try
                {
                    string[] data = File.ReadAllLines(filePath);
                    foreach(string s in data)
                    {
                        try
                        {
                            if (s.Contains("/-"))
                                continue;
                            if (string.IsNullOrEmpty(s) || string.IsNullOrWhiteSpace(s))
                                continue;
                            string x = s;
                            if (s.Contains(';'))
                            {
                                x = s.Substring(0, s.IndexOf(';'));
                            }
                            x = x.Replace(" ", string.Empty);
                            string[] cmds = x.Split('=');
                            if (cmds.Length < 2)
                            {
                                Logging.Log("INVALID SETTING: " + cmds[0]);
                                continue;
                            }

                            Logging.Log("Settings fetch: " + cmds[0] + " -> " + cmds[1]);

                            if (cmds[0] == "MaxNumberOfCars")
                            {
                                MaxNumberOfCars = int.Parse(cmds[1]);
                            }
                            else if(cmds[0] == "MaxNumberOfStreamedInCars")
                            {
                                MaxNumberOfStreamedInCars = int.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveDuringMissions")
                            {
                                SaveDuringMissions = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "EnablePhoneTurnOnByController")
                            {
                                EnablePhoneTurnOnByController = bool.Parse(cmds[1]);
                            }
                            else if(cmds[0] == "StreamInDistance")
                            {
                                StreamInDistance = float.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "StreamOutDistance")
                            {
                                StreamOutDistance = float.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "EnableBlips")
                            {
                                EnableBlips = bool.Parse(cmds[1]);
                            }
                            else if(cmds[0] == "EnableVehicleStreamer")
                            {
                                EnableVehicleStreamer = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "PlaySpeechOnCarDelivery")
                            {
                                PlaySpeechOnCarDelivery = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "EnableRemoteSystem")
                            {
                                EnableRemoteSystem = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "EnableRemoteMovement")
                            {
                                EnableRemoteMovement = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "FocusOnCarDelivery")
                            {
                                FocusOnCarDelivery = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveCharacterWeapons")
                            {
                                SaveCharacterWeapons = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "ReturnVehiclesToSafeLocation")
                            {
                                ReturnVehiclesToSafeLocation = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "ShowVehicleOutlines")
                            {
                                ShowVehicleOutlines = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "RemovePersonalVehicles")
                            {
                                RemovePersonalVehicles = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SpawnAllVehiclesOnStartup")
                            {
                                SpawnAllVehiclesOnStartup = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SpawnAllVehiclesAtSafeSpawn")
                            {
                                SpawnAllVehiclesAtSafeSpawn = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "DataSavingTime")
                            {
                                DataSavingTime = int.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveKey")
                            {
                                SaveKey = cmds[1];
                            }
                            else if (cmds[0] == "SaveModifier")
                            {
                                SaveModifier = cmds[1];
                            }
                            else if (cmds[0] == "PhoneKey")
                            {
                                PhoneKey = cmds[1];
                            }
                            else if (cmds[0] == "PhoneModifier")
                            {
                                PhoneModifier = cmds[1];
                            }
                            else if (cmds[0] == "SaveCharacterPosition")
                            {
                                SaveCharacterPosition = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveCharacterSkin")
                            {
                                SaveCharacterSkin = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveCharacterClothes")
                            {
                                SaveCharacterClothes = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveCharacterHealthArmor")
                            {
                                SaveCharacterHealthArmor = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveVehicleMods")
                            {
                                SaveVehicleMods = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveVehicleExtras")
                            {
                                SaveVehicleExtras = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveVehicleDoorState")
                            {
                                SaveVehicleDoorState = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveVehicleWindowState")
                            {
                                SaveVehicleWindowState = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveVehicleLightState")
                            {
                                SaveVehicleLightState = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveVehicleEngineState")
                            {
                                SaveVehicleEngineState = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveVehicleConvertibleState")
                            {
                                SaveVehicleConvertibleState = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveVehicleWheelTurn")
                            {
                                SaveVehicleWheelTurn = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveGameTime")
                            {
                                SaveGameTime = bool.Parse(cmds[1]);
                            }
                            else if (cmds[0] == "SaveGameWeather")
                            {
                                SaveGameWeather = bool.Parse(cmds[1]);
                            }
                        }
                        catch(Exception e)
                        {
                            Logging.Log("ERROR PARSING SETTING: (" + s + ") -> " + e.ToString());
                        }
                    }

                    return true;
                }
                catch(Exception e)
                {
                    Logging.Log($"ERROR PARSING SETTINGS: " + e.ToString());
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
    }
}
