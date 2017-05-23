using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using GestaoSMSAddin.Modelo;

namespace GestaoSMSAddin.DataAccess.Extensions
{
    public static class UnidadeSaudeExtensions
    {
        // <summary>
        /// Recupera os dados das unidades de saude com base no Data Reader.
        /// </summary>
        /// <param name="unidadeSaude">A Unidade de Saude.</param>
        /// <param name="reader">O objeto data reader.</param>
        public static void FromNpgsqlDataReader(this UnidadeSaude unidadeSaude, NpgsqlDataReader reader)
        {
            unidadeSaude.CodigoUnidadeSaude = reader.GetInt32(0);
            unidadeSaude.Nome = reader.GetString(1);
        }
    }
}
