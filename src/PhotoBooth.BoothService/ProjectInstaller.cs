using System.ComponentModel;
using System.Configuration.Install;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;
using System.ServiceProcess;

namespace PhotoBooth.BoothService
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();
        }

        private void ProjectInstaller_AfterInstall(object sender, InstallEventArgs e)
        {
            string myAssembly = Path.GetFullPath(Context.Parameters["assemblypath"]);
            string logPath = Path.Combine(Path.GetDirectoryName(myAssembly), "logs");
            ReplacePermissions(logPath, WellKnownSidType.NetworkServiceSid, FileSystemRights.FullControl);

            new ServiceController(serviceInstaller1.ServiceName).Start();
        }
        static void ReplacePermissions(string filepath, WellKnownSidType sidType, FileSystemRights allow)
        {
            FileSecurity sec = File.GetAccessControl(filepath);
            SecurityIdentifier sid = new SecurityIdentifier(sidType, null);
            sec.PurgeAccessRules(sid); //remove existing
            sec.AddAccessRule(new FileSystemAccessRule(sid, allow, AccessControlType.Allow));
            File.SetAccessControl(filepath, sec);
        }
    }
}
