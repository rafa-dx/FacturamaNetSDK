

using System.Runtime.CompilerServices;

namespace FacturamaNetSDK.Configuration
{
    public sealed class CircuitBreakerOptions
    {
        public bool Enabled { get; init; } = true;

        public int FailuresBeforeBreaking { get; init; } = 5;

        public TimeSpan BreakDuration { get; init; } = TimeSpan.FromSeconds(30);

        internal void Validate()
        {
            if (!Enabled)
            {
                return;
            }

            if(FailuresBeforeBreaking< 2)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(FailuresBeforeBreaking),
                    "Debe ser al menos 2. Con 1 fallo, cualquier error aislado abre el circuito.");
                    
            }
            if (BreakDuration <= TimeSpan.Zero)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(BreakDuration),
                    "Debe ser mayor a cero.");
            }
        }
    }
}
