package com.example.facturadorsiges;

import android.app.AlertDialog;
import android.content.Context;
import android.content.DialogInterface;
import android.content.SharedPreferences;
import android.database.Cursor;
import android.os.Bundle;
import android.text.Editable;
import android.text.TextWatcher;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.navigation.fragment.NavHostFragment;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.TimeUnit;

import ApiConnectionClasses.IVentas;
import Modelo.Factura;
import Modelo.FormasDePagos;
import Modelo.Tercero;
import Modelo.TipoIdentificacion;
import PrinterHelper.BluetoothUtil;
import PrinterHelper.ESCUtil;
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

public class SecondFragment extends Fragment {

    IDataCommunication dataCommunication;
    Factura factura;
    List<TipoIdentificacion> tiposIdentificaciones;
    List<FormasDePagos> formas;
    private DBManager dbManager;
    private TextView informacionFacturaTextView;
    private EditText identificacion;
    private EditText telefono;
    private EditText nombre;
    private EditText direccion;
    private EditText correo;
    private EditText placa;
    private EditText kilometraje;
    private Button imprimir;
    private Spinner s2;
    private Spinner s3;
    private int finalVecesImpresa;
    private IVentas jsonPlaceHolderApi;
    private int codCar;
    private String finalIp;
    private boolean imprimirPDA;
    private Call<String> listCall;
    private ConfigurationManager cm;

    @Override
    public void onAttach(Context context){
        super.onAttach(context);
        try{
            dataCommunication = (IDataCommunication)context;
        }catch(Exception e){}

    }

    @Override
    public View onCreateView(
            LayoutInflater inflater, ViewGroup container,
            Bundle savedInstanceState
    ) {
        SunmiPrintHelper.getInstance().initPrinter();
        return inflater.inflate(R.layout.fragment_second, container, false);

    }

    public String getFacturaInfo(Factura _factura){
        StringBuilder informacionVenta = new StringBuilder();
        informacionVenta.append("Razón social\n\r");
        informacionVenta.append("NIT \n\r");
        informacionVenta.append("Nombre estación\n\r");
        informacionVenta.append("Dirección\n\r");
        informacionVenta.append("Teléfono\n\r");
        informacionVenta.append("------------------------------------------------" + "\n\r");

        if (_factura.Consecutivo == 0)
        {

            informacionVenta.append("Orden de despacho\n\r");
        }
        else
        {
            informacionVenta.append("Sistema P.O.S No: " + _factura.DescripcionResolucion + "-" + _factura.Consecutivo + "\n\r");
        }
        informacionVenta.append("------------------------------------------------" + "\n\r");

            if (_factura.Venta.COD_FOR_PAG != 4)
            {
                informacionVenta.append("Vendido a : " + _factura.Tercero.Nombre + "\n\r");
                informacionVenta.append("Nit/C.C. : " + _factura.Tercero.identificacion + "\n\r");
                informacionVenta.append("Placa : PLACA\n\r");
                informacionVenta.append("Kilometraje : KILOMETRAJE\n\r");
                informacionVenta.append("Cod Int : " + _factura.Venta.COD_INT + "\n\r");
            }
            else
            {
                informacionVenta.append("Vendido a : CONSUMIDOR FINAL\n\r");
                informacionVenta.append("Nit/C.C. : 222222222222\n\r");
                informacionVenta.append("Placa : PLACA\n\r");
                informacionVenta.append("Kilometraje : KILOMETRAJE\n\r");
            }
            if (_factura.Venta.FECH_ULT_ACTU!=null)
            {
                informacionVenta.append("Proximo mantenimiento : " + _factura.Venta.FECH_ULT_ACTU + "\n\r");
            }
            informacionVenta.append("------------------------------------------------" + "\n\r");
            informacionVenta.append("Fecha : " + _factura.fecha + "\n\r");
            informacionVenta.append("Surtidor : " + _factura.Venta.COD_SUR + "\n\r");
            informacionVenta.append("Cara : " + _factura.Venta.COD_CAR + "\n\r");
            informacionVenta.append("Manguera : " + _factura.Manguera.COD_MAN + "\n\r");
            informacionVenta.append("------------------------------------------------" + "\n\r");

            informacionVenta.append("Producto  Cant.     Precio    Total    " + "\n\r");
            informacionVenta.append(getLienaTarifas(_factura.Manguera.DESCRIPCION.trim(), _factura.Venta.CANTIDAD+"", _factura.Venta.PRECIO_UNI+"", _factura.Venta.TOTAL+"") + "\n\r");
            informacionVenta.append("------------------------------------------------" + "\n\r");
            informacionVenta.append("DISCRIMINACION TARIFAS IVA" + "\n\r");
            informacionVenta.append("------------------------------------------------" + "\n\r");

            informacionVenta.append("Producto  Cant.     Tafira    Total    " + "\n\r");
        informacionVenta.append(getLienaTarifas(_factura.Manguera.DESCRIPCION.trim(), _factura.Venta.CANTIDAD+"", "0%", _factura.Venta.TOTAL+"") + "\n\r");
        informacionVenta.append("------------------------------------------------" + "\n\r");
        informacionVenta.append("Subtotal sin IVA : " + _factura.Venta.TOTAL + "\n\r");
        informacionVenta.append("------------------------------------------------" + "\n\r");
        informacionVenta.append("Descuent : " + _factura.Venta.Descuento + "\n\r");
            informacionVenta.append("------------------------------------------------" + "\n\r");
            informacionVenta.append("Subtotal IVA : 0,00 \n\r");
            informacionVenta.append("------------------------------------------------" + "\n\r");
        informacionVenta.append("TOTAL : " + _factura.Venta.TOTAL + "\n\r");
        informacionVenta.append("------------------------------------------------" + "\n\r");
        if (_factura.Consecutivo != 0)
        {
            informacionVenta.append("Forma de pago : FORMA DE PAGO\n\r");
            informacionVenta.append("------------------------------------------------" + "\n\r");
        }



        return informacionVenta.toString();
    }

