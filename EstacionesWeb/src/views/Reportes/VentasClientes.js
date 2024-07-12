import { React, useState, useEffect, useRef } from 'react'
import Filtrarfacturas from '../../services/Filtrarfacturas'
import FiltrarOrdenes from '../../services/FiltrarOrdenes'
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
} from '@coreui/react'
import Toast from '../toast/Toast'
var pdfMake = require('pdfmake/build/pdfmake.js')
var pdfFonts = require('pdfmake/build/vfs_fonts.js')
pdfMake.vfs = pdfFonts.pdfMake.vfs

let cop = Intl.NumberFormat('en-US', {
  style: 'currency',
  currency: 'USD',
})
const Facturas = (props) => {
  if (props.facturas) {
    return (
      <>
        <CButton style={{ margin: '2pt' }} onClick={props.descargarReporte}>
          Descargar
        </CButton>
        {props.placas.map((placa) => (
          <div key={placa}>
            <label>
              Placa <b>{placa}</b>
            </label>
            <CTable>
              <CTableHead>
                <CTableRow>
                  <CTableHeaderCell scope="col">Tipo</CTableHeaderCell>
                  <CTableHeaderCell scope="col">Consecutivo</CTableHeaderCell>
                  <CTableHeaderCell scope="col">Fecha</CTableHeaderCell>
                  <CTableHeaderCell scope="col">Articulo</CTableHeaderCell>
                  <CTableHeaderCell scope="col">Km Actual</CTableHeaderCell>
                  <CTableHeaderCell scope="col">Cantidad</CTableHeaderCell>
                  <CTableHeaderCell scope="col">Valor</CTableHeaderCell>
                </CTableRow>
              </CTableHead>
              <CTableBody>
                {props.facturas
                  .filter((x) => x.placa == placa)
                  .map((venta) => (
                    <CTableRow key={venta.placa + venta.idVentaLocal}>
                      <CTableHeaderCell>F</CTableHeaderCell>
                      <CTableHeaderCell>{venta.consecutivo}</CTableHeaderCell>
                      <CTableHeaderCell>{venta.fecha}</CTableHeaderCell>
                      <CTableHeaderCell>{venta.combustible}</CTableHeaderCell>
                      <CTableHeaderCell>{venta.kilometraje}</CTableHeaderCell>
                      <CTableHeaderCell>{venta.cantidad}</CTableHeaderCell>
                      <CTableHeaderCell>{cop.format(venta.total)}</CTableHeaderCell>
                    </CTableRow>
                  ))}
                {props.ordenes
                  .filter((x) => x.placa == placa)
                  .map((venta) => (
                    <CTableRow key={venta.placa + venta.idVentaLocal}>
                      <CTableHeaderCell>O</CTableHeaderCell>
                      <CTableHeaderCell>{venta.idVentaLocal}</CTableHeaderCell>
                      <CTableHeaderCell>{venta.fecha}</CTableHeaderCell>
                      <CTableHeaderCell>{venta.combustible}</CTableHeaderCell>
                      <CTableHeaderCell>{venta.kilometraje}</CTableHeaderCell>
                      <CTableHeaderCell>{venta.cantidad}</CTableHeaderCell>
                      <CTableHeaderCell>{cop.format(venta.total)}</CTableHeaderCell>
                    </CTableRow>
                  ))}
              </CTableBody>
            </CTable>
            <label>
              Total{' '}
              <b>
                {[
                  ...new Set(
                    [
                      ...props.ordenes.filter((x) => x.placa == placa).map((x) => x.total),
                      ...props.facturas.filter((x) => x.placa == placa).map((x) => x.total),
                    ].filter((x, i, a) => a.indexOf(x) == i),
                  ),
                ].reduce((accumulator, value) => {
                  return accumulator + value
                }, 0)}
              </b>
            </label>
          </div>
        ))}

        <CButton style={{ margin: '2pt' }} onClick={props.descargarReporte}>
          Descargar
        </CButton>
      </>
    )
  } else {
    return <></>
  }
}

