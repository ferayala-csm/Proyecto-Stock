using System;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace ABM_presentacion
{
    public partial class FormAgregarAlumno : Form
    {
        private readonly FormAlumnos formAlumnos;

        public FormAgregarAlumno(FormAlumnos formAlumnos)
        {
            InitializeComponent();
            this.formAlumnos = formAlumnos;
        }

        private void AgregarAlumno()
        {
            using (SqlConnection conexion = ConexionBD.ObtenerConexion())
            {
                if (conexion != null)
                {
                    try
                    {
                        string consulta = "INSERT INTO alumnos (nombre, apellido, dni, esbecado, fechadenac, edad) VALUES (@nombre, @apellido, @dni, @esbecado, @fechadenac, @edad)";

                        using (SqlCommand cmd = new SqlCommand(consulta, conexion))
                        {
                            cmd.Parameters.AddWithValue("@nombre", textBox1.Text);
                            cmd.Parameters.AddWithValue("@apellido", textBox2.Text);
                            cmd.Parameters.AddWithValue("@dni", textBox3.Text);
                            cmd.Parameters.AddWithValue("@esbecado", checkBox1.Checked);
                            cmd.Parameters.AddWithValue("@fechadenac", dateTimePicker2.Value);
                            cmd.Parameters.AddWithValue("@edad", textBox5.Text);

                            cmd.ExecuteNonQuery();
                        }

                        MessageBox.Show("Alumno agregado con éxito.", "Éxito", MessageBoxButtons.OK, MessageBoxIcon.Information);

                        // Notificar al formulario FormAlumnos para que actualice el DataGridView
                        formAlumnos.RecargarDatos();

                        // Cerrar el formulario actual
                        this.Close();
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error al agregar el alumno: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {
            AgregarAlumno();
        }
    }
}