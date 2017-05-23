using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GestaoSMSAddin.Modelo
{
    public class DoencaEmUnidadeSaude
    {
        public int CodigoDoenca { get; set; }
        public int CodigoUnidadeSaude { get; set; }
        public int Incidencia { get; set; }
        public string DescricaoDoenca { get; set; }
        public string NomeUnidadeSaude { get; set; }
        public string NomeRegional { get; set; }
    }
}
