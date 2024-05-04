import { React, useState, useEffect } from 'react'
import EstacionesDropdown from '../estaciones/EstacionesDropdown'
import GetEstaciones from '../../services/GetEstaciones'
import { CRow } from '@coreui/react'
import { Link, useNavigate } from 'react-router-dom'

const Dashboard = () => {
  let navigate = useNavigate()
  const [estaciones, setEstaciones] = useState([])
  const [estacionSeleccionada, setEstacionSeleccionada] = useState({})

  const fetchEstaciones = async () => {
    let response = await GetEstaciones()
    if (response == 'fail') {
      navigate('/Login', { replace: true })
    } else {
      setEstaciones(response)
      setEstacionSeleccionada(response[0].guid)
      if (!localStorage.getItem('estacion')) {
        localStorage.setItem('estacion', response[0].guid)
        localStorage.setItem('estacionNombre', response[0].nombre)
        localStorage.setItem('estacionNit', response[0].nit)
      }
    }
  }

  useEffect(() => {
    fetchEstaciones()
  }, [])

  return (
    <>
      <CRow>
        {estaciones.map((estacion) => (
          <EstacionesDropdown
            key={estacion.guid}
            estacion={estacion}
            setEstacionSeleccionada={setEstacionSeleccionada}
            estacionSeleccionada={estacionSeleccionada}
          />
        ))}
      </CRow>
    </>
  )
}

export default Dashboard
