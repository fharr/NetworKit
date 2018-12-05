using System;
using System.Collections.Generic;
using System.Text;

namespace NetworKit
{
    public class ConnectionStatus
    {
        public bool ConnectionGranted { get; }
        public string Status { get; }

        public ConnectionStatus(bool connectionGranted, string status)
        {
            this.ConnectionGranted = connectionGranted;
            this.Status = status;
        }
    }
}
