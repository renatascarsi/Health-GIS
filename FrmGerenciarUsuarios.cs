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
using System.Reflection;

namespace GestaoSMSAddin.Forms
{
    public partial class FrmGerenciarUsuarios : FrmBase
    {
        #region Properties

        protected Usuario _usuario = null;
        private List<Usuario> _listaDeUsuarios = null;

        #endregion

        #region Construtor

        public FrmGerenciarUsuarios()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Valida os campos.
        /// </summary>
        /// <returns>True se os campos foram validados, e false se um erro foi encontrado.</returns>
        private bool ValidarCampos()
        {
            this.SuspendLayout();

            if (this.txtNomeUsuario.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Nome Usuário] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.txtNomeUsuario.Focus();
                return false;
            }

            if (this.txtLoginUsuario.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Login] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.txtLoginUsuario.Focus();
                return false;
            }

            if (this.txtSenhaUsuario.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Senha] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.txtSenhaUsuario.Focus();
                return false;
            }

            return true;
        }

        /// <summary>
        /// Limpa todos os campos para um novo cadastro.
        /// </summary>
        private void LimparCampos()
        {
            this.SuspendLayout();
            this.txtNomeUsuario.Clear();
            this.txtLoginUsuario.Clear();
            this.txtSenhaUsuario.Clear();
            this.ResumeLayout();
        }

        /// <summary>
        /// Lista de usuários.
        /// </summary>
        private void ListarUsuarios(int codigoUsuarioSelecionado = -1)
        {
            this.lvUsuarios.BeginUpdate();
            this.lvUsuarios.Items.Clear();

            try
            {
                var usuariosRepositorio = new UsuarioRepositorio();

                List<Usuario> listaDeUsuarios = usuariosRepositorio.Listar();
                var listaUsuarios = listaDeUsuarios.OrderBy(z => z.Nome).ToList();
                this._listaDeUsuarios = listaDeUsuarios;

                this.lvUsuarios.Items.Clear();

                foreach (var usuario in listaDeUsuarios)
                {
                    ListViewItem lvItem = this.lvUsuarios.Items.Add(usuario.Nome.ToString());
                    lvItem.SubItems.Add(usuario.Login.ToString());
                    lvItem.Tag = usuario.CodigoUsuario;
                }
            }
            catch (Exception ex)
            {
                this.MostrarErro(ex);
            }
            finally
            {
                this.lvUsuarios.EndUpdate();
            }

            if (codigoUsuarioSelecionado != -1)
            {
                var lvItems = this.lvUsuarios.Items.Find(codigoUsuarioSelecionado.ToString(), true);
                if (lvItems.Length > 0)
                    lvItems[0].Selected = true;
            }
        }

        #endregion

        #region Events

        private void FrmGerenciarUsuarios_Load(object sender, EventArgs e)
        {
            //listagem de usuários
            this.ListarUsuarios();
        }

        private void btnIncluirUsuario_Click(object sender, EventArgs e)
        {
            // valida os campos
            if (!this.ValidarCampos())
                return;

            this.Cursor = Cursors.WaitCursor;

            try
            {

                // repositório
                var usuarioRepositorio = new UsuarioRepositorio();

                // verifica se o usuário já existe

                var listaDeUsuarios =
                    usuarioRepositorio.Listar();

                if (listaDeUsuarios
                    .Count(u => u.Login.Equals(this.txtLoginUsuario.Text, StringComparison.InvariantCultureIgnoreCase)) > 0)
                {
                    MessageBox.Show(this, "Já existe um usuário com o mesmo login. Entre com um novo usuário.",
                        this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.txtLoginUsuario.Focus();
                    return;
                }

                //cria nova instância

                this._usuario = new Usuario();
                this._usuario.Nome = txtNomeUsuario.Text;
                this._usuario.Login = txtLoginUsuario.Text;
                this._usuario.Senha = txtSenhaUsuario.Text;

                //cria o usuário
                this._usuario.CodigoUsuario = usuarioRepositorio.Inserir(this._usuario);

                //mostra mensagem
                MessageBox.Show("O usuário [" + txtNomeUsuario.Text +
                    "] foi cadastrado com o login [" + txtLoginUsuario.Text + "]!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);

            }
            catch
            {
                this._usuario = null;
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.LimparCampos();
                this.ListarUsuarios();
            }
        }

        private void btnAlterarUsuario_Click(object sender, EventArgs e)
        {
            // valida os campos
            if (!this.ValidarCampos())
                return;

            this.Cursor = Cursors.WaitCursor;

            try
            {
                //alterar usuário
                var usuarioRepositorio = new UsuarioRepositorio();
                

                //verifica se o usuário já existe
                var listaDeUsuarios = usuarioRepositorio.Listar();

                if (listaDeUsuarios.Count(u =>
                        u.Login.Equals(this.txtLoginUsuario.Text, StringComparison.InvariantCultureIgnoreCase) &&
                        u.CodigoUsuario != this._usuario.CodigoUsuario) > 0)
                {
                    MessageBox.Show(this, "Já existe um usuário com o mesmo login. Entre com um novo usuário.",
                        this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                //alteração do usuário
                this._usuario.Nome = txtNomeUsuario.Text;
                this._usuario.Login = txtLoginUsuario.Text;
                this._usuario.Senha = txtSenhaUsuario.Text;

                usuarioRepositorio.Alterar(this._usuario);

                MessageBox.Show("O usuário [" + txtNomeUsuario.Text +
                    "] foi alterado com sucesso!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                this.MostrarErro(ex);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.LimparCampos();
                this.ListarUsuarios();
            }

        }

        private void btnExcluirUsuario_Click(object sender, EventArgs e)
        {
            // valida os campos
            if (!this.ValidarCampos())
                return;

            this.Cursor = Cursors.WaitCursor;

            try
            {
                //excluir usuário
                var usuarioRepositorio = new UsuarioRepositorio();

                //exclusão do usuário

                this._usuario.Nome = txtNomeUsuario.Text;
                this._usuario.Login = txtLoginUsuario.Text;
                this._usuario.Senha = txtSenhaUsuario.Text;

                usuarioRepositorio.Excluir(this._usuario);

                MessageBox.Show("O usuário [" + txtNomeUsuario.Text +
                    "] foi removido com sucesso!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                this.MostrarErro(ex);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.LimparCampos();
                this.ListarUsuarios();
            }
        }

        private void lvUsuarios_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.lvUsuarios.SelectedItems.Count == 0)
                return;

            this.Cursor = Cursors.WaitCursor;

            try
            {
                // repositório
                var usuarioRepositorio = new UsuarioRepositorio();

                // recupera o usuário

                int codigoUsuario =
                    Convert.ToInt32(this.lvUsuarios.SelectedItems[0].Tag);

                this._usuario = usuarioRepositorio.RecuperarPorId(codigoUsuario);

                this.txtNomeUsuario.Text = this._usuario.Nome;
                this.txtLoginUsuario.Text = this._usuario.Login;
                this.txtSenhaUsuario.Text = this._usuario.Senha;
            }
            catch (Exception ex)
            {
                this.MostrarErro(ex);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        #endregion
    }
}
