package com.example.facturadorsiges;

import androidx.appcompat.app.AppCompatActivity;

import android.content.Intent;
import android.database.Cursor;
import android.os.Bundle;
import android.view.View;
import android.widget.EditText;
import android.widget.Switch;
import android.widget.Toast;

import sqlite.DBManager;

public class MainActivity2 extends AppCompatActivity {

    private DBManager dbManager;
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main2);

        dbManager = new DBManager(this);
        dbManager.open();
        int vecesImpresa = 0;
        String ip = null;
        int id = 0;
        boolean impresionLocal = false;
        boolean convertirF = false;
        boolean convertirO = false;
        boolean generarFE = false;
        final EditText vecesImpresaText =  (EditText) findViewById(R.id.editTextTextPersonName);
        final EditText ipText =  (EditText) findViewById(R.id.editTextTextPersonName2);
        final Switch impresionPDA =  (Switch) findViewById(R.id.impresionPDA);
        final Switch convertirFactura =  (Switch) findViewById(R.id.convertirFactura);
        final Switch convertirOrden =  (Switch) findViewById(R.id.convertirOrden);
        final Switch generarFacturaElectronica =  (Switch) findViewById(R.id.generarFacturaElectronica);
        Cursor cursor = dbManager.fetch();
        if (cursor.moveToFirst()) {
            do {
                id = Integer.parseInt(cursor.getString(0));
                vecesImpresa = Integer.parseInt(cursor.getString(1));
                ip = cursor.getString(2);
                impresionLocal = cursor.getInt(3) == 1;
                convertirF = cursor.getInt(4) == 1;
                convertirO = cursor.getInt(5) == 1;
                generarFE = cursor.getInt(6) == 1;
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
            convertirFactura.setChecked(convertirF);
            convertirOrden.setChecked(convertirO);
            generarFacturaElectronica.setChecked(generarFE);
        }
        final int finalId = id;
        findViewById(R.id.button2).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                int isImpresionLocal = impresionPDA.isChecked()?1:0;
                int isConvertirF = convertirFactura.isChecked()?1:0;
                int isConvertirOrden = convertirOrden.isChecked()?1:0;
                int isGeneraFacturaElectronica = generarFacturaElectronica.isChecked()?1:0;
                dbManager.update(finalId,
                        Integer.parseInt(vecesImpresaText.getText().toString()),
                        ipText.getText().toString(),
                        isImpresionLocal,
                        isConvertirF,
                        isConvertirOrden,
                        isGeneraFacturaElectronica);

                Toast.makeText(getApplicationContext(),"Configuración guardada",Toast.LENGTH_LONG).show();
            }
        });

        findViewById(R.id.button).setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View view) {
                Intent intent = new Intent(getApplicationContext(), MainActivity.class);
                startActivity(intent);
            }
        });
    }


}