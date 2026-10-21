using MySql.Data.MySqlClient;

namespace FerreteriaSistema
{
    public class FormLogin : Form
    {
        TextBox txtUser = new TextBox();
        TextBox txtPass = new TextBox();

        public FormLogin()
        {
            Text = "Ferretería - Iniciar sesión";
            ClientSize = new Size(300, 170);
            StartPosition = FormStartPosition.CenterScreen;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox = false;

            var lblUser = new Label { Text = "Usuario:", Left = 20, Top = 25, Width = 70 };
            var lblPass = new Label { Text = "Password:", Left = 20, Top = 65, Width = 70 };
            txtUser.SetBounds(100, 22, 170, 25);
            txtPass.SetBounds(100, 62, 170, 25);
            txtPass.UseSystemPasswordChar = true;

            var btn = new Button { Text = "Ingresar", Left = 100, Top = 110, Width = 100 };
            btn.Click += Ingresar;
            AcceptButton = btn;

            Controls.AddRange(new Control[] { lblUser, lblPass, txtUser, txtPass, btn });
        }

        void Ingresar(object? s, EventArgs e)
        {
            try
            {
                using var conn = Db.Conectar();
                using var cmd = new MySqlCommand(
                    "SELECT id, rol FROM usuarios WHERE username=@u AND password=@p", conn);
                cmd.Parameters.AddWithValue("@u", txtUser.Text);
                cmd.Parameters.AddWithValue("@p", txtPass.Text);
                using var r = cmd.ExecuteReader();
                if (r.Read())
                {
                    Db.IdUsuario = r.GetInt32("id");
                    Db.RolActual = r.GetString("rol");
                    Db.UsuarioActual = txtUser.Text;
                    DialogResult = DialogResult.OK;
                    Close();
                }
                else
                {
                    MessageBox.Show("Credenciales incorrectas", "Error",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error de conexión: " + ex.Message);
            }
        }
    }
}