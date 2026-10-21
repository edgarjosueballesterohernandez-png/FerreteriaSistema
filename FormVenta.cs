using System.Data;
using MySql.Data.MySqlClient;

namespace FerreteriaSistema
{
    public class FormVenta : Form
    {
        TextBox txtCodigo = new TextBox();
        NumericUpDown numCantidad = new NumericUpDown();
        DataGridView grid = new DataGridView();
        Label lblTotal = new Label();
        DataTable carrito = new DataTable();

        public FormVenta()
        {
            Text = "Registrar Venta";
            ClientSize = new Size(640, 450);
            StartPosition = FormStartPosition.CenterScreen;

            carrito.Columns.Add("id", typeof(int));
            carrito.Columns.Add("codigo", typeof(string));
            carrito.Columns.Add("nombre", typeof(string));
            carrito.Columns.Add("cantidad", typeof(int));
            carrito.Columns.Add("precio", typeof(decimal));
            carrito.Columns.Add("subtotal", typeof(decimal));

            Controls.Add(new Label { Text = "Código:", Left = 10, Top = 15, Width = 55 });
            txtCodigo.SetBounds(70, 12, 140, 25);
            Controls.Add(new Label { Text = "Cantidad:", Left = 230, Top = 15, Width = 60 });
            numCantidad.SetBounds(295, 12, 70, 25);
            numCantidad.Minimum = 1; numCantidad.Maximum = 10000; numCantidad.Value = 1;

            var btnAgregar = new Button { Text = "Agregar", Left = 380, Top = 10, Width = 100 };
            var btnQuitar = new Button { Text = "Quitar fila", Left = 490, Top = 10, Width = 100 };
            btnAgregar.Click += (s, e) => AgregarAlCarrito();
            btnQuitar.Click += (s, e) =>
            {
                if (grid.CurrentRow != null) { grid.Rows.Remove(grid.CurrentRow); Recalcular(); }
            };
            AcceptButton = btnAgregar;

            grid.SetBounds(10, 50, 620, 290);
            grid.DataSource = carrito;
            grid.ReadOnly = true;
            grid.AllowUserToAddRows = false;
            grid.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill;
            grid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;

            lblTotal.SetBounds(10, 355, 300, 30);
            lblTotal.Font = new Font(Font.FontFamily, 14, FontStyle.Bold);
            lblTotal.Text = "Total: 0.00";

            var btnFinalizar = new Button { Text = "Finalizar venta", Left = 440, Top = 390, Width = 190, Height = 40 };
            btnFinalizar.Click += (s, e) => Finalizar();

            Controls.AddRange(new Control[] { txtCodigo, numCantidad, btnAgregar, btnQuitar, grid, lblTotal, btnFinalizar });
        }

        void AgregarAlCarrito()
        {
            try
            {
                using var conn = Db.Conectar();
                using var cmd = new MySqlCommand(
                    "SELECT id, nombre, precio, stock FROM productos WHERE codigo=@c", conn);
                cmd.Parameters.AddWithValue("@c", txtCodigo.Text.Trim());
                using var r = cmd.ExecuteReader();
                if (!r.Read()) { MessageBox.Show("Producto no encontrado"); return; }

                int id = r.GetInt32("id");
                string nombre = r.GetString("nombre");
                decimal precio = r.GetDecimal("precio");
                int stock = r.GetInt32("stock");
                int cantidad = (int)numCantidad.Value;

                int yaEnCarrito = carrito.AsEnumerable()
                    .Where(x => x.Field<int>("id") == id).Sum(x => x.Field<int>("cantidad"));
                if (yaEnCarrito + cantidad > stock)
                {
                    MessageBox.Show($"Stock insuficiente. Disponible: {stock - yaEnCarrito}");
                    return;
                }

                carrito.Rows.Add(id, txtCodigo.Text.Trim(), nombre, cantidad, precio, precio * cantidad);
                Recalcular();
                txtCodigo.Clear(); numCantidad.Value = 1; txtCodigo.Focus();
            }
            catch (Exception ex) { MessageBox.Show("Error: " + ex.Message); }
        }

        void Recalcular()
        {
            decimal total = carrito.AsEnumerable().Sum(x => x.Field<decimal>("subtotal"));
            lblTotal.Text = $"Total: {total:N2}";
        }

        void Finalizar()
        {
            if (carrito.Rows.Count == 0) { MessageBox.Show("El carrito está vacío"); return; }
            decimal total = carrito.AsEnumerable().Sum(x => x.Field<decimal>("subtotal"));

            using var conn = Db.Conectar();
            using var tx = conn.BeginTransaction();
            try
            {
                int idVenta;
                using (var cmd = new MySqlCommand(
                    "INSERT INTO ventas (total, id_usuario) VALUES (@t, @u); SELECT LAST_INSERT_ID();", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@t", total);
                    cmd.Parameters.AddWithValue("@u", Db.IdUsuario);
                    idVenta = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach (DataRow row in carrito.Rows)
                {
                    using (var cmd = new MySqlCommand(
                        @"INSERT INTO detalle_ventas (id_venta, id_producto, cantidad, precio_unitario, subtotal)
                          VALUES (@v, @p, @c, @pu, @st)", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@v", idVenta);
                        cmd.Parameters.AddWithValue("@p", row["id"]);
                        cmd.Parameters.AddWithValue("@c", row["cantidad"]);
                        cmd.Parameters.AddWithValue("@pu", row["precio"]);
                        cmd.Parameters.AddWithValue("@st", row["subtotal"]);
                        cmd.ExecuteNonQuery();
                    }
                    using (var cmd = new MySqlCommand(
                        "UPDATE productos SET stock = stock - @c WHERE id=@p AND stock >= @c", conn, tx))
                    {
                        cmd.Parameters.AddWithValue("@c", row["cantidad"]);
                        cmd.Parameters.AddWithValue("@p", row["id"]);
                        if (cmd.ExecuteNonQuery() == 0)
                            throw new Exception("Stock insuficiente para " + row["nombre"]);
                    }
                }

                tx.Commit();
                MessageBox.Show($"Venta registrada. Total: {total:N2}");
                carrito.Clear(); Recalcular();
            }
            catch (Exception ex)
            {
                tx.Rollback();
                MessageBox.Show("Error al registrar venta: " + ex.Message);
            }
        }
    }
}