using System;
using System.Collections.Generic;
using DTO.Entidades;

namespace DAL
{
    public class Query : Conexao
    {
        private string clienteid = System.Configuration.ConfigurationManager.AppSettings["clienteid"];
        private string tipointegracao = System.Configuration.ConfigurationManager.AppSettings["tipointegracao"];

        public DadosIntegracao ConsultaDadosIntegracao()
        {
            var retorno = new DadosIntegracao();
            try
            {
                string sqlQuery = $@"select * from Integracoes WITH(NOLOCK) where cliente = {clienteid} and tipointegracao = '{tipointegracao}';";
                retorno = ExecutaSelect<DadosIntegracao>(sqlQuery);
            }
            catch (Exception ex)
            {
                var erro = ex.Message;
                LogErro(erro, "ConsultaDadosIntegracao");
            }

            return retorno;
        }

        public List<Pedidos> ConsultaPedidos()
        {
            var retorno = new List<Pedidos>();

            try
            {
                string sqlQuery = $@"select * from vw_BuscaPedidosSino";
                retorno = ExecutaSelectLista<Pedidos>(sqlQuery);
                return retorno;
            }

            catch (Exception ex)
            {
                var erro = ex.Message;
                LogErro(erro, "ConsultaPedidos");
            }

            return retorno;
        }

        public void IntegraEDI(int pedidoid)
        {
            try
            {
                var sqlQuery = $@"UPDATE Checkout SET IntegrouEdiTransportadora = 1 WHERE PedidoId = {pedidoid};";
                ExecutaComando(sqlQuery);
            }
            catch (Exception ex)
            {
                var erro = ex.Message;
            }
        }

        public void RetornoJson(int pedidoid, string enviojson)
        {
            try
            {
                var sqlQuery = $@"UPDATE Pedido SET RetornoTMS = '{enviojson}' WHERE PedidoId = {pedidoid};";
                ExecutaComando(sqlQuery);
            }
            catch (Exception ex)
            {
                var erro = ex.Message;
            }
        }

        public void ProximoEnvio()
        {
            try
            {
                var dt = DateTime.Now.AddMinutes(10);
                var data = dt.ToString("yyyy-MM-dd HH:mm:ss.fff");
                var sqlQuery = $@"UPDATE Integracoes SET proximoenvio = '{data}' 
                                WHERE cliente = {clienteid} AND tipointegracao = '{tipointegracao}'";
                ExecutaComando(sqlQuery);
            }
            catch (Exception ex)
            {
                var error = ex.Message;
            }
        }

        public void InsereEtiqueta(int pedidoid, string zpl)
        {
            try
            {
                var rastreio = "";
                var sqlQuery = $@"INSERT INTO EtiquetaPedido 
                                (Pedidoid, Zpl, Ativo, Situacao, DtaIncAlt, UserIncAlt, Rastreio)
                                  VALUES ({pedidoid}, '{zpl}', 1, 1, GETDATE(), 60, '{rastreio}');";
                ExecutaComando(sqlQuery);
            }
            catch(Exception ex)
            {
                var error = ex.Message;
            }
        }
        public void LogErro(string erro, string metodo)
        {
            var data = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            erro = erro.Replace("'", "");

            try
            {
                var sqlQuery = $@"INSERT INTO logerro (clienteid,data,erro,origem,metodo)
                                  VALUES ({clienteid}, '{data}', '{erro}', '{tipointegracao}', '{metodo}')";
                ExecutaComando(sqlQuery);
            }

            catch (Exception ex)
            {
                var error = ex.Message;
            }
        }

    }

}
