using System;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geodatabase;
using GestaoSMSAddin.Modelo;
using GestaoSMSAddin.Utils;
using GestaoSMSAddin.DataAccess;

namespace GestaoSMSAddin
{
    /// <summary>
    /// Extensão do ArcGIS para o Gestão SMS.
    /// </summary>
    public class GestaoSMSExtension : ESRI.ArcGIS.Desktop.AddIns.Extension
    {
        #region Constants

        /// <summary>
        /// Constante que possui a senha para a customização do ArcGIS.
        /// </summary>
        protected const string SenhaParaCustomizacao = "gestaosms";

        #endregion

        #region Fields

        /// <summary>
        /// Instância única da extensão.
        /// </summary>
        private static GestaoSMSExtension _instancia;

        /// <summary>
        /// Flag para indicar que o MXD corrente é válido
        /// para a aplicação.
        /// </summary>
        private bool _arquivoMXDValido;

        /// <summary>
        /// Caminho completo do MXD que é válido para a aplicação.
        /// </summary>
        private readonly string _caminhoMXDValido;

        /// <summary>
        /// A pasta de salvamento dos mapas temáticos.
        /// </summary>
        private string _pastaMapasTematicos;

        /// <summary>
        /// A pasta de salvamento dos relatórios.
        /// </summary>
        private string _pastaRelatorios;

        /// <summary>
        /// Flag para indicar se a versão do ArcGIS corrente possui
        /// licença para utilizar o Editor.
        /// </summary>
        private bool _possuiLicencaEditor;

        /// <summary>
        /// Usuário que está conectado na aplicação.
        /// </summary>
        private Usuario _usuario = null;

        /// <summary>
        /// O nome do arquivo MDB.
        /// </summary>
        private string _nomeArquivoMDB = string.Empty;

        /// <summary>
        /// Workspace que o usuário realizará conexão (o workspace pode
        /// possuir permissões de leitura e escrita).
        /// </summary>
        private IWorkspace _workspace = null;

        #endregion
        
        #region Constructors

        /// <summary>
        /// Construtor.
        /// </summary>
        public GestaoSMSExtension()
        {
            _instancia = this;

            // flag de validação do MXD
            this._arquivoMXDValido = false;

            // flag de edição
            this._possuiLicencaEditor = false;

            // inicializa configurações

            if (AddInConfiguration.Settings["ArquivoMXD"] != null)
                this._caminhoMXDValido = AddInConfiguration.Settings["ArquivoMXD"].Value;
            else
                this._caminhoMXDValido = string.Empty;

            if (AddInConfiguration.Settings["PastaMapasTematicos"] != null)
                this._pastaMapasTematicos = AddInConfiguration.Settings["PastaMapasTematicos"].Value;
            else
                this._pastaMapasTematicos = string.Empty;

            if (AddInConfiguration.Settings["PastaRelatorios"] != null)
                this._pastaRelatorios = AddInConfiguration.Settings["PastaRelatorios"].Value;
            else
                this._pastaRelatorios = string.Empty;
        }

        #endregion

        #region Static Properties

