using Npgsql;
using GestaoSMSAddin.Modelo;

namespace GestaoSMSAddin.DataAccess.Extensions
{
    public static class BairroExtensions
    {
        // <summary>
        /// Recupera os dados dos bairros com base no Data Reader.
        /// </summary>
        /// <param name="bairro">O bairro.</param>
        /// <param name="reader">O objeto data reader.</param>
        public static void FromNpgsqlDataReader(this Bairro bairro, NpgsqlDataReader reader)
        {
            bairro.CodigoBairro = reader.GetInt32(0);
            bairro.Nome = reader.GetString(1);
        }
    }
}
