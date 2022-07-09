using System.Runtime.Serialization;

namespace WebServicesEnrollment.Models
{
    [DataContract]
    public class EnrollmentRequest
    {
        [DataMember]
        public string NoExpediente { get ; set;}
        [DataMember]
        public string Ciclo {get; set;}
        [DataMember]
        public int MesInicioPago {get; set;}
        [DataMember]
        public string CarreraId {get; set;}
    }
}