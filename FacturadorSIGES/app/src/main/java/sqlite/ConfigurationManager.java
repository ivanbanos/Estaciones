package sqlite;

import android.content.Context;
import android.database.Cursor;

public class ConfigurationManager {
    public String ip;
    public int vecesImpresa;
    public int id;
    public boolean impresionPDA;
    public boolean convertirFactura;
    public boolean convertirOrden;
    public boolean generarFacturaElectronica;
    private DBManager dbManager;



    public ConfigurationManager(Context c){

        dbManager = new DBManager(c);
        dbManager.open();
        Cursor cursor = dbManager.fetch();
        if (cursor.moveToFirst()) {
            do {
                id = Integer.parseInt(cursor.getString(0));
                vecesImpresa = Integer.parseInt(cursor.getString(1));
                ip = cursor.getString(2);
                impresionPDA = cursor.getInt(3)==1;
                convertirFactura = cursor.getInt(4)==1;
                convertirOrden = cursor.getInt(5)==1;
                generarFacturaElectronica = cursor.getInt(6)==1;
            } while (cursor.moveToNext());
        }
        if(ip==null){
            dbManager.insert(2, "192.168.1.0", 0, 0, 0, 0);
            vecesImpresa = 2;
            ip = "192.168.1.0";
            impresionPDA = false;
            convertirFactura = false;
            convertirOrden = false;
            generarFacturaElectronica = false;
            Cursor cursor2 = dbManager.fetch();
            if (cursor2.moveToFirst()) {
                do {
                    id = Integer.parseInt(cursor.getString(0));
                } while (cursor.moveToNext());
            }
        }
    }


}
