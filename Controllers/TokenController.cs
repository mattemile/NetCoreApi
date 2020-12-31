using System;
using System.Data;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using Newtonsoft.Json.Linq;
using TestApp.FWK;

namespace TestApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TokenController : ControllerBase
    {
        public ConnectionDB Db { get; set; }

        public TokenController()
        {
        }

        [HttpPost]
        public IActionResult Post([FromBody] Object data)
        {
            var input = JObject.Parse(data.ToString().Replace(@"\", ""));

            Db = new ConnectionDB();
            try
            {
                if (!Global.isNullOrDbNull(Db.Connection) && !Global.isNullOrDbNullorEmpty(Db.Connection.ToString()))
                {
                    var task = Task.Run(() => CheckUser(input["email"].ToString(), input["password"].ToString()));
                    task.Wait();
                    DataTable dtResult = task.Result;

                    if (dtResult.Rows.Count > 0)
                    {
                        JObject jbResult = new JObject();
                        jbResult.Add("jwt", JwtManager.GenerateToken(dtResult.Rows[0]["email"].ToString(),
                                                                    dtResult.Rows[0]["id_utente"].ToString(),
                                                                    Db.Connection.Database.ToString()));
                        jbResult.Add("email", dtResult.Rows[0]["email"].ToString());
                        jbResult.Add("id_utente", dtResult.Rows[0]["id_utente"].ToString());
                        jbResult.Add("nominativo", (dtResult.Rows[0]["nome"].ToString() + " " + dtResult.Rows[0]["cognome"].ToString().ToString()));
                        jbResult.Add("response", true);
                        return Global.setResult((int)HttpStatusCode.OK, jbResult);
                    }
                    else
                    {
                        JObject result = new JObject();
                        result.Add("response", false);
                        result.Add("result", "Nome utente o password errati!");
                        return Global.setResult((int)HttpStatusCode.Unauthorized, result);
                    }
                }
                else
                {
                    return Global.setResult((int)HttpStatusCode.NoContent, null);
                }
            }
            catch (Exception Ex)
            {
                JObject result = new JObject();
                result.Add("response", false);
                result.Add("result", Ex.Message.ToString());
                return Global.setResult((int)HttpStatusCode.NoContent, result);
            }
        }

        private async Task<DataTable> CheckUser(string username, string password)
        {
            await Db.Connection.OpenAsync();
            using var cmd = Db.Connection.CreateCommand();
            DataTable dt = new DataTable();

            try
            {
                string strQuery = "SELECT * FROM utente WHERE email = '" + username + "' and password ='" + sha256_hash(password) + "'";
                cmd.CommandText = @strQuery;
                var dataReader = cmd.ExecuteReader();
                dt.Load(dataReader);
            }
            catch (MySqlException e)
            {
                throw;
            }
            finally
            {
                cmd.Connection.Dispose();
                cmd.Connection.Close();
                await Db.Connection.ClearAllPoolsAsync();
                await Db.Connection.DisposeAsync();
                await Db.Connection.CloseAsync();
            }
            return dt;
        }

        private static String sha256_hash(String value)
        {
            StringBuilder Sb = new StringBuilder();

            using (SHA256 hash = SHA256Managed.Create())
            {
                Encoding enc = Encoding.UTF8;
                Byte[] result = hash.ComputeHash(enc.GetBytes(value));

                foreach (Byte b in result)
                    Sb.Append(b.ToString("x2"));
            }

            return Sb.ToString();
        }
    }
}