const VentasClientes = () => {
  let navigate = useNavigate()
  const [identificacion, setIdentificacion] = useState([])
  const [fechaInicial, setFechaInicial] = useState([])
  const [fechaFinal, setFechaFinal] = useState([])
  const [placas, setPlacas] = useState([])
  const [facturas, setFacturas] = useState([])
  const [ordenes, setOrdenes] = useState([])

  const toastRef = useRef()
  const FiltrarfacturasHandler = async () => {
    console.log('identificacion' + identificacion)
    if (!identificacion || identificacion == '') {
      toastRef.current.showToast('Debe colocar identificación')
    } else {
      let response = await Filtrarfacturas(fechaInicial, fechaFinal, identificacion)
      if (response == 'fail') {
        navigate('/Login', { replace: true })
      } else {
        setFacturas(response)
        let responseOrdenes = await FiltrarOrdenes(fechaInicial, fechaFinal, identificacion)
        if (responseOrdenes == 'fail') {
          navigate('/Login', { replace: true })
        } else {
          setOrdenes(response)
          setPlacas([
            ...new Set(
              [...response.map((x) => x.placa), ...response.map((x) => x.placa)].filter(
                (x, i, a) => a.indexOf(x) == i,
              ),
            ),
          ])
        }
      }
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

  const descargarReporte = async (event) => {
    let estacionNombre = localStorage.getItem('estacionNombre')
    let estacionNit = localStorage.getItem('estacionNit')

    var elements = [
      {
        stack: [estacionNombre, { text: 'Nit ' + estacionNit, style: 'subheader' }],
        style: 'header',
      },
      {
        text: 'Reporte de ventas por Cliente',
        style: 'header',
      },
      {
        text: 'Cliente ' + identificacion,
      },
      {
        text: 'Fecha Inicial ' + fechaInicial + ' Fecha Final ' + fechaFinal,
      },
    ]
    for (const placa of placas) {
      let placaTabla = [
        ['Tipo', 'Consecutivo', 'Fecha', 'Articulo', 'Km Actual', 'Cantidad', 'Valor'],
      ]
      let placasFacturaTableElements = facturas
        .filter((x) => x.placa == placa)
        .map((venta) => [
          'F',
          venta.consecutivo,
          venta.fecha,
          venta.combustible,
          venta.kilometraje,
          venta.cantidad,
          { text: cop.format(venta.total), style: 'tableRight' },
        ])
      placaTabla = placaTabla.concat(placasFacturaTableElements)
      let placasOrdenesTableElements = ordenes
        .filter((x) => x.placa == placa)
        .map((venta) => [
          'O',
          venta.idVentaLocal,
          venta.fecha,
          venta.combustible,
          venta.kilometraje,
          venta.cantidad,
          { text: cop.format(venta.total), style: 'tableRight' },
        ])
      placaTabla = placaTabla.concat(placasOrdenesTableElements)
      elements = elements.concat({
        stack: ['Placa ' + placa],
      })
      elements = elements.concat({
        style: 'tableExample',
        table: {
          headerRows: 1,
          body: placaTabla,
        },
      })
      elements = elements.concat({
        stack: [
          'Total ' +
            [
              ...new Set(
                [
                  ...ordenes.filter((x) => x.placa == placa).map((x) => x.total),
                  ...facturas.filter((x) => x.placa == placa).map((x) => x.total),
                ].filter((x, i, a) => a.indexOf(x) == i),
              ),
            ].reduce((accumulator, value) => {
              return accumulator + value
            }, 0),
        ],
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
        'ReporteVentas - ' +
          estacionNombre +
          ' - ' +
          identificacion +
          ' - ' +
          fechaInicial +
          '-' +
          fechaFinal +
          '.pdf',
      )
  }

  // Función para verificar si la fecha es válida
  const isValidDate = (date) => {
    const pattern = /^\d{4}-\d{2}-\d{2}$/
    return pattern.test(date)
  }
  const handleIdentificacionSelectedChange = (event) => {
    const value = event.target.value
    setIdentificacion(value)
  }

  return (
    <>
      <Toast ref={toastRef}></Toast>
      <h1>Reporte de Ventas por Cliente</h1>

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
        <CCol xs={2}>
          <label>Identificación</label>
        </CCol>
        <CCol xs={4}>
          <input
            className="form-control modal-tercero-input"
            name="fechaTurno"
            value={identificacion}
            onChange={handleIdentificacionSelectedChange}
          ></input>
        </CCol>
        <CButton style={{ margin: '2pt' }} onClick={FiltrarfacturasHandler}>
          Filtrar
        </CButton>
        <Facturas
          placas={placas}
          facturas={facturas}
          ordenes={ordenes}
          descargarReporte={descargarReporte}
        />
      </CRow>
    </>
  )
}

export default VentasClientes
