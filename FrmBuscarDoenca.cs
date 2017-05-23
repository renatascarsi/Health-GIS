using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using GestaoSMSAddin.DataAccess.Repositorios;
using GestaoSMSAddin.DataAccess;
using GestaoSMSAddin.Modelo;
using GestaoSMSAddin.Extensions;

namespace GestaoSMSAddin.Forms
{
    public partial class FrmBuscarDoenca : FrmBase
    {
        #region Properties

        /// <summary>
        /// Lista de doenças.
        /// </summary>
        private List<DoencaEmUnidadeSaude> _listaDeIncidencias = null;


        public static string doencaBusca = "";
        public static string unidadeDeSaudeBusca = "";
        public static string incidenciaBusca = "";

        #endregion

        #region Construtor

        public FrmBuscarDoenca()
        {
            InitializeComponent();
        }

        #endregion

        #region Methods

        /// <summary>
        /// Lista de incidencias.
        /// </summary>
        private void ListarIncidencias()
        {
            var incidenciasRepositorio = new DoencaEmUnidadeSaudeRepositorio();

            List<DoencaEmUnidadeSaude> listaDeIncidencias = incidenciasRepositorio.Listar();
            var listaIncidencias = listaDeIncidencias.OrderBy(z => z.DescricaoDoenca).ToList();
            this._listaDeIncidencias = listaDeIncidencias;

            this.lvIncidenciaDoenca.Items.Clear();

            foreach (var incidencia in listaDeIncidencias)
            {
                ListViewItem lvItem = this.lvIncidenciaDoenca.Items.Add(incidencia.DescricaoDoenca.ToString());
                lvItem.SubItems.Add(incidencia.NomeUnidadeSaude.ToString());
                lvItem.SubItems.Add(incidencia.Incidencia.ToString());

            }
        }

        #endregion

        #region Events

        private void btnSelecionar_Click(object sender, EventArgs e)
        {
            if (this.lvIncidenciaDoenca.SelectedItems.Count == 0)
                return;

            try
            {
                doencaBusca = this.lvIncidenciaDoenca.SelectedItems[0].SubItems[0].Text;
                unidadeDeSaudeBusca = this.lvIncidenciaDoenca.SelectedItems[0].SubItems[1].Text;
                incidenciaBusca = this.lvIncidenciaDoenca.SelectedItems[0].SubItems[2].Text;

                FrmCadastrarIncidenciaDeDoenca frmRetornoBusca = new FrmCadastrarIncidenciaDeDoenca();
                frmRetornoBusca.ShowDialog();
            }

            catch (Exception ex)
            {
                this.MostrarErro(ex);
            }
            finally
            {
                this.Visible = false;
            }
        }

        private void FrmBuscarDoenca_Load(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;
            try
            {
                var incidenciasRepositorio = new DoencaEmUnidadeSaudeRepositorio();

                List<DoencaEmUnidadeSaude> _listaDeIncidencias = incidenciasRepositorio.Listar();
                var _listaIncidencias = _listaDeIncidencias.OrderBy(z => z.DescricaoDoenca).ToList();
            }
            catch (Exception ex)
            {
                this.MostrarErro(ex);
            }
            finally
            {
                this.Cursor = Cursors.Default;
                this.lvIncidenciaDoenca.Items.Clear();
            }
        }

        /// <summary>
        /// Evento chamado quando uma incidencia de doença é selecionada.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvIncidenciaDoenca_SelectedIndexChanged(object sender, EventArgs e)
        {
            this.btnSelecionar.Enabled = (this.lvIncidenciaDoenca.SelectedItems.Count == 1);
        }

        /// <summary>
        /// Duplo-clique para selecionar a incidência de doença
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void lvIncidenciaDoenca_DoubleClick(object sender, EventArgs e)
        {
            btnSelecionar.PerformClick();
        }

        private void btnPesquisar_Click(object sender, EventArgs e)
        {
            this.Cursor = Cursors.WaitCursor;

            string filtro = this.txtIncidencia.Text.Trim();

            try
            {
                if (filtro.IsNullEmptyOrWhiteSpace())
                    this.ListarIncidencias();
                else
                {
                    var incidenciasRepositorio = new DoencaEmUnidadeSaudeRepositorio();

                    List<DoencaEmUnidadeSaude> listaDeIncidencias = incidenciasRepositorio.Listar();
                    var listaIncidencias = listaDeIncidencias.Where(w => w.DescricaoDoenca.Contains(filtro)).OrderBy(z => z.DescricaoDoenca).ToList();
                    this._listaDeIncidencias = listaIncidencias;

                    this.lvIncidenciaDoenca.Items.Clear();

                    foreach (var incidencia in _listaDeIncidencias)
                    {
                        ListViewItem lvItem = this.lvIncidenciaDoenca.Items.Add(incidencia.DescricaoDoenca.ToString());
                        lvItem.SubItems.Add(incidencia.NomeUnidadeSaude.ToString());
                        lvItem.SubItems.Add(incidencia.Incidencia.ToString());
                    }
                }

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
