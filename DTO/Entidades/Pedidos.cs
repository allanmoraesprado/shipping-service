using System;

namespace DTO.Entidades
{
    public class Pedidos
    {
        public string Nome { get; set; }
        public string NumeroDocumento { get; set; }
        public string Endereco { get; set; }
        public int Numero { get; set; }
        public string Complemento { get; set; }
        public string Bairro { get; set; }
        public string Municipio { get; set; }
        public string UF { get; set; }
        public string CEP { get; set; }
        public int Volume { get; set; }
        public int PedidoId { get; set; }
        public int NumeroNF { get; set; }
        public int SerieNF { get; set; }
        public DateTime DataEmissaoNF { get; set; }
        public decimal ValorNF { get; set; }
        public decimal ValorTotalPedido { get; set; }
        public string ChaveNF { get; set; }
    }
}
