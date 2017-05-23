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
    public class DoencaEmUnidadeSaudeRepositorio : RepositorioBase
    {
        /// <summary>
        /// Construtor.
        /// </summary>
        public DoencaEmUnidadeSaudeRepositorio()
            : base() { }


        /// <summary>
        /// Lista todas as doenças.
        /// </summary>
        /// <returns>Uma lista de doenças.</returns>
        public List<DoencaEmUnidadeSaude> Listar()
        {
            NpgsqlConnection conn = 
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                return RunFunctionAndExecuteReader<DoencaEmUnidadeSaude>(
                    "renatascarsi.listar_incidenciadoenca",

                    (command) =>
                    {
                    },

                    (doencaEmUnidadeSaude, reader) =>
                    {
                        doencaEmUnidadeSaude.DescricaoDoenca = reader.GetString(0);
                        doencaEmUnidadeSaude.NomeUnidadeSaude = reader.GetString(1);
                        doencaEmUnidadeSaude.Incidencia = reader.GetInt16(3);
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
        /// Lista todas as doenças com as regionais.
        /// </summary>
        /// <returns>Uma lista de doenças.</returns>
        public List<DoencaEmUnidadeSaude> ListarDoencasERegionais(int codigoDoenca, int anoInicial, int anoFinal)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                return RunFunctionAndExecuteReader<DoencaEmUnidadeSaude>(
                    "renatascarsi.listar_incidenciadoenca_unidadesaude_relatorio",

                    (command) =>
                    {
                        command.Parameters.Add("codigodoenca", NpgsqlTypes.NpgsqlDbType.Integer).Value = codigoDoenca;
                        command.Parameters.Add("anoinicial", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoInicial;
                        command.Parameters.Add("anofinal", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoFinal;
                    },

                    (doencaEmUnidadeSaude, reader) =>
                    {
                        doencaEmUnidadeSaude.NomeUnidadeSaude = reader.GetString(0);
                        doencaEmUnidadeSaude.NomeRegional = reader.GetString(1);
                        doencaEmUnidadeSaude.Incidencia = reader.GetInt32(2);
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
        /// Lista todas as doenças com as regionais.
        /// </summary>
        /// <returns>Uma lista de doenças.</returns>
        public List<DoencaEmUnidadeSaude> ListarDoencasDeUmaUnidadeSaude(int codigoUnidadeSaude, int anoInicial, int anoFinal)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                return RunFunctionAndExecuteReader<DoencaEmUnidadeSaude>(
                    "renatascarsi.listar_unidadesaude_doencas_relatorio",

                    (command) =>
                    {
                        command.Parameters.Add("codigounidadesaude", NpgsqlTypes.NpgsqlDbType.Integer).Value = codigoUnidadeSaude;
                        command.Parameters.Add("anoinicial", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoInicial;
                        command.Parameters.Add("anofinal", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoFinal;
                    },

                    (doencaEmUnidadeSaude, reader) =>
                    {
                        doencaEmUnidadeSaude.DescricaoDoenca = reader.GetString(0);
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
        /// Lista todas as doenças de uma regional.
        /// </summary>
        /// <returns>Uma lista de doenças.</returns>
        public List<DoencaEmUnidadeSaude> ListarDoencasDeUmDistritoSanitario(int codigoDistritoSanitario, int anoInicial, int anoFinal)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                return RunFunctionAndExecuteReader<DoencaEmUnidadeSaude>(
                    "renatascarsi.listar_distritosanitario_doencas_relatorio",

                    (command) =>
                    {
                        command.Parameters.Add("codigodistritosanitario", NpgsqlTypes.NpgsqlDbType.Integer).Value = codigoDistritoSanitario;
                        command.Parameters.Add("anoinicial", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoInicial;
                        command.Parameters.Add("anofinal", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoFinal;
                    },

                    (doencaEmUnidadeSaude, reader) =>
                    {
                        doencaEmUnidadeSaude.DescricaoDoenca = reader.GetString(0);
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
        /// Gera mapa temático de incidência de doença em unidades de saúde.
        /// </summary>
        /// <returns>Um mapa temático de incidência de doença em unidades de saúde.</returns>
        public List<MapaDoencaEmUnidadeSaude> GerarMapaTematicoEmUnidadesDeSaude(int codigoDoenca, int anoInicial, int anoFinal)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                return RunFunctionAndExecuteReader<MapaDoencaEmUnidadeSaude>(
                    "renatascarsi.listar_incidenciadoenca_unidadesaude",

                    (command) =>
                    {
                        command.Parameters.Add("codigodoenca", NpgsqlTypes.NpgsqlDbType.Integer).Value = codigoDoenca;
                        command.Parameters.Add("anoinicial", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoInicial;
                        command.Parameters.Add("anofinal", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoFinal;
                    },

                    (doencaEmUnidadeSaude, reader) =>
                    {
                        doencaEmUnidadeSaude.UnidadesSaude = (byte[])reader.GetValue(0);
                        doencaEmUnidadeSaude.TotalIncidencias = reader.GetInt32(2);
                        doencaEmUnidadeSaude.Nome = reader.GetString(1);
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
        /// Gera mapa temático de incidência de doença em distritos sanitários.
        /// </summary>
        /// <returns>Um mapa temático de incidência de doença em distritos sanitários.</returns>
        public List<MapaDoencaEmDistritoSanitario> GerarMapaTematicoEmDistritosSanitarios(int codigoDoenca, int anoInicial, int anoFinal)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                return RunFunctionAndExecuteReader<MapaDoencaEmDistritoSanitario>(
                    "renatascarsi.listar_incidenciadoenca_distritosanitario",

                    (command) =>
                    {
                        command.Parameters.Add("codigodoenca", NpgsqlTypes.NpgsqlDbType.Integer).Value = codigoDoenca;
                        command.Parameters.Add("anoinicial", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoInicial;
                        command.Parameters.Add("anofinal", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoFinal;
                    },

                    (doencaEmDistritoSanitario, reader) =>
                    {
                        doencaEmDistritoSanitario.DistritosSanitarios = (byte[])reader.GetValue(0);
                        doencaEmDistritoSanitario.TotalIncidencias = reader.GetInt32(2);
                        doencaEmDistritoSanitario.Nome = reader.GetString(1);
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
        /// Gera mapa temático de incidência de doença em unidades de saúde em um distrito sanitário específico.
        /// </summary>
        /// <returns>Um mapa temático de incidência de doença em unidades de saúde em um distrito sanitário específico.</returns>
        public List<MapaDoencaEmUnidadeSaude> GerarMapaTematicoEmUnidadesDeSaudeDeDistritoEspecifico(int codigoDoenca, int codigoDistritoSanitario, int anoInicial,
            int anoFinal)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                return RunFunctionAndExecuteReader<MapaDoencaEmUnidadeSaude>(
                    "renatascarsi.listar_incidenciadoenca_unidadesaude_distritoespecifico",

                    (command) =>
                    {
                        command.Parameters.Add("codigodoenca", NpgsqlTypes.NpgsqlDbType.Integer).Value = codigoDoenca;
                        command.Parameters.Add("codigodistritosanitario", NpgsqlTypes.NpgsqlDbType.Integer).Value = codigoDistritoSanitario;
                        command.Parameters.Add("anoinicial", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoInicial;
                        command.Parameters.Add("anofinal", NpgsqlTypes.NpgsqlDbType.Integer).Value = anoFinal;
                    },

                    (doencaEmUnidadeSaude, reader) =>
                    {
                        doencaEmUnidadeSaude.UnidadesSaude = (byte[])reader.GetValue(0);
                        doencaEmUnidadeSaude.TotalIncidencias = reader.GetInt32(2);
                        doencaEmUnidadeSaude.Nome = reader.GetString(1);
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
        /// Inserir incidencia de doencas.
        /// </summary>
        /// <param name="connection">A conexão a ser utilizada.</param>
        /// 
        public int Criar(DoencaEmUnidadeSaude incidenciaDoencaEmUnidadeSaude)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand1 = "INSERT INTO doencaemunidadesaude(incidenciadoenca,codigounidadesaude,codigodoenca,data) VALUES (@incidenciadoenca,@codigounidadesaude,@codigodoenca,@data)";

                NpgsqlCommand command1 = new NpgsqlCommand(sqlCommand1, conn);
                command1.CommandType = CommandType.Text;
                command1.Parameters.Add("@incidenciadoenca", NpgsqlTypes.NpgsqlDbType.Integer, 100).Value = incidenciaDoencaEmUnidadeSaude.Incidencia;
                command1.Parameters.Add("@codigounidadesaude", NpgsqlTypes.NpgsqlDbType.Integer, 100).Value = incidenciaDoencaEmUnidadeSaude.CodigoUnidadeSaude;
                command1.Parameters.Add("@codigodoenca", NpgsqlTypes.NpgsqlDbType.Integer, 100).Value = incidenciaDoencaEmUnidadeSaude.CodigoDoenca;
                command1.Parameters.Add("@data", NpgsqlTypes.NpgsqlDbType.Date).Value = DateTime.Now;
                
                return (int)command1.ExecuteNonQuery();
                
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
        /// Alterar incidencia de doencas.
        /// </summary>
        /// <param name="connection">A conexão a ser utilizada.</param>
        /// 
        public int Alterar(DoencaEmUnidadeSaude incidenciaDoencaEmUnidadeSaude)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand1 = "UPDATE doencaemunidadesaude SET incidenciadoenca = @incidenciadoenca WHERE (codigounidadesaude = @codigounidadesaude AND codigodoenca = @codigodoenca)";

                NpgsqlCommand command1 = new NpgsqlCommand(sqlCommand1, conn);
                command1.CommandType = CommandType.Text;
                command1.Parameters.Add("@incidenciadoenca", NpgsqlTypes.NpgsqlDbType.Integer, 100).Value = incidenciaDoencaEmUnidadeSaude.Incidencia;
                command1.Parameters.Add("@codigounidadesaude", NpgsqlTypes.NpgsqlDbType.Integer, 100).Value = incidenciaDoencaEmUnidadeSaude.CodigoUnidadeSaude;
                command1.Parameters.Add("@codigodoenca", NpgsqlTypes.NpgsqlDbType.Integer, 100).Value = incidenciaDoencaEmUnidadeSaude.CodigoDoenca;

                return (int)command1.ExecuteNonQuery();

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
        /// Excluir incidencia de doencas.
        /// </summary>
        /// <param name="connection">A conexão a ser utilizada.</param>
        /// 
        public int Excluir(DoencaEmUnidadeSaude incidenciaDoencaEmUnidadeSaude)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand1 = "DELETE FROM doencaemunidadesaude WHERE (incidenciadoenca = @incidenciadoenca AND codigounidadesaude = @codigounidadesaude AND codigodoenca = @codigodoenca)";

                NpgsqlCommand command1 = new NpgsqlCommand(sqlCommand1, conn);
                command1.CommandType = CommandType.Text;
                command1.Parameters.Add("@incidenciadoenca", NpgsqlTypes.NpgsqlDbType.Integer, 100).Value = incidenciaDoencaEmUnidadeSaude.Incidencia;
                command1.Parameters.Add("@codigounidadesaude", NpgsqlTypes.NpgsqlDbType.Integer, 100).Value = incidenciaDoencaEmUnidadeSaude.CodigoUnidadeSaude;
                command1.Parameters.Add("@codigodoenca", NpgsqlTypes.NpgsqlDbType.Integer, 100).Value = incidenciaDoencaEmUnidadeSaude.CodigoDoenca;

                return (int)command1.ExecuteNonQuery();

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
