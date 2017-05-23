using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using GestaoSMSAddin.Modelo;

namespace GestaoSMSAddin.DataAccess.Extensions
{
    /// <summary>
    /// Métodos de extensão para a incidencia de doenças em unidades de saude.
    /// </summary>
    public static class UsuarioExtensions
    {
        /// <summary>
        /// Recupera os dados dos usuarios com base no Data Reader.
        /// </summary>
        /// <param name="usuario">o usuario.</param>
        /// <param name="reader">O objeto data reader.</param>
        public static void FromNpgsqlDataReader(this Usuario usuario, NpgsqlDataReader reader)
        {
            usuario.CodigoUsuario = reader.GetInt32(0);
            usuario.Nome = reader.GetString(1);
            usuario.Login = reader.GetString(2);
            usuario.Senha = reader.GetString(3);
        }
    }
}
