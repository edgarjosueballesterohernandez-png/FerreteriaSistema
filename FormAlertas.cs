using System.Data;
using MySql.Data.MySqlClient;

namespace FerreteriaSistema
{
    public class FormAlertas : Form
    {
        public FormAlertas()
        {
            Text = "Alertas de Stock Mínimo";
            ClientSize = new Size(640, 380);
            StartPosition = FormStartPosition.CenterScreen;

            var lbl = new Label { Left = 10, Top = 10, Width = 600, Height = 25 };
            var grid = new DataGridView
            {
                Left = 10,
                Top = 40,
                Width = 620,
                Height = 320,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill
            };
            Controls.AddRange(new Control[] { lbl, grid });

            try
            {
                using var conn = Db.Conectar();
                var da = new MySqlDataAdapter(
                    @"SELECT codigo, nombre, categoria, stock, stock_minimo
                      FROM productos WHERE stock <= stock_minimo ORDER BY stock ASC", conn);
                var dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
                lbl.Text = dt.Rows.Count == 0
                    ? "✓ Todos los productos tienen stock suficiente."
                    : $"⚠ {dt.Rows.Count} producto(s) con stock bajo";

                foreach (DataGridViewRow row in grid.Rows)
                    row.DefaultCellStyle.BackColor = Color.MistyRose;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }
    }
}