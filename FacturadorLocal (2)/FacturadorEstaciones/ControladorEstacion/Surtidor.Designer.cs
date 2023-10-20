using Color = System.Drawing.Color;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;
using SizeF = System.Drawing.SizeF;

namespace ControladorEstacion
{
    partial class Surtidor
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            Nombre = new Label();
            EstadoPar = new Label();
            EstadoImpar = new Label();
            Turno = new Label();
            Islero = new Label();
            panel1 = new Panel();
            panel1.SuspendLayout();
            SuspendLayout();
            // 
            // Nombre
            // 
            Nombre.AutoSize = true;
            Nombre.BackColor = Color.FromArgb(5, 29, 56);
            Nombre.Font = new Font("Segoe UI", 18F, FontStyle.Bold, GraphicsUnit.Point);
            Nombre.ForeColor = Color.White;
            Nombre.Location = new Point(113, 18);
            Nombre.Name = "Nombre";
            Nombre.Size = new Size(28, 32);
            Nombre.TabIndex = 0;
            Nombre.Text = "1";
            // 
            // EstadoPar
            // 
            EstadoPar.AutoSize = true;
            EstadoPar.BackColor = Color.FromArgb(1, 5, 29, 56);
            EstadoPar.Font = new Font("Segoe UI", 6F, FontStyle.Regular, GraphicsUnit.Point);
            EstadoPar.ForeColor = Color.White;
            EstadoPar.Location = new Point(70, 80);
            EstadoPar.Name = "EstadoPar";
            EstadoPar.Size = new Size(57, 11);
            EstadoPar.TabIndex = 2;
            EstadoPar.Text = "Desautorizada";
            // 
            // EstadoImpar
            // 
            EstadoImpar.AutoSize = true;
            EstadoImpar.BackColor = Color.FromArgb(1, 5, 29, 56);
            EstadoImpar.Font = new Font("Segoe UI", 6F, FontStyle.Regular, GraphicsUnit.Point);
            EstadoImpar.ForeColor = Color.White;
            EstadoImpar.Location = new Point(151, 80);
            EstadoImpar.Name = "EstadoImpar";
            EstadoImpar.Size = new Size(57, 11);
            EstadoImpar.TabIndex = 4;
            EstadoImpar.Text = "Desautorizada";
            // 
            // Turno
            // 
            Turno.AutoSize = true;
            Turno.BackColor = Color.FromArgb(1, 5, 29, 56);
            Turno.ForeColor = Color.White;
            Turno.Location = new Point(249, 42);
            Turno.Name = "Turno";
            Turno.Size = new Size(86, 15);
            Turno.TabIndex = 5;
            Turno.Text = "Turno: Cerrado";
            // 
            // Islero
            // 
            Islero.AutoSize = true;
            Islero.BackColor = Color.FromArgb(1, 5, 29, 56);
            Islero.ForeColor = Color.White;
            Islero.Location = new Point(249, 83);
            Islero.Name = "Islero";
            Islero.Size = new Size(63, 15);
            Islero.TabIndex = 6;
            Islero.Text = "Empleado:";
            // 
            // panel1
            // 
            panel1.BackColor = Color.FromArgb(1, 5, 29, 56);
            panel1.BackgroundImage = Properties.Resources.Visualizador___Surtidor_1__3_;
            panel1.Controls.Add(Nombre);
            panel1.Controls.Add(EstadoImpar);
            panel1.Controls.Add(Islero);
            panel1.Controls.Add(Turno);
            panel1.Controls.Add(EstadoPar);
            panel1.Location = new Point(0, 0);
            panel1.Name = "panel1";
            panel1.Size = new Size(400, 110);
            panel1.TabIndex = 7;
            // 
            // Surtidor
            // 
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            BackColor = Color.FromArgb(1, 5, 29, 56);
            BackgroundImage = Properties.Resources.Visualizador__Fondo__1_;
            Controls.Add(panel1);
            Name = "Surtidor";
            Size = new Size(400, 110);
            panel1.ResumeLayout(false);
            panel1.PerformLayout();
            ResumeLayout(false);
        }

        #endregion

        private Label Nombre;
        private Label EstadoPar;
        private Label EstadoImpar;
        private Label Turno;
        private Label Islero;
        private Panel panel1;
    }
}
