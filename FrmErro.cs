using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GestaoSMSAddin.Forms
{
    public partial class FrmErro : Form
    {
        #region Construtor

        public FrmErro()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="ex">A exceção.</param>
        public FrmErro(Exception ex)
            : this()
        {
            string message = ex.Message;

            message = message.Replace("\r\n", "\n");
            message = message.Replace("\n", "\r\n");

            this.txtDescricao.Text = message;
            this.txtDetalhes.Text = message + "\r\n" + ex.StackTrace;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Copia os dados do erro para o Clipboard (Área de Trabalho).
        /// </summary>
        private void btnCopiar_Click(object sender, EventArgs e)
        {
            Clipboard.SetDataObject(this.txtDetalhes.Text);
        }

        #endregion
    }
}
