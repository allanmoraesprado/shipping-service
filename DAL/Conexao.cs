using Dapper;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;

namespace DAL
{

    public class Conexao
    {
        SqlConnection con = new SqlConnection();
        private int timeOut = 0;
        public Conexao()
        {
            con.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
        }
        public SqlConnection conectar()
        {
            if (con.State == System.Data.ConnectionState.Closed)
            {
                if (String.IsNullOrEmpty(con.ConnectionString))
                {
                    con.ConnectionString = System.Configuration.ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
                }
                con.Open();
            }
            return con;
        }
        public void desconectar()
        {
            if (con.State == System.Data.ConnectionState.Open)
            {
                con.Close();
            }
        }
        protected List<T> ExecutaSelectLista<T>(string query)
        {
            using (var connection = conectar())
            {
                return connection.Query<T>(query, commandTimeout: timeOut).AsList();
            }
        }
        protected T ExecutaSelect<T>(string sqlQuery)
        {
            using (var connection = conectar())
            {
                return connection.QueryFirstOrDefault<T>(sqlQuery);
            }
        }
        protected bool ExecutaComando(string sqlQuery)
        {
            using (var connection = conectar())
            {
                return connection.Execute(sqlQuery) > 0;
            }
        }

        protected T EXECUTAPROC<T>(string sqlQuery)
        {
            using (var connection = conectar())
            {
                return connection.QueryFirstOrDefault<T>(sqlQuery);
            }
        }
    }
}
