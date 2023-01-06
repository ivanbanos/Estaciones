package sqlite;

import android.content.ContentValues;
import android.content.Context;
import android.database.Cursor;
import android.database.SQLException;
import android.database.sqlite.SQLiteDatabase;

public class DBManager {

    private DatabaseHelper dbHelper;

    private Context context;

    private SQLiteDatabase database;

    public DBManager(Context c) {
        context = c;
    }

    public DBManager open() throws SQLException {
        dbHelper = new DatabaseHelper(context);
        database = dbHelper.getWritableDatabase();
        return this;
    }

    public void close() {
        dbHelper.close();
    }

    public void insert(int name, String desc, int impresionPDA, int convertirFactura, int convertirOrden, int generarFacturaElectronica) {
        ContentValues contentValue = new ContentValues();
        contentValue.put(DatabaseHelper.SUBJECT, name);
        contentValue.put(DatabaseHelper.DESC, desc);
        contentValue.put(DatabaseHelper.IMPRESIONPDA, impresionPDA);
        contentValue.put(DatabaseHelper.CONVERTIRFACTURA, convertirFactura);
        contentValue.put(DatabaseHelper.CONVERTIRORDEN, convertirOrden);
        contentValue.put(DatabaseHelper.GENERARFACTURAELECTRONICA, generarFacturaElectronica);
        database.insert(DatabaseHelper.TABLE_NAME, null, contentValue);
    }

    public Cursor fetch() {
        String[] columns = new String[] { DatabaseHelper._ID, DatabaseHelper.SUBJECT, DatabaseHelper.DESC, DatabaseHelper.IMPRESIONPDA, DatabaseHelper.CONVERTIRFACTURA, DatabaseHelper.CONVERTIRORDEN, DatabaseHelper.GENERARFACTURAELECTRONICA };
        Cursor cursor = database.query(DatabaseHelper.TABLE_NAME, columns, null, null, null, null, null);
        if (cursor != null) {
            cursor.moveToFirst();
        }
        return cursor;
    }

    public int update(long _id, int name, String desc, int impresionPDA, int convertirFactura, int convertirOrden, int generarFacturaElectronica) {
        ContentValues contentValues = new ContentValues();
        contentValues.put(DatabaseHelper.SUBJECT, name);
        contentValues.put(DatabaseHelper.DESC, desc);
        contentValues.put(DatabaseHelper.IMPRESIONPDA, impresionPDA);
        contentValues.put(DatabaseHelper.CONVERTIRFACTURA, convertirFactura);
        contentValues.put(DatabaseHelper.CONVERTIRORDEN, convertirOrden);
        contentValues.put(DatabaseHelper.GENERARFACTURAELECTRONICA, generarFacturaElectronica);
        int i = database.update(DatabaseHelper.TABLE_NAME, contentValues, DatabaseHelper._ID + " = " + _id, null);
        return i;
    }

    public void delete(long _id) {
        database.delete(DatabaseHelper.TABLE_NAME, DatabaseHelper._ID + "=" + _id, null);
    }

}