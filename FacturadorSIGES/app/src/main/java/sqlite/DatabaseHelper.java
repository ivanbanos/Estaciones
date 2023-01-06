package sqlite;

import android.content.Context;
import android.database.sqlite.SQLiteDatabase;
import android.database.sqlite.SQLiteOpenHelper;

public class DatabaseHelper extends SQLiteOpenHelper {

    // Table Name
    public static final String TABLE_NAME = "CONFIGURACION";

    // Table columns
    public static final String _ID = "_id";
    public static final String SUBJECT = "vecesImpresa";
    public static final String DESC = "ip";
    public static final String IMPRESIONPDA = "impresionPDA";
    public static final String CONVERTIRFACTURA = "convertirFactura";
    public static final String CONVERTIRORDEN = "convertirOrden";
    public static final String GENERARFACTURAELECTRONICA = "generarFacturaElectronica";
    // Database Information
    static final String DB_NAME = "CONFIGVALUES.DB";

    // database version
    static final int DB_VERSION = 3;

    // Creating table query
    private static final String CREATE_TABLE = "create table " + TABLE_NAME + "(" + _ID
            + " INTEGER PRIMARY KEY AUTOINCREMENT, " + SUBJECT + " INTEGER  NOT NULL, " + DESC + " TEXT, "
            + IMPRESIONPDA + " INTEGER, "
            + CONVERTIRFACTURA + " INTEGER, "
            + CONVERTIRORDEN + " INTEGER, "
            + GENERARFACTURAELECTRONICA + " INTEGER) " ;

    public DatabaseHelper(Context context) {
        super(context, DB_NAME, null, DB_VERSION);
    }

    @Override
    public void onCreate(SQLiteDatabase db) {
        db.execSQL(CREATE_TABLE);
    }

    @Override
    public void onUpgrade(SQLiteDatabase db, int oldVersion, int newVersion) {
        db.execSQL("DROP TABLE IF EXISTS " + TABLE_NAME);
        onCreate(db);
    }
}