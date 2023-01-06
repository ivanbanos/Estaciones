package com.example.facturadorsiges;

import android.app.Activity;
import android.content.Context;
import android.content.Intent;
import android.database.Cursor;
import android.os.Bundle;

import com.google.android.material.floatingactionbutton.FloatingActionButton;
import com.google.android.material.snackbar.Snackbar;

import androidx.appcompat.app.AppCompatActivity;
import androidx.appcompat.widget.Toolbar;

import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.TimeUnit;

import Adapters.CarasAdapter;
import ApiConnectionClasses.IVentas;
import Modelo.Cara;
import Modelo.Isla;
import okhttp3.OkHttpClient;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import sqlite.DBManager;

public class CerrarTurno extends AppCompatActivity {

    List<Isla> islas;
    private DBManager dbManager;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_cerrar_turno);
        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);

        dbManager = new DBManager(this);
        dbManager.open();
        Cursor cursor = dbManager.fetch();
        String ip = null;
        int id = 0;
        if (cursor.moveToFirst()) {
            do {
                id = Integer.parseInt(cursor.getString(0));
                ip = cursor.getString(2);
            } while (cursor.moveToNext());
        }
        if(ip==null){
            dbManager.insert(2, "192.168.1.0", 0,0,0,0);
            ip = "192.168.1.0";
            Cursor cursor2 = dbManager.fetch();
            if (cursor2.moveToFirst()) {
                do {
                    id = Integer.parseInt(cursor.getString(0));
                } while (cursor.moveToNext());
            }
        }
        System.out.println("Hola");
        final Spinner s = (Spinner) findViewById(R.id.spinner4);
        Retrofit retrofit = new Retrofit.Builder().baseUrl("http://"+ip+":5544/api/Ventas/")
                .addConverterFactory(GsonConverterFactory.create())
                .build();
        final ListView carasView = (ListView) findViewById(R.id.gridCaras);
        final Button button = findViewById(R.id.button3);
        final TextView codigo = findViewById(R.id.editTextTextPersonName3);
        button.setEnabled(false);
        final Context thisContext = this;
        final OkHttpClient okHttpClient = new OkHttpClient.Builder()
                .connectTimeout(20, TimeUnit.SECONDS)
                .writeTimeout(20, TimeUnit.SECONDS)
                .readTimeout(120, TimeUnit.SECONDS)
                .build();
        final IVentas jsonPlaceHolderApi = retrofit.create(IVentas.class);
        button.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                String cara = (String)s.getSelectedItem();
                int idIsla = 0;
                for(Isla islaS : islas){
                    if(islaS.descripcion ==cara ){
                        idIsla = islaS.idIsla;
                    }
                }
                String codigoText = codigo.getText().toString();
                String trama = "000000CER0"+idIsla+codigoText;

                Call<String> listCall = jsonPlaceHolderApi.Trama(trama);

                listCall.enqueue(new Callback<String>() {
                    @Override
                    public void onResponse(Call<String> call, Response<String> response) {
                        if (!response.isSuccessful()) {

                            System.out.println("notSucessful"+response.message());
                            return;
                        }

                        System.out.println("Success");
                        String resultado = response.body();
                        String traduccion = "";
                        if(resultado.contains("000000CERX")){
                            traduccion = "Turno cerrado";
                        } else{
                            traduccion = "Error cerrando turno";
                        }
                        Toast.makeText(thisContext,resultado+": "+traduccion,Toast.LENGTH_LONG).show();
                        Intent intent = new Intent(getApplicationContext(), Home.class);
                        startActivity(intent);
                    }

                    @Override
                    public void onFailure(Call<String> call, Throwable t) {
                        try{
                            Toast.makeText(thisContext,t.getMessage(),Toast.LENGTH_LONG).show();
                            System.out.println("notSucessful");
                            System.out.println(t.getMessage());}catch(Exception e){}
                    }
                });
            }
        });


        Call<List<Isla>> listCall = jsonPlaceHolderApi.getIslas();
        final CerrarTurno thisActivity = this;
        listCall.enqueue(new Callback<List<Isla>>() {
            @Override
            public void onResponse(Call<List<Isla>> call, Response<List<Isla>> response) {
                if (!response.isSuccessful()) {

                    System.out.println("notSucessful");
                    return;
                }

                System.out.println("Success");
                islas = response.body();
                final ArrayList<String> carasL = new ArrayList<>();
                System.out.println(islas.size());
                for (Isla isla : islas) {
                    Cara cara = new Cara();
                    cara.COD_CAR = isla.idIsla;
                    cara.DESCRIPCION = isla.descripcion;
                    carasL.add(cara.DESCRIPCION);
                }
                ArrayAdapter<String> arrayAdapter = new ArrayAdapter<String>(thisContext,                         android.R.layout.simple_spinner_item, carasL);
                arrayAdapter.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
                s.setAdapter(arrayAdapter);
                button.setEnabled(true);

            }

            @Override
            public void onFailure(Call<List<Isla>> call, Throwable t) {
                try{
                    Toast.makeText(thisContext,"NO hay conexion! Cambiar IP",Toast.LENGTH_LONG).show();
                    System.out.println("notSucessful");
                    System.out.println(t.getMessage());}catch(Exception e){}
            }
        });
    }
}