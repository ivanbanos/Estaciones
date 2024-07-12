const FiltrarFacturas = async (fechaInicial, fechaFinal, identificacion) => {
  try {
    const token = localStorage.getItem('token')
    const estacion = localStorage.getItem('estacion')
    let body = {
      fechaInicial,
      fechaFinal,
      identificacion,
      estacion,
    }
    if (!token) {
      return 'fail'
    }
    const response = await fetch(window.SERVER_URL + '/api/Factura/GetFactura', {
      method: 'POST',
      mode: 'cors',
      body: JSON.stringify(body),
      headers: {
        'Access-Control-Allow-Origin': '*',
        accept: 'text/plain',
        'Content-Type': 'application/json',
        Authorization: 'Bearer ' + token,
        'sec-fetch-mode': 'cors',
        'Access-Control-Allow-Headers': 'Content-Type',
        'Access-Control-Allow-Methods': 'OPTIONS,POST,GET',
      },
    })
    if (response.status == 200) {
      let reporte = await response.json()
      return reporte
    }
    if (response.status == 403) {
      return 'fail'
    }
    return 'fail'
  } catch (error) {
    return 'fail'
  }
}

export default FiltrarFacturas
