using System.Data;
using MySql.Data.MySqlClient;

namespace FerreteriaSistema
{
    public class FormProductos : Form
    {
        DataGridView grid = new DataGridView();
        TextBox txtCodigo = new TextBox(), txtNombre = new TextBox(), txtCategoria = new TextBox();
        TextBox txtPrecio = new TextBox(), txtStock = new TextBox(), txtStockMin = new TextBox();

        public FormProductos()
        {
            Text = "Gestión de Productos";
            ClientSize = new Size(760, 520);
            StartPosition = FormStartPosition.CenterScreen;

            grid.SetBounds(10, 10, 740, 300);
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.CellClick += (s, e) => { if (e.RowIndex >= 0) LlenarCampos(); };

            Campo("Código:", txtCodigo, 10, 325);
            Campo("Nombre:", txtNombre, 250, 325);
            Campo("Categoría:", txtCategoria, 500, 325);
            Campo("Precio:", txtPrecio, 10, 375);
            Campo("Stock:", txtStock, 250, 375);
            Campo("Stock mín.:", txtStockMin, 500, 375);

            var btnAdd = Boton("Agregar", 10);
            var btnUpd = Boton("Actualizar stock", 160);
            var btnDel = Boton("Eliminar", 310);
            var btnRef = Boton("Refrescar", 460);
            btnAdd.Click += (s, e) => Agregar();
            btnUpd.Click += (s, e) => ActualizarStock();
            btnDel.Click += (s, e) => Eliminar();
            btnRef.Click += (s, e) => Cargar();

            Controls.Add(grid);
            Controls.AddRange(new Control[] { btnAdd, btnUpd, btnDel, btnRef });
            Cargar();
        }

        void Campo(string etiqueta, TextBox txt, int left, int top)
        {
            Controls.Add(new Label { Text = etiqueta, Left = left, Top = top + 3, Width = 70 });
            txt.SetBounds(left + 75, top, 140, 25);
            Controls.Add(txt);
        }

        Button Boton(string texto, int left) =>
            new Button { Text = texto, Left = left, Top = 430, Width = 140, Height = 35 };

        void Cargar()
        {
            try
            {
                using var conn = Db.Conectar();
                var da = new MySqlDataAdapter(
                    "SELECT codigo, nombre, categoria, precio, stock, stock_minimo " +
                    "FROM productos ORDER BY categoria, nombre", conn);
                var dt = new DataTable();
                da.Fill(dt);
                grid.DataSource = dt;
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        void LlenarCampos()
        {
            var row = grid.CurrentRow;
            if (row == null) return;
            txtCodigo.Text = row.Cells["codigo"].Value?.ToString();
            txtNombre.Text = row.Cells["nombre"].Value?.ToString();
            txtCategoria.Text = row.Cells["categoria"].Value?.ToString();
            txtPrecio.Text = row.Cells["precio"].Value?.ToString();
            txtStock.Text = row.Cells["stock"].Value?.ToString();
            txtStockMin.Text = row.Cells["stock_minimo"].Value?.ToString();
        }

        void Agregar()
        {
            if (!decimal.TryParse(txtPrecio.Text, out var precio) ||
                !int.TryParse(txtStock.Text, out var stock) ||
                !int.TryParse(txtStockMin.Text, out var stockMin) ||
                string.IsNullOrWhiteSpace(txtCodigo.Text) ||
                string.IsNullOrWhiteSpace(txtNombre.Text))
            {
                MessageBox.Show("Revisa los datos: código, nombre y valores numéricos válidos.");
                return;
            }
            try
            {
                using var conn = Db.Conectar();
                using var cmd = new MySqlCommand(
                    @"INSERT INTO productos (codigo, nombre, categoria, precio, stock, stock_minimo)
                      VALUES (@c, @n, @cat, @p, @s, @sm)", conn);
                cmd.Parameters.AddWithValue("@c", txtCodigo.Text);
                cmd.Parameters.AddWithValue("@n", txtNombre.Text);
                cmd.Parameters.AddWithValue("@cat", txtCategoria.Text);
                cmd.Parameters.AddWithValue("@p", precio);
                cmd.Parameters.AddWithValue("@s", stock);
                cmd.Parameters.AddWithValue("@sm", stockMin);
                cmd.ExecuteNonQuery();
                MessageBox.Show("Producto agregado");
                Cargar();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        void ActualizarStock()
        {
            if (!int.TryParse(txtStock.Text, out var stock) || string.IsNullOrWhiteSpace(txtCodigo.Text))
            {
                MessageBox.Show("Selecciona un producto e ingresa un stock válido.");
                return;
            }
            try
            {
                using var conn = Db.Conectar();
                using var cmd = new MySqlCommand("UPDATE productos SET stock=@s WHERE codigo=@c", conn);
                cmd.Parameters.AddWithValue("@s", stock);
                cmd.Parameters.AddWithValue("@c", txtCodigo.Text);
                MessageBox.Show(cmd.ExecuteNonQuery() > 0 ? "Stock actualizado" : "Producto no encontrado");
                Cargar();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        void Eliminar()
        {
            if (string.IsNullOrWhiteSpace(txtCodigo.Text)) { MessageBox.Show("Selecciona un producto."); return; }
            if (MessageBox.Show("¿Eliminar este producto?", "Confirmar",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;
            try
            {
                using var conn = Db.Conectar();
                using var cmd = new MySqlCommand("DELETE FROM productos WHERE codigo=@c", conn);
                cmd.Parameters.AddWithValue("@c", txtCodigo.Text);
                MessageBox.Show(cmd.ExecuteNonQuery() > 0 ? "Producto eliminado" : "Producto no encontrado");
                Cargar();
            }
            catch (Exception ex)
            {
                MessageBox.Show("No se pudo eliminar (puede tener ventas asociadas): " + ex.Message);
            }
        }
    }
}