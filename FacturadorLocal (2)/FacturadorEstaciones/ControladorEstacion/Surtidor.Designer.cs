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
            this.Nombre = new System.Windows.Forms.Label();
            this.label1 = new System.Windows.Forms.Label();
            this.EstadoPar = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.EstadoImpar = new System.Windows.Forms.Label();
            this.Turno = new System.Windows.Forms.Label();
            this.Islero = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // Nombre
            // 
            this.Nombre.AutoSize = true;
            this.Nombre.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.Nombre.ForeColor = System.Drawing.Color.White;
            this.Nombre.Location = new System.Drawing.Point(9, 11);
            this.Nombre.Name = "Nombre";
            this.Nombre.Size = new System.Drawing.Size(99, 25);
            this.Nombre.TabIndex = 0;
            this.Nombre.Text = "[surtidor]";
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(9, 78);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(52, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Cara Par";
            // 
            // EstadoPar
            // 
            this.EstadoPar.AutoSize = true;
            this.EstadoPar.ForeColor = System.Drawing.Color.White;
            this.EstadoPar.Location = new System.Drawing.Point(9, 93);
            this.EstadoPar.Name = "EstadoPar";
            this.EstadoPar.Size = new System.Drawing.Size(81, 15);
            this.EstadoPar.TabIndex = 2;
            this.EstadoPar.Text = "Desautorizada";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(9, 108);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(67, 15);
            this.label3.TabIndex = 3;
            this.label3.Text = "Cara Impar";
            // 
            // EstadoImpar
            // 
            this.EstadoImpar.AutoSize = true;
            this.EstadoImpar.ForeColor = System.Drawing.Color.White;
            this.EstadoImpar.Location = new System.Drawing.Point(9, 123);
            this.EstadoImpar.Name = "EstadoImpar";
            this.EstadoImpar.Size = new System.Drawing.Size(81, 15);
            this.EstadoImpar.TabIndex = 4;
            this.EstadoImpar.Text = "Desautorizada";
            // 
            // Turno
            // 
            this.Turno.AutoSize = true;
            this.Turno.ForeColor = System.Drawing.Color.White;
            this.Turno.Location = new System.Drawing.Point(9, 36);
            this.Turno.Name = "Turno";
            this.Turno.Size = new System.Drawing.Size(86, 15);
            this.Turno.TabIndex = 5;
            this.Turno.Text = "Turno: Cerrado";
            // 
            // Islero
            // 
            this.Islero.AutoSize = true;
            this.Islero.ForeColor = System.Drawing.Color.White;
            this.Islero.Location = new System.Drawing.Point(9, 51);
            this.Islero.Name = "Islero";
            this.Islero.Size = new System.Drawing.Size(63, 15);
            this.Islero.TabIndex = 6;
            this.Islero.Text = "Empleado:";
            // 
            // Surtidor
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.SteelBlue;
            this.Controls.Add(this.Islero);
            this.Controls.Add(this.Turno);
            this.Controls.Add(this.EstadoImpar);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.EstadoPar);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.Nombre);
            this.Name = "Surtidor";
            this.Size = new System.Drawing.Size(162, 158);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label Nombre;
        private Label label1;
        private Label EstadoPar;
        private Label label3;
        private Label EstadoImpar;
        private Label Turno;
        private Label Islero;
    }
}
