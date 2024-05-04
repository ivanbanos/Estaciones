import { React, useState, useEffect, useRef } from 'react'
import EstacionesDropdown from '../estaciones/EstacionesDropdown'
import GetCuposPorClientes from '../../services/GetCuposPorClientes'
import { Link, useNavigate } from 'react-router-dom'
import { Page, Text, View, Document, StyleSheet } from '@react-pdf/renderer'
import CIcon from '@coreui/icons-react'
import { cilPlus, cilPencil, cilX } from '@coreui/icons'
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
const PorCliente = () => {
  let navigate = useNavigate()
  const [cupos, setCupos] = useState([])

  const toastRef = useRef()
  const fetchCupos = async () => {
    let response = await GetCuposPorClientes()
    if (response == 'fail') {
      navigate('/Login', { replace: true })
    } else {
      setCupos(response)
    }
  }

  const descargarReporte = async (event) => {
    var today = new Date()
    var dd = String(today.getDate()).padStart(2, '0')
    var mm = String(today.getMonth() + 1).padStart(2, '0') //January is 0!
    var yyyy = today.getFullYear()

    today = dd + '/' + mm + '/' + yyyy
    let estacionNombre = localStorage.getItem('estacionNombre')
    let estacionNit = localStorage.getItem('estacionNit')
    let cuposTabla = [['Cliente', 'Nit/CC', 'Cupo asignado', 'Cupo disponible', 'Debe']]
    let cuposTableElements = cupos.map((cupo) => [
      cupo.cliente,
      cupo.nit,
      { text: cop.format(cupo.cupoAsignado), style: 'tableRight' },
      { text: cop.format(cupo.cupoDisponible), style: 'tableRight' },
      { text: cop.format(cupo.cupoAsignado - cupo.cupoDisponible), style: 'tableRight' },
    ])
    cuposTabla = cuposTabla.concat(cuposTableElements)
    console.log(cuposTabla)
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
          text: 'Reporte de Cupos de Clientes',
          style: 'header',
        },
        {
          style: 'tableExample',
          table: {
            headerRows: 1,
            body: cuposTabla,
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

    pdfMake
      .createPdf(dd)
      .download('ReporteCuposClientes - ' + estacionNombre + ' - ' + today + '.pdf')
  }

  useEffect(() => {
    fetchCupos()
  }, [])

  return (
    <>
      <Toast ref={toastRef}></Toast>
      <h1>Reporte de cupos por clientes</h1>

      <CButton style={{ margin: '2pt' }} onClick={descargarReporte}>
        Descargar
      </CButton>
      <CRow>
        <CTable>
          <CTableHead>
            <CTableRow>
              <CTableHeaderCell scope="col">Cliente</CTableHeaderCell>
              <CTableHeaderCell scope="col">Nit/CC</CTableHeaderCell>
              <CTableHeaderCell scope="col">Cupo Asignado</CTableHeaderCell>
              <CTableHeaderCell scope="col">Cupo Disponible</CTableHeaderCell>
              <CTableHeaderCell scope="col">Debe</CTableHeaderCell>
            </CTableRow>
          </CTableHead>
          <CTableBody>
            {cupos.map((cupo) => (
              <CTableRow key={cupo.Placa}>
                <CTableHeaderCell>{cupo.cliente}</CTableHeaderCell>
                <CTableHeaderCell>{cupo.nit}</CTableHeaderCell>
                <CTableHeaderCell>{cop.format(cupo.cupoAsignado)}</CTableHeaderCell>
                <CTableHeaderCell>{cop.format(cupo.cupoDisponible)}</CTableHeaderCell>
                <CTableHeaderCell>
                  {cop.format(cupo.cupoAsignado - cupo.cupoDisponible)}
                </CTableHeaderCell>
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

export default PorCliente
