using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using System.IO;
using GestaoSMSAddin.Modelo;
using GestaoSMSAddin.Extensions;
using GestaoSMSAddin.DataAccess;
using GestaoSMSAddin.DataAccess.Repositorios;


namespace GestaoSMSAddin.Forms
{
    public partial class FrmCadastrarIncidenciaDeDoenca : FrmBase
    {
        #region Construtor

        public FrmCadastrarIncidenciaDeDoenca()
        {
            InitializeComponent();
            cmbDoenca.Refresh();
            cmbUS.Refresh();
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

            if (this.cmbDoenca.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Doença] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.cmbDoenca.Focus();
                return false;
            }

            if (this.cmbUS.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Unidade de Saúde] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.cmbUS.Focus();
                return false;
            }

            if (this.txtNumeroCasos.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Número de Casos] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.txtNumeroCasos.Focus();
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
            this.cmbDoenca.Refresh();
            this.cmbDoenca.ResetText();
            this.cmbUS.Refresh();
            this.cmbUS.ResetText();
            this.txtNumeroCasos.Clear();
            this.ResumeLayout();
        }

        /// <summary>
        /// Lista de doenças.
        /// </summary>
        private void ListarDoencas()
        {
            var doencaRepositorio = new DoencaRepositorio();

            List<Doenca> listaDoencas = doencaRepositorio.Listar();
            var listaDeDoencas = listaDoencas.OrderBy(y => y.Nome).ToList();

            this.cmbDoenca.Items.Clear();

            //binding
            this.cmbDoenca.DataSource = listaDeDoencas;
            this.cmbDoenca.DisplayMember = "Nome";
            this.cmbDoenca.ValueMember = "CodigoDoenca";

            if (this.cmbDoenca.Items.Count > 0)
                this.cmbDoenca.SelectedIndex = -1;
        }

        /// <summary>
        /// Lista de unidades de saude.
        /// </summary>
        private void ListarUnidadesSaude()
        {
            var unidadeSaudeRepositorio = new UnidadeSaudeRepositorio();

            List<UnidadeSaude> lista = unidadeSaudeRepositorio.Listar();
            var listaDeUnidadesSaude = lista.OrderBy(x => x.Nome).ToList();

            this.cmbUS.Items.Clear();
            
            //binding
            this.cmbUS.DataSource = listaDeUnidadesSaude;
            this.cmbUS.DisplayMember = "Nome";
            this.cmbUS.ValueMember = "CodigoUnidadeSaude";

            if (this.cmbUS.Items.Count > 0)
                this.cmbUS.SelectedIndex = -1;
        }

        #endregion

        #region Events

        private void FrmCadastrarIncidenciaDeDoenca_Load(object sender, EventArgs e)
        {
            //limpar campos
            this.LimparCampos();

            //listagem de doencas
            this.ListarDoencas();

            //listagem de doencas
            this.ListarUnidadesSaude();

            //retorno da busca da incidencia de doença
            cmbDoenca.Text = FrmBuscarDoenca.doencaBusca;
            cmbUS.Text = FrmBuscarDoenca.unidadeDeSaudeBusca;
            txtNumeroCasos.Text = FrmBuscarDoenca.incidenciaBusca;
        }

        /// <summary>
        /// Cadastrar uma incidencia de doenca em uma unidade de saude no banco de dados
        /// </summary>
        public void btnIncluirIncidenciaDoenca_Click(object sender, EventArgs e)
        {

            // valida os campos
            if (!this.ValidarCampos())
                return;

            this.Cursor = Cursors.WaitCursor;

            //criar incidencia de doenca
            try
            {
                var incidenciaDoencaEmUnidadeSaude = new DoencaEmUnidadeSaudeRepositorio();

                if (txtNumeroCasos != null)
                {
                    var incidenciaDoenca = new DoencaEmUnidadeSaude();
                    incidenciaDoenca.Incidencia = txtNumeroCasos.Text.ToInt32();
                    incidenciaDoenca.CodigoUnidadeSaude = (int)this.cmbUS.SelectedValue;
                    incidenciaDoenca.CodigoDoenca = (int)this.cmbDoenca.SelectedValue;
                    incidenciaDoenca.Incidencia = incidenciaDoencaEmUnidadeSaude.Criar(incidenciaDoenca);
                }

                this.Cursor = Cursors.Default;

                MessageBox.Show("A incidência da doença [" + cmbDoenca.Text +
                    "] na Unidade de Saúde [" + cmbUS.Text + "] foi cadastrada com sucesso!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                this.MostrarErro(ex);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.LimparCampos();
            }
        }

        public void btnAlterarDoenca_Click(object sender, EventArgs e)
        {
            // valida os campos
            if (!this.ValidarCampos())
                return;

            this.Cursor = Cursors.WaitCursor;

            //alterar incidencia de doenca

            try
            {
                var incidenciaDoencaEmUnidadeSaude = new DoencaEmUnidadeSaudeRepositorio();

                if (txtNumeroCasos != null)
                {
                    var incidenciaDoenca = new DoencaEmUnidadeSaude();
                    incidenciaDoenca.Incidencia = txtNumeroCasos.Text.ToInt32();
                    incidenciaDoenca.CodigoUnidadeSaude = (int)this.cmbUS.SelectedValue;
                    incidenciaDoenca.CodigoDoenca = (int)this.cmbDoenca.SelectedValue;
                    incidenciaDoenca.Incidencia = incidenciaDoencaEmUnidadeSaude.Alterar(incidenciaDoenca);
                }

                this.Cursor = Cursors.Default;

                MessageBox.Show("A incidência da doença [" + cmbDoenca.Text +
                    "] na Unidade de Saúde [" + cmbUS.Text + "] foi alterada com sucesso!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                this.MostrarErro(ex);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.LimparCampos();
            }
        }

        private void btnBuscar_Click(object sender, EventArgs e)
        {
            this.Visible = false;

            var frmBuscarDoenca = new GestaoSMSAddin.Forms.FrmBuscarDoenca();
            frmBuscarDoenca.ShowDialog(new ArcMapWindow(ArcMap.Application));
        }

        /// <summary>
        /// Valida apenas numeros
        /// </summary>
        private void txtNumeroCasos_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsNumber(e.KeyChar))
            {
                e.Handled = true;
                MessageBox.Show(this, "O campo [Número de Casos] deve ser preenchido apenas com números !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.txtNumeroCasos.Focus();

            }
        }

        private void btnExcluirDoenca_Click(object sender, EventArgs e)
        {
            // valida os campos
            if (!this.ValidarCampos())
                return;

            this.Cursor = Cursors.WaitCursor;

            //excluir incidencia de doenca
            try
            {
                var incidenciaDoencaEmUnidadeSaude = new DoencaEmUnidadeSaudeRepositorio();

                if (txtNumeroCasos != null)
                {
                    var incidenciaDoenca = new DoencaEmUnidadeSaude();
                    incidenciaDoenca.Incidencia = txtNumeroCasos.Text.ToInt32();
                    incidenciaDoenca.CodigoUnidadeSaude = (int)this.cmbUS.SelectedValue;
                    incidenciaDoenca.CodigoDoenca = (int)this.cmbDoenca.SelectedValue;
                    incidenciaDoenca.Incidencia = incidenciaDoencaEmUnidadeSaude.Excluir(incidenciaDoenca);
                }

                this.Cursor = Cursors.Default;

                MessageBox.Show("A incidência da doença [" + cmbDoenca.Text +
                    "] na Unidade de Saúde [" + cmbUS.Text + "] foi excluída com sucesso!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
            }
            catch (Exception ex)
            {
                this.MostrarErro(ex);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.LimparCampos();
            }
        }

        #endregion
    }
}
