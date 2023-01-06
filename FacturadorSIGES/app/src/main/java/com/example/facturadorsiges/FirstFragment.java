package com.example.facturadorsiges;

import android.app.Activity;
import android.content.Context;
import android.content.SharedPreferences;
import android.database.Cursor;
import android.os.Bundle;
import android.renderscript.RenderScript;
import android.util.Log;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.GridView;
import android.widget.ListView;
import android.widget.Spinner;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.fragment.app.Fragment;
import androidx.navigation.fragment.NavHostFragment;

import com.androidnetworking.AndroidNetworking;
import com.androidnetworking.common.Priority;
import com.androidnetworking.error.ANError;
import com.androidnetworking.interfaces.JSONArrayRequestListener;

import org.json.JSONArray;
import org.json.JSONException;
import org.json.JSONObject;

import java.io.BufferedReader;
import java.io.InputStreamReader;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.ArrayList;
import java.util.List;

import Adapters.CarasAdapter;
import ApiConnectionClasses.IVentas;
import Modelo.Cara;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import sqlite.DBManager;

import static android.content.ContentValues.TAG;

public class FirstFragment extends Fragment {
    IDataCommunication dataCommunication;
    List<Cara> caras;
    private DBManager dbManager;

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

        // Inflate the layout for this fragment
        return inflater.inflate(R.layout.fragment_first, container, false);
    }

    public void onViewCreated(@NonNull View view, Bundle savedInstanceState) {
        super.onViewCreated(view, savedInstanceState);
        dbManager = new DBManager(getContext());
        dbManager.open();
        Cursor cursor = dbManager.fetch();
        int vecesImpresa = 0;
        String ip = null;
        int id = 0;
        if (cursor.moveToFirst()) {
            do {
                id = Integer.parseInt(cursor.getString(0));
                vecesImpresa = Integer.parseInt(cursor.getString(1));
                ip = cursor.getString(2);
            } while (cursor.moveToNext());
        }
        if(ip==null){
            dbManager.insert(2, "192.168.1.0", 0,0,0,0);
            vecesImpresa = 2;
            ip = "192.168.1.0";
            Cursor cursor2 = dbManager.fetch();
            if (cursor2.moveToFirst()) {
                do {
                    id = Integer.parseInt(cursor.getString(0));
                } while (cursor.moveToNext());
            }
        }
        System.out.println("Hola");
        final Spinner s = (Spinner) view.findViewById(R.id.spinner);
        Retrofit retrofit = new Retrofit.Builder().baseUrl("http://"+ip+":5544/api/Ventas/")
                .addConverterFactory(GsonConverterFactory.create())
                .build();
        final Button button = view.findViewById(R.id.button_first);

        IVentas jsonPlaceHolderApi = retrofit.create(IVentas.class);

        Call<List<Cara>> listCall = jsonPlaceHolderApi.getCaras();
        final Context thisContext = this.getContext();
        final ListView carasView = (ListView) view.findViewById(R.id.gridCaras);
        listCall.enqueue(new Callback<List<Cara>>() {
            @Override
            public void onResponse(Call<List<Cara>> call, Response<List<Cara>> response) {
                if (!response.isSuccessful()) {

                    System.out.println("notSucessful");
                    return;
                }

                System.out.println("Success");
                caras = response.body();
                final ArrayList<Cara> carasL = new ArrayList<>();
                System.out.println(caras.size());
                for (Cara cara : caras) {
                    carasL.add(cara);
                    System.out.println(cara.DESCRIPCION);
                }
                CarasAdapter adapter = new CarasAdapter((Activity) thisContext, carasL);

                carasView.setAdapter(adapter);
                carasView.setOnItemClickListener(new AdapterView.OnItemClickListener(){
                    @Override
                    public void onItemClick(AdapterView<?> adapterView, View view, int i, long l) {

                        Cara cara = carasL.get(i);

                        SharedPreferences sharedPref = getActivity().getPreferences(Context.MODE_PRIVATE);
                        SharedPreferences.Editor editor = sharedPref.edit();
                        editor.putInt("Cara", cara.COD_CAR);
                        editor.apply();

                        dataCommunication.setCara(cara);
                        NavHostFragment.findNavController(FirstFragment.this)
                                .navigate(R.id.action_FirstFragment_to_SecondFragment);
                    }
                });

            }

            @Override
            public void onFailure(Call<List<Cara>> call, Throwable t) {
                try{
                Toast.makeText(getContext(),"NO hay conexion! Cambiar IP",Toast.LENGTH_LONG).show();
                System.out.println("notSucessful");
                System.out.println(t.getMessage());}catch(Exception e){}
            }
        });

    }

}