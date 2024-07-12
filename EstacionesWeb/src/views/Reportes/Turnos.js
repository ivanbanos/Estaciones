import { React, useState, useEffect, useRef } from 'react'
import FiltrarInfoTurnos from '../../services/FiltrarInfoTurnos'
import { Link, useNavigate } from 'react-router-dom'
import {
  CButton,
  CRow,
  CCol,
  CTable,
  CTableBody,
  CTableHead,
  CTableHeaderCell,
  CTableRow,
  CFormInput,
  CModal,
  CModalBody,
  CModalFooter,
  CModalHeader,
  CModalTitle,
  CFormSelect,
  CTableDataCell,
} from '@coreui/react'
import Toast from '../toast/Toast'
var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

let cop = Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
})
const Turnos = () => {
  let navigate = useNavigate()
  const [fechaInicial, setFechaInicial] = useState([])
  const [fechaFinal, setFechaFinal] = useState([])
  const [turnos, setTurnos] = useState([])
  const [turnosDetalle, setTurnosDetalle] = useState([])

  const toastRef = useRef()
  const FiltrarInfoTurnosHandler = async () => {
    let response = await FiltrarInfoTurnos(fechaInicial, fechaFinal)
    if (response == 'fail') {
      navigate('/Login', { replace: true })
    } else {
      setTurnos([...new Set(response.map((item) => item.turno))])
      setTurnosDetalle(response)
    }
  }

  const handlefechaInicialSelectedChange = (event) => {
    const selectedDate = event.target.value

    if (isValidDate(selectedDate)) {
      setFechaInicial(selectedDate)
    }
  }
  const handlefechaFinalSelectedChange = (event) => {
    const selectedDate = event.target.value

    if (isValidDate(selectedDate)) {
      setFechaFinal(selectedDate)
    }
  }

  // Función para verificar si la fecha es válida
  const isValidDate = (date) => {
    const pattern = /^\d{4}-\d{2}-\d{2}$/
    return pattern.test(date)
  }
  const descargarReporte = async (event) => {
    let estacionNombre = localStorage.getItem('estacionNombre')
    let estacionNit = localStorage.getItem('estacionNit')

    var elements = [
      {
        stack: [estacionNombre, { text: 'Nit ' + estacionNit, style: 'subheader' }],
        style: 'header',
      },
      {
        text: 'Reporte de Turnos',
        style: 'header',
      },
      {
        text: 'Fecha Inicial ' + fechaInicial + ' Fecha Final ' + fechaFinal,
      },
    ]
    for (const turno of turnos) {
      let turnoTabla = [
        ['Sur', 'Man', 'Combustible', 'Precio', 'Apertura', 'Cierre', 'Diferencia', 'total'],
      ]
      let turnoTablaElements = turnosDetalle
        .filter((x) => x.turno == turno)
        .map((detalle) => [
          detalle.surtidor,
          detalle.manguera,
          detalle.combustible,
          { text: cop.format(detalle.precio), style: 'tableRight' },
          detalle.apertura,
          detalle.cierre,
          detalle.diferencia.toFixed(2),
          { text: cop.format(detalle.total.toFixed(2)), style: 'tableRight' },
        ])
      turnoTabla = turnoTabla.concat(turnoTablaElements)

      elements = elements.concat({
        stack: ['Turno ' + turno],
      })
      elements = elements.concat({
        style: 'tableExample',
        table: {
          headerRows: 1,
          body: turnoTabla,
        },
      })
    }
    var dd = {
      watermark: {
        text: 'SIGES Soluciones',
        color: 'blue',
        opacity: 0.1,
        bold: true,
        italics: false,
      },
      footer: function (currentPage, pageCount) {
        return (
          'SIGES Soluciones Reportes - ' +
          estacionNombre +
          ' - ' +
          currentPage.toString() +
          ' of ' +
          pageCount
        )
      },

      content: elements,

      styles: {
        header: {
          fontSize: 18,
          bold: true,
          alignment: 'center',
        },
        subheader: {
          fontSize: 14,
        },
        superMargin: {
          fontSize: 15,
        },

        tableRight: {
          alignment: 'right',
        },
        tableHeader: {
          bold: true,
          fontSize: 13,
          color: 'black',
        },
      },
    }

    pdfMake
      .createPdf(dd)
      .download(
        'ReporteTurnos - ' + estacionNombre + ' - ' + fechaInicial + '-' + fechaFinal + '.pdf',
      )
  }
  return (
    <>
      <Toast ref={toastRef}></Toast>
      <h1>Reporte de Turnos</h1>

      <CRow>
        <CCol xs={2}>
          <label>Fecha Inicial</label>
        </CCol>
        <CCol xs={4}>
          <input
            type="date"
            className="form-control modal-tercero-input"
            name="fechaTurno"
            value={fechaInicial}
            onChange={handlefechaInicialSelectedChange}
          ></input>
        </CCol>
        <CCol xs={2}>
          <label>Fecha Final</label>
        </CCol>
        <CCol xs={4}>
          <input
            type="date"
            className="form-control modal-tercero-input"
            name="fechaTurno"
            value={fechaFinal}
            onChange={handlefechaFinalSelectedChange}
          ></input>
        </CCol>
        <CButton style={{ margin: '2pt' }} onClick={FiltrarInfoTurnosHandler}>
          Filtrar
        </CButton>
      </CRow>

      <CButton style={{ margin: '2pt' }} onClick={descargarReporte}>
        Descargar
      </CButton>
      <h2>Reporte por turno</h2>
      {turnos.map((turno) => (
        <div key={turno}>
          <h3>Turno {turno}</h3>
          <CTable>
            <CTableHead>
              <CTableRow>
                <CTableHeaderCell scope="col">Surtidor</CTableHeaderCell>
                <CTableHeaderCell scope="col">Manguera</CTableHeaderCell>
                <CTableHeaderCell scope="col">Combustible</CTableHeaderCell>
                <CTableHeaderCell scope="col">Precio</CTableHeaderCell>
                <CTableHeaderCell scope="col">Apertura</CTableHeaderCell>
                <CTableHeaderCell scope="col">Cierre</CTableHeaderCell>
                <CTableHeaderCell scope="col">Diferencia</CTableHeaderCell>
                <CTableHeaderCell scope="col">Total</CTableHeaderCell>
              </CTableRow>
            </CTableHead>
            <CTableBody>
              {turnosDetalle
                .filter((x) => x.turno == turno)
                .map((detalle) => (
                  <CTableRow key={detalle.surtidor + detalle.manguera + turno}>
                    <CTableDataCell>{detalle.surtidor}</CTableDataCell>
                    <CTableDataCell>{detalle.manguera}</CTableDataCell>
                    <CTableDataCell>{detalle.combustible}</CTableDataCell>
                    <CTableDataCell>{cop.format(detalle.precio)}</CTableDataCell>
                    <CTableDataCell>{detalle.apertura}</CTableDataCell>
                    <CTableDataCell>{detalle.cierre}</CTableDataCell>
                    <CTableDataCell>{detalle.diferencia.toFixed(2)}</CTableDataCell>
                    <CTableDataCell>{cop.format(detalle.total.toFixed(2))}</CTableDataCell>
                  </CTableRow>
                ))}
            </CTableBody>
          </CTable>
        </div>
      ))}

      <CButton style={{ margin: '2pt' }} onClick={descargarReporte}>
        Descargar
      </CButton>
    </>
  )
}

export default Turnos
