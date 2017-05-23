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
using System.Globalization;
using Microsoft.Reporting.WinForms;
using System.IO;
using System.Diagnostics;

namespace GestaoSMSAddin.Forms
{
    public partial class FrmRelatorioDoencas : FrmBase
    {
        #region Construtor

        public FrmRelatorioDoencas()
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

            if (this.cmbDoenca.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Doença] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.cmbDoenca.Focus();
                return false;
            }

            return true;

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

                //repositorio
                var incidenciaDoencaPorAno = new DoencaRepositorio();

                //recupera informações para o relatório
                var listaIncidenciaDoencaEmUnidadeSaude = incidenciaDoencaEmUnidadeSaude.ListarDoencasERegionais((int)this.cmbDoenca.SelectedValue,
                            this.dtDe.Value.Year, this.dtAte.Value.Year);

                var listaDeIncidenciaDoencaEmUnidadeSaude = listaIncidenciaDoencaEmUnidadeSaude.OrderBy(y => y.DescricaoDoenca).ToList();

                //recupera lista de incidencia da doenca por ano
                var listaIncidenciaPorAno = incidenciaDoencaPorAno.ListarIncidenciaDoencaPorAno((int)this.cmbDoenca.SelectedValue,
                            this.dtDe.Value.Year, this.dtAte.Value.Year);

                //ordena a lista por ano
                var listaDeIncidenciaPorAno = listaIncidenciaPorAno.OrderBy(w => w.Ano).ToList();

                var incidencia = listaIncidenciaPorAno.OrderBy(x => x.Incidencia).ToList();

                //define os datasources do relatório
                var localReport = new LocalReport();
                localReport.ReportEmbeddedResource = "GestaoSMSAddin.DataAccess.Reports.DoencaEmUnidadesSaudeRpt.rdlc";
                localReport.DataSources.Clear();
                localReport.DataSources.Add(new ReportDataSource("DoencaEmUnidadesSaude", listaDeIncidenciaDoencaEmUnidadeSaude));
                localReport.DataSources.Add(new ReportDataSource("Doenca", listaDeIncidenciaPorAno));
                localReport.Refresh();

                // parâmetros do relatório
                string descricaoDoenca = this.cmbDoenca.Text;
                string ano = DateTime.Now.Year.ToString();
                string soma = listaIncidenciaDoencaEmUnidadeSaude.Sum(w => w.Incidencia).ToString();

                var rptParameter1 = new ReportParameter("descricaoDoenca", descricaoDoenca);
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
                this.MostrarErro(ex);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void FrmRelatorioDoencas_Load(object sender, EventArgs e)
        {
            //Lista de Doenças no cmbDoenca
            this.ListarDoencas();
        }

        #endregion

        
    }
}
