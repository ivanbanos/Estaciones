package com.example.facturadorsiges;

import android.database.Cursor;
import android.os.Bundle;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.Switch;
import android.widget.TextView;
import android.widget.Toast;

import androidx.annotation.NonNull;
import androidx.appcompat.app.ActionBar;
import androidx.appcompat.app.AppCompatActivity;
import androidx.navigation.fragment.NavHostFragment;
import androidx.preference.PreferenceFragmentCompat;

import java.io.IOException;
import java.util.ArrayList;
import java.util.List;

import ApiConnectionClasses.IVentas;
import Modelo.Cara;
import Modelo.Factura;
import Modelo.TipoIdentificacion;
import okhttp3.ResponseBody;
import retrofit2.Call;
import retrofit2.Callback;
import retrofit2.Response;
import retrofit2.Retrofit;
import retrofit2.converter.gson.GsonConverterFactory;
import sqlite.DBManager;

public class SettingsActivity extends AppCompatActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.settings_activity);
        getSupportFragmentManager()
                .beginTransaction()
                .replace(R.id.settings, new SettingsFragment())
                .commit();
        ActionBar actionBar = getSupportActionBar();
        if (actionBar != null) {
            actionBar.setDisplayHomeAsUpEnabled(true);
        }
    }

    public static class SettingsFragment extends PreferenceFragmentCompat {

        private DBManager dbManager;

        @Override
        public void onCreatePreferences(Bundle savedInstanceState, String rootKey) {
            setPreferencesFromResource(R.xml.root_preferences, rootKey);
        }

        @Override
        public void onViewCreated(@NonNull View view, Bundle savedInstanceState) {
            super.onViewCreated(view, savedInstanceState);
            dbManager = new DBManager(getContext());
            dbManager.open();
            int vecesImpresa = 0;
            String ip = null;
            int id = 0;
            boolean impresionLocal = false;
            final EditText vecesImpresaText =  (EditText) view.findViewById(R.id.editTextTextPersonName);
            final EditText ipText =  (EditText) view.findViewById(R.id.editTextTextPersonName2);
            final Switch impresionPDA =  (Switch) view.findViewById(R.id.impresionPDA);
            Cursor cursor = dbManager.fetch();
            if (cursor.moveToFirst()) {
                do {
                    id = Integer.parseInt(cursor.getString(0));
                    vecesImpresa = Integer.parseInt(cursor.getString(1));
                    ip = cursor.getString(2);
                    impresionLocal = cursor.getInt(3) == 1;
                } while (cursor.moveToNext());
            }
            if(ip==null){
                dbManager.insert(2, "192.168.1.0", 0,0,0,0);
                Cursor cursor2 = dbManager.fetch();
                if (cursor2.moveToFirst()) {
                    do {
                        id = Integer.parseInt(cursor.getString(0));
                    } while (cursor.moveToNext());
                }
            } else{
                vecesImpresaText.setText(vecesImpresa+"");
                ipText.setText(ip);
                impresionPDA.setChecked(impresionLocal);
            }
            final int finalId = id;
            view.findViewById(R.id.button2).setOnClickListener(new View.OnClickListener() {
                @Override
                public void onClick(View view) {
                    int isImpresionLocal = impresionPDA.isChecked()?1:0;
                    dbManager.update(finalId, Integer.parseInt(vecesImpresaText.getText().toString()),ipText.getText().toString(), isImpresionLocal,0,0,0 );
                }
            });
        }
    }


}