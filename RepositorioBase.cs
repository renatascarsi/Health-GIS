using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Npgsql;
using GestaoSMSAddin.DataAccess.Extensions;
using System.Data;

namespace GestaoSMSAddin
{
    /// <summary>
    /// Repositório base para todos os repositórios.
    /// </summary>
    public abstract class RepositorioBase
    {
        #region Fields

        /// <summary>
        /// String de conexão.
        /// </summary>
        protected readonly string _connectionString;

        #endregion

        #region Properties

        /// <summary>
        /// Recupera a string de conexão.
        /// </summary>
        public string ConnectionString
        {
            get
            {
                return this._connectionString;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Construtor.
        /// </summary>
        public RepositorioBase()
        {
            this._connectionString = "User ID=renatascarsi;Password=renata;Host=localhost;Port=5432;Database=gestaosms;Timeout=120;CommandTimeout=60";
        }

        #endregion

        #region Methods
        /// <summary>
        /// Método genérico para executar uma function com parâmetros e devolver uma
        /// lista de entidades.
        /// </summary>
        /// <param name="nomeFunction">O nome da function. Ex: criar_logradouro.</param>
        /// <param name="adicionarParametros">Método para adicionar os parâmetros
        /// no comando.</param>
        /// <param name="mapReaderToEntity">Método para mapear o DataReader com a entidade.</param>
        /// <param name="connection">A conexão a ser utilizada.</param>
        /// <param name="caching">Define se os dados serão armazenados em cache ou não.</param>
        /// <returns>Uma lista de entidades.</returns>
        protected List<T> RunFunctionAndExecuteReader<T>(string functionName,
            Action<NpgsqlCommand> addParameters,
            Action<T, NpgsqlDataReader> mapReaderToEntity,
            NpgsqlConnection connection = null,
            bool caching = false) where T : new()
        {
            List<T> listOfObjects = new List<T>();

            // verifica se deve realizar cache

            if (caching)
            {
                var cachedList =
                    (List<T>)CacheProvider.Instance.Get(functionName);
                if (cachedList != null)
                    return cachedList;
            }

            // recupera dados

            if (connection == null)
            {
                NpgsqlConnection conn = new NpgsqlConnection(this._connectionString);

                try
                {
                    conn.Open();

                    NpgsqlTransaction transaction = conn.BeginTransaction();

                    try
                    {
                        NpgsqlCommand command = new NpgsqlCommand(functionName, conn);
                        command.CommandType = CommandType.StoredProcedure;
                        command.CommandTimeout = 0;

                        if (addParameters != null)
                            addParameters(command);

                        NpgsqlDataReader reader = command.ExecuteReader();

                        while (reader.Read())
                        {
                            T item = new T();
                            mapReaderToEntity(item, reader);
                            listOfObjects.Add(item);
                        }

                        transaction.Commit();
                    }
                    catch
                    {
                        transaction.Rollback();
                        throw;
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
            }
            else
            {
                try
                {
                    NpgsqlCommand command = new NpgsqlCommand(functionName, connection);
                    command.CommandType = CommandType.StoredProcedure;
                    command.CommandTimeout = 0;

                    if (addParameters != null)
                        addParameters(command);

                    NpgsqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        T item = new T();
                        mapReaderToEntity(item, reader);
                        listOfObjects.Add(item);
                    }
                }
                catch
                {
                    throw;
                }
            }

            if (caching)
            {
                CacheProvider.Instance.Add(functionName, listOfObjects, null,
                    DateTime.Now.AddHours(5.0), TimeSpan.Zero,
                    System.Web.Caching.CacheItemPriority.High, null);
            }

            return listOfObjects;
        }
        
        #endregion
    }
}
