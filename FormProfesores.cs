using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Data.SqlClient;

namespace ABM_presentacion
{
    public partial class FormProfesores : Form
    {
        public FormProfesores()
        {
            InitializeComponent();
            InitializeComponent();
            this.StartPosition = FormStartPosition.Manual;
            comboBox1.SelectedIndexChanged += comboBox1_SelectedIndexChanged;
            dataGridView1.CellFormatting += dataGridView1_CellFormatting;
            comboBox1.Items.Add("Nombre");
            comboBox1.Items.Add("Apellido");
            comboBox1.Items.Add("EsTitular");
            comboBox1.Items.Add("DNI");
            comboBox1.Items.Add("Edad");
            comboBox1.Items.Add("Todos");
        }

        private void button4_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void FormProfesores_Load(object sender, EventArgs e)
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
                        string consulta = "SELECT * FROM profesores WHERE ";

                        // Verifica si la opción seleccionada en el ComboBox es "Todos"
                        if (comboBox1.SelectedItem != null && comboBox1.SelectedItem.ToString() != "Todos")
                        {
                            // Si no es "Todos", aplica el filtro a la columna seleccionada
                            consulta += $"{columnaBusqueda} LIKE @termino AND ";
                        }

                        // Agrega la condición para el filtro del TextBox
                        consulta += "(@termino = '' OR (nombre LIKE @termino OR apellido LIKE @termino OR estitular LIKE @termino OR dni LIKE @termino OR edad LIKE @termino))";

                        SqlDataAdapter adaptador = new SqlDataAdapter(consulta, conexion);
                        adaptador.SelectCommand.Parameters.AddWithValue("@termino", $"%{textBox4.Text}%");

                        DataTable tablaProfesores = new DataTable();
                        adaptador.Fill(tablaProfesores);

                        // Asegurarse de que la columna "estitular" tenga un tipo booleano
                        tablaProfesores.Columns["estitular"].DataType = typeof(bool);

                        dataGridView1.DataSource = tablaProfesores;
                        dataGridView1.Columns["id"].Visible = false;

