using System.Diagnostics;

namespace Launch_Android_Stuido_Nicely
{
    public class Program
    {
        static void Main(string[] args)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.UseShellExecute = true;
            startInfo.FileName = "C:\\Users\\Sauraav\\AppData\\Roaming\\Microsoft\\Windows\\Start Menu\\Nox\\Nox.lnk";
            Process.Start(startInfo);
            System.Threading.Thread.Sleep(5000);
            startInfo.FileName = "C:\\ProgramData\\Microsoft\\Windows\\Start Menu\\Programs\\Android Studio\\Android Studio.lnk";
            Process.Start(startInfo);
            System.Threading.Thread.Sleep(15000);
            startInfo.FileName = "cmd.exe";
            startInfo.Arguments = "/C cd \"C:\\Program Files (x86)\\Nox\\bin\" && nox_adb.exe connect 127.0.0.1:62001";
            startInfo.UseShellExecute = false;
            startInfo.RedirectStandardOutput = true;
            startInfo.CreateNoWindow = true;
            Process cmdProcess = new Process();
            cmdProcess.StartInfo = startInfo;
            cmdProcess.Start();
        }
    }
}
