using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GestaoSMSAddin.DataAccess.Repositorios;
using GestaoSMSAddin.Modelo;
using GestaoSMSAddin.DataAccess;
using Microsoft.Reporting.WinForms;
using System.IO;
using System.Diagnostics;

namespace GestaoSMSAddin.Forms
{
    public partial class FrmRelatorioUnidadeSaude : FrmBase
    {
        #region Construtor

        public FrmRelatorioUnidadeSaude()
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

            if (this.cmbUS.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Unidade de Saúde] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.cmbUS.Focus();
                return false;
            }

            return true;

        }

        /// <summary>
        /// Lista de doenças.
        /// </summary>
        private void ListarUnidadesSaude()
        {
            var unidadeDeSaudeRepositorio = new UnidadeSaudeRepositorio();

            List<UnidadeSaude> listaUnidadesSaude = unidadeDeSaudeRepositorio.Listar();
            var listaDeUnidadesSaude = listaUnidadesSaude.OrderBy(y => y.Nome).ToList();

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

        private void btnOK_Click(object sender, EventArgs e)
        {
            // valida os campos
            if (!this.ValidarCampos())
                return;

            this.Cursor = Cursors.WaitCursor;

            try
            {
                //repositório
                var incidenciaDoencaEmUnidadeSaude = new DoencaEmUnidadeSaudeRepositorio();

                //recupera informações para o relatório
                var listaIncidenciaDoencasEmUnidadeSaude = incidenciaDoencaEmUnidadeSaude.ListarDoencasDeUmaUnidadeSaude((int)this.cmbUS.SelectedValue,
                            this.dtDe.Value.Year, this.dtAte.Value.Year);

                //ordena a lista
                var listaDeIncidenciaDoencaEmUnidadeSaude = listaIncidenciaDoencasEmUnidadeSaude.OrderBy(y => y.DescricaoDoenca).ToList();

                //define os datasources do relatório
                var localReport = new LocalReport();
                localReport.ReportEmbeddedResource = "GestaoSMSAddin.DataAccess.Reports.UnidadeSaudeDoencasRpt.rdlc";
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("DoencaEmUnidadeSaude", listaIncidenciaDoencasEmUnidadeSaude));
                localReport.Refresh();

                // parâmetros do relatório
                string descricaoUS = this.cmbUS.Text;
                string ano = DateTime.Now.Year.ToString();
                string soma = listaIncidenciaDoencasEmUnidadeSaude.Sum(w => w.Incidencia).ToString();

                var rptParameter1 = new ReportParameter("nomeUnidadeSaude", descricaoUS);
                var rptParameter2 = new ReportParameter("ano", ano);
                var rptParameter3 = new ReportParameter("somaIncidencia", soma);

                localReport.SetParameters(
                    new ReportParameter[] { 
                        rptParameter1, rptParameter2, rptParameter3});

                if (this.dtDe.Value.Year == this.dtAte.Value.Year)
                {
                    string anoIncidencia = this.dtAte.Value.Year.ToString();
                    var rptParameter4 = new ReportParameter("anoIncidencia", anoIncidencia);
                    localReport.SetParameters(
                    new ReportParameter[] { 
                        rptParameter4});
                }
                else
                {
                    string anoIncidencia = this.dtDe.Value.Year.ToString() + " - " + this.dtAte.Value.Year.ToString();
                    var rptParameter4 = new ReportParameter("anoIncidencia", anoIncidencia);
                    localReport.SetParameters(
                    new ReportParameter[] { 
                        rptParameter4});
                }

                //nome do relatório
                string nomePdfRelatorio =
                            System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString() + ".pdf");

                //criação de arquivo do relatório
                byte[] pdfBytes = localReport.Render("PDF");
                var binaryWriter = new BinaryWriter(new FileStream(nomePdfRelatorio, FileMode.CreateNew, FileAccess.Write));
                binaryWriter.Write(pdfBytes, 0, pdfBytes.Length);
                binaryWriter.Close();

                //mostra o arquivo de relatório ao usuário
                Process.Start(nomePdfRelatorio);
            }
            catch (Exception ex)
            {
                MostrarErro(ex);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void FrmRelatorioUnidadeSaude_Load(object sender, EventArgs e)
        {
            //Listas as unidades de saude
            this.ListarUnidadesSaude();
        }

        #endregion

        
    }
}
