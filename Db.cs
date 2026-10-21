using MySql.Data.MySqlClient;

namespace FerreteriaSistema
{
    public static class Db
    {
        public const string ConnectionString =
            "server=localhost;database=ferreteria_db;uid=root;pwd=1234;";

        public static int IdUsuario;
        public static string UsuarioActual = "";
        public static string RolActual = "";

        public static MySqlConnection Conectar()
        {
            var conn = new MySqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }
    }
}