const GetCuposPorClientes = async () => {
  try {
    const token = localStorage.getItem('token')
    const estacion = localStorage.getItem('estacion')
    if (!token) {
      return 'fail'
    }
    const response = await fetch(window.SERVER_URL + '/api/CuposInfo/Clientes/' + estacion, {
      method: 'GET',
      mode: 'cors',
      headers: {
        'Access-Control-Allow-Origin': '*',
        accept: 'text/plain',
        Authorization: 'Bearer ' + token,
        'sec-fetch-mode': 'cors',
        'Access-Control-Allow-Headers': 'Content-Type',
        'Access-Control-Allow-Methods': 'OPTIONS,POST,GET',
      },
    })
    if (response.status == 200) {
      let cupos = await response.json()
      return cupos
    }
    if (response.status == 403) {
      return 'fail'
    }
    return 'fail'
  } catch (error) {
    return 'fail'
  }
}

export default GetCuposPorClientes
