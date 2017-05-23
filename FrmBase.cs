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
    public partial class FrmBase : Form
    {
        #region Construtor

        public FrmBase()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Mostra formulário de erro.
        /// </summary>
        /// <param name="ex">O objeto exception que contém o erro a ser mostrado.</param>
        protected void MostrarErro(Exception ex)
        {
            FrmErro frmErro = new FrmErro(ex);
            frmErro.ShowDialog(this);
        }

        #endregion
    }
}
