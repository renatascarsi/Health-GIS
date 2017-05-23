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
using GestaoSMSAddin.DataAccess.Repositorios;
using GestaoSMSAddin.Utils;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Display;
using System.Globalization;

namespace GestaoSMSAddin.Forms
{
    public partial class FrmIncidenciaDoencaEmUnidadeSaude : FrmBase
    {
        #region Fields

        protected IFeatureLayer _layerDeDistritosSanitarios;
        protected IFeatureLayer _layerDeUnidadesDeSaude;
        protected IFeature _distritoSanitarioSelecionado = null;
        protected IFeatureLayer _layerDeMapaDeIncidenciaEmUnidadesDeSaude = new FeatureLayerClass();

        #endregion

        #region Construtor

        public FrmIncidenciaDoencaEmUnidadeSaude()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="layerDeLoteamentos">Os layer de distrito sanitario e unidades de saude.</param>
        public FrmIncidenciaDoencaEmUnidadeSaude(IFeatureLayer layerDeUnidadesDeSaude, IFeatureLayer layerDeDistritosSanitarios)
            : this()
        {
            this._layerDeDistritosSanitarios = layerDeDistritosSanitarios;
            this._layerDeUnidadesDeSaude = layerDeUnidadesDeSaude;
        }

        #endregion

