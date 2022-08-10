using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using DAL;
using DTO.Entidades;
using Newtonsoft.Json;
using RestSharp;

namespace BLL
{
    public class Processar
    {
        public class PedidoEnvio
        {
            public string pedido { get; set; }
            public string enviojson { get; set; }
            public string retornojson { get; set; }
        }
        public string EnviarPedidos()
        {
            var dadosIntegracao = new Query().ConsultaDadosIntegracao();
            if (dadosIntegracao.cliente > 0)
            {
                var pedidos = new Query().ConsultaPedidos();

                if (pedidos.Count > 0)
                {
                    var token = GerarToken(dadosIntegracao);

                    if (!String.IsNullOrEmpty(token))
                    {
                        var jsonpedido = MontarJson(pedidos, dadosIntegracao);
                        var envio = EnviarAPI(token, jsonpedido, dadosIntegracao, pedidos);
                    }
                }
            }
            return "";
        }

        public string GerarToken(DadosIntegracao dados)
        {
            var ret = "";
            try
            {
                var gettoken = new GetToken()
                {
                    grant_type = "password",
                    username = dados.usuario,
                    password = dados.senha
                };
                using (WebClient client = new WebClient())
                {
                    try
                    {
                        string url = dados.webserviceadicional1;
                        HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
                        request.ContentType = "application/json";
                        request.Method = "POST";
                        request.Accept = "application/json";
                        request.Headers.Add(HttpRequestHeader.Authorization, "Basic SUNTOnRvdGFs");
                        var json = JsonConvert.SerializeObject(gettoken);
                        using (var streamWriter = new StreamWriter(request.GetRequestStream()))
                        {
                            var resToWrite = json;
                            streamWriter.Write(resToWrite);
                            streamWriter.Flush();
                            streamWriter.Close();
                        }
                        WebResponse response = request.GetResponse();
                        var streamReader = new StreamReader(response.GetResponseStream());
                        var result = streamReader.ReadToEnd();
                        var retorno = JsonConvert.DeserializeObject<RetornoToken>(result);
                        ret = retorno.access_token;
                    }
                    catch (Exception ex)
                    {
                        var erro = ex.Message;
                        var envialog = new Query();
                        envialog.LogErro(erro, "GerarToken");
                    }
                }
            }
            catch (Exception ex)
            {
                var erro = ex.Message;
                var envialog = new Query();
                envialog.LogErro(erro, "GerarToken");
            }
            return ret;
        }

        public List<PedidoEnvio> MontarJson(List<Pedidos> pedidos, DadosIntegracao dados)
        {
            var sJson = "";
            var lista = new List<PedidoEnvio>();
            try
            {
                foreach (var item in pedidos)
                {
                    var json = new Registrar()
                    {
                        remetenteId = 25671,
                        cnpj = "22922742000298",
                        encomendas = new List<Encomendas>
                        {
                            new Encomendas()
                            {
                                servicoTipo = 1,
                                entregaTipo = 0,
                                volumes = item.Volume,
                                condFrete = "CIF",
                                pedido = item.PedidoId.ToString(),
                                natureza = "diversos",
                                icmsIsencao = 0,
                                destinatario = new Destinatario
                                {
                                        nome            = item.Nome,
                                        cpfCnpj         = item.NumeroDocumento,
                                        endereco = new Endereco
                                        {
                                            logradouro   = item.Endereco,
                                            numero       = item.Numero.ToString(),
                                            bairro       = item.Bairro,
                                            cidade       = item.Municipio,
                                            estado       = item.UF,
                                            cep          = item.CEP.PadLeft(8, '0').Replace("-","")
                                        },
                                },
                                docFiscal = new DocFiscal
                                {
                                   nfe = new List<Nfe>
                                   {
                                       new Nfe()
                                       {
                                           nfeNumero = item.NumeroNF,
                                           nfeSerie = item.SerieNF,
                                           nfeData = item.DataEmissaoNF,
                                           nfeValTotal = item.ValorNF,
                                           nfeValProd = item.ValorTotalPedido,
                                           nfeChave = item.ChaveNF
                                       },
                                   },
                                },
                            },
                        },
                    };

                    sJson = JsonConvert.SerializeObject(json);
                    var ped = new PedidoEnvio()
                    {
                        enviojson = sJson,
                        pedido = item.PedidoId.ToString(),
                    };

                    lista.Add(ped);
                }
            }

            catch (Exception ex)
            {
                var erro = ex.Message;
                var envialog = new Query();
                envialog.LogErro(erro, "MontaJson");
            }

            return lista;
        }

