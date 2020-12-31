using System.Data;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using MySql.Data.MySqlClient;
using TestApp.FWK;

namespace TestApp.AppData
{
    public class Base
    {
        internal ConnectionDB Db { get; set; }

        public Base()
        {
        }

        internal Base(ConnectionDB db)
        {
            Db = db;
        }

        public async Task<int> FnUpdateAsync(MySqlTransaction iTransaction, MySqlDataAdapter daQuery, DataTable dtTabella, string strTabella, string strPK, int intIdUtente)
        {
            using var cmd = Db.Connection.CreateCommand();
            DataTable dtQuery = new DataTable();
            int intId = -1;
            try
            {
                if (dtTabella.Rows.Count > 0)
                {
                    string strQuery = " SELECT   * " +
                                      " FROM     " + strTabella +
                                      " WHERE    " + strPK + " = " + dtTabella.Rows[0][strPK].ToString();
                    cmd.CommandText = @strQuery;
                    await cmd.ExecuteNonQueryAsync();
                    daQuery = new MySqlDataAdapter(cmd);
                    daQuery.SelectCommand.Transaction = iTransaction;
                    daQuery.SelectCommand = new MySqlCommand(strQuery, Db.Connection);
                    MySqlCommandBuilder objCB = new MySqlCommandBuilder(daQuery);
                    daQuery.Fill(dtQuery);

                    setDa(ref daQuery, strTabella);

                    bool blnNewRecord = (dtQuery.Rows.Count == 0);
                    if (dtQuery.Rows.Count == 0)
                        dtQuery.Rows.Add(dtQuery.NewRow());

                    string strNomiColonne = "";
                    for (int c = 0; c < dtTabella.Columns.Count; c++)
                        strNomiColonne += (strNomiColonne.Length == 0 ? "" : ",") + "[" + dtTabella.Columns[c].ColumnName + "]";

                    for (int c = 0; c < dtQuery.Columns.Count; c++)
                    {
                        if (strNomiColonne.Contains("[" + dtQuery.Columns[c].ColumnName + "]") &&
                            dtTabella.Columns[dtQuery.Columns[c].ColumnName].ColumnName.ToLower() != strPK &&
                            dtTabella.Rows[0][dtQuery.Columns[c].ColumnName] != DBNull.Value &&
                            dtTabella.Rows[0][dtQuery.Columns[c].ColumnName].ToString() != "")
                        {
                            dtQuery.Rows[0][c] = dtTabella.Rows[0][dtQuery.Columns[c].ColumnName];
                        }
                        else if (strNomiColonne.Contains("[" + dtQuery.Columns[c].ColumnName + "]") &&
                            dtTabella.Columns[dtQuery.Columns[c].ColumnName].ColumnName.ToLower() != strPK &&
                            (dtTabella.Rows[0][dtQuery.Columns[c].ColumnName] == DBNull.Value ||
                            dtTabella.Rows[0][dtQuery.Columns[c].ColumnName].ToString() == ""))
                            dtQuery.Rows[0][c] = DBNull.Value;
                    }
                    daQuery.Update(dtQuery);

                    if (blnNewRecord)
                    {
                        DataTable dt = new DataTable();
                        cmd.CommandText = @"SELECT LAST_INSERT_ID() as id; ";
                        MySqlDataAdapter da = new MySqlDataAdapter(cmd);
                        da.Fill(dt);
                        if (dt.Rows.Count > 0)
                            intId = Convert.ToInt32(dt.Rows[0]["id"]);
                    }
                    else
                        intId = Convert.ToInt32(dtQuery.Rows[0][strPK]);
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                cmd.Dispose();
                //cmd.Connection.Dispose();
                //cmd.Connection.Close();
                //Db.Connection.Dispose();
                //Db.Connection.Close();
            }
            return intId;
        }

        public async Task<DataTable> FnUpdateDettaglioAsync(MySqlTransaction iTransaction, MySqlDataAdapter daQuery, DataTable dtTabella, string strTabella, string strPK, int intIdUtente)
        {
            //await Db.Connection.OpenAsync();
            var cmd = Db.Connection.CreateCommand();
            DataTable dtQuery = new DataTable();
            DataTable dtQueryResult = new DataTable();
            string strQuery = string.Empty;
            // MySqlDataAdapter daQuery;
            MySqlCommandBuilder objCB;
            try
            {
                if (dtTabella.Rows.Count > 0)
                {
                    foreach (DataRow dr in dtTabella.Rows)
                    {
                        cmd = Db.Connection.CreateCommand();
                        dtQuery = new DataTable();
                        strQuery = " SELECT   * " +
                                          " FROM     " + strTabella +
                                          " WHERE    " + strPK + " = " + dr[strPK].ToString();
                        cmd.CommandText = @strQuery;
                        await cmd.ExecuteNonQueryAsync();
                        daQuery = new MySqlDataAdapter(cmd);
                        daQuery.SelectCommand.Transaction = iTransaction;
                        objCB = new MySqlCommandBuilder(daQuery);
                        daQuery.Fill(dtQuery);

                        bool blnNewRecord = (dtQuery.Rows.Count == 0);
                        if (dtQuery.Rows.Count == 0)
                            dtQuery.Rows.Add(dtQuery.NewRow());

                        string strNomiColonne = "";
                        for (int c = 0; c < dtTabella.Columns.Count; c++)
                            strNomiColonne += (strNomiColonne.Length == 0 ? "" : ",") + "[" + dtTabella.Columns[c].ColumnName + "]";

                        for (int c = 0; c < dtQuery.Columns.Count; c++)
                        {
                            if (strNomiColonne.Contains("[" + dtQuery.Columns[c].ColumnName + "]") &&
                                dtTabella.Columns[dtQuery.Columns[c].ColumnName].ColumnName.ToLower() != strPK &&
                                dr[dtQuery.Columns[c].ColumnName] != DBNull.Value &&
                                dr[dtQuery.Columns[c].ColumnName].ToString() != "")
                            {
                                dtQuery.Rows[0][c] = dr[dtQuery.Columns[c].ColumnName];
                            }
                            else if (strNomiColonne.Contains("[" + dtQuery.Columns[c].ColumnName + "]") &&
                                dtTabella.Columns[dtQuery.Columns[c].ColumnName].ColumnName.ToLower() != strPK &&
                                (dr[dtQuery.Columns[c].ColumnName] == DBNull.Value ||
                                dr[dtQuery.Columns[c].ColumnName].ToString() == ""))
                                dtQuery.Rows[0][c] = DBNull.Value;
                        }

                        daQuery.Update(dtQuery);

                        cmd.Dispose();
                    }
                }
            }
            catch (Exception ex)
            {
                throw;
            }
            finally
            {
                cmd.Dispose();
                //cmd.Connection.Dispose();
                //cmd.Connection.Close();
                //Db.Connection.Dispose();
                //Db.Connection.Close();
            }
            return dtQuery;
        }

        private void setDa(ref MySqlDataAdapter da, string table_name)
        {
            MySqlCommandBuilder objCB = new MySqlCommandBuilder(da);

            ////NOME DELLA TABELLA
            //string strSQL = da.SelectCommand.CommandText.ToString().ToLower().Replace("from", "§").Replace("where", "§").Replace("order by", "§");
            //string strTable = strSQL.Substring(strSQL.IndexOf('§') + 1).Trim();
            //if (strTable.IndexOf('§') > 0)
            //    strTable = strTable.Substring(0, strTable.IndexOf('§') - 1).Trim();

            //PRIMARY KEY DELLA TABELLA
            string strKeyColumn = "";
            DataTable dtKey = new DataTable();
            var daKey = new MySqlDataAdapter("SHOW KEYS FROM " + table_name + " WHERE Key_name = 'PRIMARY' ", da.SelectCommand.Connection);
            daKey.Fill(dtKey);
            strKeyColumn = dtKey.Rows[0]["column_name"].ToString();

            #region InsertCommand
            MySqlCommand cmdInsert = new MySqlCommand(objCB.GetInsertCommand().CommandText, Db.Connection);
            cmdInsert.CommandText += "; SELECT @@Identity AS " + strKeyColumn;

            MySqlParameter[] iParams = new MySqlParameter[objCB.GetInsertCommand().Parameters.Count];
            objCB.GetInsertCommand().Parameters.CopyTo(iParams, 0);
            objCB.GetInsertCommand().Parameters.Clear();

            for (int i = 0; i < iParams.Length; i++) { cmdInsert.Parameters.Add(iParams[i]); }

            da.InsertCommand = cmdInsert;
            da.InsertCommand.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;
            #endregion

            #region UpdateCommand
            /* non funziona, non salva il datatable cancella tutte le righe */
            //MySqlCommand cmdUpdate = new MySqlCommand(objCB.GetUpdateCommand().CommandText, Conn);
            //cmdUpdate.CommandText += "; SELECT @@Identity AS " + strKeyColumn;
            //
            //iParams = new MySqlParameter[objCB.GetUpdateCommand().Parameters.Count];
            //objCB.GetUpdateCommand().Parameters.CopyTo(iParams, 0);
            //objCB.GetUpdateCommand().Parameters.Clear();
            //
            //for (int i = 0; i < iParams.Length; i++) { cmdUpdate.Parameters.Add(iParams[i]); }
            //
            //da.UpdateCommand = cmdUpdate;
            //da.UpdateCommand.UpdatedRowSource = UpdateRowSource.FirstReturnedRecord;
            #endregion
        }

    }
}
