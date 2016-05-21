using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Collections.Generic;

namespace PwrSwitch
{
    // I did not write some of this class
    // so credits go to http://stackoverflow.com/questions/26289533/get-windows-power-plans-schemes-in-c-sharp-using-winapi
    // for about 2/3 of it
    public class PwrList
    {
        // Direct imports
        [DllImport("PowrProf.dll")]
        public static extern UInt32 PowerEnumerate(IntPtr RootPowerKey, IntPtr SchemeGuid, IntPtr SubGroupOfPowerSettingGuid, UInt32 AcessFlags, UInt32 Index, ref Guid Buffer, ref UInt32 BufferSize);

        [DllImport("PowrProf.dll")]
        public static extern UInt32 PowerReadFriendlyName(IntPtr RootPowerKey, ref Guid SchemeGuid, IntPtr SubGroupOfPowerSettingGuid, IntPtr PowerSettingGuid, IntPtr Buffer, ref UInt32 BufferSize);

        [DllImport("PowrProf.dll")]
        public static extern UInt32 PowerSetActiveScheme(IntPtr RootPowerKey, ref Guid SchemeGuid);

        [DllImport("PowrProf.dll")]
        public static extern UInt32 PowerGetActiveScheme(IntPtr RootPowerKey, ref IntPtr ActivePolicyGuid);

        // Wrapper around imports
        public enum AccessFlags : uint
        {
            ACCESS_SCHEME = 16,
            ACCESS_SUBGROUP = 17,
            ACCESS_INDIVIDUAL_SETTING = 18
        }

        private static string ReadFriendlyName(Guid schemeGuid)
        {
            // Max power plan size name
            uint sizeName = 1024;

            // allocate memory
            IntPtr pSizeName = Marshal.AllocHGlobal((int)sizeName);

            string friendlyName;

            try
            {
                // call dll function
                PowerReadFriendlyName(IntPtr.Zero, ref schemeGuid, IntPtr.Zero, IntPtr.Zero, pSizeName, ref sizeName);
                friendlyName = Marshal.PtrToStringUni(pSizeName);
            }
            finally
            {
                Marshal.FreeHGlobal(pSizeName);
            }

            return friendlyName;
        }

        private static UInt32 setPlan(Guid id)
        {
            // call dll function
            return PowerSetActiveScheme(IntPtr.Zero, ref id);
        }

        private static Guid getActivePlan()
        {
            // credits to http://www.pinvoke.net/default.aspx/powrprof.powergetactivescheme for showing that I need a ref IntPtr and not Guid
            // activeGuid starts out as NULL
            IntPtr activeGuid = IntPtr.Zero;
            UInt32 resp = PowerGetActiveScheme(IntPtr.Zero, ref activeGuid);

            // If there was an error message, print it out!
            if (resp != 0)
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                Console.WriteLine(errorMessage);
            }

            // also credits to https://msdn.microsoft.com/en-us/library/2zhzfk83%28v=vs.100%29.aspx?f=255&MSPPError=-2147217396
            // for showing how to use Marshal.PtrToStructure (never thought i'd be thanking msdn lol)
            return (Guid)Marshal.PtrToStructure(activeGuid, typeof(Guid));
        }

        public static IEnumerable<Guid> GetAll()
        {
            var schemeGuid = Guid.Empty;

            uint sizeSchemeGuid = (uint)Marshal.SizeOf(typeof(Guid));
            uint schemeIndex = 0;

            while (PowerEnumerate(IntPtr.Zero, IntPtr.Zero, IntPtr.Zero, (uint)AccessFlags.ACCESS_SCHEME, schemeIndex, ref schemeGuid, ref sizeSchemeGuid) == 0)
            {
                yield return schemeGuid;
                schemeIndex++;
            }
        }

        // Nicer ways of interfacing
        public static void printPlans()
        {
            var guidPlans = GetAll();

            foreach (Guid guidPlan in guidPlans)
            {
                Console.WriteLine(ReadFriendlyName(guidPlan));
            }
        }

        public static List<PowerPlan> getPlansList()
        {
            var guidPlans = GetAll();

            List<PowerPlan> ret = new List<PowerPlan>();

            foreach (Guid guidPlan in guidPlans)
            {
                ret.Add(new PowerPlan(guidPlan, ReadFriendlyName(guidPlan)));
            }

            return ret;
        }

        public static bool setPlan(PowerPlan p)
        {
            UInt32 result = setPlan(p.guid);
            if (result == 0)
            {
                return true;
            } else
            {
                string errorMessage = new Win32Exception(Marshal.GetLastWin32Error()).Message;
                Console.WriteLine(errorMessage);
                return false;
            }
        }

        public static PowerPlan getCurrActivePlan()
        {
            Guid planGuid = getActivePlan();
            PowerPlan p = new PowerPlan(planGuid, ReadFriendlyName(planGuid));
            return p;
        }
    }
}