package com.example.facturadorsiges;

import androidx.annotation.NonNull;
import androidx.appcompat.app.AppCompatActivity;

import android.content.Context;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.TimeUnit;

import ApiConnectionClasses.IVentas;
import Modelo.CanastillaFactura;
import Modelo.FacturaCanastilla;
import Modelo.FormasDePagos;
import Modelo.FormasPagos;
import Modelo.Tercero;
import PrinterHelper.SunmiPrintHelper;
import okhttp3.OkHttpClient;
import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import sqlite.ConfigurationManager;
import sqlite.DBManager;

public class Canastilla extends AppCompatActivity {
    private Spinner canastillas;
    private IVentas jsonPlaceHolderApi;
    private ConfigurationManager cm;
    private Button Agregar;
    private Button Borrar;
    private Button Generar;
    private TextView factura;
    private EditText identificacion;
    private EditText nombreText;
    private EditText cantidad;
    private Context thisContext;
    private List<Modelo.Canastilla> canastillasModelo;

    private FacturaCanastilla facturaCanastilla;
    private Tercero _terceroCanastilla;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_canastilla);
        SunmiPrintHelper.getInstance().initPrinter();
    }

    @Override
    protected void onStart() {
        super.onStart();
        thisContext = this;
        cm = new ConfigurationManager(this);
        canastillas = (Spinner) findViewById(R.id.canastillas);
        factura =  (TextView) findViewById(R.id.factura);
        identificacion =  (EditText) findViewById(R.id.identificacion);
        cantidad =  (EditText) findViewById(R.id.cantidad);
        nombreText =  (EditText) findViewById(R.id.nombreText);
        Agregar =  (Button) findViewById(R.id.Agregar);
        Borrar =  (Button) findViewById(R.id.Borrar);
        Generar =  (Button) findViewById(R.id.Generar);
        facturaCanastilla = new FacturaCanastilla();
        facturaCanastilla.canastillas = new ArrayList<CanastillaFactura>();

        final OkHttpClient okHttpClient = new OkHttpClient.Builder()
                .callTimeout(120000, TimeUnit.MILLISECONDS)
                .connectTimeout(120000, TimeUnit.MILLISECONDS)
                .writeTimeout(120000, TimeUnit.MILLISECONDS)
                .readTimeout(120000, TimeUnit.MILLISECONDS)
                .build();


        Retrofit retrofit = new Retrofit.Builder().baseUrl("http://"+cm.ip+":5544/api/Ventas/")
                .addConverterFactory(GsonConverterFactory.create()).client(okHttpClient)
                .build();

        jsonPlaceHolderApi = retrofit.create(IVentas.class);

        LlenarCanastillas();

        identificacion.setText("222222222222");
        BuscarTercero();
        Borrar.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) { Borrar(); }});


        Generar.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) { Generar(); }});


        Agregar.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) { Agregar(); }});

        identificacion.addTextChangedListener(new TextWatcher() {

            @Override
            public void beforeTextChanged(CharSequence s, int start, int count, int after) {

            }

            @Override
            public void onTextChanged(CharSequence s, int start, int before, int count) {

            }

            @Override
            public void afterTextChanged(Editable s) {
                BuscarTercero();

            }
        });
    }

    private void Generar() {

        //Facturar
        if (facturaCanastilla.canastillas.size() == 0)
        {

            Toast.makeText(thisContext,"Debe selecionar al menos un item de canastilla para generar factura",Toast.LENGTH_LONG).show();

        }
        if(_terceroCanastilla == null)
        {
            _terceroCanastilla = new Tercero();
            _terceroCanastilla.terceroId = -1;
            _terceroCanastilla.identificacion = identificacion.getText().toString();

        }
        _terceroCanastilla.Nombre = nombreText.getText().toString();
        facturaCanastilla.terceroId = _terceroCanastilla;
        facturaCanastilla.descuento = 0;
        facturaCanastilla.codigoFormaPago = new FormasPagos();
        facturaCanastilla.codigoFormaPago.Id = 4;
        try
        {
            System.out.println("imprimePDA "+cm.impresionPDA);

            Call<String> facturacionCall = jsonPlaceHolderApi.GenerarFacturaCanastilla(!cm.impresionPDA, facturaCanastilla);
            facturacionCall.enqueue(new Callback<String>() {
                @Override
                public void onResponse(Call<String> call, Response<String> response) {
                    if (!response.isSuccessful()) {

                        System.out.println("notSucessful");
                        return;
                    }
                    if(response.body() != "0"){
                        Toast.makeText(thisContext,"Factura "+response.body()+" creada",Toast.LENGTH_LONG).show();
                        if(cm.impresionPDA){
                            imprimirFactura(response.body());
                        }

                        Borrar();
                    }else {
                        Toast.makeText(thisContext,"No se puede generar facturas sin items",Toast.LENGTH_LONG).show();

                    }


                }

                @Override
                public void onFailure(Call<String> call, Throwable t) {

                    System.out.println("notSucessful");
                    System.out.println(t.getMessage());
                }
            });

        }
        catch (Exception ex)
        {
            Toast.makeText(thisContext,"Error generando factura, favor verificar la resolución o comunicarse con el administrador",Toast.LENGTH_LONG).show();

        }


    }

    private void imprimirFactura(String consecutivo) {

        SunmiPrintHelper.getInstance().initSunmiPrinterService(this);
        Call<String> facturacionCall = jsonPlaceHolderApi.GetFacturaCanastilla(consecutivo);
        facturacionCall.enqueue(new Callback<String>() {
            @Override
            public void onResponse(Call<String> call, Response<String> response) {
                if (!response.isSuccessful()) {

                    System.out.println("notSucessful");
                    return;
                }

                SunmiPrintHelper.getInstance().printText(response.body(), 12, false, false);
                SunmiPrintHelper.getInstance().feedPaper();


                Borrar();

            }

            @Override
            public void onFailure(Call<String> call, Throwable t) {

                System.out.println("notSucessful");
                System.out.println(t.getMessage());
            }
        });


    }

    private void Agregar() {
        Modelo.CanastillaFactura canastillaAagregar = getCanastillaSelecionada();
        if(canastillaAagregar == null){
            return;
        }
        if(alreadyAddedCanastilla(canastillaAagregar))
        {
            canastillaAagregar = getCanastillaFromList(canastillaAagregar);

        }
        canastillaAagregar.iva = (canastillaAagregar.Canastilla.precio * canastillaAagregar.Canastilla.iva / 100.0f) / (1 + canastillaAagregar.Canastilla.iva / 100.0f);
        canastillaAagregar.precio = canastillaAagregar.Canastilla.precio - canastillaAagregar.iva;
        canastillaAagregar.total = canastillaAagregar.Canastilla.precio * canastillaAagregar.cantidad;
        canastillaAagregar.subtotal = canastillaAagregar.precio * canastillaAagregar.cantidad;

        facturaCanastilla.canastillas.add(canastillaAagregar);

        facturaCanastilla.subtotal = 0;
        facturaCanastilla.iva = 0;
        facturaCanastilla.total = 0;
        for(CanastillaFactura cf : facturaCanastilla.canastillas){

            facturaCanastilla.subtotal += cf.subtotal;
            facturaCanastilla.iva += cf.iva*cf.cantidad;
            facturaCanastilla.total += cf.total;
        }

        LlenarTextoFactura();
    }

    private CanastillaFactura getCanastillaSelecionada() {

        try{
            CanastillaFactura cf = new CanastillaFactura();

            String canastillaS = (String)canastillas.getSelectedItem();
            for (Modelo.Canastilla c: canastillasModelo) {
                if(canastillaS.equals(c.descripcion)){
                    cf.Canastilla = c;
                }
            }
            int cant = Integer.parseInt(String.valueOf(cantidad.getText()));
            if(cant<=0){
                Toast.makeText(thisContext,"Error la cantidad debe ser un numero entero positivo",Toast.LENGTH_LONG).show();
            }
            cf.cantidad = cant;
            return cf;
        } catch(Exception ex){
            Toast.makeText(thisContext,"Error la cantidad debe ser un numero entero positivo",Toast.LENGTH_LONG).show();
            return null;}

    }

    private CanastillaFactura getCanastillaFromList(CanastillaFactura canastillaAagregar) {
        CanastillaFactura ce = null;
        for(CanastillaFactura cf : facturaCanastilla.canastillas){

            if(cf.Canastilla.descripcion == canastillaAagregar.Canastilla.descripcion){
                ce = cf;
                break;
            }
        }
        facturaCanastilla.canastillas.remove(ce);
        ce.cantidad = canastillaAagregar.cantidad;
        return ce;
    }


    private boolean alreadyAddedCanastilla(CanastillaFactura canastillaAagregar) {
        for(CanastillaFactura cf : facturaCanastilla.canastillas){

            if(cf.Canastilla.descripcion == canastillaAagregar.Canastilla.descripcion){
                return true;
            }
        }
        return false;
    }


    private void Borrar() {

        facturaCanastilla = new FacturaCanastilla();
        facturaCanastilla.canastillas = new ArrayList<CanastillaFactura>();
        _terceroCanastilla = null;
        nombreText.setText("");
        identificacion.setText("222222222222");
        cantidad.setText("");
        factura.setText("Agregue producto para empezar");
        try
        {
            LlenarCanastillas();
        } catch(Exception ex)
        {

        }
    }

    private void LlenarTextoFactura() {

        StringBuilder informacionVenta = new StringBuilder();
        if(_terceroCanastilla != null){

            nombreText.setText(_terceroCanastilla.Nombre);
            informacionVenta.append("Vendido a :  " +_terceroCanastilla.Nombre+ "\n\r");
            informacionVenta.append("Identificación :  " +_terceroCanastilla.identificacion +"\n\r");
        } else{

            informacionVenta.append("Vendido a : "+nombreText.getText()+"\n\r");
            informacionVenta.append("Identificación :  " +identificacion.getText() +"\n\r");
        }

        if(!facturaCanastilla.canastillas.isEmpty()) {
            for (Modelo.CanastillaFactura canastilla : facturaCanastilla.canastillas) {
                informacionVenta.append("Producto: " + canastilla.Canastilla.descripcion + "\n\r");
                informacionVenta.append("Cantidad: " + canastilla.cantidad + "\n\r");
                informacionVenta.append("precio: " + canastilla.precio + "\n\r");
                informacionVenta.append("subtotal: " + canastilla.subtotal + "\n\r");
            }
            informacionVenta.append("DISCRIMINACION TARIFAS IVA" + "\n\r");

            for (Modelo.CanastillaFactura canastilla : facturaCanastilla.canastillas) {

                informacionVenta.append("Producto: " + canastilla.Canastilla.descripcion + "\n\r");
                informacionVenta.append("Cantidad: " + canastilla.cantidad + "\n\r");
                informacionVenta.append("iva: " + canastilla.iva + "\n\r");
                informacionVenta.append("total: " + canastilla.total + "\n\r");
            }
            informacionVenta.append("------------------------------------------------" + "\n\r");
            informacionVenta.append("Subtotal sin IVA : " + facturaCanastilla.subtotal + "\n\r");
            informacionVenta.append("Descuento : " + 0 + "\n\r");
            informacionVenta.append("Subtotal IVA : " + facturaCanastilla.iva + " \n\r");
            informacionVenta.append("TOTAL : " + facturaCanastilla.total + "\n\r");
        }
        factura.setText(informacionVenta.toString());
    }

    private void BuscarTercero() {

        Call<Tercero> listCall = jsonPlaceHolderApi.GetTerceros(identificacion.getText().toString());

        listCall.enqueue(new Callback<Tercero>() {
            @Override
            public void onResponse(Call<Tercero> call, Response<Tercero> response) {
                if (!response.isSuccessful()) {

                    System.out.println("notSucessful"+response.message());
                    return;
                }

                System.out.println("Success");
                Tercero tercero = response.body();

                _terceroCanastilla = tercero;
                LlenarTextoFactura();

            }

            @Override
            public void onFailure(Call<Tercero> call, Throwable t) {
                try{
                    Toast.makeText(thisContext,"NO hay conexion! Cambiar IP",Toast.LENGTH_LONG).show();
                    System.out.println("notSucessful");
                    System.out.println(t.getMessage());}catch(Exception e){}
            }
        });
    }

    private void LlenarCanastillas() {
        Call<List<Modelo.Canastilla>> listCall = jsonPlaceHolderApi.GetCanastilla();

        listCall.enqueue(new Callback<List<Modelo.Canastilla>>() {
            @Override
            public void onResponse(Call<List<Modelo.Canastilla>> call, Response<List<Modelo.Canastilla>> response) {
                if (!response.isSuccessful()) {

                    System.out.println("notSucessful"+response.message());
                    return;
                }

                System.out.println("Success");
                canastillasModelo = response.body();
                ArrayList<String> canastillasS = new ArrayList<>();
                String descripcion = "";
                for (Modelo.Canastilla c : canastillasModelo) {
                    canastillasS.add(c.descripcion);
                }
                ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(thisContext, android.R.layout.simple_spinner_item, canastillasS);
                arrayAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
                canastillas.setAdapter(arrayAdapter);
                canastillas.setSelection(arrayAdapter.getPosition(descripcion));
            }

            @Override
            public void onFailure(Call<List<Modelo.Canastilla>> call, Throwable t) {
                try{
                    Toast.makeText(thisContext,"NO hay conexion! Cambiar IP",Toast.LENGTH_LONG).show();
                    System.out.println("notSucessful");
                    System.out.println(t.getMessage());}catch(Exception e){}
            }
        });
    }
}