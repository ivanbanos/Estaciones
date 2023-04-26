namespace FacturadorEstacionesPOSWinForm
{
    partial class Sicom
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


        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.button1 = new System.Windows.Forms.Button();
            this.Vehiculo = new System.Windows.Forms.Label();
            this.Idrom = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.Fecha = new System.Windows.Forms.Label();
            this.Autorizado = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // button1
            // 
            this.button1.BackColor = System.Drawing.Color.Gray;
            this.button1.Font = new System.Drawing.Font("Microsoft Sans Serif", 36F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.button1.ForeColor = System.Drawing.Color.White;
            this.button1.Location = new System.Drawing.Point(11, 376);
            this.button1.Margin = new System.Windows.Forms.Padding(2);
            this.button1.Name = "button1";
            this.button1.Size = new System.Drawing.Size(973, 92);
            this.button1.TabIndex = 51;
            this.button1.Text = "Cerrar";
            this.button1.UseVisualStyleBackColor = false;
            this.button1.Click += new System.EventHandler(this.button1_Click);
            // 
            // Vehiculo
            // 
            this.Vehiculo.AutoSize = true;
            this.Vehiculo.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Vehiculo.ForeColor = System.Drawing.Color.Black;
            this.Vehiculo.Location = new System.Drawing.Point(11, 9);
            this.Vehiculo.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Vehiculo.Name = "Vehiculo";
            this.Vehiculo.Size = new System.Drawing.Size(279, 73);
            this.Vehiculo.TabIndex = 52;
            this.Vehiculo.Text = "Vehiculo";
            // 
            // Idrom
            // 
            this.Idrom.AutoSize = true;
            this.Idrom.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Idrom.ForeColor = System.Drawing.Color.Black;
            this.Idrom.Location = new System.Drawing.Point(11, 82);
            this.Idrom.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Idrom.Name = "Idrom";
            this.Idrom.Size = new System.Drawing.Size(196, 73);
            this.Idrom.TabIndex = 53;
            this.Idrom.Text = "Idrom";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.Color.Black;
            this.label3.Location = new System.Drawing.Point(11, 155);
            this.label3.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(660, 73);
            this.label3.TabIndex = 54;
            this.label3.Text = "Fecha de vencimiento";
            // 
            // Fecha
            // 
            this.Fecha.AutoSize = true;
            this.Fecha.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Fecha.ForeColor = System.Drawing.Color.Black;
            this.Fecha.Location = new System.Drawing.Point(11, 228);
            this.Fecha.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Fecha.Name = "Fecha";
            this.Fecha.Size = new System.Drawing.Size(660, 73);
            this.Fecha.TabIndex = 55;
            this.Fecha.Text = "Fecha de vencimiento";
            // 
            // Autorizado
            // 
            this.Autorizado.AutoSize = true;
            this.Autorizado.Font = new System.Drawing.Font("Microsoft Sans Serif", 48F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.Autorizado.ForeColor = System.Drawing.Color.Black;
            this.Autorizado.Location = new System.Drawing.Point(11, 301);
            this.Autorizado.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.Autorizado.Name = "Autorizado";
            this.Autorizado.Size = new System.Drawing.Size(340, 73);
            this.Autorizado.TabIndex = 56;
            this.Autorizado.Text = "Autorizado";
            // 
            // Sicom
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(995, 475);
            this.Controls.Add(this.Autorizado);
            this.Controls.Add(this.Fecha);
            this.Controls.Add(this.label3);
            this.Controls.Add(this.Idrom);
            this.Controls.Add(this.Vehiculo);
            this.Controls.Add(this.button1);
            this.Margin = new System.Windows.Forms.Padding(2);
            this.Name = "Sicom";
            this.Text = "Vehiculo SUIC";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.Button button1;
        private System.Windows.Forms.Label Vehiculo;
        private System.Windows.Forms.Label Idrom;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label Fecha;
        private System.Windows.Forms.Label Autorizado;
    }
}