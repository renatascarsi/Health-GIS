using Npgsql;
using GestaoSMSAddin.Modelo;

namespace GestaoSMSAddin.DataAccess.Extensions
{
    /// <summary>
    /// Métodos de extensão para as doencas.
    /// </summary>
    public static class DoencaExtensions
    {
        /// <summary>
        /// Recupera os dados das doencas com base no Data Reader.
        /// </summary>
        /// <param name="doenca">A doenca.</param>
        /// <param name="reader">O objeto data reader.</param>
        public static void FromNpgsqlDataReader(this Doenca doenca, NpgsqlDataReader reader)
        {
            doenca.CodigoDoenca = reader.GetInt32(0);
            doenca.Nome = reader.GetString(1);
        }
    }
}
