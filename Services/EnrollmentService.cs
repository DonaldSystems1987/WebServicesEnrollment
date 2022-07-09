using WebServicesEnrollment.Models;
using System.Data.SqlClient; //Libreria Descargada que se ingreso en consola con siguiente comando "dotnet add package System.Data.SqlClient --version 4.8.3"
using System.Data;//Libreria para utilizar libreria DataSet
using System.Text.Json;
using System.Runtime.Serialization.Json;
using Serilog;

namespace WebServicesEnrollment.Services
{
     public class EnrollmentService : IEnrollmentService
    {
        private SqlConnection connection = new SqlConnection("Server=localhost;Database=kalum_test;User Id=sa;Password=Progra.2022;");//1. Crear cadena de conexion hacia nuestro servicios
        
        private AppLog AppLog = new AppLog();

        public EnrollmentService()
        {
            
        }


        public EnrollmentResponse EnrollmentProcess(EnrollmentRequest request)
        {
            AppLog.ResponseTime = Convert.ToInt16(DateTime.Now.ToString("fff"));//Registrando tiempo inicial de la transaccion
            AppLog.DateTime = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            EnrollmentResponse respuesta = null;
            Aspirante aspirante = buscarAspirante(request.NoExpediente);

           // Console.WriteLine("{0} {1}",aspirante.Email, aspirante.Estatus); //Validacion de la conexion debe imprimir en consola el correo

            if( aspirante == null)
            {
                respuesta = new EnrollmentResponse() {Codigo = 204, Respuesta = "No existen registros"};
                ImprimirLog(204, $"No existen registros para el numero de expediente {request.NoExpediente}","Information");
            }
            else
            {
                respuesta = EjecutarProcedimiento(request);
            }

            return respuesta;
        }
        
        private EnrollmentResponse EjecutarProcedimiento(EnrollmentRequest request)//llamar procedimiento almacenado
        {
            EnrollmentResponse response = null;
            SqlCommand cmd = new SqlCommand("sp_EnrollmentProcess",connection); //copiamos el nombre de procedimiento almacenado de nuestra bd kalum_test el cual es ""
            cmd.CommandType = CommandType.StoredProcedure; //Indicando que ejecutaremos un procedimiento almacenado
            cmd.Parameters.Add(new SqlParameter("@NoExpediente",request.NoExpediente));
            cmd.Parameters.Add(new SqlParameter("@Ciclo",request.Ciclo));
            cmd.Parameters.Add(new SqlParameter("@MesInicioPago",request.MesInicioPago));
            cmd.Parameters.Add(new SqlParameter("@CarreraId",request.CarreraId));
            SqlDataReader reader = null;

            try
            {
                connection.Open();
                reader = cmd.ExecuteReader();
                while(reader.Read())
                {
                    response = new EnrollmentResponse(){Respuesta = reader.GetValue(0).ToString(), Carne = reader.GetValue(1).ToString()};
                    if(reader.GetValue(0).ToString().Equals("TRANSACTION SUCCESS"))
                    {
                        response.Codigo = 201;
                        ImprimirLog(201, reader.GetValue(0).ToString(), "Information");
                    }
                    else if(reader.GetValue(0).ToString().Equals("TRANSACTION ERROR"))
                    {
                        response.Codigo = 503;
                        ImprimirLog(503, reader.GetValue(0).ToString(), "Error");
                    }
                    else
                    {
                        response.Codigo = 503;
                        ImprimirLog(503,"Error al momento de llamar al procedimiento almacenado", "Error");
                    }
                }
                reader.Close();
                connection.Close();
            }
            catch(Exception e)
            {
                response = new EnrollmentResponse(){Codigo = 503 , Respuesta = "Error al momento de ejecutar el proceso de Inscripcion", Carne = "0"};
                ImprimirLog(503,"Error al momento de ejecutar el proceso de Inscripcion", "Error");

             }
             finally
             {
                 connection.Close();
             }  
            return response;
        }

        //Metodo para mandar a imprimir nuestros logs
        private void ImprimirLog(int responseCode, string message, string typeLog)
        {
            AppLog.ResponseCode = responseCode;
            AppLog.Message = message;
            AppLog.ResponseTime = Convert.ToInt16(DateTime.Now.ToString("fff")) - AppLog.ResponseTime; //Almacena cuanto tiempo se llevo la transaccion en ejecutarse
            if(typeLog.Equals("Information"))
            {
                AppLog.Level = 20;
                Log.Information(JsonSerializer.Serialize(AppLog));
            }
            else if(typeLog.Equals("Error"))
            {
                AppLog.Level = 40;
                Log.Error(JsonSerializer.Serialize(AppLog));        
            }
            else if(typeLog.Equals("Debug"))
            {
                AppLog.Level = 10;
                Log.Debug(JsonSerializer.Serialize(AppLog));
            }
        }

        private Aspirante buscarAspirante(string noExpediente)
        {
            Aspirante resultado = null;
            SqlDataAdapter daAspirante = new SqlDataAdapter($"select * from Aspirante a  where a.NoExpediente = '{noExpediente}'",connection);
            DataSet dsAspirante = new DataSet();
            daAspirante.Fill(dsAspirante,"Aspirante");
            if(dsAspirante.Tables["Aspirante"].Rows.Count > 0)
            {
                resultado = new Aspirante() 
                {
                    NoExpediente = dsAspirante.Tables["Aspirante"].Rows[0][0].ToString(),
                    Apellidos = dsAspirante.Tables["Aspirante"].Rows[0][1].ToString(),
                    Nombres = dsAspirante.Tables["Aspirante"].Rows[0][2].ToString(),
                    Direccion = dsAspirante.Tables["Aspirante"].Rows[0][3].ToString(),
                    Telefono = dsAspirante.Tables["Aspirante"].Rows[0][4].ToString(),
                    Email = dsAspirante.Tables["Aspirante"].Rows[0][5].ToString(),
                    Estatus = dsAspirante.Tables["Aspirante"].Rows[0][6].ToString(),
                    CarreraId = dsAspirante.Tables["Aspirante"].Rows[0][7].ToString(),
                    JornadaId = dsAspirante.Tables["Aspirante"].Rows[0][8].ToString()
                };
            }
            return resultado;
        }

        public string Test(string s)
        {
            Console.WriteLine("Test method executed");
            return s;
        }
    }
}