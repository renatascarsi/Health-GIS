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
    public partial class FrmRelatorioDistritoSanitario : FrmBase
    {
        #region Construtor

        public FrmRelatorioDistritoSanitario()
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

            if (this.cmbDS.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Distrito Sanitário] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.cmbDS.Focus();
                return false;
            }

            return true;

        }

        /// <summary>
        /// Lista de distritos sanitario.
        /// </summary>
        private void ListarDistritosSanitarios()
        {
            var distritoSanitarioRepositorio = new DistritoSanitarioRepositorio();

            List<DistritoSanitario> listaDistritosSanitarios = distritoSanitarioRepositorio.Listar();
            var listaDeDistritosSanitarios = listaDistritosSanitarios.OrderBy(y => y.Nome).ToList();

            this.cmbDS.Items.Clear();

            //binding
            this.cmbDS.DataSource = listaDeDistritosSanitarios;
            this.cmbDS.DisplayMember = "Nome";
            this.cmbDS.ValueMember = "CodigoDistritoSanitario";

            if (this.cmbDS.Items.Count > 0)
                this.cmbDS.SelectedIndex = -1;
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
                var incidenciaDoencaEmDistritoSanitario = new DoencaEmUnidadeSaudeRepositorio();

                //recupera informações para o relatório
                var listaIncidenciaDoencasEmDistritoSanitario = incidenciaDoencaEmDistritoSanitario.ListarDoencasDeUmDistritoSanitario((int)this.cmbDS.SelectedValue,
                            this.dtDe.Value.Year, this.dtAte.Value.Year);

                var listaDeIncidenciaDoencaEmDistritoSanitario = listaIncidenciaDoencasEmDistritoSanitario.OrderBy(y => y.DescricaoDoenca).ToList();

                //define os datasources do relatório
                var localReport = new LocalReport();
                localReport.ReportEmbeddedResource = "GestaoSMSAddin.DataAccess.Reports.DistritoSanitarioDoencasRpt.rdlc";
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("DoencasEmDistritoSanitario", listaDeIncidenciaDoencaEmDistritoSanitario));
                localReport.Refresh();

                // parâmetros do relatório
                string descricaoDoenca = this.cmbDS.Text;
                string ano = DateTime.Now.Year.ToString();
                string soma = listaIncidenciaDoencasEmDistritoSanitario.Sum(w => w.Incidencia).ToString();

                var rptParameter1 = new ReportParameter("nomeDistritoSanitario", descricaoDoenca);
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

        private void FrmRelatorioDistritoSanitario_Load(object sender, EventArgs e)
        {
            //Listas os distritos sanitarios
            this.ListarDistritosSanitarios();
        }

        #endregion

        
    }
}