                        // Limpiar la última fila después de cargar los datos
                        LimpiarUltimaFila();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al cargar los datos: " + ex.Message);
                    }
                }
            }
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
                    case "EsTitular":
                        return "estitular";
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

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            CargarDatos();
        }

        private void button2_Click(object sender, EventArgs e)
        {
            CargarDatos();

            if (dataGridView1.Rows.Count > 0)
            {
                DialogResult resultado = MessageBox.Show("No se encontraron resultados. ¿Deseas agregar un nuevo profesor?", "Agregar Profesor", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                if (resultado == DialogResult.Yes)
                {
                    FormAgregarProfesor formAgregarProfesor = new FormAgregarProfesor(this);
                    formAgregarProfesor.Show();
                }
            }
            else
            {
                MessageBox.Show("No se encontraron profesores.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void dataGridView1_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0 && e.RowIndex < dataGridView1.Rows.Count)
            {
                DataGridViewRow filaSeleccionada = dataGridView1.Rows[e.RowIndex];
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
                }

                object esBecadoValue = filaSeleccionada.Cells["estitular"].Value;

                if (esBecadoValue != DBNull.Value)
                {
                    checkBox1.Checked = Convert.ToBoolean(esBecadoValue);
                }
                else
                {
                    // En este caso, esBecadoValue es DBNull, así que puedes decidir qué hacer cuando el valor es nulo.
                    // Por ejemplo, podrías establecer el valor de checkBox1.Checked en false o hacer algo más.
                    checkBox1.Checked = false; // O cualquier otro valor predeterminado
                }
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            if (dataGridView1.CurrentRow != null)
            {
                int idProfesorSeleccionado = Convert.ToInt32(dataGridView1.CurrentRow.Cells["id"].Value);
                ActualizarProfesor(idProfesorSeleccionado);
                CargarDatos();
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
        private void ActualizarProfesor(int idProfesor)
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

                        string consulta = "UPDATE profesores SET nombre = @nombre, apellido = @apellido, dni = @dni, estitular = @estitular, fechadenac = @fechadenac, edad = @edad WHERE id = @id";

                        using (SqlCommand cmd = new SqlCommand(consulta, conexion))
                        {
                            cmd.Parameters.AddWithValue("@id", idProfesor);
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

                            cmd.Parameters.AddWithValue("@estitular", checkBox1.Checked);
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
        private void AgregarProfesor()
        {
            string nombre = textBox1.Text;
            string apellido = textBox2.Text;
            string dni = textBox3.Text;
            bool esTitular = checkBox1.Checked;
            DateTime fechaNacimiento = dateTimePicker2.Value;
            string edad = textBox5.Text;

            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(nombre) || string.IsNullOrWhiteSpace(apellido) || string.IsNullOrWhiteSpace(dni))
            {
                MessageBox.Show("Todos los campos son obligatorios.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // Validar si ya existe un profesor con el mismo DNI (excepto el profesor actualmente seleccionado para edición)
            if (DniYaExisteProfesor(dni, -1)) // Pasamos -1 para ignorar el DNI del profesor actualmente seleccionado
            {
                MessageBox.Show("Ya existe un profesor con el mismo DNI.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                if (conexion != null)
                {
                    try
                    {
                        string consulta = "INSERT INTO profesores (nombre, apellido, dni, estitular, fechadenac, edad) VALUES (@nombre, @apellido, @dni, @estitular, @fechadenac, @edad)";

                        using (SqlCommand cmd = new SqlCommand(consulta, conexion))
                        {
                            cmd.Parameters.AddWithValue("@nombre", nombre);
                            cmd.Parameters.AddWithValue("@apellido", apellido);
                            cmd.Parameters.AddWithValue("@dni", dni);
                            cmd.Parameters.AddWithValue("@estitular", esTitular);
                            cmd.Parameters.AddWithValue("@fechadenac", fechaNacimiento);
                            cmd.Parameters.AddWithValue("@edad", edad);

                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Profesor agregado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        CargarDatos();
                        LimpiarCampos(); // Limpiar campos después de agregar un profesor
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al agregar el profesor: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private bool DniYaExisteProfesor(string dni, int idProfesorActual)
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                if (conexion != null)
                {
                    try
                    {
                        string consulta = "SELECT COUNT(*) FROM profesores WHERE dni = @dni AND id <> @idProfesorActual";

                        using (SqlCommand cmd = new SqlCommand(consulta, conexion))
                        {
                            cmd.Parameters.AddWithValue("@dni", dni);
                            cmd.Parameters.AddWithValue("@idProfesorActual", idProfesorActual);
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
        private void EliminarProfesor(int idProfesor)
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                if (conexion != null)
                {
                    try
                    {
                        string consulta = "DELETE FROM profesores WHERE id = @id";

                        using (SqlCommand cmd = new SqlCommand(consulta, conexion))
                        {
                            cmd.Parameters.AddWithValue("@id", idProfesor);
                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Profesor eliminado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al eliminar el profesor: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }


        }

        private void button3_Click(object sender, EventArgs e)
        {
            AgregarProfesor();
        }

        private void button3_KeyPress(object sender, KeyPressEventArgs e)
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
                int idProfesorSeleccionado = Convert.ToInt32(dataGridView1.CurrentRow.Cells["id"].Value);
                EliminarProfesor(idProfesorSeleccionado);
                RecargarDatos();
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
                    DialogResult resultado = MessageBox.Show("No se encontraron resultados. ¿Deseas agregar un nuevo profesor?", "Agregar Profesor", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

                    if (resultado == DialogResult.Yes)
                    {
                        FormAgregarProfesor formAgregarProfesor = new FormAgregarProfesor(this);
                        formAgregarProfesor.Show();
                    }
                }
                else
                {
                    MessageBox.Show("No se encontraron profesores.", "Información", MessageBoxButtons.OK, MessageBoxIcon.Information);
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

        private void dataGridView1_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            // Manejar el valor DBNull en la columna "estitular"
            if (e.Value == DBNull.Value && e.DesiredType == typeof(bool))
            {
                e.Value = false; // O establecer otro valor predeterminado
                e.FormattingApplied = true;
            }
        }
    }
}
