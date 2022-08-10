using System;
using System.Collections.Generic;

namespace DTO.Entidades
{
    public class Registrar
    {
        public int remetenteId { get; set; }
        public string cnpj { get; set; }
        public List<Encomendas> encomendas { get; set; }
    }

    public class Encomendas
    {
        public int servicoTipo { get; set; }
        public int entregaTipo { get; set; }
        public int volumes { get; set; }
        public string condFrete { get; set; }
        public string pedido { get; set; }
        public string natureza { get; set; }
        public int icmsIsencao { get; set; }
        public Destinatario destinatario { get; set; }
        public DocFiscal docFiscal { get; set; }
    }

    public class Destinatario
    {
        public string nome { get; set; }
        public string cpfCnpj { get; set; }
        public Endereco endereco { get; set; }            
    }
    public class Endereco
    {
        public string logradouro { get; set; }
        public string numero { get; set; }
        public string bairro { get; set; }
        public string cidade { get; set; }
        public string estado { get; set; }
        public string cep { get; set; }
    }
    public class DocFiscal
    {
        public List<Nfe> nfe { get; set; }
    }
    public class Nfe
    {
        public int nfeNumero { get; set; }
        public int nfeSerie { get; set; }
        public DateTime nfeData { get; set; }
        public decimal nfeValTotal { get; set; }
        public decimal nfeValProd { get; set; }
        public string nfeChave { get; set; }
    }
}

