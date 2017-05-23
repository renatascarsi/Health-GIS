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
    class DistritoSanitarioRepositorio : RepositorioBase
    {
        /// <summary>
        /// Construtor.
        /// </summary>
        public DistritoSanitarioRepositorio()
            : base() { }

        /// <summary>
        /// Lista todos os distritos sanitarios.
        /// </summary>
        /// <returns>Uma lista de distritos sanitarios.</returns>
        public List<DistritoSanitario> Listar()
        {
            var listaDeDistritosSanitarios = new List<DistritoSanitario>();

            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand = "SELECT codigodistritosanitario, nome FROM distritossanitarios";

                NpgsqlCommand command = new NpgsqlCommand(sqlCommand, conn);
                command.CommandType = CommandType.Text;

                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var distritoSanitario = new DistritoSanitario();
                    distritoSanitario.FromNpgsqlDataReader(reader);
                    listaDeDistritosSanitarios.Add(distritoSanitario);
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

            return listaDeDistritosSanitarios;
        }
    }
}
