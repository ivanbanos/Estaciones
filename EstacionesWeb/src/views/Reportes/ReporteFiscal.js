import { React, useState, useEffect, useRef } from 'react'
import ReporteFiscalCall from '../../services/ReporteFiscal'
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
const ReporteFiscal = () => {
  let navigate = useNavigate()
  const [fechaInicial, setFechaInicial] = useState([])
  const [fechaFinal, setFechaFinal] = useState([])

  const toastRef = useRef()
  const FiltrarReporteFiscalHandler = async () => {
    let response = await ReporteFiscalCall(fechaInicial, fechaFinal)
    if (response == 'fail') {
      navigate('/Login', { replace: true })
    } else {
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

  return (
    <>
      <Toast ref={toastRef}></Toast>
      <h1>Reporte Fiscal</h1>

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
        <CButton style={{ margin: '2pt' }} onClick={FiltrarReporteFiscalHandler}>
          Filtrar
        </CButton>
      </CRow>
    </>
  )
}

export default ReporteFiscal
