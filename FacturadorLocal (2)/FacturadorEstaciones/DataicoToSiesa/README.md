# DataicoToSiesa - Programa de Envío de Facturas Canastilla a Siesa

## Descripción
Programa standalone para procesar facturas desde un archivo CSV, obtener los detalles de Dataico y enviarlas al sistema contable Siesa.

## Funcionalidades
- ✅ Lee facturas desde archivo CSV (COMPARATIVO SIGES DIAN SIESA 2025.csv)
- ✅ Consulta detalles de cada factura en la API de Dataico
- ✅ Envía facturas a Siesa mediante API
- ✅ Marca facturas como "SUBIDA" en el CSV después de envío exitoso
- ✅ Permite reanudar el proceso (salta facturas ya subidas)
- ✅ Genera logs detallados de cada operación
- ✅ Maneja facturas con y sin IVA de forma diferenciada

## Requisitos
- .NET Framework 4.7.2
- Acceso a internet (APIs de Dataico y Siesa)
- Archivo CSV en la ruta configurada
- Configuración de credenciales en App.config

## Configuración

### Archivo CSV
Por defecto, el programa busca el archivo en:
```
C:\Users\ivana\Documents\GitHub\Estaciones\FacturadorLocal (2)\FacturadorEstaciones\COMPARATIVO SIGES DIAN SIESA 2025.csv
```

Si necesitas cambiar la ruta, edita la constante `CSV_FILE_PATH` en Program.cs.

### Estructura del CSV
El archivo debe tener las siguientes columnas:
1. FECHA FACT.
2. FACTURA DATAICO (número de factura)
3. ORDEN DATAICO
4. ORDEN SIGES
5. VALOR SIGES
6. ESTADO (NO SUBIDA / SUBIDA)
7. MOTIVO
8. VERIFICACION (debe ser "CANASTILLA")

### App.config
Las credenciales de Dataico y Siesa ya están configuradas. Si necesitas cambiarlas:

**Dataico:**
```xml
<add key="dataico_token" value="7be2a493120e4c1cee67dbd8f424a39d"/>
```

**Siesa:**
```xml
<add key="urlsiesa" value="https://servicios.siesacloud.com"/>
<add key="key" value="cea9b2d24cd71a9b317caa64a4c2c10f"/>
<add key="token" value="eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."/>
```

## Uso

### Compilación
1. Abrir la solución FacturadorEstaciones.sln en Visual Studio
2. Buscar el proyecto DataicoToSiesa
3. Compilar en modo Release
4. El ejecutable estará en: `DataicoToSiesa\bin\Release\DataicoToSiesa.exe`

### Ejecución
1. Asegúrate de que el archivo CSV existe en la ruta configurada
2. Ejecuta `DataicoToSiesa.exe`
3. El programa mostrará el progreso en consola:
   ```
   [1] Procesando factura FEEJ42865...
   ✓ Factura FEEJ42865 enviada exitosamente a Siesa
   Progreso: 10 procesadas | 9 exitosas | 1 fallidas | 100 ya subidas
   ```
4. Al finalizar, verás un resumen completo:
   ```
   === RESUMEN FINAL ===
   Total procesadas: 150
   Exitosas: 145
   Fallidas: 5
   Ya subidas (saltadas): 200
   ```

### Reanudar Proceso Interrumpido
Si el programa se interrumpe por cualquier motivo:
1. Simplemente ejecútalo de nuevo
2. Saltará automáticamente todas las facturas con ESTADO="SUBIDA"
3. Continuará desde donde se quedó

## Logs
Los logs se guardan en la carpeta `logs/` con formato:
```
logs/DataicoToSiesa_2025-01-15.log
```

Contienen información detallada de:
- Facturas procesadas
- Respuestas de las APIs
- Errores encontrados
- Tiempos de ejecución

## Comportamiento del Programa

### Procesamiento de Facturas
1. Lee todas las líneas del CSV
2. Para cada factura con ESTADO != "SUBIDA" y VERIFICACION = "CANASTILLA":
   - Consulta los detalles en Dataico (con 3 reintentos)
   - Crea el objeto de factura con items, totales e IVA
   - Determina configuración (con/sin IVA)
   - Envía a Siesa
   - Actualiza el CSV inmediatamente

### Manejo de Errores
- **Error de Dataico**: Marca como "NO SUBIDA" con motivo "Error: No se encontró en Dataico"
- **Error de Siesa**: Marca como "NO SUBIDA" con motivo "Error al enviar a Siesa"
- **Factura ya existe en Siesa**: Se considera exitosa y marca como "SUBIDA"
- **Error general**: Captura excepción, registra en log y continúa con siguiente factura

### Actualización del CSV
Después de cada factura (exitosa o no):
- Actualiza la columna ESTADO
- Actualiza la columna MOTIVO con timestamp
- Guarda el archivo CSV inmediatamente
- Esto garantiza trazabilidad incluso si el programa se interrumpe

## Notas Importantes

⚠️ **IMPORTANTE**: El programa modifica el archivo CSV original. Se recomienda hacer backup antes de ejecutar.

⚠️ **Conexión a Internet**: Requiere conexión estable a internet para las APIs.

⚠️ **Tiempo de Ejecución**: Con 10,000+ facturas, puede tomar varias horas (hay pausa de 500ms entre facturas).

⚠️ **Progreso cada 10 facturas**: Se muestra resumen de progreso cada 10 facturas procesadas.

## Troubleshooting

### "Archivo CSV no encontrado"
- Verifica la ruta en `CSV_FILE_PATH`
- Asegúrate de que el archivo existe

### "No se pudo obtener información de Dataico"
- Verifica conectividad a internet
- Revisa el token de Dataico en App.config
- Verifica que el número de factura sea correcto

### "Error al enviar a Siesa"
- Verifica credenciales de Siesa (key y token)
- Revisa logs para detalles específicos del error
- Verifica que el ID de documento sea correcto (206116)

### Facturas con errores repetidos
- Revisa los logs para identificar el patrón
- Verifica datos en Dataico para esas facturas específicas
- Pueden requerir corrección manual

## Soporte
Para problemas o preguntas, consulta los logs en `logs/DataicoToSiesa_[fecha].log`
