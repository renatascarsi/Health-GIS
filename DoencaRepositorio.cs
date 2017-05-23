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

namespace GestaoSMSAddin.DataAccess
{
    public class DoencaRepositorio : RepositorioBase
    {
        /// <summary>
        /// Construtor.
        /// </summary>
        public DoencaRepositorio()
            : base() { }


        /// <summary>
        /// Lista todas as doenças.
        /// </summary>
        /// <returns>Uma lista de doenças.</returns>
        public List<Doenca> Listar()
        {
            var listaDeDoencas = new List<Doenca>();

            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand = "SELECT codigodoenca, descricao FROM doenca";

                NpgsqlCommand command = new NpgsqlCommand(sqlCommand, conn);
                command.CommandType = CommandType.Text;

                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var doenca = new Doenca();
                    doenca.FromNpgsqlDataReader(reader);
                    listaDeDoencas.Add(doenca);
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

            return listaDeDoencas;
        }

        /// <summary>
        /// Lista todas as doenças de uma regional.
        /// </summary>
        /// <returns>Uma lista de doenças.</returns>
        public List<Doenca> ListarIncidenciaDoencaPorAno(int codigoDoenca, int anoInicial, int anoFinal)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                return RunFunctionAndExecuteReader<Doenca>(
                    "renatascarsi.listar_incidenciadoenca_porano_relatorio",

                    (command) =>
                    {
                        command.Parameters.Add("codigodoenca", NpgsqlTypes.NpgsqlDbType.Integer).Value = codigoDoenca;
                        command.Parameters.Add("anoinicial", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoInicial;
                        command.Parameters.Add("anofinal", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoFinal;
                    },

                    (doencaEmUnidadeSaude, reader) =>
                    {
                        doencaEmUnidadeSaude.Ano = reader.GetDouble(0);
                        doencaEmUnidadeSaude.Incidencia = reader.GetInt32(1);
                    }
                );
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
        }

        /// <summary>
        /// Inserir nova doenca.
        /// </summary>
        /// <param name="connection">A conexão a ser utilizada.</param>
        /// 
        public int Criar(Doenca doenca)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand1 = "INSERT INTO doenca(descricao) VALUES (@descricaodoenca)";

                NpgsqlCommand command1 = new NpgsqlCommand(sqlCommand1, conn);
                command1.CommandType = CommandType.Text;
                command1.Parameters.Add("@descricaodoenca", NpgsqlTypes.NpgsqlDbType.Varchar, 100).Value = doenca.Nome;
                
                return (int) command1.ExecuteNonQuery();
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
        }
    }
}
