using GTA;
using GTA.Native;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Metadata
{
    public static class Decor
    {
        public static bool ExistsOn(string propertyName)
        {
            return Function.Call<bool>(Hash.DOES_VEHICLE_EXIST_WITH_DECORATOR, propertyName);
        }

        public static bool ExistsOn(this Entity entity, string propertyName)
        {
            return Function.Call<bool>(Hash.DECOR_EXIST_ON, entity, propertyName);
        }

        public static float GetFloat(this Entity entity, string propertyName)
        {
            return Function.Call<float>(Hash._DECOR_GET_FLOAT, entity, propertyName);
        }

        public static int GetInt(this Entity entity, string propertyName)
        {
            return Function.Call<int>(Hash.DECOR_GET_INT, entity, propertyName);
        }

        public static bool GetBool(this Entity entity, string propertyName)
        {
            return Function.Call<bool>(Hash.DECOR_GET_BOOL, entity, propertyName);
        }

        public static void SetBool(this Entity entity, string propertyName, bool value)
        {
            Function.Call(Hash.DECOR_SET_BOOL, entity, propertyName, value);
        }

        public static void SetInt(this Entity entity, string propertyName, int value)
        {
            Function.Call(Hash.DECOR_SET_INT, entity, propertyName, value);
        }

        public static void SetFloat(this Entity entity, string propertyName, float value)
        {
            Function.Call(Hash._DECOR_SET_FLOAT, entity, propertyName, value);
        }

        public static void Register(string propertyName, eDecorType type)
        {
            if (!Function.Call<bool>(Hash.DECOR_IS_REGISTERED_AS_TYPE, propertyName, (int)type))
            {
                Function.Call(Hash.DECOR_REGISTER, propertyName, (int)type);
            }
        }

        public static bool Registered(string propertyName, eDecorType type)
        {
            return Function.Call<bool>(Hash.DECOR_IS_REGISTERED_AS_TYPE, propertyName, (int)type);
        }

        public static bool Remove(this Entity entity, string propertyName)
        {
            return Function.Call<bool>(Hash.DECOR_REMOVE, entity, propertyName);
        }

       public static void Unlock()
        {
            unsafe
            {
                try
                {
                    IntPtr addr = (IntPtr)FindPattern("\x40\x53\x48\x83\xEC\x20\x80\x3D\x00\x00\x00\x00\x00\x8B\xDA\x75\x29", "xxxxxxxx????xxxxx");
                    if (addr != IntPtr.Zero)
                    {
                        byte* g_bIsDecorRegisterLockedPtr = (byte*)(addr + *(int*)(addr + 8) + 13);
                        *g_bIsDecorRegisterLockedPtr = 0;
                    }
                }
                catch (Exception ex)
                {
                    Logger.Log($"{ex.Message}{ex.HResult}{ex.StackTrace}");
                }

            }
        }
        public static void Lock()
        {
            unsafe
            {

                try
                {
                   IntPtr addr = (IntPtr)FindPattern("\x40\x53\x48\x83\xEC\x20\x80\x3D\x00\x00\x00\x00\x00\x8B\xDA\x75\x29", "xxxxxxxx????xxxxx");
                   if (addr != IntPtr.Zero)
                    {
                        byte* g_bIsDecorRegisterLockedPtr = (byte*)(addr + *(int*)(addr + 8) + 13);
                        *g_bIsDecorRegisterLockedPtr = 1;
                    }
                 }
                catch (Exception ex)
                {
                    Logger.Log($"{ex.Message}{ex.HResult}{ex.StackTrace}");
                }
            }
        }

        public unsafe static byte* FindPattern(string pattern, string mask)
        {
            try
            {
             ProcessModule module = Process.GetCurrentProcess().MainModule;

            ulong address = (ulong)module.BaseAddress.ToInt64();
            ulong endAddress = address + (ulong)module.ModuleMemorySize;

            for (; address < endAddress; address++)
            {
                for (int i = 0; i < pattern.Length; i++)
                {
                    if (mask[i] != '?' && ((byte*)address)[i] != pattern[i])
                    {
                        break;
                    }
                    else if (i + 1 == pattern.Length)
                    {
                        return (byte*)address;
                    }
                }
            }
        }
                catch (Exception ex)
                {
                    Logger.Log($"{ex.Message}{ex.HResult}{ex.StackTrace}");
                }
            return null;
        }

        public enum eDecorType
        {
            Float = 1,
            Bool,
            Int,
            Unk,
            Time
        }
    }
}