        #region Métodos

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
            if (this.rbtnDSEspecifico.Checked == true)
            {
                if (this.cmbDSEspecifico.Text.Length == 0)
                {
                    MessageBox.Show(this, "O campo [Distrito Sanitário] é obrigatório !", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Error);
                    this.cmbDSEspecifico.Focus();
                    return false;
                }
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
            this.dtInicio.ResetText();
            this.dtFinal.ResetText();
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
        /// Lista de distritos sanitario.
        /// </summary>
        private void ListarDistritosSanitarios()
        {
            var distritoSanitarioRepositorio = new DistritoSanitarioRepositorio();

            List<DistritoSanitario> listaDistritosSanitarios = distritoSanitarioRepositorio.Listar();
            var listaDeDistritosSanitarios = listaDistritosSanitarios.OrderBy(y => y.CodigoDistritoSanitario).ToList();

            this.cmbDSEspecifico.Items.Clear();

            //binding
            this.cmbDSEspecifico.DataSource = listaDeDistritosSanitarios;
            this.cmbDSEspecifico.DisplayMember = "Nome";
            this.cmbDSEspecifico.ValueMember = "CodigoDistritoSanitario";

            if (this.cmbDSEspecifico.Items.Count > 0)
                this.cmbDSEspecifico.SelectedIndex = -1;
        }

        /// <summary>
        /// Criação de um novo shapefile contendo os mesmos atributos do layer
        /// das unidades de saúde.
        /// </summary>
        /// <param name="outputShapeFilename">O nome do shapefile.</param>
        /// <returns>Uma referência a feature class.</returns>
        private IFeatureClass CreateNewShapeFile(string outputShapeFilename)
        {
            IFeatureClass referenceFeatureClass =
                this._layerDeUnidadesDeSaude.FeatureClass;

            IGeoDataset geoDataset = this._layerDeUnidadesDeSaude as IGeoDataset;

            string folder = System.IO.Path.GetDirectoryName(outputShapeFilename);
            string fileName = System.IO.Path.GetFileNameWithoutExtension(outputShapeFilename);

            // remove arquivos existentes

            try
            {
                var files = System.IO.Directory.GetFiles(folder, fileName + ".*",
                    System.IO.SearchOption.TopDirectoryOnly);

                foreach (var file in files)
                    System.IO.File.Delete(file);
            }
            catch { }

            IWorkspaceFactory workspaceFactory = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace featureWorkspace = workspaceFactory.OpenFromFile(folder, 0) as IFeatureWorkspace;

            IFields fields = new FieldsClass();
            IFieldsEdit fieldsEdit = fields as IFieldsEdit;

            // campo com geometria

            IField field = new FieldClass();
            IFieldEdit fieldEdit = field as IFieldEdit;
            fieldEdit.Name_2 = referenceFeatureClass.ShapeFieldName;
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeGeometry;

            IGeometryDef geometryDef = new GeometryDefClass();
            IGeometryDefEdit geometryDefEdit = geometryDef as IGeometryDefEdit;

            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPoint;
            geometryDefEdit.SpatialReference_2 = geoDataset.SpatialReference;

            fieldEdit.GeometryDef_2 = geometryDef;
            fieldsEdit.AddField(field);

            // campo com nome

            field = new FieldClass();
            fieldEdit = field as IFieldEdit;
            fieldEdit.Name_2 = "nome";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeString;
            fieldsEdit.AddField(field);

            // campo com incidencia de doença

            field = new FieldClass();
            fieldEdit = field as IFieldEdit;
            fieldEdit.Name_2 = "incidencia";
            fieldEdit.Type_2 = esriFieldType.esriFieldTypeInteger;
            fieldsEdit.AddField(field);

            IFeatureClass featureClass = featureWorkspace.CreateFeatureClass(fileName, fields, null,
                    null, esriFeatureType.esriFTSimple, referenceFeatureClass.ShapeFieldName, "");

            return featureClass;
        }

        /// <summary>
        /// Converte um array de bytes (WKB) em uma geometria.
        /// </summary>
        /// <param name="wkbBytes">O array de bytes (WKIB).</param>
        /// <returns>A geometria.</returns>
        public static IGeometry WKBToGeometry(byte[] wkbBytes)
        {
            IGeometryFactory3 geometryfactory = new GeometryEnvironmentClass();
            IGeometry outputGeometry;
            int bytesRead;

            geometryfactory.CreateGeometryFromWkbVariant(wkbBytes, out outputGeometry, out bytesRead);
            return outputGeometry;
        }

        #endregion

        #region Events

        private void rbtnDSEspecifico_CheckedChanged(object sender, EventArgs e)
        {
            if (rbtnDSEspecifico.Checked == true)
            {
                cmbDSEspecifico.Enabled = true;
            }

            else
            {
                cmbDSEspecifico.Enabled = false;
                cmbDSEspecifico.ResetText();
            }
        }

        public void btnOk_Click(object sender, EventArgs e)
        {
            // valida os campos
            if (!this.ValidarCampos())
                return;

            this.Cursor = Cursors.WaitCursor;

            //enviar os parametros pra function do banco
            try
            {
                var incidenciaDoencaEmUnidadeSaude = new DoencaEmUnidadeSaudeRepositorio();

                if (cmbDoenca.SelectedItem != null && rbtnTodas.Checked == true)
                {
                    //retorna uma lista de unidades de saude com incidencia da doença escolhida
                    var listaIncidenciaDoencaEmUnidadeSaude = incidenciaDoencaEmUnidadeSaude.GerarMapaTematicoEmUnidadesDeSaude((int)this.cmbDoenca.SelectedValue,
                        this.dtInicio.Value.Year, this.dtFinal.Value.Year);
                    listaIncidenciaDoencaEmUnidadeSaude.Count.ToString();

                    //criar a Feature Class
                    IFeatureClass featureClass = CreateNewShapeFile(@"C:\Sistemas\GestaoSMS\Files\Mapas\MapaIncidencia"+cmbDoenca.Text+"EmUnidadeDeSaude" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".shp");

                    //criar uma feature buffer
                    IFeatureBuffer featureBuffer = featureClass.CreateFeatureBuffer();

                    //campo nome
                    int indexNome = featureClass.FindField("nome");

                    //campo incidências
                    int indexIncidencia = featureClass.FindField("incidencia");

                    //criar um feature cursor com buffer ativado
                    IFeatureCursor featureCursor = featureClass.Insert(true);

                    foreach (MapaDoencaEmUnidadeSaude incidencia in listaIncidenciaDoencaEmUnidadeSaude)
                    {
                        //passar de Wkb para IGeometry
                        var geometry = WKBToGeometry(incidencia.UnidadesSaude);

                        //atribuir a geometria ao buffer
                        featureBuffer.Shape = geometry;
                        featureBuffer.set_Value(indexNome, incidencia.Nome);
                        featureBuffer.set_Value(indexIncidencia, incidencia.TotalIncidencias);

                        //inserir a feature na featureclass
                        featureCursor.InsertFeature(featureBuffer);    
                    }

                    Marshal.FinalReleaseComObject(featureCursor);

                    try
                    {
                        //cria featureLayer
                        IFeatureLayer featureLayer = new FeatureLayerClass();
                        featureLayer.FeatureClass = featureClass;

                        if (featureLayer != null)
                        {

                            //cria a geofeaturelayer
                            IGeoFeatureLayer geoFeatureLayer = (IGeoFeatureLayer)featureLayer;

                            //ativar os Labels
                            //geoFeatureLayer.DisplayAnnotation = true;

                            //calculo do raio minimo das pontos
                            var minimo = (double)listaIncidenciaDoencaEmUnidadeSaude.Min(p => p.TotalIncidencias);
                            var maximo = (double)listaIncidenciaDoencaEmUnidadeSaude.Max(p => p.TotalIncidencias);
                            var n = (double)listaIncidenciaDoencaEmUnidadeSaude.Max(p => p.TotalIncidencias) / (Math.PI * Math.Pow((double)listaIncidenciaDoencaEmUnidadeSaude.Max(p => p.UnidadesSaude.LongLength), 2.0));
                            var div = minimo / n;
                            var pow = Math.Pow(div, 0.57);
                            var tamanho = pow * 0.564;

                            //cria o símbolo do marcador
                            ISimpleMarkerSymbol symbol = new SimpleMarkerSymbolClass();
                            symbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
                            symbol.Size = tamanho;
                            ISymbol symbolteste = symbol as ISymbol;

                            //Renderer de simbolos proporcionais
                            IProportionalSymbolRenderer teste = new ProportionalSymbolRendererClass();
                            teste.ValueUnit = ESRI.ArcGIS.esriSystem.esriUnits.esriUnknownUnits;
                            teste.Field = "incidencia";
                            teste.MinDataValue = listaIncidenciaDoencaEmUnidadeSaude.Min(p => p.TotalIncidencias);
                            teste.MaxDataValue = listaIncidenciaDoencaEmUnidadeSaude.Max(p => p.TotalIncidencias);
                            teste.FlanneryCompensation = false;
                            teste.LegendSymbolCount = 5;
                            teste.MinSymbol = symbolteste;
                            teste.CreateLegendSymbols();
                            geoFeatureLayer.Renderer = (IFeatureRenderer)teste;

                            //carregar o shape no mapa
                            geoFeatureLayer.FeatureClass = featureClass;
                            geoFeatureLayer.Name = featureClass.AliasName;
                            geoFeatureLayer.Visible = true;
                            ArcMap.Document.FocusMap.AddLayer(geoFeatureLayer);

                            MessageBox.Show("O mapa de incidência da doença [" + cmbDoenca.Text +
                    "] em todas as Unidade de Saúde foi gerado com sucesso!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarErro(ex);
                        MessageBox.Show("Escolha um período que contenha incidências da doença [" + cmbDoenca.Text + "].", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                    }
                }

                else if (cmbDoenca.SelectedItem != null && rbtnDSEspecifico.Checked == true)
                {
                    //retorna uma lista de unidades de saude com incidencia da doença escolhida
                    var listaIncidenciaDoencaEmUnidadeSaude = incidenciaDoencaEmUnidadeSaude.GerarMapaTematicoEmUnidadesDeSaudeDeDistritoEspecifico(
                        (int)cmbDoenca.SelectedValue, (int)cmbDSEspecifico.SelectedValue, this.dtInicio.Value.Year, this.dtFinal.Value.Year);
                    listaIncidenciaDoencaEmUnidadeSaude.Count.ToString();

                    //criar a Feature Class
                    IFeatureClass featureClass = CreateNewShapeFile(@"C:\Sistemas\GestaoSMS\Files\Mapas\MapaIncidencia" + cmbDoenca.Text + 
                        "EmUS" + cmbDSEspecifico.Text + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".shp");

                    //criar uma feature buffer
                    IFeatureBuffer featureBuffer = featureClass.CreateFeatureBuffer();

                    //campo nome
                    int indexNome = featureClass.FindField("nome");

                    //campo incidências
                    int indexIncidencia = featureClass.FindField("incidencia");

                    //criar um feature cursor com buffer ativado
                    IFeatureCursor featureCursor = featureClass.Insert(true);

                    foreach (MapaDoencaEmUnidadeSaude incidencia in listaIncidenciaDoencaEmUnidadeSaude)
                    {
                        //passar de Wkb para IGeometry
                        var geometry = WKBToGeometry(incidencia.UnidadesSaude);
                        
                        //atribuir a geometria ao buffer
                        featureBuffer.Shape = geometry;
                        featureBuffer.set_Value(indexNome, incidencia.Nome);
                        featureBuffer.set_Value(indexIncidencia, incidencia.TotalIncidencias);

                        //inserir a feature na featureclass
                        featureCursor.InsertFeature(featureBuffer);
                    }

                    Marshal.FinalReleaseComObject(featureCursor);

                    try
                    {
                        //cria featureLayer
                        IFeatureLayer featureLayer = new FeatureLayerClass();
                        featureLayer.FeatureClass = featureClass;

                        if (featureLayer != null)
                        {

                            //cria a geofeaturelayer
                            IGeoFeatureLayer geoFeatureLayer = (IGeoFeatureLayer)featureLayer;

                            //ativar os Labels
                            //geoFeatureLayer.DisplayAnnotation = true;

                            //calculo do raio minimo das pontos
                            var minimo = (double)listaIncidenciaDoencaEmUnidadeSaude.Min(p => p.TotalIncidencias);
                            var maximo = (double)listaIncidenciaDoencaEmUnidadeSaude.Max(p => p.TotalIncidencias);
                            var n = (double)listaIncidenciaDoencaEmUnidadeSaude.Max(p => p.TotalIncidencias) / (Math.PI * Math.Pow((double)listaIncidenciaDoencaEmUnidadeSaude.Max(p => p.UnidadesSaude.LongLength), 2.0));
                            var div = minimo / n;
                            var pow = Math.Pow(div, 0.57);
                            var tamanho = pow * 0.564;

                            //cria o símbolo do marcador
                            ISimpleMarkerSymbol symbol = new SimpleMarkerSymbolClass();
                            symbol.Style = esriSimpleMarkerStyle.esriSMSCircle;
                            symbol.Size = tamanho;
                            ISymbol symbolteste = symbol as ISymbol;

                            //Renderer de simbolos proporcionais
                            IProportionalSymbolRenderer teste = new ProportionalSymbolRendererClass();
                            teste.ValueUnit = ESRI.ArcGIS.esriSystem.esriUnits.esriUnknownUnits;
                            teste.Field = "incidencia";
                            teste.MinDataValue = listaIncidenciaDoencaEmUnidadeSaude.Min(p => p.TotalIncidencias);
                            teste.MaxDataValue = listaIncidenciaDoencaEmUnidadeSaude.Max(p => p.TotalIncidencias);
                            teste.FlanneryCompensation = false;
                            teste.LegendSymbolCount = 5;
                            teste.MinSymbol = symbolteste;
                            teste.CreateLegendSymbols();
                            geoFeatureLayer.Renderer = (IFeatureRenderer)teste;

                            //carregar o shape no mapa
                            geoFeatureLayer.FeatureClass = featureClass;
                            geoFeatureLayer.Name = featureClass.AliasName;
                            geoFeatureLayer.Visible = true;
                            ArcMap.Document.FocusMap.AddLayer(geoFeatureLayer);

                            MessageBox.Show("O mapa de incidência da doença [" + cmbDoenca.Text +
                    "] nas Unidades de Saúde da [" + cmbDSEspecifico.Text +
                    "] foi gerado com sucesso!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                        }
                    }
                    catch (Exception ex)
                    {
                        MostrarErro(ex);
                        MessageBox.Show("Escolha um período que contenha incidências da doença [" + cmbDoenca.Text + "] na [" + cmbDSEspecifico.Text + "].", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
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

        private void FrmIncidenciaDoencaEmUnidadeSaude_Load(object sender, EventArgs e)
        {
            //Lista de Doenças no cmbDoenca
            this.ListarDoencas();

            //Lista de Distritos Sanitarios no cmbDSEspecifico
            this.ListarDistritosSanitarios();

        }

        #endregion
    }
}
