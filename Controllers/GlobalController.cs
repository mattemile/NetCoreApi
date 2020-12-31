using System;
using System.Collections.Generic;
using System.Data;
using System.Net;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Google.Protobuf;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using TestApp.FWK;
using TestApp.Model;


namespace TestApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GlobalController : ControllerBase
    {
        public ConnectionDB Db { get; set; }

        #region Http
        [HttpPost]
        public IActionResult Post([FromBody] Object data)
        {
            var input = JObject.Parse(data.ToString().Replace(@"\", ""));
            JObject jbResult = new JObject();
            var token = JwtManager.GetPrincipal(input["token"].ToString());
            if (Global.isNullOrDbNullorEmpty(Global.db_name.ToString()))
                return Unauthorized();
            Db = new ConnectionDB();
            AnagraficaModel model = new AnagraficaModel(Db);
            if (token != null)
            {
                switch (input["action"].ToString().ToLower())
                {
                    case "getanagrafica":
                        {
                            var task = Task.Run(() => model.getAnagrafica());
                            task.Wait();

                            jbResult = task.Result;
                            return Global.setResult(jbResult);
                        }
                    case "getanagraficabyid":
                        {
                            var task = Task.Run(() => model.getAnagraficaById(Convert.ToInt32(input["id"])));
                            task.Wait();

                            jbResult = task.Result;
                            return Global.setResult(jbResult);
                        }
                    case "deleteanagraficabyid":
                        {
                            var task = Task.Run(() => model.deleteAnagrafica(Convert.ToInt32(input["id"])));
                            task.Wait();

                            jbResult = task.Result;
                            return Global.setResult(jbResult);
                        }
                    default:
                        break;
                }
            }
            else
            {
                return Unauthorized();
            }
            return NoContent();
        }

        [HttpPut]
        public IActionResult Put([FromBody] Object data)
        {
            var input = JObject.Parse(data.ToString().Replace(@"\", ""));
            var token = JwtManager.GetPrincipal(input["token"].ToString());
            JObject jbResult = new JObject();
            if (Global.isNullOrDbNullorEmpty(Global.db_name.ToString()))
                return Unauthorized();
            Db = new ConnectionDB();
            AnagraficaModel model = new AnagraficaModel(Db);
            if (input != null)
            {
                switch (input["action"].ToString().ToLower())
                {
                    case "setanagrafica":
                        {
                            var task = Task.Run(() => model.setAnagrafica(input["user"].ToString(),
                                                                         Convert.ToInt32(input["id_utente"])));
                            task.Wait();

                            jbResult = task.Result;
                            return Global.setResult(jbResult);
                        }
                    default:
                        break;
                }
            }
            return NoContent();
        }

        [HttpDelete]
        public IActionResult Delete([FromBody] Object data)
        {
            var input = JObject.Parse(data.ToString().Replace(@"\", ""));
            var token = JwtManager.GetPrincipal(input["token"].ToString());
            JObject jbResult = new JObject();
            if (Global.isNullOrDbNullorEmpty(Global.db_name.ToString()))
                return Unauthorized();
            Db = new ConnectionDB();
            AnagraficaModel model = new AnagraficaModel(Db);
            if (input != null)
            {
                switch (input["action"].ToString().ToLower())
                {
                    case "deleteanagraficabyid":
                        {
                            var task = Task.Run(() => model.deleteAnagrafica(Convert.ToInt32(input["id"])));
                            task.Wait();

                            jbResult = task.Result;
                            return Global.setResult(jbResult);
                        }
                    default:
                        break;
                }
            }
            return NoContent();
        }
        #endregion
    }
}
