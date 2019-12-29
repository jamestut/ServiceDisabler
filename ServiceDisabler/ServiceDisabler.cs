using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using Microsoft.Win32;
using System.Timers;

namespace ServiceDisabler
{
    public partial class ServiceDisabler : ServiceBase
    {
        private string[] ServiceNames { get; set; }
        private int CheckFrequency { get; set; }

        private Timer _MonitorTimer;

        public ServiceDisabler()
        {
            InitializeComponent();
        }

        protected override void OnStart(string[] args)
        {
            //Debugger.Launch();
            // open registry settings
            try
            {
                var regKey = Registry.LocalMachine.OpenSubKey($"SYSTEM\\CurrentControlSet\\Services\\{ServiceName}\\Config");
                if (regKey == null)
                {
                    RaiseError(0x80070002);
                    return;
                }
                var svcNames = (string)regKey.GetValue("ServiceNames");
                ServiceNames = svcNames.Split(';');
                CheckFrequency = (int)regKey.GetValue("CheckFrequency", 5);
            }
            catch(System.Security.SecurityException ex)
            {
                RaiseError(0x80070005);
                return;
            }
            catch(InvalidCastException ex)
            {
                RaiseError(0x80070013);
                return;
            }
            catch(Exception ex)
            {
                RaiseError(0x80004005);
                return;
            }

            // check if services are available
            try
            {
                for (int i = 0; i < ServiceNames.Length; ++i)
                {
                    var svcObj = new ServiceController(ServiceNames[i]);
                    // trigger service not found exception :)
                    var a = svcObj.Status;
                }
            }
            catch(Exception ex)
            {
                RaiseError(0x80070003);
                return;
            }

            // start the timer
            _MonitorTimer = new Timer();
            _MonitorTimer.Interval = CheckFrequency * 1000;
            _MonitorTimer.Elapsed += _MonitorTimer_Elapsed;
            _MonitorTimer.Start();
        }

        private void _MonitorTimer_Elapsed(object sender, ElapsedEventArgs e)
        {
            foreach(var svcName in ServiceNames)
            {
                var svcObj = new ServiceController(svcName);
                try
                {
                    switch (svcObj.Status)
                    {
                        case ServiceControllerStatus.Stopped:
                        case ServiceControllerStatus.StopPending:
                            continue;
                        default:
                            svcObj.Stop();
                            break;
                    }
                }
                catch (Exception ex) { continue; }
            }
        }

        protected override void OnStop()
        {
            _MonitorTimer.Stop();
        }

        private void RaiseError(uint code)
        {
            ExitCode = unchecked((int)code);
            Stop();
        }
    }
}
