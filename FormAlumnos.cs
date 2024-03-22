using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ABM_presentacion
{
    public partial class FormAlumnos : Form
    {
        public FormAlumnos()
        {
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            comboBox1.Items.Add("Nombre");
            comboBox1.Items.Add("Apellido");
            comboBox1.Items.Add("Es becado");
            comboBox1.Items.Add("DNI");
            comboBox1.Items.Add("Edad");
            comboBox1.Items.Add("Todos");


        }

        private void FormAlumnos_Load(object sender, EventArgs e)
        {
            CargarDatos();
        }

        private void CargarDatos()
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                if (conexion != null)
                {
                    try
                    {
                        string columnaBusqueda = ObtenerColumnaBusquedaSeleccionada();
                        string consulta = "SELECT * FROM alumnos WHERE ";

                        // Verifica si la opción seleccionada en el ComboBox es "Todos"
                        if (comboBox1.SelectedItem != null && comboBox1.SelectedItem.ToString() != "Todos")
                        {
                            // Si no es "Todos", aplica el filtro a la columna seleccionada
                            consulta += $"{columnaBusqueda} LIKE @termino AND ";
                        }

                        // Agrega la condición para el filtro del TextBox

                        consulta += "(@termino = '' OR (nombre LIKE @termino OR apellido LIKE @termino OR esbecado LIKE @termino OR dni LIKE @termino OR edad LIKE @termino))";

                        SqlDataAdapter adaptador = new SqlDataAdapter(consulta, conexion);
                        adaptador.SelectCommand.Parameters.AddWithValue("@termino", $"%{textBox4.Text}%");

                        DataTable tablaAlumnos = new DataTable();
                        adaptador.Fill(tablaAlumnos);
                        dataGridView1.DataSource = tablaAlumnos;
                        dataGridView1.Columns["id"].Visible = false;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al cargar los datos: " + ex.Message);
                    }
                }
            }
            LimpiarUltimaFila();
        }

        private string ObtenerColumnaBusquedaSeleccionada()
        {
            if (comboBox1.SelectedItem != null)
            {
                string selectedItem = comboBox1.SelectedItem.ToString();

                switch (selectedItem)
                {
                    case "Nombre":
                        return "nombre";
                    case "Apellido":
                        return "apellido";
                    case "Esbecado":
                        return "esbecado";
                    case "DNI":
                        return "dni";
                    case "Edad":
                        return "edad";
                    default:
                        return "nombre";
                }
            }

            return "nombre";
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            CargarDatos();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CargarDatos();

            if (dataGridView1.Rows.Count > 0)
            {
                DialogResult resultado = MessageBox.Show("No se encontraron resultados. ¿Deseas agregar un nuevo alumno?", "Agregar Alumno", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resultado == DialogResult.Yes)
                {
                    FormAgregarAlumno formAgregarAlumno = new FormAgregarAlumno(this);
                    formAgregarAlumno.Show();
                }
            }
                else
                {
                    MessageBox.Show("No se encontraron alumnos.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                DataGridViewRow filaSeleccionada = dataGridView1.Rows[e.RowIndex];
                int idAlumnoSeleccionado = filaSeleccionada.Cells["id"].Value != DBNull.Value
                 ? Convert.ToInt32(filaSeleccionada.Cells["id"].Value)
                 : 0;

                // Actualizar campos del formulario con los datos de la fila seleccionada
                textBox1.Text = filaSeleccionada.Cells["nombre"].Value?.ToString() ?? string.Empty;
                textBox2.Text = filaSeleccionada.Cells["apellido"].Value?.ToString() ?? string.Empty;
                textBox3.Text = filaSeleccionada.Cells["dni"].Value?.ToString() ?? string.Empty;
                textBox5.Text = filaSeleccionada.Cells["edad"].Value?.ToString() ?? string.Empty;

                if (DateTime.TryParse(filaSeleccionada.Cells["fechadenac"].Value?.ToString(), out DateTime fechaNacimiento))
                {
                    textBox6.Text = fechaNacimiento.ToString("dd/MM/yyyy");
                }
                else
                {
                    textBox6.Text = string.Empty;
                    MessageBox.Show("La fecha de nacimiento no es válida.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }

                // Verificar si el valor de "esbecado" es DBNull antes de intentar convertirlo
                object esBecadoValue = filaSeleccionada.Cells["esbecado"].Value;
                checkBox1.Checked = (esBecadoValue != DBNull.Value) ? Convert.ToBoolean(esBecadoValue) : false;

            }
            else
            {
                // No hay ninguna fila seleccionada, puedes mostrar un mensaje de advertencia si lo deseas. 
                MessageBox.Show("No se ha seleccionado ninguna fila.", "Advertencia", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }

        }
        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int idAlumnoSeleccionado = Convert.ToInt32(dataGridView1.CurrentRow.Cells["id"].Value);
                ActualizarAlumno(idAlumnoSeleccionado);
                CargarDatos();
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un alumno antes de realizar esta acción.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        private void LimpiarUltimaFila()
        {
            if (dataGridView1.Rows.Count > 0)
            {
                DataGridViewRow ultimaFila = dataGridView1.Rows[dataGridView1.Rows.Count - 1];

                if (!ultimaFila.IsNewRow && !ultimaFila.HasDefaultCellStyle)
                {
                    dataGridView1.Rows.RemoveAt(dataGridView1.Rows.Count - 1);
                }
            }
        }

        private void ActualizarAlumno(int idAlumno)
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                if (conexion != null)
                {
                    try
                    {
                        // Validar campos obligatorios
                        if (string.IsNullOrWhiteSpace(textBox1.Text) || string.IsNullOrWhiteSpace(textBox2.Text) || string.IsNullOrWhiteSpace(textBox3.Text))
                        {
                            MessageBox.Show("Todos los campos son obligatorios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            return;
                        }

                        string consulta = "UPDATE alumnos SET nombre = @nombre, apellido = @apellido, dni = @dni, esbecado = @esbecado, fechadenac = @fechadenac, edad = @edad WHERE id = @id";

                        using (SqlCommand cmd = new SqlCommand(consulta, conexion))
                        {
                            cmd.Parameters.AddWithValue("@id", idAlumno);
                            cmd.Parameters.AddWithValue("@nombre", textBox1.Text);
                            cmd.Parameters.AddWithValue("@apellido", textBox2.Text);
                            cmd.Parameters.AddWithValue("@dni", textBox3.Text);

                            // Validar y convertir la fecha de nacimiento
                            if (DateTime.TryParse(textBox6.Text, out DateTime fechaNacimiento))
                            {
                                cmd.Parameters.AddWithValue("@fechadenac", fechaNacimiento);
                            }
                            else
                            {
                                MessageBox.Show("La fecha de nacimiento no tiene un formato válido.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                return; // Salir del método si la fecha no es válida
                            }

                            cmd.Parameters.AddWithValue("@esbecado", checkBox1.Checked);
                            cmd.Parameters.AddWithValue("@edad", textBox5.Text);

                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Cambios guardados con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (SqlException ex)
                    {
                        MessageBox.Show("Error al guardar los cambios: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Otro error: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void AgregarAlumno()
        {
            string nombre = textBox1.Text;
            string apellido = textBox2.Text;
            string dni = textBox3.Text;
            bool esBecado = checkBox1.Checked;
            DateTime fechaNacimiento = dateTimePicker2.Value;
            string edad = textBox5.Text;

            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(dni))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validar si ya existe un alumno con el mismo DNI
            if (DniYaExiste(dni))
            {
                MessageBox.Show("Ya existe un alumno con el mismo DNI.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                if (conexion != null)
                {
                    try
                    {
                        string consulta = "INSERT INTO alumnos (nombre, apellido, dni, esbecado, fechadenac, edad) VALUES (@nombre, @apellido, @dni, @esbecado, @fechadenac, @edad)";

                        using (SqlCommand cmd = new SqlCommand(consulta, conexion))
                        {
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.Parameters.AddWithValue("@apellido", apellido);
                            cmd.Parameters.AddWithValue("@dni", dni);
                            cmd.Parameters.AddWithValue("@esbecado", esBecado);
                            cmd.Parameters.AddWithValue("@fechadenac", fechaNacimiento);
                            cmd.Parameters.AddWithValue("@edad", edad);

                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Alumno agregado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarDatos();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al agregar el alumno: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private bool DniYaExiste(string dni)
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                if (conexion != null)
                {
                    try
                    {
                        string consulta = "SELECT COUNT(*) FROM alumnos WHERE dni = @dni";

                        using (SqlCommand cmd = new SqlCommand(consulta, conexion))
                        {
                            cmd.Parameters.AddWithValue("@dni", dni);
                            int cantidad = (int)cmd.ExecuteScalar();
                            return cantidad > 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al verificar la existencia del DNI: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return false;
                    }
                }
                return false;
            }
        }
        private void EliminarAlumno(int idAlumno)
        {
            // Obtener el nombre y apellido del alumno antes de eliminar
            string nombre = ObtenerNombreAlumno(idAlumno);
            string apellido = ObtenerApellidoAlumno(idAlumno);

            // Mostrar mensaje de confirmación
            DialogResult resultado = MessageBox.Show($"¿Estás seguro de eliminar al alumno {nombre} {apellido}?", "Confirmar Eliminación", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            // Verificar la respuesta del usuario
            if (resultado == DialogResult.Yes)
            {
                // Usuario confirmó la eliminación
                using (SqlConnection conexion = ConexionBD.ObtenerConexion())
                {
                    if (conexion != null)
                    {
                        try
                        {
                            string consulta = "DELETE FROM alumnos WHERE id = @id";

                            using (SqlCommand cmd = new SqlCommand(consulta, conexion))
                            {
                                cmd.Parameters.AddWithValue("@id", idAlumno);
                                cmd.ExecuteNonQuery();
                            }

                            MessageBox.Show("Alumno eliminado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show("Error al eliminar el alumno: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }
                    }
                }
            }
            // Si el usuario elige "No", no se realiza la eliminación.
        }
        private string ObtenerNombreAlumno(int idAlumno)
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                if (conexion != null)
                {
                    try
                    {
                        string consulta = "SELECT nombre FROM alumnos WHERE id = @id";

                        using (SqlCommand cmd = new SqlCommand(consulta, conexion))
                        {
                            cmd.Parameters.AddWithValue("@id", idAlumno);
                            object resultado = cmd.ExecuteScalar();

                            if (resultado != null)
                            {
                                return resultado.ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al obtener el nombre del alumno: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                return string.Empty;
            }
        }

        private string ObtenerApellidoAlumno(int idAlumno)
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                if (conexion != null)
                {
                    try
                    {
                        string consulta = "SELECT apellido FROM alumnos WHERE id = @id";

                        using (SqlCommand cmd = new SqlCommand(consulta, conexion))
                        {
                            cmd.Parameters.AddWithValue("@id", idAlumno);
                            object resultado = cmd.ExecuteScalar();

                            if (resultado != null)
                            {
                                return resultado.ToString();
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al obtener el apellido del alumno: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                return string.Empty;
            }
        }


        private void button3_Click(object sender, EventArgs e)
        {
            AgregarAlumno();
        }

        private void textBox3_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (!char.IsDigit(e.KeyChar) && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
            // Limitar la longitud a 8 caracteres
            if (textBox3.Text.Length >= 8 && !char.IsControl(e.KeyChar))
            {
                e.Handled = true;
            }
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            string selectedItem = comboBox1.SelectedItem.ToString();

            if (selectedItem == "Borrar filtro" || selectedItem == "Todos")
            {
                // Lógica para manejar la selección de "Borrar filtro" o "Todos"
            }
            else
            {
                // Lógica para manejar la selección normal
                CargarDatos();
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int idAlumnoSeleccionado = Convert.ToInt32(dataGridView1.CurrentRow.Cells["id"].Value);
                EliminarAlumno(idAlumnoSeleccionado);
                CargarDatos();
            }
            else
            {
                MessageBox.Show("Por favor, selecciona un alumno antes de eliminar.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
        public void RecargarDatos() 
        {
            CargarDatos();
        
        }

        private void textBox4_KeyPress(object sender, KeyPressEventArgs e)
        {
            // Verificar si la tecla presionada es Enter (código ASCII 13)
            if (e.KeyChar == (char)Keys.Enter)
            {
                // Evitar que se procese el caracter Enter en el TextBox
                e.Handled = true;

                // Realizar la búsqueda
                CargarDatos();

                // Verificar los resultados
                if (dataGridView1.Rows.Count > 0)
                {
                    DialogResult resultado = MessageBox.Show("No se encontraron resultados. ¿Deseas agregar un nuevo alumno?", "Agregar Alumno", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (resultado == DialogResult.Yes)
                    {
                        FormAgregarAlumno formAgregarAlumno = new FormAgregarAlumno(this);
                        formAgregarAlumno.Show();
                    }
                }
                else
                {
                    MessageBox.Show("No se encontraron alumnos.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
        }
        private void LimpiarCampos()
        {
            // Limpiar TextBox
            textBox1.Text = string.Empty;
            textBox2.Text = string.Empty;
            textBox3.Text = string.Empty;
            textBox5.Text = string.Empty;
            textBox6.Text = string.Empty;

            // Limpiar DateTimePicker
            dateTimePicker2.Value = DateTime.Now; // Puedes establecer la fecha actual o cualquier otra fecha predeterminada

            // Limpiar CheckBox
            checkBox1.Checked = false;
        }

        private void button6_Click(object sender, EventArgs e)
        {
            LimpiarCampos();
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Evita que se procese la tecla Enter en el TextBox actual
                SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Evita que se procese la tecla Enter en el TextBox actual
                SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        private void textBox3_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Evita que se procese la tecla Enter en el TextBox actual
                SelectNextControl((Control)sender, true, true, true, true);
            }
        }

        private void textBox5_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true; // Evita que se procese la tecla Enter en el TextBox actual
                SelectNextControl((Control)sender, true, true, true, true);
            }
        }
    }
}
