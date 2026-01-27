using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using OGT.WatchTower.Core.Logging;

namespace OGT.WatchTower.Core.Prevention
{
    public static class ProcessManager
    {
        [DllImport("ntdll.dll")]
        private static extern int NtSuspendProcess(IntPtr processHandle);

        [DllImport("ntdll.dll")]
        private static extern int NtResumeProcess(IntPtr processHandle);

        public static bool SuspendProcess(int pid)
        {
            try
            {
                var process = Process.GetProcessById(pid);
                if (process == null || process.HasExited) return false;

                int result = NtSuspendProcess(process.Handle);
                if (result == 0) // STATUS_SUCCESS
                {
                    Logger.Instance.Log("ACTION", "Process Suspended", $"Suspended process {process.ProcessName} (PID: {pid})");
                    return true;
                }
                else
                {
                    Logger.Instance.Log("ERROR", "Suspend Failed", $"NtSuspendProcess returned {result}");
                    return false;
                }
            }
            catch (Exception ex)
            {
                Logger.Instance.Log("ERROR", "Suspend Failed", $"Failed to suspend process {pid}: {ex.Message}");
                return false;
            }
        }

        public static bool KillProcess(int pid)
        {
             try
            {
                var process = Process.GetProcessById(pid);
                if (process == null || process.HasExited) return false;

                process.Kill();
                Logger.Instance.Log("ACTION", "Process Killed", $"Killed process {process.ProcessName} (PID: {pid})");
                return true;
            }
            catch (Exception ex)
            {
                Logger.Instance.Log("ERROR", "Kill Failed", $"Failed to kill process {pid}: {ex.Message}");
                return false;
            }
        }
    }
}
