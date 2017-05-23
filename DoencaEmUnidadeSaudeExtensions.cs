using Npgsql;
using GestaoSMSAddin.Modelo;

namespace GestaoSMSAddin.DataAccess.Extensions
{
    /// <summary>
    /// Métodos de extensão para a incidencia de doenças em unidades de saude.
    /// </summary>
    public static class DoencaEmUnidadeSaudeExtensions
    {
         /// <summary>
         /// Recupera os dados das doencas com base no Data Reader.
         /// </summary>
         /// <param name="incidencia">A doenca.</param>
         /// <param name="reader">O objeto data reader.</param>
         public static void FromNpgsqlDataReader(this DoencaEmUnidadeSaude incidencia, NpgsqlDataReader reader)
         {
             incidencia.CodigoDoenca = reader.GetInt32(0);
             incidencia.CodigoUnidadeSaude = reader.GetInt32(1);
             incidencia.Incidencia = reader.GetInt16(2);
             incidencia.DescricaoDoenca = reader.GetString(3);
             incidencia.NomeUnidadeSaude = reader.GetString(4);
         }
    }
}
