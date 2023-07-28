using System;
using System.Management;

namespace RemoteSystemInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            string ComputerName = Environment.MachineName; // Replace with the name or IP of the remote computer

            Console.WriteLine($"Device info for : {ComputerName}");

            try
            {
                // Connect to the remote computer's WMI service
                ConnectionOptions options = new ConnectionOptions();
                // Specify these details when connecting to a remote computer
                // But don't specify them for the local computer
                //options.Username = "USERNAME"; // Replace with a username with appropriate permissions on the remote computer
                //options.Password = "PASSWORD"; // Replace with the password for the provided username
                ManagementScope scope = new ManagementScope($"\\\\{ComputerName}\\root\\CIMV2", options);
                scope.Connect();

                GetCPUUsage(scope);

                GetMemoryUsage(scope);

                GetDiskUsage(scope);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        }

        private static void GetCPUUsage(ManagementScope scope)
        {
            // Get CPU usage
            ObjectQuery cpuQuery = new ObjectQuery("SELECT LoadPercentage FROM Win32_Processor");
            using (ManagementObjectSearcher cpuSearcher = new ManagementObjectSearcher(scope, cpuQuery))
            {
                foreach (ManagementObject obj in cpuSearcher.Get())
                {
                    Console.WriteLine($"CPU Usage: {obj["LoadPercentage"]}%");
                }
            }
        }

        private static void GetMemoryUsage(ManagementScope scope)
        {
            // Get memory usage
            ObjectQuery memoryQuery = new ObjectQuery("SELECT FreePhysicalMemory, TotalVisibleMemorySize FROM Win32_OperatingSystem");
            using (ManagementObjectSearcher memorySearcher = new ManagementObjectSearcher(scope, memoryQuery))
            {
                foreach (ManagementObject obj in memorySearcher.Get())
                {
                    ulong totalMemory = Convert.ToUInt64(obj["TotalVisibleMemorySize"]);
                    ulong freeMemory = Convert.ToUInt64(obj["FreePhysicalMemory"]);
                    ulong usedMemory = totalMemory - freeMemory;
                    Console.WriteLine($"Memory Usage: {usedMemory / (1024 * 1024)} MB / {totalMemory / (1024 * 1024)} MB");
                }
            }
        }

        private static void GetDiskUsage(ManagementScope scope)
        {
            // Get disk usage
            ObjectQuery diskQuery = new ObjectQuery("SELECT FreeSpace, Size, DeviceID FROM Win32_LogicalDisk WHERE DriveType = 3");
            using (ManagementObjectSearcher diskSearcher = new ManagementObjectSearcher(scope, diskQuery))
            {
                foreach (ManagementObject obj in diskSearcher.Get())
                {
                    ulong totalDiskSpace = Convert.ToUInt64(obj["Size"]);
                    ulong freeDiskSpace = Convert.ToUInt64(obj["FreeSpace"]);
                    string deviceID = obj["DeviceID"].ToString();
                    ulong usedDiskSpace = totalDiskSpace - freeDiskSpace;
                    Console.WriteLine($"Disk : {deviceID} Usage: {usedDiskSpace / (1024 * 1024 * 1024)} GB / {totalDiskSpace / (1024 * 1024 * 1024)} GB");
                }
            }
        }
    }
}