        public List<PedidoEnvio> EnviarAPI(string token, List<PedidoEnvio> pedidos, DadosIntegracao dados, List<Pedidos> zplInfo)
        {
            var ret = new List<PedidoEnvio>();
            try
            {
                foreach (var item in pedidos)
                {                  
                    var client = new RestClient(dados.webservice);
                    client.Timeout = -1;
                    var met = Method.POST;

                    var request = new RestRequest(met);
                    request.AddHeader("Content-Type", "application/json");
                    request.AddHeader("Authorization", $"Bearer {token}");
                    
                    if (!string.IsNullOrWhiteSpace(item.enviojson))                    
                        request.AddParameter("application/json", item.enviojson, ParameterType.RequestBody);
                    
                    IRestResponse response = client.Execute(request);
                    var jsonRetorno = response.Content;
                    jsonRetorno = jsonRetorno.Replace("[", "").Replace("]", "");
                    item.retornojson = jsonRetorno;

                    if (!String.IsNullOrEmpty(response.Content))
                    {
                        if (response.StatusCode == HttpStatusCode.OK)
                        {
                            try
                            {
                                var update = new Query();
                                update.IntegraEDI(Convert.ToInt32(item.pedido));
                                update.RetornoJson(Convert.ToInt32(item.pedido), item.retornojson);
                                update.ProximoEnvio();

                                List<string> list = new List<string>();

                                var diaMes = DateTime.Now.ToString("dd/MM");
                                var horaMinuto = DateTime.Now.ToString("HH:mm");

                                var zpl = @"C:\arquivosclientes\SinoTotalExpress\ZPL.prn";
                                zpl = zpl.Replace(@"\\", @"\");

                                string[] zplParams = new string[] {"{NOME}", "{ENDERECO}", "{BAIRRO}", "{COMPLEMENTO}",
                                "{MUNICIPIO}", "{UF}", "{dd/MM}", "{HH:mm}", "{CEP}", "{TIPO}",
                                "{QRCODE}", "{ROTA}", "{CODBARRAS}",
                                "{PEDIDOID}"};

                                if (File.Exists(zpl))
                                {
                                    string[] lines = File.ReadAllLines(zpl);

                                    int countStr = Regex.Matches(jsonRetorno, "awb").Count;
                                    var index = 0;

                                    while (countStr > index)
                                    {
                                        foreach (string line in lines)
                                        {
                                            var param = zplParams.FirstOrDefault(s => line.Contains(s));

                                            foreach (var zplParam in zplInfo)
                                            {
                                                switch (param)
                                                {
                                                    case "{NOME}":
                                                        {
                                                            list.Add(line.Replace("{NOME}", $"{zplParam.Nome}"));
                                                            break;
                                                        }
                                                    case "{ENDERECO}":
                                                        {
                                                            list.Add(line.Replace("{ENDERECO}", $"{zplParam.Endereco}"));
                                                            break;
                                                        }
                                                    case "{BAIRRO}":
                                                        {
                                                            list.Add(line.Replace("{BAIRRO}", $"{zplParam.Bairro}"));
                                                            break;
                                                        }
                                                    case "{COMPLEMENTO}":
                                                        {
                                                            list.Add(line.Replace("{COMPLEMENTO}", $"{zplParam.Complemento}"));
                                                            break;
                                                        }
                                                    case "{MUNICIPIO}":
                                                        {
                                                            list.Add(line.Replace("{CEP} {MUNICIPIO}/{UF}", $"{zplParam.CEP} {zplParam.Municipio}/{zplParam.UF}"));
                                                            break;
                                                        }
                                                    case "{dd/MM}":
                                                        {
                                                            list.Add(line.Replace("{dd/MM}", $"{diaMes}"));
                                                            break;
                                                        }
                                                    case "{HH:mm}":
                                                        {
                                                            list.Add(line.Replace("{HH:mm}", $"{horaMinuto}"));
                                                            break;
                                                        }
                                                    case "{CEP}":
                                                        {
                                                            list.Add(line.Replace("{CEP}", $"{zplParam.CEP}"));
                                                            break;
                                                        }
                                                    case "{TIPO}":
                                                        {
                                                            list.Add(line.Replace("{TIPO}", $"STD"));
                                                            break;
                                                        }
                                                    case "{QRCODE}":
                                                        {
                                                            list.Add(line.Replace("{QRCODE}", "9817230190000"));
                                                            break;
                                                        }
                                                    case "{ROTA}":
                                                        {
                                                            var jsonString = jsonRetorno.Replace($"\"", " ");
                                                            var str = jsonString.Substring(0, jsonString.IndexOf(" , codigoBarras"));
                                                            str = str.Split(' ').Last();
                                                            list.Add(line.Replace("{ROTA}", $"{str}"));
                                                            break;
                                                        }
                                                    case "{CODBARRAS}":
                                                        {
                                                            var jsonString = jsonRetorno.Replace($"\"", " ");
                                                            var str = jsonString.Substring(0, jsonString.IndexOf(" }, documentoFiscal"));
                                                            str = str.Split(' ').Last();
                                                            list.Add(line.Replace("{CODBARRAS}", $"{str}"));
                                                            break;
                                                        }
                                                    case "{PEDIDOID}":
                                                        {
                                                            list.Add(line.Replace("{PEDIDOID}", $"{item.pedido}"));
                                                            break;
                                                        }
                                                    default:
                                                        list.Add(line);
                                                        break;
                                                }
                                            }
                                        }
                                        string zplText = string.Join("", list);
                                        var newPedidoId = int.Parse(item.pedido);
                                        new Query().InsereEtiqueta(newPedidoId, zplText);
                                        index++;
                                    }
                                }
                            }
                            catch (Exception ex)
                            {
                                var erro = ex.Message;
                                var envialog = new Query();
                                envialog.LogErro(erro, "EnviarAPI");
                            }
                        }
                        ret.Add(item);
                    }
                }
            }
            catch (WebException ex)
            {
                var erro = ex.Message;
                var envialog = new Query();
                envialog.LogErro(erro, "EnviarAPI");
            }

            return ret;
        }
    }
}
