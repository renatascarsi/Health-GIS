using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestaoSMSAddin.Domain.Modelo
{
    class Operacao
    {
        public int CodigoOperacao { get; set; }
        public string Nome { get; set; }
        public string Mensagem { get; set; }
        public int CodigoTipoUsuario { get; set; }
    }
}
