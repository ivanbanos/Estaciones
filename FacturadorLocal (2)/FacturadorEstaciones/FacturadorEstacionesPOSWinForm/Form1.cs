using EnviadorInformacionService.Models;
using FactoradorEstacionesModelo.Objetos;
using FactoradorEstacionesModelo.Siges;
using FacturadorEstacionesPOSWinForm.Repo;
using FacturadorEstacionesRepositorio;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Drawing.Printing;
using System.Globalization;
using System.Linq;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace FacturadorEstacionesPOSWinForm
{
    public partial class Islas : Form
    {
        private readonly IEstacionesRepositorio _estacionesRepositorio;
        private readonly IConexionEstacionRemota _conexionEstacionRemota;
        private readonly InfoEstacion _infoEstacion;
        private Venta _venta;
        private Tercero _tercero;
        private Factura _factura;
        private Manguera _mangueras;
        private List<TipoIdentificacion> _tiposIdentificacion;
        private List<Canastilla> _canastillas;
        private int _charactersPerPage;
        private List<FormasPagos> formas;
        private FacturaCanastilla facturaCanastilla;
        private Tercero _terceroCanastilla;
        private Tercero _terceroCrear;
        private string pestanaActual = "Combustible";

        private System.Timers.Timer timer1;
        private System.Timers.Timer timer4;
        public Islas(IEstacionesRepositorio estacionesRepositorio, IOptions<InfoEstacion> infoEstacion, IConexionEstacionRemota conexionEstacionRemota)
        {
            _estacionesRepositorio = estacionesRepositorio;
            InitializeComponent();
            textBox1.PlaceholderText = "Identificación";
            Placa.PlaceholderText = "Placa";
            Kilometraje.PlaceholderText = "Kilometraje";

            button2.Enabled = false;
            timer1 = new System.Timers.Timer(3000);
            this.timer1.Elapsed += timer1_Elapsed;
            timer4 = new System.Timers.Timer(3000);
            this.timer4.Elapsed += timer4_Elapsed;
            try
            {
                _tiposIdentificacion = _estacionesRepositorio.getTiposIdentifiaciones();
                formas = _estacionesRepositorio.BuscarFormasPagos();
                var caras = _estacionesRepositorio.getCaras().ToArray();
                comboBox3.Items.Clear();
                comboBox3.Items.AddRange(caras);
                _infoEstacion = infoEstacion.Value;
                _charactersPerPage = _infoEstacion.CaracteresPorPagina;
                if (_charactersPerPage == 0)
                {
                    _charactersPerPage = 40;
                }
                textBox4.Text = "222222222222";
                facturaCanastilla = new FacturaCanastilla()
                {
                    canastillas = new List<CanastillaFactura>()
                };

                _conexionEstacionRemota = conexionEstacionRemota;

                _canastillas = _conexionEstacionRemota.GetCanastilla();
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(_canastillas.ToArray());
                

                this.comboBox4.Text = "Selec. Tipo Identificacion";

                _tiposIdentificacion = _estacionesRepositorio.getTiposIdentifiaciones();
                comboBox4.ValueMember = "Descripcion";
                comboBox4.Items.Clear();

                comboBox4.Items.AddRange(_tiposIdentificacion.ToArray());
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
        }


        private void timer1_Elapsed(object sender, System.Timers.ElapsedEventArgs args)
        {
            this.timer1.Stop();
            this.BeginInvoke(new MethodInvoker(this.OnTextBox1ChangedComplete));
        }
        private void timer4_Elapsed(object sender, System.Timers.ElapsedEventArgs args)
        {
            this.timer4.Stop();
            this.BeginInvoke(new MethodInvoker(this.OnTextBox4ChangedComplete));
        }

        private void OnTextBox1ChangedComplete()
        {
            if (string.IsNullOrEmpty(textBox1.Text))
            {
                return;
            }
            _tercero = _estacionesRepositorio.getTercero(textBox1.Text);
            if (_tercero != null)
            {
                StringBuilder clienteInfo = new StringBuilder();
                clienteInfo.Append("Nombre: ").AppendLine(_tercero.Nombre)
                    .Append("Teléfono: ").AppendLine(_tercero.Telefono)
                    .Append("Correo: ").AppendLine(_tercero.Correo)
                    .Append("Dirección: ").AppendLine(_tercero.Direccion);
                if (_tiposIdentificacion.Any(ti => _tercero.tipoIdentificacion == ti.TipoIdentificacionId)
                    && _tiposIdentificacion.Single(ti => _tercero.tipoIdentificacion == ti.TipoIdentificacionId).Descripcion != "No especificada")
                {

                    clienteInfo.Append("Tipo de indeitificación: ").AppendLine(_tiposIdentificacion.Single(ti => _tercero.tipoIdentificacion == ti.TipoIdentificacionId).Descripcion);
                }
                this.labelCliente.Text = clienteInfo.ToString();
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("Tercero no existe, ¿Desea crearlo?", "Tercero", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    textBox8.Text = textBox1.Text;
                    tabControl1.SelectedTab = tabPage3;
                }
            }
        }


        private void OnTextBox4ChangedComplete()
        {
            if (string.IsNullOrEmpty(textBox4.Text)){
                return;
            }
            _terceroCanastilla = _estacionesRepositorio.getTercero(textBox4.Text);
            if (_terceroCanastilla != null)
            {
                StringBuilder clienteInfo = new StringBuilder();
                clienteInfo.Append("Nombre: ").AppendLine(_terceroCanastilla.Nombre)
                    .Append("Teléfono: ").AppendLine(_terceroCanastilla.Telefono)
                    .Append("Correo: ").AppendLine(_terceroCanastilla.Correo)
                    .Append("Dirección: ").AppendLine(_terceroCanastilla.Direccion);
                if (_tiposIdentificacion.Any(ti => _terceroCanastilla.tipoIdentificacion == ti.TipoIdentificacionId)
                    && _tiposIdentificacion.Single(ti => _terceroCanastilla.tipoIdentificacion == ti.TipoIdentificacionId).Descripcion != "No especificada")
                {

                    clienteInfo.Append("Tipo de indeitificación: ").AppendLine(_tiposIdentificacion.Single(ti => _terceroCanastilla.tipoIdentificacion == ti.TipoIdentificacionId).Descripcion);
                }
                this.label1.Text = clienteInfo.ToString();
            }
            else
            {
                DialogResult dialogResult = MessageBox.Show("Tercero no existe, ¿Desea crearlo?", "Tercero", MessageBoxButtons.YesNo);
                if (dialogResult == DialogResult.Yes)
                {
                    textBox8.Text = textBox4.Text;
                    tabControl1.SelectedTab = tabPage3;
                }
            }
        }



        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            if (!this.timer1.Enabled)
                this.timer1.Start();
            else
            {
                this.timer1.Stop();
                this.timer1.Start();
            }
            

        }


        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            if (!this.timer4.Enabled)
                this.timer4.Start();
            else
            {
                this.timer4.Stop();
                this.timer4.Start();
            }
            

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e)
        {
            cleanVenta();
            _tiposIdentificacion = _estacionesRepositorio.getTiposIdentifiaciones();
            var COD_CAR = ((Cara)comboBox3.SelectedItem).COD_CAR;
            _factura = _estacionesRepositorio.getUltimasFacturas(COD_CAR, 1).FirstOrDefault();
            button2.Enabled = false;
            if (_factura == null)
            {
                MessageBox.Show("No se ha generado factura para esa cara, \n\r Verificar Resolución vencida!");
                return;
            }
            if (_factura.Venta.CONSECUTIVO == -1)
            {
                MessageBox.Show("No existe venta en la cara selecionada!");
                return;
            }
            if (_factura.impresa <= _infoEstacion.vecesPermitidasImpresion)
            {
                button2.Enabled = true;
            }

            

            _venta = _factura.Venta;
            _tercero = _factura.Tercero;



            if (!string.IsNullOrWhiteSpace(_factura.Venta.NIT) && _factura.Tercero.identificacion == "222222222222" && _factura.Tercero.identificacion != _factura.Venta.NIT)
            {
                _tercero = _estacionesRepositorio.getTercero(_factura.Venta.NIT);
                if(_tercero != null) 
                {
                    _estacionesRepositorio.ActualizarFactura(_factura.facturaPOSId, _venta.PLACA, _venta.KILOMETRAJE.HasValue ? _venta.KILOMETRAJE.Value.ToString("F") : "", _factura.codigoFormaPago, _tercero.terceroId, _factura.ventaId);
                }

            }

            _mangueras = _factura.Manguera;
            if (!string.IsNullOrEmpty(_venta.PLACA))
            {
                Placa.Text = _venta.PLACA;
                Placa.Enabled = false;
            }
            else
            {
                Placa.Text = "";
                Placa.Enabled = true;
            }
            Kilometraje.Text = _venta.KILOMETRAJE.HasValue ? _venta.KILOMETRAJE.Value.ToString("F") : "";
            try
            {
                
                var informacionVenta = new StringBuilder();
                informacionVenta.Append("------------------------------------------------" + "\n\r");
                if (_factura.Consecutivo == 0)
                {

                    informacionVenta.Append("Orden de despacho No:"+_venta.CONSECUTIVO+"\n\r");
                }
                else
                {
                    informacionVenta.Append("Factura de venta P.O.S No: " + _factura.DescripcionResolucion + "-" + _factura.Consecutivo + "\n\r");
                }
                informacionVenta.Append("------------------------------------------------" + "\n\r");
                if (_venta.COD_FOR_PAG != 4)
                {
                    informacionVenta.Append("Vendido a : " + _tercero.Nombre + "\n\r");
                    informacionVenta.Append("Nit/C.C. : " + _tercero.identificacion + "\n\r");
                    informacionVenta.Append("Placa : PLACA\n\r");
                    informacionVenta.Append("Kilometraje : KILOMETRAJE\n\r");
                    informacionVenta.Append("Cod Int : " + _venta.COD_INT + "\n\r");
                }
                else
                {
                    informacionVenta.Append("Vendido a : CONSUMIDOR FINAL\n\r");
                    informacionVenta.Append("Nit/C.C. : 222222222222\n\r");
                    informacionVenta.Append("Placa : PLACA\n\r");
                    informacionVenta.Append("Kilometraje : KILOMETRAJE\n\r");
                }
                if (_venta.FECH_ULT_ACTU.HasValue && _mangueras.DESCRIPCION.ToLower().Contains("gn"))
                {
                    informacionVenta.Append("Proximo mantenimiento : " + _venta.FECH_ULT_ACTU.Value.ToString("dd/MM/yyyy") + "\n\r");
                }
                informacionVenta.Append("------------------------------------------------" + "\n\r");
                informacionVenta.Append("Fecha : " + _factura.fecha.ToString("dd/MM/yyyy HH:mm:ss") + "\n\r");
                informacionVenta.Append("Surtidor : " + ((Cara)comboBox3.SelectedItem).COD_SUR + "\n\r");
                informacionVenta.Append("Cara : " + ((Cara)comboBox3.SelectedItem).COD_CAR + "\n\r");
                informacionVenta.Append("Manguera : " + _mangueras.COD_MAN + "\n\r");
                informacionVenta.Append("Vendedor : " + _venta.EMPLEADO + "\n\r");
                informacionVenta.Append("------------------------------------------------" + "\n\r");

                informacionVenta.Append("Producto  Cant.     Precio    Total    " + "\n\r");
                informacionVenta.Append(getLienaTarifas(_mangueras.DESCRIPCION.Trim(), String.Format("{0:#,0.000}", _venta.CANTIDAD), _venta.PRECIO_UNI.ToString("F"), String.Format("{0:#,0.00}", _venta.VALORNETO)) + "\n\r");
                //informacionVenta.Append("------------------------------------------------" + "\n\r");
                informacionVenta.Append("DISCRIMINACION TARIFAS IVA" + "\n\r");
               // informacionVenta.Append("------------------------------------------------" + "\n\r");

                informacionVenta.Append("Producto  Cant.     Tafira    Total    " + "\n\r");
                informacionVenta.Append(getLienaTarifas(_mangueras.DESCRIPCION.Trim(), String.Format("{0:#,0.000}", _venta.CANTIDAD), "0%", String.Format("{0:#,0.00}", _venta.VALORNETO)) + "\n\r");
                informacionVenta.Append("------------------------------------------------" + "\n\r");
                informacionVenta.Append("Subtotal sin IVA : " + String.Format("{0:#,0.00}", _venta.TOTAL) + "\n\r");
                //informacionVenta.Append("------------------------------------------------" + "\n\r");
                informacionVenta.Append("Descuento : " + String.Format("{0:#,0.00}", _venta.Descuento) + "\n\r");
                //informacionVenta.Append("------------------------------------------------" + "\n\r");
                informacionVenta.Append("Subtotal IVA : 0,00 \n\r");
               // informacionVenta.Append("------------------------------------------------" + "\n\r");
                informacionVenta.Append("TOTAL : " + String.Format("{0:#,0.00}", _venta.TOTAL) + "\n\r");
                //informacionVenta.Append("------------------------------------------------" + "\n\r");
                if (_factura.Consecutivo != 0)
                {
                    informacionVenta.Append("Forma de pago : FORMA DE PAGO\n\r");
                    informacionVenta.Append("------------------------------------------------" + "\n\r");
                    
                }

                InfoVenta.Text = informacionVenta.ToString();


                comboBox1.Items.Clear();
                comboBox1.Items.AddRange(formas.ToArray());
                comboBox1.SelectedItem = formas.First(x => x.Id == _factura.Venta.COD_FOR_PAG || x.Id==4);

                textBox1.Enabled = true;
                textBox1.Text = _tercero.identificacion;
                StringBuilder clienteInfo = new StringBuilder();
                clienteInfo.Append("Nombre: ").AppendLine(_tercero.Nombre)
                    .Append("Teléfono: ").AppendLine(_tercero.Telefono)
                    .Append("Correo: ").AppendLine(_tercero.Correo)
                    .Append("Dirección: ").AppendLine(_tercero.Direccion);
                if (_tiposIdentificacion.Any(ti => _tercero.tipoIdentificacion == ti.TipoIdentificacionId)
                    && _tiposIdentificacion.Single(ti => _tercero.tipoIdentificacion == ti.TipoIdentificacionId).Descripcion != "No especificada")
                {

                    clienteInfo.Append("Tipo de indeitificación: ").AppendLine(_tiposIdentificacion.Single(ti => _tercero.tipoIdentificacion == ti.TipoIdentificacionId).Descripcion);
                }
                this.labelCliente.Text = clienteInfo.ToString();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message+" "+ex.StackTrace);
            }

            
        }

        private void cleanVenta()
        {


            InfoVenta.Text = "No se ha selecionado venta";
            button2.Enabled = false;
            textBox1.Text = "";
            this.labelCliente.Text = "";
            _venta = null;
            _tercero = null;
            _factura = null;
            _mangueras = null;
        }

        private void button2_Click(object sender, EventArgs e)
        {
            if (_tercero == null)
            {
                OnTextBox1ChangedComplete();
                MessageBox.Show("Tercero no existe. por favor crearlo");
                textBox8.Text = textBox1.Text;
                tabControl1.SelectedTab = tabPage3;
                return;
            }
            _factura.Tercero = _tercero;
            _venta = _factura.Venta;
            
            _mangueras = _factura.Manguera;
            _charactersPerPage = _infoEstacion.CaracteresPorPagina;
            var _cara = _estacionesRepositorio.getCaras().Single(x => x.COD_CAR == _venta.COD_CAR);

            var placa = string.IsNullOrEmpty(Placa.Text) ? _venta.PLACA : Placa.Text; 
            var kilometraje = string.IsNullOrEmpty(Kilometraje.Text) ? _venta.KILOMETRAJE!=null?_venta.KILOMETRAJE.Value.ToString("F"):"" : Kilometraje.Text;

            _factura.codigoFormaPago = ((FormasPagos)comboBox1.SelectedItem).Id;
            _estacionesRepositorio.ActualizarFactura(_factura.facturaPOSId, placa, kilometraje, _factura.codigoFormaPago, _tercero.terceroId, _factura.ventaId);
            int vecesImprimir = _infoEstacion.vecesImprimir;


            if (_factura.Consecutivo == 0 && _infoEstacion.ConvertirAFactura)
            {
                var opcion = MessageBox.Show("¿Deséa convertir orden de despacho en factura?", "Convertir orden de despacho en factura", MessageBoxButtons.YesNo);
                if (opcion == DialogResult.Yes)
                {
                    _estacionesRepositorio.ConvertirAFactura(_factura.ventaId);
                    this.comboBox3_SelectedIndexChanged(null, null);
                }
                var opcionGenerar = MessageBox.Show("¿Deséa generar factura electrónica?", "Generar factura electrónica", MessageBoxButtons.YesNo);
                if (opcionGenerar == DialogResult.Yes)
                {
                    var result = _conexionEstacionRemota.EnviarFactura(_factura);
                    if (result == "\"Ok\"")
                    {
                        MessageBox.Show("Factura generada!");
                    }
                    else
                    {
                        MessageBox.Show("Factura no generada. Razon " + result);
                    }

                }
            }
            else
            if (_factura.Consecutivo > 0 && _infoEstacion.ConvertirAOrden)
            {
                var opcion = MessageBox.Show("¿Deséa convertir factura en orden de despacho?", "Convertir factura", MessageBoxButtons.YesNo);
                if (opcion == DialogResult.Yes)
                {
                    _estacionesRepositorio.ConvertirAOrden(_factura.ventaId);
                    this.comboBox3_SelectedIndexChanged(null, null);
                }
                var opcionGenerar = MessageBox.Show("¿Deséa generar factura electrónica?", "Generar factura electrónica", MessageBoxButtons.YesNo);
                if (opcionGenerar == DialogResult.Yes)
                {
                    var result = _conexionEstacionRemota.EnviarFactura(_factura);
                    if (result == "\"Ok\"")
                    {
                        MessageBox.Show("Factura generada!");
                    }
                    else
                    {
                        MessageBox.Show("Factura no generada. Razon " + result);
                    }

                }
            }



            for (int i = 0; i < vecesImprimir; i++)
            {

                _estacionesRepositorio.MandarImprimir(_factura.ventaId);

            }
            _factura.impresa++;
            if (_factura.impresa + 1 > _infoEstacion.vecesPermitidasImpresion)
            {
                button2.Enabled = false;
            }

            MessageBox.Show("Factura impresa!");
        }

        private string getLienaTarifas(string v1, string v2, string v3, string v4)
        {
            var spacesInPage = _charactersPerPage / 4;
            var tabs = new StringBuilder();
            tabs.Append(v1.Substring(0, v1.Length< spacesInPage ? v1.Length: spacesInPage));
            var whitespaces = spacesInPage - v1.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

            tabs.Append(v2.Substring(0, v2.Length < spacesInPage ? v2.Length : spacesInPage));
            whitespaces = spacesInPage - v2.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

            tabs.Append(v3.Substring(0, v3.Length < spacesInPage ? v3.Length : spacesInPage));
            whitespaces = spacesInPage - v3.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);

            tabs.Append(v4.Substring(0, v4.Length < spacesInPage ? v4.Length : spacesInPage));
            whitespaces = spacesInPage - v4.Length;
            whitespaces = whitespaces < 0 ? 0 : whitespaces;
            tabs.Append(' ', whitespaces);
            return tabs.ToString();
        }

        private void abrir_Click(object sender, EventArgs e)
        {
            var form = new Form2(_estacionesRepositorio, _infoEstacion);
            form.Show();
        }

        private void cerrar_Click(object sender, EventArgs e)
        {
            var form = new Form3(_estacionesRepositorio, _infoEstacion);
            form.Show();
        }

        private void fidelizar_Click(object sender, EventArgs e)
        {
            var form = new Form4(_estacionesRepositorio, _infoEstacion);
            form.Show();
        }


        private void button1_Click_2(object sender, EventArgs e)
        {
            //Agregar textBox2 comboBox2
            if(!float.TryParse(textBox2.Text, out float cantidad) && cantidad<=0)
            {
                MessageBox.Show("Debe colocar un valor númerico mayor a 0");
                return;
            }
            var canastilla = (Canastilla)comboBox2.SelectedItem;
            if(canastilla == null)
            {
                MessageBox.Show("Debe selecionar un item de canastilla");
                return;
            }
            var canastillaFactura = new CanastillaFactura() { Canastilla = canastilla , cantidad=cantidad};

            agregarAFactura(canastillaFactura);
        }

        private void agregarAFactura(CanastillaFactura canastillaFactura)
        {
            if(facturaCanastilla.canastillas.Any(x=>x.Canastilla.CanastillaId == canastillaFactura.Canastilla.CanastillaId))
            {
                var canastillaFacturaExistente = facturaCanastilla.canastillas.First(x => x.Canastilla.CanastillaId == canastillaFactura.Canastilla.CanastillaId);
                facturaCanastilla.canastillas.Remove(canastillaFacturaExistente);

            }
                canastillaFactura.iva = (canastillaFactura.Canastilla.precio * canastillaFactura.Canastilla.iva / 100.0f) / (1 + canastillaFactura.Canastilla.iva / 100.0f);
                canastillaFactura.precio = canastillaFactura.Canastilla.precio - canastillaFactura.iva;
                canastillaFactura.total = canastillaFactura.Canastilla.precio * canastillaFactura.cantidad;
                canastillaFactura.subtotal = canastillaFactura.precio * canastillaFactura.cantidad;

                facturaCanastilla.canastillas.Add(canastillaFactura);
            

            facturaCanastilla.subtotal = facturaCanastilla.canastillas.Sum(x=>x.subtotal);
            facturaCanastilla.iva = facturaCanastilla.canastillas.Sum(x => x.iva*x.cantidad);
            facturaCanastilla.total = facturaCanastilla.canastillas.Sum(x => x.total);

            setDatosFacturaCanastilla();
        }

        private void setDatosFacturaCanastilla()
        {

                var informacionVenta = new StringBuilder();
                

                informacionVenta.Append("Producto  Cant.     Precio    Subtotal " + "\n\r");

            foreach(var canastilla in facturaCanastilla.canastillas)
            {
                informacionVenta.Append(getLienaTarifas(canastilla.Canastilla.descripcion.Trim(), String.Format("{0:#,0.000}", canastilla.cantidad), canastilla.precio.ToString("F"), String.Format("{0:#,0.00}", canastilla.subtotal)) + "\n\r");

            }
            informacionVenta.Append("DISCRIMINACION TARIFAS IVA" + "\n\r");

                informacionVenta.Append("Producto  Cant.     Tafira    Total    " + "\n\r");
            foreach (var canastilla in facturaCanastilla.canastillas)
            {
                informacionVenta.Append(getLienaTarifas(canastilla.Canastilla.descripcion.Trim(), String.Format("{0:#,0.000}", canastilla.cantidad), canastilla.Canastilla.iva+"%", String.Format("{0:#,0.000}", canastilla.total)) + "\n\r");

            }
            informacionVenta.Append("------------------------------------------------" + "\n\r");
                informacionVenta.Append("Subtotal sin IVA : " + String.Format("{0:#,0.00}", facturaCanastilla.subtotal) + "\n\r");
                informacionVenta.Append("Descuento : " + String.Format("{0:#,0.00}", 0) + "\n\r");
                informacionVenta.Append($"Subtotal IVA : {facturaCanastilla.iva} \n\r");
                informacionVenta.Append("TOTAL : " + String.Format("{0:#,0.00}", facturaCanastilla.total) + "\n\r");
                
                label8.Text = informacionVenta.ToString();

        }

        private void button3_Click(object sender, EventArgs e)
        {
            //Borrar
            facturaCanastilla = new FacturaCanastilla()
            {
                canastillas = new List<CanastillaFactura>()
            };
            _terceroCanastilla = null;
            textBox4.Text = "222222222222";
            OnTextBox4ChangedComplete();
            textBox2.Text = "";
            label8.Text = "Agregue producto para empezar";
            label1.Text = "No se ha selecionado cliente";
            try
            {

                _canastillas = _conexionEstacionRemota.GetCanastilla();
                comboBox2.Items.Clear();
                comboBox2.Items.AddRange(_canastillas.ToArray());
                comboBox2.SelectedIndex = 0;

            } catch(Exception ex)
            {

            }
        }


        private void textBox1_Leave(object sender, EventArgs e)
        {
            timer1.Stop();
            OnTextBox1ChangedComplete();
        }
        private void textBox4_Leave(object sender, EventArgs e)
        {
            timer4.Stop();
            OnTextBox4ChangedComplete();
        }

        private void button4_Click(object sender, EventArgs e)
        {
            //Facturar
            if (!facturaCanastilla.canastillas.Any())
            {

                MessageBox.Show("Debe selecionar al menos un item de canastilla para generar factura");
            }
            if(_terceroCanastilla == null)
            {

                OnTextBox4ChangedComplete();
                MessageBox.Show("Tercero no existe. por favor crearlo");
                textBox8.Text = textBox4.Text;
                tabControl1.SelectedTab = tabPage3;
                return;
            }
            facturaCanastilla.terceroId = _terceroCanastilla;
            facturaCanastilla.descuento = 0;
            facturaCanastilla.codigoFormaPago = new FormaPagoSiges() { Id = 4 };
            try
            {
                var consecutivo = _conexionEstacionRemota.GenerarFacturaCanastilla(facturaCanastilla);
                MessageBox.Show($"Factura {consecutivo} creada");
                button3_Click(null, null);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error generando factura, favor verificar la resolución o comunicarse con el administrador. {ex.Message}");
            }
        }




        private void textBox8_TextChanged(object sender, EventArgs e)
        {

            button5.Enabled = this.textBox8.Text != "222222222222";

            _terceroCrear = _estacionesRepositorio.getTercero(textBox8.Text);

            if (_terceroCrear != null)
            {
                button5.Text = "Actualizar tercero";
                textBox5.Text = _terceroCrear.Nombre;
                textBox3.Text = _terceroCrear.Direccion;
                textBox6.Text = _terceroCrear.Telefono;
                textBox7.Text = _terceroCrear.Correo;
                if(_tiposIdentificacion.Any(x => x.Descripcion == _terceroCrear.tipoIdentificacionS)) 
                {
                    comboBox4.SelectedItem = _tiposIdentificacion.Where(x => x.Descripcion == _terceroCrear.tipoIdentificacionS).FirstOrDefault();
                }
            }
            else
            {

                button5.Text = "Crear tercero";
                this.comboBox4.Text = "Selec. Tipo Identificacion";
                comboBox4.SelectedIndex = 0;
                textBox5.Text = "";
                textBox3.Text = "";
                textBox6.Text = "";
                textBox7.Text = "";
            }
        }

        private void button5_Click(object sender, EventArgs e)
        {
            if ((TipoIdentificacion)comboBox4.SelectedItem == null)
            {

                MessageBox.Show("Debe seleccionar tipo de identificación");
                return;
            }
            var identificacio = textBox8.Text;
            var nombre = textBox5.Text;
            var direccion = textBox3.Text;
            var telefono = textBox6.Text;
            var correo = textBox7.Text;
            var terceroId = _terceroCrear?.terceroId;
            _estacionesRepositorio.crearTercero(terceroId.HasValue? terceroId.Value:0, (TipoIdentificacion)comboBox4.SelectedItem, identificacio, nombre, telefono, correo, direccion, "");
            _terceroCrear = _estacionesRepositorio.getTercero(textBox8.Text);

            MessageBox.Show("Tercero creado/actualizado");
            this.comboBox4.Text = "Selec. Tipo Identificacion";
            comboBox4.SelectedIndex = 0;
            textBox5.Text = "";
            textBox3.Text = "";
            textBox6.Text = "";
            textBox7.Text = "";
            textBox8.Text = "";
        }


        private void TabIndexChange(object sender, EventArgs e)
        {
            var ev = (TabControlCancelEventArgs)e;
            var pestanaSiguiente = ev.TabPage.Text;
            if(pestanaActual == "Combustible" && pestanaSiguiente == "Terceros")
            {
                textBox8.Text = textBox1.Text;
            }
            else if (pestanaActual == "Canastilla" && pestanaSiguiente == "Terceros")
            {
                textBox8.Text = textBox4.Text;

            }
            else if(pestanaActual == "Terceros" && pestanaSiguiente == "Combustible")
            {
                OnTextBox1ChangedComplete();
            }
            else if(pestanaActual == "Terceros" && pestanaSiguiente == "Canastilla")
            {
                OnTextBox4ChangedComplete();
            }
            pestanaActual = pestanaSiguiente;
        }
    }
}
