using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GestaoSMSAddin.Modelo;
using Npgsql;
using System.Data;
using GestaoSMSAddin.DataAccess.Extensions;

namespace GestaoSMSAddin.DataAccess
{
    public class UsuarioRepositorio : RepositorioBase
    {
        /// <summary>
        /// Construtor.
        /// </summary>
        public UsuarioRepositorio()
            : base()
        {
        }

        /// <summary>
        /// Cria um novo usuário.
        /// </summary>
        /// <param name="usuario">O usuário.</param>
        /// <returns>O código do usuário criado.</returns>
        public int Inserir(Usuario usuario)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand1 = "INSERT INTO usuarios(nome,login,senha) VALUES (@nome,@login,@senha)";

                NpgsqlCommand command1 = new NpgsqlCommand(sqlCommand1, conn);
                command1.CommandType = CommandType.Text;
                command1.Parameters.Add("@nome", NpgsqlTypes.NpgsqlDbType.Varchar, 100).Value = usuario.Nome;
                command1.Parameters.Add("@login", NpgsqlTypes.NpgsqlDbType.Varchar, 100).Value = usuario.Login;
                command1.Parameters.Add("@senha", NpgsqlTypes.NpgsqlDbType.Varchar, 100).Value = usuario.Senha;
                
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

        /// <summary>
        /// Alterar dados de usuário.
        /// </summary>
        /// <param name="connection">A conexão a ser utilizada.</param>
        /// 
        public int Alterar(Usuario usuario)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand = "UPDATE usuarios SET nome = @nome, login = @login, senha = @senha WHERE objectid = @objectid";

                NpgsqlCommand command = new NpgsqlCommand(sqlCommand, conn);
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@nome", NpgsqlTypes.NpgsqlDbType.Varchar, 100).Value = usuario.Nome;
                command.Parameters.Add("@login", NpgsqlTypes.NpgsqlDbType.Varchar, 100).Value = usuario.Login;
                command.Parameters.Add("@senha", NpgsqlTypes.NpgsqlDbType.Varchar, 100).Value = usuario.Senha;
                command.Parameters.Add("@objectid", NpgsqlTypes.NpgsqlDbType.Integer, 100).Value = usuario.CodigoUsuario;

                return (int)command.ExecuteNonQuery();
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
        /// Excluir usuário.
        /// </summary>
        /// <param name="connection">A conexão a ser utilizada.</param>
        /// 
        public void Excluir(Usuario codigoUsuario)
        {
            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand = "DELETE FROM usuarios WHERE objectid = @objectid";

                NpgsqlCommand command = new NpgsqlCommand(sqlCommand, conn);
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@objectid", NpgsqlTypes.NpgsqlDbType.Integer, 100).Value = codigoUsuario.CodigoUsuario;
                command.ExecuteNonQuery();
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
        /// Lista todas os usuários.
        /// </summary>
        /// <returns>Uma lista de usuários.</returns>
        public List<Usuario> Listar()
        {
            var listaDeUsuarios = new List<Usuario>();

            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand = "SELECT objectid,nome,login,senha FROM usuarios";
                NpgsqlCommand command = new NpgsqlCommand(sqlCommand, conn);
                command.CommandType = CommandType.Text;

                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var usuarios = new Usuario();
                    usuarios.FromNpgsqlDataReader(reader);
                    listaDeUsuarios.Add(usuarios);
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

            return listaDeUsuarios;
        }

        /// <summary>
        /// Executa a busca das informações do usuário dado seu código.
        /// </summary>
        /// <param name="codigoUsuario">O código do usuário.</param>
        /// <returns>O usuário.</returns>
        public Usuario RecuperarPorId(int codigoUsuario)
        {
            var listaDeUsuarios = new List<Usuario>();

            NpgsqlConnection conn =
                new NpgsqlConnection(this._connectionString);

            try
            {
                conn.Open();

                string sqlCommand = "SELECT objectid,nome,login,senha FROM usuarios WHERE  objectid = @objectid";
                NpgsqlCommand command = new NpgsqlCommand(sqlCommand, conn);
                command.CommandType = CommandType.Text;
                command.Parameters.Add("@objectid", NpgsqlTypes.NpgsqlDbType.Integer).Value = codigoUsuario;

                NpgsqlDataReader reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var usuarios = new Usuario();
                    usuarios.FromNpgsqlDataReader(reader);
                    listaDeUsuarios.Add(usuarios);
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

            if (listaDeUsuarios.Count == 0)
                throw new Exception("Usuário não encontrado.");
            return listaDeUsuarios[0];
        }

    }
}
