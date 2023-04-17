using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Newtonsoft.Json;
using System.Data.SqlClient;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.SystemTextJson.DefaultLambdaJsonSerializer))]

namespace VaccinationPrescribedLambda;

public class Function
{

    /// <summary>
    /// A simple function that takes a string and does a ToUpper
    /// </summary>
    /// <param name="input"></param>
    /// <param name="context"></param>
    /// <returns></returns>
    public async Task<APIGatewayProxyResponse> FunctionHandler(APIGatewayProxyRequest input, ILambdaContext context)
    {
        string jsonresponse = string.Empty;
        try
        {

            List<Vaccination> vacination = new List<Vaccination>();
            string id = string.Empty;
            input.PathParameters?.TryGetValue("id", out id);
            string query = $"select * from vacinationPrescribeds where PrescriptionId = {id}";
            string connectionString = "Server=clinic-mssql.cyhbv4dqbk22.us-east-1.rds.amazonaws.com,1433;User Id=admin;Password=#$Suadmin#$;Trusted_Connection=false; MultipleActiveResultSets=true;database=clinicDb;TrustServerCertificate=True";
            using (var conn = new SqlConnection(connectionString))
            {
                using (var cmd = new SqlCommand(query, conn))
                {
                    Console.WriteLine("Try connecting to RDS");
                    conn.Open();
                    Console.WriteLine("connection successfull!");
                    var rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        vacination.Add(new Vaccination
                        {
                            Id = Convert.ToInt32(rdr[0]),
                            VaccinationId = Convert.ToInt32(rdr[1]),
                            Dose = Convert.ToInt32(rdr[2]),
                            PrescriptionId = Convert.ToInt32(rdr[3]),
                            
                        });
                    }
                }
            }

            jsonresponse = JsonConvert.SerializeObject(vacination);

            return new APIGatewayProxyResponse
            {

                StatusCode = 200,
                Body = jsonresponse,
                Headers = { }
            };
        }
        catch (Exception ex)
        {
            jsonresponse = ex.Message;
            return new APIGatewayProxyResponse
            {

                StatusCode = 502,
                Body = jsonresponse,
                Headers = { }
            };


        }
    }
}


class Vaccination
{
    public int Id { get; set; }

    public int VaccinationId { get; set; }

    public double Dose { get; set; }

    public int PrescriptionId { get; set; }

}