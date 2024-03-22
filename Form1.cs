using System;
using System.Drawing;
using System.Windows.Forms;

namespace ABM_presentacion
{
    public partial class Form1 : Form
    {
        private Form formActivo;
        private Point previousMousePosition;

        public Form1()
        {
            InitializeComponent();
            this.LocationChanged += Form1_LocationChanged;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            MostrarFormulario<FormAlumnos>();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            MostrarFormulario<FormProfesores>();
        }

        private void button3_Click(object sender, EventArgs e)
        {
            MostrarFormulario<FormCursos>();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            MostrarFormulario<FormMaterias>();
        }

        private void MostrarFormulario<T>() where T : Form, new()
        {
            // Cerrar el formulario activo actual, si existe
            if (formActivo != null && !formActivo.IsDisposed)
            {
                formActivo.Close();
            }

            // Mostrar el nuevo formulario
            T nuevoForm = new T();
            nuevoForm.Location = new Point(this.Location.X + 280, this.Location.Y + 115);
            nuevoForm.Show(this);

            // Actualizar el formulario activo
            formActivo = nuevoForm;
        }

        private void Form1_LocationChanged(object sender, EventArgs e)
        {
            UpdateSecondaryFormsPosition();
            if (this.Focused)
            {
                if (formActivo != null && !formActivo.IsDisposed && formActivo.Visible)
                {
                    formActivo.Activate();
                    formActivo.BringToFront();
                }
            }
        }

        private void UpdateSecondaryFormsPosition()
        {
            if (formActivo != null && !formActivo.IsDisposed)
            {
                formActivo.Left += Left - previousMousePosition.X;
                formActivo.Top += Top - previousMousePosition.Y;
                formActivo.BringToFront();
            }

            previousMousePosition = new Point(Left, Top);
        }

        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            if (formActivo != null && !formActivo.IsDisposed)
            {
                formActivo.BringToFront();
            }
        }
    }
}
