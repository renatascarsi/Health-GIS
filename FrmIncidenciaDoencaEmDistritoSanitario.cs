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
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.Geometry;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.CartoUI;

namespace GestaoSMSAddin.Forms
{
    public partial class FrmIncidenciaDoencaEmDistritoSanitario : FrmBase
    {
        #region Fields

        protected IFeatureLayer _layerDeDistritosSanitarios;
        protected IFeatureLayer _layerDeMapaDeIncidenciaEmUnidadesDeSaude = new FeatureLayerClass();

        #endregion

        #region Construtor

        public FrmIncidenciaDoencaEmDistritoSanitario()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Construtor.
        /// </summary>
        /// <param name="layerDeLoteamentos">O layer de distrito sanitario.</param>
        public FrmIncidenciaDoencaEmDistritoSanitario(IFeatureLayer layerDeDistritosSanitarios)
            : this()
        {
            this._layerDeDistritosSanitarios = layerDeDistritosSanitarios;
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
        /// Limpa todos os campos para um novo cadastro.
        /// </summary>
        private void LimparCampos()
        {
            this.SuspendLayout();
            this.cmbDoenca.Refresh();
            this.cmbDoenca.ResetText();
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
        /// Criação de um novo shapefile contendo os mesmos atributos do layer
        /// dos distritos sanitarios.
        /// </summary>
        /// <param name="outputShapeFilename">O nome do shapefile.</param>
        /// <returns>Uma referência a feature class.</returns>
        private IFeatureClass CreateNewShapeFile(string outputShapeFilename)
        {
            IFeatureClass referenceFeatureClass =
                this._layerDeDistritosSanitarios.FeatureClass;

            IGeoDataset geoDataset = this._layerDeDistritosSanitarios as IGeoDataset;

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

            geometryDefEdit.GeometryType_2 = esriGeometryType.esriGeometryPolygon;
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

        private void btnOK_Click(object sender, EventArgs e)
        {
            // valida os campos
            if (!this.ValidarCampos())
                return;

            this.Cursor = Cursors.WaitCursor;

            //enviar os parametros pra function do banco
            try
            {
                var incidenciaDoencaEmUnidadeSaude = new DoencaEmUnidadeSaudeRepositorio();


                //retorna uma lista de unidades de saude com incidencia da doença escolhida
                var listaIncidenciaDoencaEmDistritosSanitarios = incidenciaDoencaEmUnidadeSaude.GerarMapaTematicoEmDistritosSanitarios((int)this.cmbDoenca.SelectedValue,
                    this.dateTPDe.Value.Year, this.dateTPAte.Value.Year);
                listaIncidenciaDoencaEmDistritosSanitarios.Count.ToString();

                //criar a Feature Class
                IFeatureClass featureClass = CreateNewShapeFile(@"C:\Sistemas\GestaoSMS\Files\Mapas\MapaIncidencia" + cmbDoenca.Text + "EmDistritosSanitarios" + DateTime.Now.ToString("ddMMyyyyhhmmss") + ".shp");

                //criar uma feature buffer
                IFeatureBuffer featureBuffer = featureClass.CreateFeatureBuffer();

                //campo nome
                int indexNome = featureClass.FindField("nome");

                //campo incidências
                int indexIncidencia = featureClass.FindField("incidencia");

                //criar um feature cursor com buffer ativado
                IFeatureCursor featureCursor = featureClass.Insert(true);

                foreach (MapaDoencaEmDistritoSanitario incidencia in listaIncidenciaDoencaEmDistritosSanitarios)
                {
                    //passar de Wkb para IGeometry
                    var geometry = WKBToGeometry(incidencia.DistritosSanitarios);

                    //atribuir a geometria ao buffer
                    featureBuffer.Shape = geometry;
                    featureBuffer.set_Value(indexNome, incidencia.Nome);
                    featureBuffer.set_Value(indexIncidencia, incidencia.TotalIncidencias);

                    //inserir a feature na featureclass
                    featureCursor.InsertFeature(featureBuffer);
                }

                featureCursor.Flush();
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

                        //tabela dos dados
                        ITable table = (ITable)featureClass;

                        //define o metodo de classificacao
                        IClassifyGEN classifyGEN = new NaturalBreaks();

                        //objetos com array de frequencia de dados e valores dos dados
                        object dataFrequency;
                        object dataValues;

                        //histograma
                        ITableHistogram tableHistogram = new TableHistogramClass();
                        IHistogram histogram = (IHistogram)tableHistogram;
                        tableHistogram.Field = "incidencia";
                        tableHistogram.Table = table;
                        histogram.GetHistogram(out dataValues, out dataFrequency);
                        double[] data = dataValues as double[];
                        int[] freq = dataFrequency as int[];
                        classifyGEN.Classify(data,freq, 5);

                        //Renderer de simbolos proporcionais
                        IClassBreaksRenderer render = new ClassBreaksRenderer();
                        double[] cb = (double[])classifyGEN.ClassBreaks;
                        render.Field = "incidencia";
                        render.BreakCount = 5;
                        //render.MinimumBreak = listaIncidenciaDoencaEmDistritosSanitarios.Min(p => p.TotalIncidencias);
                        render.MinimumBreak = cb[0];

                        //define a escala de cores
                        IRgbColor color1 = new RgbColor();
                        IRgbColor color2 = new RgbColor();
                        color1.Red = 255;
                        color1.Green = 255;
                        color1.Blue = 0;
                        color2.Red = 140;
                        color2.Green = 23;
                        color2.Blue = 23;

                        IAlgorithmicColorRamp colorRamp = new AlgorithmicColorRamp();
                        colorRamp.FromColor = color1;
                        colorRamp.ToColor = color2;
                        colorRamp.Algorithm = esriColorRampAlgorithm.esriCIELabAlgorithm;
                        colorRamp.Size = 5;
                        bool ok;
                        colorRamp.CreateRamp(out ok);
                        IEnumColors enumColors = colorRamp.Colors;
                        enumColors.Reset();

                        IClassBreaksUIProperties uiProperties = (IClassBreaksUIProperties)render;
                        uiProperties.ColorRamp = "Custom";

                        //loop para definir o estilo de cada geometria de distrito sanitario
                        for (int i = 0; i < 5; i++)
                        {
                            if (i != 0)
                            {
                                render.Break[i] = cb[i + 1];
                                render.Label[i] = (cb[i] + 1).ToString("0") + " - " + cb[i + 1].ToString("0");
                                uiProperties.LowBreak[i] = cb[i] + 1;
                            }
                            else
                            {
                                render.Break[i] = cb[i + 1];
                                render.Label[i] = "0 - " + cb[i + 1].ToString("0");
                                uiProperties.LowBreak[i] = cb[i] + 1;
                            }


                            ISimpleFillSymbol simpleFillSymbol = new SimpleFillSymbol();
                            simpleFillSymbol.Color = enumColors.Next();
                            simpleFillSymbol.Style = esriSimpleFillStyle.esriSFSSolid;
                            ISymbol symbolteste = simpleFillSymbol as ISymbol;
                            render.Symbol[i] = (ISymbol)symbolteste;
                        }

                        //carregar o shape no mapa
                        geoFeatureLayer.FeatureClass = featureClass;
                        geoFeatureLayer.Name = featureClass.AliasName;
                        geoFeatureLayer.Visible = true;
                        geoFeatureLayer.Renderer = (IFeatureRenderer)render;
                        ArcMap.Document.FocusMap.AddLayer(geoFeatureLayer);
                    }

                    MessageBox.Show("O mapa de incidência da doença [" + cmbDoenca.Text +
                   "] nos Distritos Sanitários foi gerado com sucesso!", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
                catch (Exception ex)
                {
                    MostrarErro(ex);
                    MessageBox.Show("Escolha um período que contenha incidências da doença [" + cmbDoenca.Text + "].", this.Text, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
                }
            }

            catch(Exception ex)
            {
                this.MostrarErro(ex);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void FrmIncidenciaDoencaEmDistritoSanitario_Load(object sender, EventArgs e)
        {
            //Lista de Doenças no cmbDoenca
            this.ListarDoencas();
        }
        #endregion
    }
}
