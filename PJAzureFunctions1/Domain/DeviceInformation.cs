using System;
using System.Collections.Generic;
using System.Text;

namespace PJAzureFunctions1.Domain
{
    class DeviceInformation
    {
    }
    public class DeviceInstallation
    {
        public string InstallationId { get; set; }

        public string Platform { get; set; }

        public string PushChannel { get; set; }

    }
}
