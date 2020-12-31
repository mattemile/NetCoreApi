using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;

namespace TestApp.FWK
{
    public class Global
    {
        public static string username = string.Empty;
        public static string db_name = string.Empty;
        public static int idUtente = -1;

        public static bool isNullOrDbNull(object obj)
        {
            return (ReferenceEquals(obj, null)) | (ReferenceEquals(obj, DBNull.Value));
        }
        public static bool isNullOrDbNullorEmpty(object obj)
        {
            return (ReferenceEquals(obj, null)) || (ReferenceEquals(obj, DBNull.Value)) || (String.IsNullOrEmpty(obj.ToString()));
        }
        public static void setFilterQuery(ref string strSQLFilter, string strFilter, bool blnOnlyFilter)
        {
            if (!string.IsNullOrEmpty(strFilter))
            {
                if (blnOnlyFilter)
                {
                    strSQLFilter += (string.IsNullOrEmpty(strSQLFilter) ? "" : " AND ") + strFilter;
                }
                else
                {
                    strSQLFilter += " AND " + strFilter;
                }
            }
        }
        public static bool IsMailValid(string emailaddress)
        {
            try
            {
                MailAddress m = new MailAddress(emailaddress);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
        }

        public static bool IsNumeric(string s)
        {
            double output;
            return double.TryParse(s, out output);
        }
        public static bool IsDate(string s)
        {
            DateTime output;
            return DateTime.TryParse(s, out output);
        }

        public static MySql.Data.Types.MySqlDateTime convertToMysqlDate(string data)
        {
            DateTime wdata = Convert.ToDateTime(data);
            MySql.Data.Types.MySqlDateTime mysqlData = new MySql.Data.Types.MySqlDateTime();
            mysqlData.Year = wdata.Year;
            mysqlData.Month = wdata.Month;
            mysqlData.Day = wdata.Day;
            mysqlData.Hour = wdata.Hour;
            mysqlData.Minute = wdata.Minute;
            mysqlData.Second = wdata.Second;
            return mysqlData;
        }

        public static IActionResult setResult(JObject joRisultato)
        {
            try
            {
                var msgResponse = joRisultato.Property("response");
                var msgResult = joRisultato.Property("result");
                if (!Global.isNullOrDbNullorEmpty(msgResponse) && Convert.ToBoolean(msgResponse.Value))
                    return Global.setResult((int)HttpStatusCode.OK, joRisultato);
                else if (!Global.isNullOrDbNullorEmpty(msgResponse) && !Convert.ToBoolean(msgResponse.Value) && Global.isNullOrDbNullorEmpty(msgResult.Value))
                    return Global.setResult((int)HttpStatusCode.NoContent, string.Empty);
                else if (!Global.isNullOrDbNullorEmpty(msgResponse) && !Convert.ToBoolean(msgResponse.Value) && !Global.isNullOrDbNullorEmpty(msgResult.Value))
                    return Global.setResult((int)HttpStatusCode.BadRequest, joRisultato);
                else return Global.setResult((int)HttpStatusCode.NoContent, joRisultato);
            }
            catch (Exception ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
        }
        public static IActionResult setResult(int status, Object obj)
        {
            try
            {
                //var result = new ObjectResult(new { statusCode = status, message = obj.ToString()});
                //result.StatusCode = status;
                //return result;

                switch (status)
                {
                    case 200: //Ok()
                        return new OkObjectResult(obj.ToString());

                    case 201: //Created()

                    case 204: //NoContent()
                        return new NoContentResult();

                    case 400: //BadRequest()
                        {
                            var result = new ObjectResult(new { statusCode = status, message = obj.ToString() });
                            result.StatusCode = status;
                            return result;
                        }

                    case 401: //Unauthorized()
                        return new UnauthorizedObjectResult(obj.ToString());

                    case 403: //Forbid()
                        return new ForbidResult(obj.ToString());

                    case 404: //NotFound()
                        return new NotFoundObjectResult(obj.ToString());
                }
            }
            catch (Exception ex)
            {
                return new NotFoundObjectResult(ex.Message);
            }
            return new NoContentResult();
        }


        #region Enum Constants
        public enum eTicketTipologia
        {
            Bug = 1,
            Attivita = 2,
        }
        #endregion

        #region JSON
        public static string GetXmlIncludingNull(System.Data.DataSet dsDataset)
        {
            System.Data.DataSet dsDatasetAux = dsDataset.Copy();
            //CREO DT DI SUPPORTO PER SALVARMI I VALORI DEI CAMPI DOVE HO N RIGHE E IN ALCUNE RIGHE I CAMPI SONO VALORIZZATI ED IN ALTRE NO
            DataTable dtUpdate = new DataTable();
            dtUpdate.Columns.Add("columnName", typeof(string)); //NOME COLONNA
            dtUpdate.Columns.Add("tmp", typeof(int));          //IDENTIFICATICO
            dtUpdate.Columns.Add("value", typeof(string));     //VALORE

            List<System.Data.DataColumn> aColumnsToReplace = new List<System.Data.DataColumn>();

            foreach (System.Data.DataTable dtTable in dsDatasetAux.Tables)
            {
                dtUpdate.Rows.Clear();
                dtTable.Columns.Add("tmp", typeof(int));
                for (int i = 0; i < dtTable.Rows.Count; i++)
                    dtTable.Rows[i]["tmp"] = i;

                aColumnsToReplace = new List<System.Data.DataColumn>();
                if (dtTable.Rows.Count > 0)
                {
                    int num_righe = dtTable.Rows.Count;
                    foreach (System.Data.DataColumn oColumn in dtTable.Columns)
                    {
                        //check if none of the the rows has a value for the column
                        if (oColumn.ColumnName.ToString() != "tmp")
                        {
                            //SE IN TUTTE LE RIGHE QUELLA COLONNA è NULL
                            if (dtTable.Select(string.Format("{0} is not null", oColumn.ColumnName)).Length == 0)
                            {
                                if ((!object.ReferenceEquals(oColumn.DataType, typeof(string))))
                                {
                                    aColumnsToReplace.Add(oColumn);
                                }
                                else
                                {
                                    foreach (DataRow dr in dtTable.Rows)
                                        dr[oColumn] = string.Empty;
                                }
                            }
                            //SE IN ALMENO UNA RIGA LA COLONNA è VALORIZZATA
                            else if (dtTable.Select(string.Format("{0} is not null", oColumn.ColumnName)).Length > 0 &&
                                    dtTable.Select(string.Format("{0} is not null", oColumn.ColumnName)).Length < num_righe)
                            {
                                aColumnsToReplace.Add(oColumn);

                                //MI SALVO IL VALORE
                                DataRow[] drColonneNull = dtTable.Select(string.Format("{0} is not null", oColumn.ColumnName));
                                foreach (DataRow drNull in drColonneNull)
                                {
                                    DataRow drUpdate = dtUpdate.NewRow();
                                    drUpdate["columnName"] = oColumn.ColumnName;
                                    drUpdate["tmp"] = drNull["tmp"];
                                    drUpdate["value"] = drNull[oColumn.ColumnName];
                                    dtUpdate.Rows.Add(drUpdate);
                                }
                            }
                        }
                    }
                    foreach (System.Data.DataColumn oColumn in aColumnsToReplace)
                    {
                        dtTable.Columns.Remove(oColumn);
                        dtTable.Columns.Add(oColumn.ColumnName, typeof(string)).DefaultValue = string.Empty;
                        //RIPOPOLO LA COLONNA CANCELLATA CON IL VALORE INIZIALE
                        foreach (DataRow dr in dtTable.Rows)
                        {
                            if (dtUpdate.Rows.Count > 0 && dtUpdate.Select(" columnName = '" + oColumn.ColumnName + "' AND tmp = " + dr["tmp"]).Length > 0)
                                dr[oColumn.ColumnName] = dtUpdate.Select(" columnName = '" + oColumn.ColumnName + "' AND tmp = " + dr["tmp"])[0]["value"].ToString();
                            else dr[oColumn.ColumnName] = string.Empty;
                        }
                    }
                    dtTable.Columns.Remove("tmp");
                }
            }
            dsDatasetAux.AcceptChanges();

            return dsDatasetAux.GetXml();
        }
        #endregion
    }
}
