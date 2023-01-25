namespace ControladorEstacion
{
    partial class Form1
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.siges = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // siges
            // 
            this.siges.AutoSize = true;
            this.siges.Font = new System.Drawing.Font("Segoe UI", 36F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.siges.Location = new System.Drawing.Point(12, 9);
            this.siges.Name = "siges";
            this.siges.Size = new System.Drawing.Size(157, 65);
            this.siges.TabIndex = 0;
            this.siges.Text = "SIGES";
            // 
            // Form1
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.LightSteelBlue;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.siges);
            this.Name = "Form1";
            this.Text = "Administrador SIGES";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Label siges;
    }
}