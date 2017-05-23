namespace GestaoSMSAddin.Modelo
{
    public class Usuario
    {
        public int CodigoUsuario { get; set; }
        public string Nome { get; set; }
        public string Login { get; set; }
        public string Senha { get; set; }
        public TipoUsuarioEnum Tipo { get; set; }
    }
}
