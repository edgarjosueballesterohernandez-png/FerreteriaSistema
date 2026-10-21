namespace FerreteriaSistema
{
    public class FormPrincipal : Form
    {
        public FormPrincipal()
        {
            Text = $"Sistema Ferretería - {Db.UsuarioActual} ({Db.RolActual})";
            ClientSize = new Size(320, 260);
            StartPosition = FormStartPosition.CenterScreen;

            var btnProd = Boton("Gestionar Productos", 30);
            var btnVenta = Boton("Registrar Venta", 85);
            var btnAlert = Boton("Alertas de Stock", 140);
            var btnSalir = Boton("Salir", 195);

            btnProd.Click += (s, e) =>
            {
                if (Db.RolActual != "admin")
                {
                    MessageBox.Show("Solo el administrador puede gestionar productos.");
                    return;
                }
                new FormProductos().ShowDialog();
            };
            btnVenta.Click += (s, e) => new FormVenta().ShowDialog();
            btnAlert.Click += (s, e) => new FormAlertas().ShowDialog();
            btnSalir.Click += (s, e) => Close();

            Controls.AddRange(new Control[] { btnProd, btnVenta, btnAlert, btnSalir });
        }

        Button Boton(string texto, int top) =>
            new Button { Text = texto, Left = 50, Top = top, Width = 220, Height = 40 };
    }
}