        /// <summary>
        /// Recupera a instância da extensão.
        /// </summary>
        public static GestaoSMSExtension Instance
        {
            get
            {
                if (_instancia == null)
                {
                    UID extID = new UIDClass();
                    extID.Value = ThisAddIn.IDs.GestaoSMSExtension;
                    ArcMap.Application.FindExtensionByCLSID(extID);
                }

                return _instancia;
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// Recupera o flag que indica se o MXD é válido para a 
        /// aplicação.
        /// </summary>
        public bool ArquivoMXDValido
        {
            get { return this._arquivoMXDValido; }
        }

        /// <summary>
        /// Caminho das plantas quadra.
        /// </summary>
        public string PastaMapasTematicos
        {
            get { return this._pastaMapasTematicos; }
        }

        /// <summary>
        /// Caminho dos Relatórios
        /// </summary>
        public string PastaRelatorios
        {
            get { return this._pastaRelatorios; }
        }

        /// <summary>
        /// Recupera o flag que indica se o ArcGIS possui licença para
        /// utilização do Editor.
        /// </summary>
        public bool PossuiLicencaEditor
        {
            get { return this._possuiLicencaEditor; }
        }

        /// <summary>
        /// Testa se existe um usuário logado.
        /// </summary>
        public bool PossuiUsuarioLogado
        {
            get { return this._usuario != null; }
        }

        /// <summary>
        /// Recupera o usuário logado. Retorna null se nenhum usuário
        /// estiver logado.
        /// </summary>
        public Usuario Usuario
        {
            get { return this._usuario; }
        }

        /// <summary>
        /// Recupera o nome do arquivo MDB.
        /// </summary>
        public string NomeArquivoMDB
        {
            get { return this._nomeArquivoMDB; }
        }

        /// <summary>
        /// Recupera o workspace utilizado na conexão do usuário
        /// logado com o SDE.
        /// </summary>
        public IWorkspace Workspace
        {
            get { return this._workspace; }
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// Verifica se o ArcGIS possui licença para utilização do Editor.
        /// </summary>
        //private void VerificarLicencaEditor()
        //{
        //    IESRILicenseInfo esriLicenseInfo = new ESRILicenseInfoClass();
        //    this._possuiLicencaEditor = (esriLicenseInfo.DefaultProduct == esriProductCode.esriProductCodeEditor ||
        //        esriLicenseInfo.DefaultProduct == esriProductCode.esriProductCodeProfessional);
        //}

        #endregion

        #region Public Methods

        

        public bool Login(string login, string senha, out string mensagemErro)
        {
            bool isOk = true;
            mensagemErro = string.Empty;

            // inicializa variáveis
            this._usuario = null;
            this._workspace = null;

            if (isOk)
            {
                try
                {
                    //Lista todos os usuários
                    var usuarioRepositorio = new UsuarioRepositorio();
                    var usuarios = usuarioRepositorio.Listar();

                    //procura pelo usuário com o login especificado
                    this._usuario = usuarios.Find(u => u.Login.ToLower() == login.ToLower());

                    //procura pela senha do usuario
                    var senhaUsuario = usuarios.Find(z => z.Senha.ToLower() == senha.ToLower());

                    if (this._usuario != null && senhaUsuario != null)
                        return isOk;
                        
                    else
                        mensagemErro = "Usuário ou senha inválidos.";
                        isOk = false;
                }
                catch
                {
                    mensagemErro = "Erro ao consultar o usuário no banco de dados.";
                    isOk = false;
                }
            }
            if (!isOk)
                this._usuario = null;

            return isOk;
        }

        /// <summary>
        /// Realiza logout.
        /// </summary>
        public void Logout()
        {
            if (this._usuario != null)
            {
                if (ArcMap.Application != null)
                    ArcMap.Application.CurrentTool = null;
                this._usuario = null;
            }
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Ativação dos eventos dos documentos.
        /// </summary>
        private void AtivarEventosDoDocumento()
        {
            // Ao abrir um documento devemos verificar se o documento
            // corrente é o documento que iremos utilizar. Se sim,
            // temos que inicializar o sistema.

            ArcMap.Events.OpenDocument += () =>
            {
                // documento corrente
                string mxdPath = ArcMapUtils.GetDocumentPath(ArcMap.Document);

                // flag de validação
                this._arquivoMXDValido = (string.Equals(mxdPath, this._caminhoMXDValido,
                    StringComparison.OrdinalIgnoreCase));

                if (this._arquivoMXDValido)
                {
                    this._nomeArquivoMDB = System.IO.Path.Combine(mxdPath, "GestaoSms.mdb");
                }
            };

            // Ao fechar um documento devemos verificar se o documento
            // corrente é o documento que iremos utilizar. Se sim,
            // temos que restaurar o ArcGIS.

            ArcMap.Events.CloseDocument += () =>
            {
                this._arquivoMXDValido = false;
            };

            // Antes de fechar um documento devemos verificar se o documento
            // corrente é o documento que iremos utilizar. Se sim,
            // temos que substituir o data source para read-only.

            ArcMap.Events.BeforeCloseDocument += () =>
            {
                if (this._arquivoMXDValido)
                {
                    //// finaliza a edição corrente
                    //FinalizarEdicaoCorrente();

                    //// realiza logout
                    //Logout();

                    //// salva documento
                    //ArcMap.Application.SaveDocument();
                }

                return false;
            };
        }

        #endregion

        #region Override Methods

        /// <summary>
        /// Chamado quando a extensão é iniciada.
        /// </summary>
        protected override void OnStartup()
        {
            //VerificarLicencaEditor();
            AtivarEventosDoDocumento();
        }

        /// <summary>
        /// Chamado quando a extensão é finalizada.
        /// </summary>
        protected override void OnShutdown()
        {
            base.OnShutdown();
        }

        #endregion
    }
}
