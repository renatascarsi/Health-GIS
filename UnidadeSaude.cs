using ESRI.ArcGIS.Geodatabase;
namespace GestaoSMSAddin.Modelo
{
    public class UnidadeSaude
    {
        #region Properties

        public int CodigoUnidadeSaude { get; set; }
        public int CodigoBairro { get; set; }
        public int CodigoDistritoSanitario { get; set; }
        public string Nome { get; set; }
        public string Endereco { get; set; }
        public string Bairro { get; set; }
        public string DistritoSanitario { get; set; }

        #endregion

        #region Methods

        /// <summary>
        /// Converte os dados do bairro nos dados da feição.
        /// </summary>
        /// <param name="unidadeSaude">O bairro.</param>
        /// <param name="feature">A feição.</param>
        /// <param name="storeFeature">Se true, armazena a feição via Store, caso contrário não
        /// armazena a feature.</param>

        internal void ToFeature(IFeature feature, bool storeFeature = true)
        {
            IFeatureClass featureClass = feature.Class as IFeatureClass;

            feature.Value[featureClass.FindField("nome")] = this.Nome;
            feature.Value[featureClass.FindField("nomerua")] = this.Endereco;
            feature.Value[featureClass.FindField("bairro")] = this.Bairro;
            feature.Value[featureClass.FindField("regional")] = this.DistritoSanitario;
            feature.Value[featureClass.FindField("codigobairro")] = this.CodigoBairro;
            feature.Value[featureClass.FindField("codigodistritosanitario")] = this.CodigoDistritoSanitario;

            if (storeFeature)
                feature.Store();
        }

        #endregion
    }
}