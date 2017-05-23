using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using GestaoSMSAddin.Modelo;

namespace GestaoSMSAddin.DataAccess.Extensions
{
    public static class DistritoSanitarioExtensions
    {
        // <summary>
        /// Recupera os dados dos distritos sanitarios com base no Data Reader.
        /// </summary>
        /// <param name="distritoSanitario">O distrito sanitario.</param>
        /// <param name="reader">O objeto data reader.</param>
        public static void FromNpgsqlDataReader(this DistritoSanitario distritoSanitario, NpgsqlDataReader reader)
        {
            distritoSanitario.CodigoDistritoSanitario = reader.GetInt32(0);
            distritoSanitario.Nome = reader.GetString(1);
        }
    }
}
