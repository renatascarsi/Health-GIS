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
    public partial class FrmCadastrarNovaDoenca : FrmBase
    {
        #region Construtor

        public FrmCadastrarNovaDoenca()
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
            this.txtNome.Text = this.txtNome.Text.Trim();

            this.ResumeLayout();

            if (this.txtNome.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Nome] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.txtNome.Focus();
                return false;
            }

            return true;
        }

        #endregion

        #region Events

        private void btnInserir_Click(object sender, EventArgs e)
        {
            
            // valida os campos
            if (!this.ValidarCampos())
                return;

            this.Cursor = Cursors.WaitCursor;

            //criar doenca
            try
            {
                var doencaRepositorio = new DoencaRepositorio();

                if (txtNome.Text != null)
                {
                    var doenca = new Doenca();
                    doenca.Nome = txtNome.Text.ToString();

                    doenca.CodigoDoenca = doencaRepositorio.Criar(doenca);

                    this.Cursor = Cursors.Default;

                    MessageBox.Show(this, "A doença [" + doenca.Nome + "] foi cadastrada com sucesso!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    this.txtNome.Focus();
                }
            }
            catch (Exception ex)
            {
                this.MostrarErro(ex);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                txtNome.Clear();
            }
        }
        #endregion
    }
}
