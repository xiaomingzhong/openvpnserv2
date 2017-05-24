﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration.Install;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;

namespace OpenVpn
{
    [RunInstaller(true)]
    public partial class ProjectInstaller : System.Configuration.Install.Installer
    {
        public ProjectInstaller()
        {
            InitializeComponent();

            EventLogInstaller installer = FindInstaller(this.Installers);

            if (installer != null)
            {
                installer.Log = OpenVpnService.DefaultServiceName;

                if (!EventLog.SourceExists(installer.Log))
                {
                    EventLog.CreateEventSource(installer.Log, "VPN");
                }
            }
        }

        private EventLogInstaller FindInstaller(InstallerCollection installers)
        {
            foreach (Installer installer in installers)
            {
                if (installer is EventLogInstaller)
                {
                    return (EventLogInstaller)installer;
                }

                EventLogInstaller eventLogInstaller = FindInstaller(installer.Installers);

                if (eventLogInstaller != null)
                {
                    return eventLogInstaller;
                }
            }

            return null;
        }


        // References http://stackoverflow.com/questions/1195478/how-to-make-a-net-windows-service-start-right-after-the-installation/1195621#1195621
        public static void Install()
        {
            using (AssemblyInstaller installer =
                    new AssemblyInstaller(typeof(OpenVpnService).Assembly, null))
            {
                installer.UseNewContext = true;
                var state = new System.Collections.Hashtable();
                try
                {
                    installer.Install(state);
                    installer.Commit(state);
                }
                catch (ArgumentException e)
                {
                }
                catch (Exception e)
                {
                    installer.Rollback(state);
                    throw;
                }
            }
        }

        public static void Uninstall()
        {
            using (AssemblyInstaller installer =
                    new AssemblyInstaller(typeof(OpenVpnService).Assembly, null))
            {
                installer.UseNewContext = true;
                var state = new System.Collections.Hashtable();
                try
                {
                    installer.Uninstall(state);
                }
                catch
                {
                    throw;
                }
            }
        }

        public static void Stop()
        {
            using (ServiceController controller =
                    new ServiceController(OpenVpnService.DefaultServiceName))
            {
                try
                {
                    if (controller.Status != ServiceControllerStatus.Stopped)
                    {
                        controller.Stop();
                        controller.WaitForStatus(ServiceControllerStatus.Stopped,
                            TimeSpan.FromSeconds(10));
                    }
                }
                catch
                {
                    throw;
                }
            }
        }

        public static void Start()
        {
            using (ServiceController controller =
                    new ServiceController(OpenVpnService.DefaultServiceName))
            {
                try
                {
                    if (controller.Status != ServiceControllerStatus.Running)
                    {
                        controller.Start();
                        controller.WaitForStatus(ServiceControllerStatus.Running,
                            TimeSpan.FromSeconds(10));
                    }
                }
                catch
                {
                    throw;
                }
            }
        }
    }
}
