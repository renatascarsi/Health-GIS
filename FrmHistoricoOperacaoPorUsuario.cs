using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GestaoSMSAddin.DataAccess;
using GestaoSMSAddin.Modelo;

namespace GestaoSMSAddin.Forms
{
    public partial class FrmHistoricoOperacaoPorUsuario : FrmBase
    {
        #region Properties

        protected Usuario _usuario = null;

        #endregion

        #region Construtor

        public FrmHistoricoOperacaoPorUsuario()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        private bool ValidarCampos()
        {
            this.SuspendLayout();

            if (this.cmbUsuario.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Usuário] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.cmbUsuario.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Lista de doenças.
        /// </summary>
        private void ListarUsuarios()
        {
            var usuarioRepositorio = new UsuarioRepositorio();

            List<Usuario> listaUsuarios = usuarioRepositorio.Listar();
            var listaDeUsuarios = listaUsuarios.OrderBy(y => y.Nome).ToList();

            this.cmbUsuario.Items.Clear();

            //binding
            this.cmbUsuario.DataSource = listaDeUsuarios;
            this.cmbUsuario.DisplayMember = "Nome";
            this.cmbUsuario.ValueMember = "CodigoUsuario";

            if (this.cmbUsuario.Items.Count > 0)
                this.cmbUsuario.SelectedIndex = -1;
        }

        #endregion

        #region Events

        private void btnOK_Click(object sender, EventArgs e)
        {
            // valida os campos
            if (!this.ValidarCampos())
                return;

            this.Cursor = Cursors.WaitCursor;
        }

        #endregion

        private void FrmHistoricoOperacaoPorUsuario_Load(object sender, EventArgs e)
        {
            //Listar os usuarios no cmbUsuario
            this.ListarUsuarios();
        }
    }
}
