namespace FacturamaNetSDK.Models.Complements.EducationalInstitution
{
    public sealed class EducationalInstitutionComplement
    {
        /// <summary>
        /// Nombre del alumno
        /// </summary>
 
        public string StudentsName { get; set; }

        /// <summary>
        /// Clave única de registro de población del alumno
        /// </summary>        

        public string Curp { get; set; }

        /// <summary>
        /// Debe ser alguno de los siguientes:
        /// Preescolar|Primaria|Secundaria|Profesional técnico|Bachillerato o su equivalente
        /// </summary>
 
        public string EducationLevel { get; set; }

        /// <summary>
        /// Clave del centro de trabajo o el reconocimiento de validez oficial de esudios que tenga la instución educativa privada donde se realiza el pago
        /// </summary>

        public string AutRvoe { get; set; }

        /// <summary>
        /// RFC de quien realiza el pago cuando sea diferente a quien recibe el servicio (opcional)
        /// </summary>

        public string PaymentRfc { get; set; }
    }
}
