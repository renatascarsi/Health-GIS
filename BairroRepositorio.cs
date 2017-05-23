using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GestaoSMSAddin.Modelo;
using Npgsql;
using System.Data;
using GestaoSMSAddin.DataAccess.Extensions;

namespace GestaoSMSAddin.DataAccess.Repositorios
{
    class BairroRepositorio : RepositorioBase
    {
        /// <summary>
        /// Construtor.
        /// </summary>
        public BairroRepositorio()
            : base() { }

        /// <summary>
        /// Lista todos bairros.
        /// </summary>
        /// <returns>Uma lista de bairros.</returns>
        public List<Bairro> Listar()
        {
            var listaDeBairros = new List<Bairro>();

            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand = "SELECT codigobairro, nome FROM bairros";

                NpgsqlCommand command = new NpgsqlCommand(sqlCommand, conn);
                command.CommandType = CommandType.Text;

                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var bairro = new Bairro();
                    bairro.FromNpgsqlDataReader(reader);
                    listaDeBairros.Add(bairro);
                }
            }
            catch
            {
                throw;
            }
            finally
            {
                if (conn.State == ConnectionState.Open)
                    conn.Close();
            }

            return listaDeBairros;
        }
    }
}
