import { React, useState, useEffect, useRef } from 'react'
import FiltrarInfoCanastilla from '../../services/FiltrarInfoCanastilla'
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
const Canastilla = () => {
  let navigate = useNavigate()
  const [fechaInicial, setFechaInicial] = useState([])
  const [fechaFinal, setFechaFinal] = useState([])
  const [facturas, setFacturas] = useState([])
  const [formas, setFormas] = useState([])
  const [articulos, setArticulos] = useState([])

  const toastRef = useRef()
  const filtrarInfoCanastilla = async () => {
    let response = await FiltrarInfoCanastilla(fechaInicial, fechaFinal)
    if (response == 'fail') {
      navigate('/Login', { replace: true })
    } else {
      setFacturas(response.facturas)
      setFormas(response.detalleFormaPago)
      setArticulos(response.detalleArticulo)
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
    var today = new Date()
    var dd = String(today.getDate()).padStart(2, '0')
    var mm = String(today.getMonth() + 1).padStart(2, '0') //January is 0!
    var yyyy = today.getFullYear()

    today = dd + '/' + mm + '/' + yyyy
    let estacionNombre = localStorage.getItem('estacionNombre')
    let estacionNit = localStorage.getItem('estacionNit')

    let articulosTabla = [['Articulo', 'Cantidad', 'Sub total', 'Iva', 'Total']]
    let articulosTableElements = articulos.map((articulo) => [
      articulo.descripcion,
      articulo.cantidad,
      { text: cop.format(articulo.subtotal), style: 'tableRight' },
      { text: cop.format(articulo.iva), style: 'tableRight' },
      { text: cop.format(articulo.total), style: 'tableRight' },
    ])
    articulosTabla = articulosTabla.concat(articulosTableElements)

    let formasTabla = [['Forma de pago', 'Pago', 'Cantidad', 'Total']]
    let formassTableElements = formas.map((forma) => [
      forma.formaDePago,
      forma.cantidad,
      forma.cantidad,
      { text: cop.format(forma.precio), style: 'tableRight' },
    ])

    formasTabla = formasTabla.concat(formassTableElements)
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

      content: [
        {
          stack: [estacionNombre, { text: 'Nit ' + estacionNit, style: 'subheader' }],
          style: 'header',
        },
        {
          text: 'Reporte de Canastilla',
          style: 'header',
        },
        {
          text: 'Fecha Inicial ' + fechaInicial + ' Fecha Final ' + fechaFinal,
        },
        {
          style: 'tableExample',
          table: {
            headerRows: 1,
            body: articulosTabla,
          },
        },
        {
          style: 'tableExample',
          table: {
            headerRows: 1,
            body: formasTabla,
          },
        },
      ],

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

    pdfMake.createPdf(dd).download('ReporteCanastilla - ' + estacionNombre + ' - ' + today + '.pdf')
  }

  return (
    <>
      <Toast ref={toastRef}></Toast>
      <h1>Reporte de Canastillas</h1>

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
        <CButton style={{ margin: '2pt' }} onClick={filtrarInfoCanastilla}>
          Filtrar
        </CButton>
      </CRow>
      <CButton style={{ margin: '2pt' }} onClick={descargarReporte}>
        Descargar
      </CButton>
      <CRow>
        <h2>Resumen por articulo</h2>
        <CTable>
          <CTableHead>
            <CTableRow>
              <CTableHeaderCell scope="col">Articulo</CTableHeaderCell>
              <CTableHeaderCell scope="col">Cantidad</CTableHeaderCell>
              <CTableHeaderCell scope="col">Sub total</CTableHeaderCell>
              <CTableHeaderCell scope="col">Iva</CTableHeaderCell>
              <CTableHeaderCell scope="col">Total</CTableHeaderCell>
            </CTableRow>
          </CTableHead>
          <CTableBody>
            {articulos.map((articulo) => (
              <CTableRow key={articulo.descripcion}>
                <CTableHeaderCell>{articulo.descripcion}</CTableHeaderCell>
                <CTableHeaderCell>{articulo.cantidad}</CTableHeaderCell>
                <CTableHeaderCell>{cop.format(articulo.subtotal)}</CTableHeaderCell>
                <CTableHeaderCell>{cop.format(articulo.iva)}</CTableHeaderCell>
                <CTableHeaderCell>{cop.format(articulo.total)}</CTableHeaderCell>
              </CTableRow>
            ))}
          </CTableBody>
        </CTable>
      </CRow>
      <CRow>
        <h2>Resumen por formas de pago</h2>
        <CTable>
          <CTableHead>
            <CTableRow>
              <CTableHeaderCell scope="col">Forma de pago</CTableHeaderCell>
              <CTableHeaderCell scope="col">Pago</CTableHeaderCell>
              <CTableHeaderCell scope="col">Cantidad</CTableHeaderCell>
              <CTableHeaderCell scope="col">Total</CTableHeaderCell>
            </CTableRow>
          </CTableHead>
          <CTableBody>
            {formas.map((forma) => (
              <CTableRow key={forma.descripcion}>
                <CTableHeaderCell>{forma.formaDePago}</CTableHeaderCell>
                <CTableHeaderCell>{forma.cantidad}</CTableHeaderCell>
                <CTableHeaderCell>{forma.cantidad}</CTableHeaderCell>
                <CTableHeaderCell>{cop.format(forma.precio)}</CTableHeaderCell>
              </CTableRow>
            ))}
          </CTableBody>
        </CTable>
      </CRow>
      <CButton style={{ margin: '2pt' }} onClick={descargarReporte}>
        Descargar
      </CButton>
    </>
  )
}

export default Canastilla