    private String getLienaTarifas(String v1, String v2, String v3, String v4)
    {
        int spacesInPage = 10;
        StringBuilder tabs = new StringBuilder();
        tabs.append(v1.substring(0, v1.length() < spacesInPage ? v1.length() : spacesInPage));
        int whitespaces = spacesInPage - v1.length();
        whitespaces = whitespaces < 0 ? 0 : whitespaces;
        tabs.append(' '*whitespaces);

        tabs.append(v2.substring(0, v2.length() < spacesInPage ? v2.length() : spacesInPage));
        whitespaces = spacesInPage - v2.length();
        whitespaces = whitespaces < 0 ? 0 : whitespaces;
        tabs.append(' '* whitespaces);

        tabs.append(v3.substring(0, v3.length() < spacesInPage ? v3.length() : spacesInPage));
        whitespaces = spacesInPage - v3.length();
        whitespaces = whitespaces < 0 ? 0 : whitespaces;
        tabs.append(' '* whitespaces);

        tabs.append(v4.substring(0, v4.length() < spacesInPage ? v4.length() : spacesInPage));
        whitespaces = spacesInPage - v4.length();
        whitespaces = whitespaces < 0 ? 0 : whitespaces;
        tabs.append(' '* whitespaces);
        return tabs.toString();
    }


    public void onViewCreated(@NonNull View view, Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);

        cm = new ConfigurationManager(getContext());
        SunmiPrintHelper.getInstance().initSunmiPrinterService(getContext());
        dbManager = new DBManager(getContext());
        dbManager.open();
        System.out.println(dataCommunication.getCara().DESCRIPCION);
        informacionFacturaTextView =  (TextView) view.findViewById(R.id.textView);
        identificacion =  (EditText) view.findViewById(R.id.identificacion);
        nombre =  (EditText) view.findViewById(R.id.Nombre);
        direccion =  (EditText) view.findViewById(R.id.Direccion);
        telefono =  (EditText) view.findViewById(R.id.Telefono);
        correo =  (EditText) view.findViewById(R.id.Correo);
        placa =  (EditText) view.findViewById(R.id.Placa);
        kilometraje =  (EditText) view.findViewById(R.id.Kilometraje);
        imprimir =  (Button) view.findViewById(R.id.Borrar);

        s2 = (Spinner) view.findViewById(R.id.spinner2);
        s3 = (Spinner) view.findViewById(R.id.spinner3);


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
        SharedPreferences sharedPref = getActivity().getPreferences(Context.MODE_PRIVATE);
        codCar = sharedPref.getInt("Cara", 0);

        finalVecesImpresa = cm.vecesImpresa;
        finalIp = cm.ip;


        BuscarYLlenarFactura();


