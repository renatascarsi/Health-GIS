using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using System.Data;
using ESRI.ArcGIS.Carto;
using System.IO;
using System.Windows.Forms;
using GestaoSMSAddin.Modelo;
using GestaoSMSAddin.DataAccess.Extensions;

namespace GestaoSMSAddin.DataAccess.Repositorios
{
    class UnidadeSaudeRepositorio : RepositorioBase
    {
        /// <summary>
        /// Construtor.
        /// </summary>
        public UnidadeSaudeRepositorio()
            : base() { }

        /// <summary>
        /// Lista todas as unidades de saúde.
        /// </summary>
        /// <returns>Uma lista de unidades de saúde.</returns>
        public List<UnidadeSaude> Listar()
        {
            var listaDeUnidadesSaude = new List<UnidadeSaude>();

            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand = "SELECT codigounidadesaude, nome FROM unidadessaude";

                NpgsqlCommand command = new NpgsqlCommand(sqlCommand, conn);
                command.CommandType = CommandType.Text;

                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var unidadeSaude = new UnidadeSaude();
                    unidadeSaude.FromNpgsqlDataReader(reader);
                    listaDeUnidadesSaude.Add(unidadeSaude);
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

            return listaDeUnidadesSaude;
        }
    }
}
