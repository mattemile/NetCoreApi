using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using TestApp.AppData;

namespace TestApp.Model
{
    public class AnagraficaModel
    {
        Base query;
        public ConnectionDB Db { get; }

        public AnagraficaModel(ConnectionDB db)
        {
            Db = db;
            query = new Base(db);
        }

        public async Task<JObject> getAnagrafica()
        {
            await Db.Connection.OpenAsync();
            using var cmd = Db.Connection.CreateCommand();
            JObject jbResult = new JObject();
            DataTable dtTabella = new DataTable();

            try
            {
                String strQuery = " SELECT  * " +
                                  " FROM    anagrafica ";

                cmd.CommandText = @strQuery;
                var dataReader = cmd.ExecuteReader();
                dtTabella.Load(dataReader);

                if (dtTabella.Rows.Count > 0)
                {
                    DataSet dsTabella = new DataSet();
                    dsTabella.Tables.Add(dtTabella);

                    string json = JsonConvert.SerializeObject(dsTabella, Formatting.Indented);
                    jbResult.Add("result", JObject.Parse(json));
                    jbResult.Add("response", true);
                }
                else
                {
                    jbResult.Add("result", "Nessun elemento trovato.");
                    jbResult.Add("response", false);
                }
            }
            catch (Exception ex)
            {
                jbResult.Add("response", false);
                jbResult.Add("result", ex.StackTrace.ToString() + "\r\n" + ex.Message.ToString());
            }
            finally
            {
                cmd.Connection.Dispose();
                cmd.Connection.Close();
                Db.Connection.Dispose();
                Db.Connection.Close();
            }
            return jbResult;
        }
        public async Task<JObject> getAnagraficaById(int id)
        {
            await Db.Connection.OpenAsync();
            using var cmd = Db.Connection.CreateCommand();
            JObject jbResult = new JObject();
            DataTable dtTabella = new DataTable();

            try
            {
                String strQuery = " SELECT  * " +
                                  " FROM    anagrafica "+
                                  " WHERE id = " + id;

                cmd.CommandText = @strQuery;
                var dataReader = cmd.ExecuteReader();
                dtTabella.Load(dataReader);

                if (dtTabella.Rows.Count > 0)
                {
                    DataSet dsTabella = new DataSet();
                    dsTabella.Tables.Add(dtTabella);

                    string json = JsonConvert.SerializeObject(dsTabella, Formatting.Indented);
                    jbResult.Add("result", JObject.Parse(json));
                    jbResult.Add("response", true);
                }
                else
                {
                    jbResult.Add("result", "Nessun elemento trovato.");
                    jbResult.Add("response", false);
                }
            }
            catch (Exception ex)
            {
                jbResult.Add("response", false);
                jbResult.Add("result", ex.StackTrace.ToString() + "\r\n" + ex.Message.ToString());
            }
            finally
            {
                cmd.Connection.Dispose();
                cmd.Connection.Close();
                Db.Connection.Dispose();
                Db.Connection.Close();
            }
            return jbResult;
        }
        public async Task<JObject> deleteAnagrafica(int id)
        {
            await Db.Connection.OpenAsync();
            using var cmd = Db.Connection.CreateCommand();
            JObject jbResult = new JObject();
            DataTable dtTabella = new DataTable();

            try
            {
                String strQuery = " UPDATE  " +
                                  " FROM    anagrafica " +
                                  " SET validita = 1 " +
                                  " WHERE id = " + id;

                cmd.CommandText = @strQuery;
                var dataReader = cmd.ExecuteReader();
                dtTabella.Load(dataReader);

                if (dtTabella.Rows.Count > 0)
                {
                    DataSet dsTabella = new DataSet();
                    dsTabella.Tables.Add(dtTabella);

                    string json = JsonConvert.SerializeObject(dsTabella, Formatting.Indented);
                    jbResult.Add("result", JObject.Parse(json));
                    jbResult.Add("response", true);
                }
                else
                {
                    jbResult.Add("result", "Nessun elemento trovato.");
                    jbResult.Add("response", false);
                }
            }
            catch (Exception ex)
            {
                jbResult.Add("response", false);
                jbResult.Add("result", ex.StackTrace.ToString() + "\r\n" + ex.Message.ToString());
            }
            finally
            {
                cmd.Connection.Dispose();
                cmd.Connection.Close();
                Db.Connection.Dispose();
                Db.Connection.Close();
            }
            return jbResult;
        }

        public async Task<JObject> setAnagrafica(string jsonData, int intIdUtente)
        {
            int intIdTabella = -1;
            JObject jbResult = new JObject();
            await Db.Connection.OpenAsync();
            MySqlTransaction iTransaction = null;
            iTransaction = Db.Connection.BeginTransaction();
            try
            {
                MySqlDataAdapter daTabella = new MySqlDataAdapter();
                DataTable dtTabella = (DataTable)JsonConvert.DeserializeObject("["+jsonData+"]", (typeof(DataTable)));
                intIdTabella = await query.FnUpdateAsync(iTransaction, daTabella, dtTabella, "anagrafica", "id", intIdUtente);

                iTransaction.Commit();
                iTransaction.Dispose();

                jbResult.Add("response", true);
                jbResult.Add("result", intIdTabella);
            }
            catch (Exception ex)
            {
                iTransaction.Dispose();
                jbResult.Add("response", false);
                jbResult.Add("result", ex.StackTrace.ToString() + "\r\n" + ex.Message.ToString());
            }
            finally
            {
                await Db.Connection.ClearAllPoolsAsync();
                await Db.Connection.DisposeAsync();
                await Db.Connection.CloseAsync();
            }
            return jbResult;
        }
    }
}
