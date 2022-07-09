using System.ServiceModel;
using WebServicesEnrollment.Models;

namespace WebServicesEnrollment.Services
{
    [ServiceContract]
    
    public interface IEnrollmentService
    {
        [OperationContract]
        string Test(string s);
        [OperationContract]
        EnrollmentResponse EnrollmentProcess(EnrollmentRequest request);

        
    }
}