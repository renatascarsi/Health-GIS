using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Linq;
using GestaoSMSAddin.Forms;

namespace GestaoSMSAddin
{
    public partial class FrmLogin : FrmBase
    {

        #region Constructor

        public FrmLogin()
        {
            InitializeComponent();
#if DEBUG
            this.txtLogin.Text = "usuario";
            this.txtSenha.Text = "senha";
#endif
        }

        #endregion

        #region Events

        /// <summary>
        /// Controla visibilidade do botão Login com base no preenchimento dos campos texto.
        /// </summary>
        private void control_Changed(object sender, EventArgs e)
        {
            this.btnLogin.Enabled = (this.txtLogin.Text.Length > 0 && this.txtSenha.Text.Length > 0);
        }

        /// <summary>
        /// Realiza o login do usuário.
        /// </summary>
        private void btnLogin_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            this.AcceptButton = btnLogin;

            string mensagemErro = string.Empty;
            string login = txtLogin.Text;
            string senha = txtSenha.Text;


            try
            {
                if (GestaoSMSExtension.Instance.Login(login,senha, out mensagemErro))
                {
                    this.DialogResult = System.Windows.Forms.DialogResult.OK;
                    this.Close();
                }
                else
                {
                    MessageBox.Show("Usuário e/ou senha inválido(s).");
                }
            }

            catch (Exception ex)
            {
                this.MostrarErro(ex);
            }

            return;
        }

        #endregion

    }
}
