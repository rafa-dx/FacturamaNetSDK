using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FacturamaNetSDK.Sandbox.Configuration
{
    public static class EnvironmentConfiguration
    {
        public static string Username = 
            Environment.GetEnvironmentVariable("FACTURAMA_USER") 
            ?? throw new InvalidOperationException("La variable de entorno FACTURAMA_USER no está configurada.");
        public static string Password = 
            Environment.GetEnvironmentVariable("FACTURAMA_PASS") 
            ?? throw new InvalidOperationException("La variable de entorno FACTURAMA_PASS no está configurada.");
    }
}
