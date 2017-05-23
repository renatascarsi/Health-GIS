using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using GestaoSMSAddin.Utils;
using ESRI.ArcGIS.Geometry;
using GestaoSMSAddin.Modelo;
using GestaoSMSAddin.Extensions;

namespace GestaoSMSAddin.Forms
{
    public partial class FrmInserirUnidadeSaudePorCopiaGeometria : FrmBase
    {
        #region Fields

        /// <summary>
        /// Layer das unidades de saúde
        /// </summary>
        protected IFeatureLayer _layerDeUnidadesDeSaude;
        protected IFeatureLayer _layerDeBairros;
        protected IFeatureLayer _layerDeDistritos;


        /// <summary>
        /// A feature para utilizar como cópia.
        /// </summary>
        protected IFeature _featureParaCopia;

        #endregion

        #region Constructors

        public FrmInserirUnidadeSaudePorCopiaGeometria()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="layerDeBairros">O layer de bairros.</param>
        /// <param name="layerDeAreasUrbanas">O layer de áreas urbanas.</param>
        /// <param name="featureParaCopia">A feature para cópia.</param>
        public FrmInserirUnidadeSaudePorCopiaGeometria(IFeatureLayer layerDeUnidadesDeSaude, 
            IFeatureLayer layerDeBairros, IFeatureLayer layerDeDistritos,
            IFeature featureParaCopia)
            : this()
        {
            this._layerDeUnidadesDeSaude = layerDeUnidadesDeSaude;
            this._layerDeBairros = layerDeBairros;
            this._layerDeDistritos = layerDeDistritos;
            this._featureParaCopia = featureParaCopia;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Realiza validação dos campos.
        /// </summary>
        private bool ValidarCampos()
        {
            this.SuspendLayout();
            this.txtNomeUS.Text = this.txtNomeUS.Text.Trim();
            this.txtEnderecoUS.Text = this.txtEnderecoUS.Text.Trim();
            this.txtBairroUS.Text = this.txtBairroUS.Text.Trim();
            this.txtDistritoUS.Text = this.txtDistritoUS.Text.Trim();
            this.ResumeLayout();

            if (this.txtNomeUS.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Nome] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.txtNomeUS.Focus();
                return false;
            }

            try
            {
                IQueryFilter queryFilter = new QueryFilterClass();
                queryFilter.WhereClause = string.Format("nome LIKE '%{0}%'", this.txtNomeUS.Text);

                if (ArcMapUtils.GetFeatureLayerFeatureCount(this._layerDeUnidadesDeSaude, queryFilter) > 0)
                {
                    MessageBox.Show(this, "Já existe uma unidade de saúde com o nome especificado.", this.Text,
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.txtNomeUS.Focus();
                    return false;
                }
            }

            catch
            {
                MessageBox.Show(this, "Não foi possível realizar a validação do nome da unidade de saúde.", this.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            if (this.txtEnderecoUS.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Endereço] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.txtEnderecoUS.Focus();
                return false;
            }

            if (this.txtBairroUS.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Bairro] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.txtBairroUS.Focus();
                return false;
            }

            if (this.txtDistritoUS.Text.Length == 0)
            {
                MessageBox.Show(this, "O campo [Distrito Sanitário] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                this.txtDistritoUS.Focus();
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


            // workspace
            IWorkspace workspace =
                ArcMapUtils.GetFeatureLayerWorkspace(this._layerDeUnidadesDeSaude);

            IWorkspaceEdit2 workspaceEdit = workspace as IWorkspaceEdit2;

            if (workspaceEdit.IsBeingEdited())
            {
                MessageBox.Show(this, "O workspace está em edição. Aguarde.", this.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Stop);
                return;
            }

            // filtro para achar o bairro e o distrito sanitário a que a unidade de saúde pertence
            ISpatialFilter spatialFilter = new SpatialFilterClass();
            spatialFilter.Geometry = this._featureParaCopia.ShapeCopy;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            
            // pegar o código do bairro
            IFeatureCursor featCursorBairros = this._layerDeBairros.Search(spatialFilter, false);
            IFeature featureBairro = featCursorBairros.NextFeature();

            int nomeIndex = this._layerDeBairros.FeatureClass.FindField("nome");

            int codigoBairro = featureBairro.OID;
            string nomeBairro = (string)featureBairro.get_Value(nomeIndex);


            //pegar o código do distrito sanitário
            IFeatureCursor featCursorDistritos = this._layerDeDistritos.Search(spatialFilter, false);
            IFeature featureDistrito = featCursorDistritos.NextFeature();

            int nomeIndex1 = this._layerDeDistritos.FeatureClass.FindField("nome");

            int codigoDistrito = featureDistrito.OID;
            string nomeDistrito = (string)featureDistrito.get_Value(nomeIndex1);

            this.Cursor = Cursors.WaitCursor;


            //cadastra a nova unidade de saúde
            try
            {

                // inicia edição
                workspaceEdit.StartEditing(true);
                workspaceEdit.StartEditOperation();

                // dados da nova Unidade de Saúde
                UnidadeSaude unidadeDeSaude = new UnidadeSaude();
                unidadeDeSaude.Nome = txtNomeUS.Text.ToString();
                unidadeDeSaude.Endereco = txtEnderecoUS.Text.ToString();
                unidadeDeSaude.Bairro = txtBairroUS.Text.ToString();
                unidadeDeSaude.DistritoSanitario = txtDistritoUS.Text.ToString();
                unidadeDeSaude.CodigoBairro = (int)codigoBairro;
                unidadeDeSaude.CodigoDistritoSanitario = (int)codigoDistrito;
                
                // criação da nova feature
                IFeature featureUnidadeDeSaude = this._layerDeUnidadesDeSaude.FeatureClass.CreateFeature();
                featureUnidadeDeSaude.Shape = _featureParaCopia.ShapeCopy;
                unidadeDeSaude.ToFeature(featureUnidadeDeSaude);
          
                // finaliza edição
                workspaceEdit.StopEditOperation();
                workspaceEdit.StopEditing(true);

                MessageBox.Show(this, "Unidade de Saúde inserida com sucesso !", this.Text,
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                // limpa seleção
                ArcMap.Document.FocusMap.ClearSelection();

                // fecha o formulário
                this.Close();
            }
            catch
            {
                // cancela edição
                if (workspaceEdit.IsInEditOperation)
                    workspaceEdit.AbortEditOperation();
                workspaceEdit.StopEditing(false);

                MessageBox.Show(this, "Não foi possível realizar a inserção de uma nova unidade de saúde !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // refresh
                IActiveView activeView = ArcMap.Document.FocusMap as IActiveView;
                activeView.Refresh();

                // cursor
                this.Cursor = Cursors.Default;
            }


        }


        public void FrmInserirUnidadeSaudePorCopiaGeometria_Load(object sender, EventArgs e)
        {
            // workspace
            IWorkspace workspace =
                ArcMapUtils.GetFeatureLayerWorkspace(this._layerDeUnidadesDeSaude);
            IWorkspaceEdit2 workspaceEdit = workspace as IWorkspaceEdit2;

            // procurar bairro

            ISpatialFilter spatialFilter = new SpatialFilterClass();
            spatialFilter.Geometry = this._featureParaCopia.ShapeCopy;
            spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;

            IFeatureCursor featCursorBairros = this._layerDeBairros.Search(spatialFilter, false);
            IFeature featureBairro = featCursorBairros.NextFeature();
            if (featureBairro == null)
            {
                // cancela edição
                if (workspaceEdit.IsInEditOperation)
                    workspaceEdit.AbortEditOperation();
                workspaceEdit.StopEditing(false);

                MessageBox.Show(this, "O ponto não está inserido em um bairro !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                int nomeIndex = this._layerDeBairros.FeatureClass.FindField("nome");

                int codigoBairro = featureBairro.OID;
                string nomeBairro = (string)featureBairro.get_Value(nomeIndex);

                this.txtBairroUS.Text = nomeBairro;
            }

            // procurar distrito sanitário

            IFeatureCursor featCursorDistritos = this._layerDeDistritos.Search(spatialFilter, false);
            IFeature featureDistrito = featCursorDistritos.NextFeature();

            if (featureDistrito == null)
            {
                // cancela edição
                if (workspaceEdit.IsInEditOperation)
                    workspaceEdit.AbortEditOperation();
                workspaceEdit.StopEditing(false);

                MessageBox.Show(this, "O ponto não está inserido em um distrito sanitário !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            else
            {
                int nomeIndex = this._layerDeDistritos.FeatureClass.FindField("nome");

                int codigoDistrito = featureDistrito.OID;
                string nomeDistrito = (string)featureDistrito.get_Value(nomeIndex);

                this.txtDistritoUS.Text = nomeDistrito;
            }
        }

        #endregion
    }
}