        view.findViewById(R.id.button_second).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                NavHostFragment.findNavController(SecondFragment.this)
                        .navigate(R.id.action_SecondFragment_to_FirstFragment);
            }
        });
        view.findViewById(R.id.Borrar).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Retrofit retrofit = new Retrofit.Builder().baseUrl("http://"+finalIp+":5544/api/Ventas/")
                        .addConverterFactory(GsonConverterFactory.create())
                        .build();

                final IVentas jsonPlaceHolderApi = retrofit.create(IVentas.class);
                String descTipoIdent = (String)s2.getSelectedItem();
                for(TipoIdentificacion t : tiposIdentificaciones){
                    if(t.Descripcion == descTipoIdent){
                        factura.Tercero.tipoIdentificacion = t.TipoIdentificacionId;
                    }
                }


                String descForma = (String)s3.getSelectedItem();
                for(FormasDePagos f : formas){
                    if(f.Descripcion == descForma){
                        factura.codigoFormaPago = f.Id;
                    }
                }
                factura.Tercero.identificacion = identificacion.getText().toString();
                factura.Tercero.Nombre = nombre.getText().toString();
                factura.Tercero.Direccion = direccion.getText().toString();
                factura.Tercero.Telefono = telefono.getText().toString();
                factura.Tercero.Correo = correo.getText().toString();
                factura.Placa = placa.getText().toString();
                factura.Kilometraje = kilometraje.getText().toString();

                System.out.println(placa.getText().toString());
                System.out.println(kilometraje.getText().toString());
                System.out.println(factura.Placa);
                System.out.println(factura.Kilometraje);

                System.out.println("imprimePDA "+imprimirPDA);
                listCall = jsonPlaceHolderApi.ImprimirFactura(!imprimirPDA, factura);
                factura.vecesImpresa++;


                    if(factura.Consecutivo == 0 && cm.convertirFactura){

                        AlertDialog.Builder builder = new AlertDialog.Builder(getActivity());
                        builder.setMessage("Convertir a factura?")
                                .setTitle("Convertir a factura?");
                        builder.setPositiveButton("Si", new DialogInterface.OnClickListener() {
                            public void onClick(DialogInterface dialog, int id) {
                                Call<Boolean> facturacionCall = jsonPlaceHolderApi.ConvertirAFactura(factura.ventaId);
                                facturacionCall.enqueue(new Callback<Boolean>() {
                                    @Override
                                    public void onResponse(Call<Boolean> call, Response<Boolean> response) {
                                        if (!response.isSuccessful()) {

                                            System.out.println("notSucessful");
                                            return;
                                        }
                                        BuscarYLlenarFactura();

                                        if(factura.vecesImpresa <= finalVecesImpresa){
                                            imprimir.setEnabled(true);
                                        }imprimir();

                                    }

                                    @Override
                                    public void onFailure(Call<Boolean> call, Throwable t) {

                                        Toast.makeText(getContext(),"Error al imprimir",Toast.LENGTH_LONG).show();
                                        System.out.println("notSucessful");
                                        System.out.println(t.getMessage());
                                    }
                                });
                            }
                        });
                        builder.setNegativeButton("No", new DialogInterface.OnClickListener() {
                            public void onClick(DialogInterface dialog, int id) {

                                if(factura.vecesImpresa <= finalVecesImpresa){
                                    imprimir.setEnabled(true);
                                }imprimir();
                            }
                        });
                        AlertDialog dialog = builder.create();
                        dialog.show();
                    }
                    else if(factura.Consecutivo > 0 && cm.convertirOrden){

                        AlertDialog.Builder builder = new AlertDialog.Builder(getActivity());
                        builder.setMessage("Convertir a Orden de despacho?")
                                .setTitle("Convertir a Orden de despacho?");
                        builder.setPositiveButton("Si", new DialogInterface.OnClickListener() {
                            public void onClick(DialogInterface dialog, int id) {
                                Call<Boolean> facturacionCall = jsonPlaceHolderApi.ConvertirAOrden(factura.ventaId);
                                facturacionCall.enqueue(new Callback<Boolean>() {
                                    @Override
                                    public void onResponse(Call<Boolean> call, Response<Boolean> response) {
                                        if (!response.isSuccessful()) {

                                            System.out.println("notSucessful");
                                            return;
                                        }
                                        BuscarYLlenarFactura();

                                        if(factura.vecesImpresa <= finalVecesImpresa){
                                            imprimir.setEnabled(true);
                                        }imprimir();

                                    }

                                    @Override
                                    public void onFailure(Call<Boolean> call, Throwable t) {

                                        Toast.makeText(getContext(),"Error al imprimir",Toast.LENGTH_LONG).show();
                                        System.out.println("notSucessful");
                                        System.out.println(t.getMessage());
                                    }
                                });
                            }
                        });
                        builder.setNegativeButton("No", new DialogInterface.OnClickListener() {
                            public void onClick(DialogInterface dialog, int id) {

                                if(factura.vecesImpresa <= finalVecesImpresa){
                                    imprimir.setEnabled(true);
                                }imprimir();
                            }
                        });
                        AlertDialog dialog = builder.create();
                        dialog.show();
                    }
                    else{
                        if(factura.vecesImpresa <= finalVecesImpresa){
                            imprimir.setEnabled(true);
                        }imprimir();
                    }



            }
        });

        identificacion.addTextChangedListener(new TextWatcher() {

            @Override
            public void afterTextChanged(Editable s) {
                Call<Tercero> GetTerceros = jsonPlaceHolderApi.GetTerceros(identificacion.getText().toString());

                GetTerceros.enqueue(new Callback<Tercero>() {
                    @Override
                    public void onResponse(Call<Tercero> call, Response<Tercero> response) {
                        if (!response.isSuccessful()) {

                            System.out.println("notSucessful"+response.message());
                            return;
                        }

                        System.out.println("Success");
                        Tercero tercero = response.body();
                        if(tercero!=null){
                        nombre.setText(tercero.Nombre);
                        direccion.setText(tercero.Direccion);
                        telefono.setText(tercero.Telefono);
                        correo.setText(tercero.Correo);}
                    }

                    @Override
                    public void onFailure(Call<Tercero> call, Throwable t) {
                        try{
                            Toast.makeText(getContext(),"NO hay conexion! Cambiar IP",Toast.LENGTH_LONG).show();
                            System.out.println("notSucessful");
                            System.out.println(t.getMessage());}catch(Exception e){}
                    }
                });

            }

            @Override
            public void beforeTextChanged(CharSequence s, int start,
                                          int count, int after) {
            }

            @Override
            public void onTextChanged(CharSequence s, int start,
                                      int before, int count) {
            }
        });
    }

    private void imprimir() {

        if(cm.generarFacturaElectronica){

            AlertDialog.Builder builder = new AlertDialog.Builder(getActivity());
            builder.setMessage("Generar factura electrónica?")
                    .setTitle("Generar factura electrónica?");
            builder.setPositiveButton("Si", new DialogInterface.OnClickListener() {
                public void onClick(DialogInterface dialog, int id) {
                    Call<String> facturacionCall = jsonPlaceHolderApi.EnviarFacturaElectronica(factura);
                    facturacionCall.enqueue(new Callback<String>() {
                        @Override
                        public void onResponse(Call<String> call, Response<String> response) {
                            if (!response.isSuccessful()) {

                                System.out.println("notSucessful");
                                return;
                            }

                            Toast.makeText(getContext(),response.body(),Toast.LENGTH_LONG).show();

                            listCall.enqueue(new Callback<String>() {
                                @Override
                                public void onResponse(Call<String> call, Response<String> response) {
                                    if (!response.isSuccessful()) {

                                        System.out.println("notSucessful");
                                        System.out.println(response.message());
                                        System.out.println(response.body());
                                        try {
                                            System.out.println(response.errorBody().string());
                                        } catch (IOException e) {
                                            e.printStackTrace();
                                        }
                                        return;
                                    }

                                    if(imprimirPDA){

                                        Toast.makeText(getContext(),"Imprimiendo en la PDA",Toast.LENGTH_LONG).show();
                                        SunmiPrintHelper.getInstance().printText(response.body(), 12, false, false);
                                        SunmiPrintHelper.getInstance().feedPaper();

                                    }
                                    Toast.makeText(getContext(),"Impresa",Toast.LENGTH_LONG).show();
                                }


                                @Override
                                public void onFailure(Call<String> call, Throwable t) {

                                    Toast.makeText(getContext(),"NO hay conexion! Cambiar IP",Toast.LENGTH_LONG).show();
                                    System.out.println("Failure");
                                    System.out.println(t.getCause());
                                    System.out.println(t.getStackTrace());
                                    System.out.println(t.getMessage());
                                }
                            });
                        }

                        @Override
                        public void onFailure(Call<String> call, Throwable t) {

                            Toast.makeText(getContext(),"Error al generar",Toast.LENGTH_LONG).show();
                            System.out.println("notSucessful");
                            System.out.println(t.getMessage());
                        }
                    });
                }
            });
            builder.setNegativeButton("No", new DialogInterface.OnClickListener() {
                public void onClick(DialogInterface dialog, int id) {

                    listCall.enqueue(new Callback<String>() {
                        @Override
                        public void onResponse(Call<String> call, Response<String> response) {
                            if (!response.isSuccessful()) {

                                System.out.println("notSucessful");
                                System.out.println(response.message());
                                System.out.println(response.body());
                                try {
                                    System.out.println(response.errorBody().string());
                                } catch (IOException e) {
                                    e.printStackTrace();
                                }
                                return;
                            }

                            if(imprimirPDA){

                                Toast.makeText(getContext(),"Imprimiendo en la PDA",Toast.LENGTH_LONG).show();
                                SunmiPrintHelper.getInstance().printText(response.body(), 12, false, false);
                                SunmiPrintHelper.getInstance().feedPaper();

                            }
                            Toast.makeText(getContext(),"Impresa",Toast.LENGTH_LONG).show();
                        }


                        @Override
                        public void onFailure(Call<String> call, Throwable t) {

                            Toast.makeText(getContext(),"NO hay conexion! Cambiar IP",Toast.LENGTH_LONG).show();
                            System.out.println("Failure");
                            System.out.println(t.getCause());
                            System.out.println(t.getStackTrace());
                            System.out.println(t.getMessage());
                        }
                    });
                }
            });
            AlertDialog dialog = builder.create();
            dialog.show();

        }
        else{

            listCall.enqueue(new Callback<String>() {
                @Override
                public void onResponse(Call<String> call, Response<String> response) {
                    if (!response.isSuccessful()) {

                        System.out.println("notSucessful");
                        System.out.println(response.message());
                        System.out.println(response.body());
                        try {
                            System.out.println(response.errorBody().string());
                        } catch (IOException e) {
                            e.printStackTrace();
                        }
                        return;
                    }

                    if(imprimirPDA){

                        Toast.makeText(getContext(),"Imprimiendo en la PDA",Toast.LENGTH_LONG).show();
                        SunmiPrintHelper.getInstance().printText(response.body(), 12, false, false);
                        SunmiPrintHelper.getInstance().feedPaper();

                    }
                    Toast.makeText(getContext(),"Impresa",Toast.LENGTH_LONG).show();
                }


                @Override
                public void onFailure(Call<String> call, Throwable t) {

                    Toast.makeText(getContext(),"NO hay conexion! Cambiar IP",Toast.LENGTH_LONG).show();
                    System.out.println("Failure");
                    System.out.println(t.getCause());
                    System.out.println(t.getStackTrace());
                    System.out.println(t.getMessage());
                }
            });
        }

    }

    private void BuscarYLlenarFactura() {


        Call<Factura> getFactura = jsonPlaceHolderApi.getFactura(dataCommunication.getCara().COD_CAR);


        getFactura.enqueue(new Callback<Factura>() {
            @Override
            public void onResponse(Call<Factura> call, Response<Factura> response) {
                if (!response.isSuccessful()) {

                    System.out.println("notSucessful");
                    System.out.println(response.body());
                    return;
                }
                factura = response.body();
                System.out.println(factura.Consecutivo);
                String informacion = getFacturaInfo(factura);
                //String informacion = getFacturaInfoBasica(factura);

                informacionFacturaTextView.setText(informacion);
                identificacion.setText(factura.Tercero.identificacion);
                nombre.setText(factura.Tercero.Nombre);
                direccion.setText(factura.Tercero.Direccion);
                telefono.setText(factura.Tercero.Telefono);
                correo.setText(factura.Tercero.Correo);
                placa.setText(factura.Placa);
                kilometraje.setText(factura.Kilometraje);

                final int tipoIdentificacion = factura.Tercero.tipoIdentificacion;
                Retrofit retrofit = new Retrofit.Builder().baseUrl("http://"+ finalIp +":5544/api/Ventas/")
                        .addConverterFactory(GsonConverterFactory.create())
                        .build();

                IVentas jsonPlaceHolderApi = retrofit.create(IVentas.class);

                Call<List<TipoIdentificacion>> GetTipoIdentificaciones = jsonPlaceHolderApi.GetTipoIdentificaciones();

                GetTipoIdentificaciones.enqueue(new Callback<List<TipoIdentificacion>>() {
                    @Override
                    public void onResponse(Call<List<TipoIdentificacion>> call, Response<List<TipoIdentificacion>> response) {
                        if (!response.isSuccessful()) {

                            System.out.println("notSucessful");
                            return;
                        }
                        ArrayList<String> identificaciones = new ArrayList<>();
                        String descripcion = "";
                        tiposIdentificaciones = response.body();
                        for (TipoIdentificacion cara : response.body()) {
                            identificaciones.add(cara.Descripcion);
                            if(tipoIdentificacion == cara.TipoIdentificacionId){
                                descripcion = cara.Descripcion;
                            }
                            System.out.println(cara.Descripcion);
                        }
                        ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(getActivity(), android.R.layout.simple_spinner_item, identificaciones);
                        arrayAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
                        s2.setAdapter(arrayAdapter);
                        s2.setSelection(arrayAdapter.getPosition(descripcion));
                        if(factura.vecesImpresa <= finalVecesImpresa){
                            imprimir.setEnabled(true);
                        }



                        Retrofit retrofit = new Retrofit.Builder().baseUrl("http://"+ finalIp +":5544/api/Ventas/")
                                .addConverterFactory(GsonConverterFactory.create())
                                .build();

                        IVentas jsonPlaceHolderApi = retrofit.create(IVentas.class);

                        Call<List<FormasDePagos>> GetFormasPago = jsonPlaceHolderApi.GetFormasPago();

                        GetFormasPago.enqueue(new Callback<List<FormasDePagos>>() {
                            @Override
                            public void onResponse(Call<List<FormasDePagos>> call, Response<List<FormasDePagos>> response) {
                                if (!response.isSuccessful()) {

                                    System.out.println("notSucessful");
                                    return;
                                }
                                ArrayList<String> formasS = new ArrayList<>();
                                String descripcion = "";
                                formas = response.body();
                                for (FormasDePagos forma : response.body()) {
                                    formasS.add(forma.Descripcion);
                                    if(factura.codigoFormaPago == forma.Id){
                                        descripcion = forma.Descripcion;
                                    }
                                    System.out.println(forma.Descripcion);
                                }
                                ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(getActivity(), android.R.layout.simple_spinner_item, formasS);
                                arrayAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
                                s3.setAdapter(arrayAdapter);
                                s3.setSelection(arrayAdapter.getPosition(descripcion));
                            }

                            @Override
                            public void onFailure(Call<List<FormasDePagos>> call, Throwable t) {

                                System.out.println("notSucessful");
                                System.out.println(t.getMessage());
                            }
                        });





                    }

                    @Override
                    public void onFailure(Call<List<TipoIdentificacion>> call, Throwable t) {

                        System.out.println("notSucessful");
                        System.out.println(t.getMessage());
                    }
                });


            }

            @Override
            public void onFailure(Call<Factura> call, Throwable t) {

                Toast.makeText(getContext(),"NO hay conexion! Cambiar IP",Toast.LENGTH_LONG).show();
                System.out.println("Failure");
                System.out.println(t.getCause());
                System.out.println(t.getStackTrace());
                System.out.println(t.getMessage());
            }
        });

    }


    private String getFacturaInfoBasica(Factura _factura) {
        StringBuilder informacionVenta = new StringBuilder();

        if (_factura.Consecutivo == 0)
        {

            informacionVenta.append("Orden de despacho\n\r");
        }
        else
        {
            informacionVenta.append("Sistema P.O.S No: " + _factura.DescripcionResolucion + "-" + _factura.Consecutivo + "\n\r");
        }
        informacionVenta.append("------------------------------------------------" + "\n\r");

        if (_factura.Venta.COD_FOR_PAG != 4)
        {
            informacionVenta.append("Vendido a : " + _factura.Tercero.Nombre + "\n\r");
            informacionVenta.append("Nit/C.C. : " + _factura.Tercero.identificacion + "\n\r");
        }
        else
        {
            informacionVenta.append("Vendido a : CONSUMIDOR FINAL\n\r");
            informacionVenta.append("Nit/C.C. : 222222222222\n\r");
        }
        informacionVenta.append("Fecha : " + _factura.fecha + "\n\r");
        informacionVenta.append("------------------------------------------------" + "\n\r");

        informacionVenta.append("Producto  Cant.     Precio    Total    " + "\n\r");
        informacionVenta.append(getLienaTarifas(_factura.Manguera.DESCRIPCION.trim(), _factura.Venta.CANTIDAD+"", _factura.Venta.PRECIO_UNI+"", _factura.Venta.TOTAL+"") + "\n\r");
        informacionVenta.append("------------------------------------------------" + "\n\r");
         informacionVenta.append("TOTAL : " + _factura.Venta.TOTAL + "\n\r");
        informacionVenta.append("------------------------------------------------" + "\n\r");



        return informacionVenta.toString();
    }
}