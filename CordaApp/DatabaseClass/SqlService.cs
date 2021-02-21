using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CordaApp.DatabaseClass
{
    public class SqlService
    {
        private MySqlConnection Connection;
        SqlDatabase msDB;

        public bool AssetTransactionModify(string assetTransactionType, string uuserAssetTransactionOperation, JObject uuserAssetTransactionJSON)
        {
            msDB = new SqlDatabase();
            Connection = msDB.SqlConnection();
            string jsonData = JsonConvert.SerializeObject(uuserAssetTransactionJSON);
            string query = "call "+ assetTransactionType +"('" + uuserAssetTransactionOperation + "','" + jsonData + "')";
            MySqlCommand command = new MySqlCommand(query, Connection);
            command.ExecuteNonQuery();
            Console.WriteLine("OK");
            msDB.SqlClose();

            return true;
        }
        public JArray AssetTransactionGet(string uuserAssetTransactionOperation, JObject uuserAssetTransactionJSON)
        {
            msDB = new SqlDatabase();
            Connection = msDB.SqlConnection();
            JArray result = new JArray();
            string jsonData = JsonConvert.SerializeObject(uuserAssetTransactionJSON);
            string query = "call spc_AssetTransactionGet('" + uuserAssetTransactionOperation + "','" + jsonData + "')";
            MySqlCommand command = new MySqlCommand(query, Connection);
            using (MySqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    JObject jObject = new JObject();
                    jObject = (JObject)JsonConvert.DeserializeObject(reader["assettransactionJSON"].ToString());
                    result.Add(jObject);
                }
            }
            msDB.SqlClose();
            return result;
        }
    }
}
