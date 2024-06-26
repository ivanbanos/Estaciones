import { React, useState } from 'react'
import { Link, useNavigate } from 'react-router-dom'
import {
  CButton,
  CCard,
  CCardBody,
  CCardGroup,
  CCol,
  CContainer,
  CForm,
  CFormInput,
  CInputGroup,
  CInputGroupText,
  CRow,
} from '@coreui/react'
import CIcon from '@coreui/icons-react'
import { cilLockLocked, cilUser } from '@coreui/icons'
import GetToken from '../../../services/GetToken'
import '../../../App.css'

const Login = () => {
  let navigate = useNavigate()
  localStorage.setItem('token', null)
  localStorage.setItem('role', null)
  localStorage.setItem('idClient', null)
  const [errorMessages, seterrormessages] = useState({})
  const [first_name, setfirst_name] = useState()
  const [password, setPassword] = useState()
  const handleFirstNameChange = (event) => {
    setfirst_name(event.target.value)
  }
  const handlePasswordChange = (event) => {
    setPassword(event.target.value)
  }
  const handleSubmit = async (event) => {
    //Prevent page reload
    event.preventDefault()
    const token = await GetToken(first_name, password)
    // Compare user info
    if (token) {
      localStorage.setItem('token', token.token)

      navigate('/Dashboard', { replace: true })
    } else {
      // Username not found
      seterrormessages({ name: 'password', message: 'Username or Password incorrect' })
    }
  }

  return (
    <div className="bg-light min-vh-100 d-flex flex-row align-items-center">
      <CContainer>
        <CRow className="justify-content-center">
          <CCol md={8}>
            <CCardGroup>
              <CCard className="p-4">
                <CCardBody>
                  <CForm>
                    <h1>SIGES</h1>
                    <p className="text-medium-emphasis">Ingresar</p>
                    <CInputGroup className="mb-3">
                      <CInputGroupText>
                        <CIcon icon={cilUser} />
                      </CInputGroupText>
                      <CFormInput
                        placeholder="Usuario"
                        name="first_name"
                        value={first_name}
                        autoComplete="username"
                        onChange={handleFirstNameChange}
                      />
                    </CInputGroup>
                    <CInputGroup className="mb-4">
                      <CInputGroupText>
                        <CIcon icon={cilLockLocked} />
                      </CInputGroupText>
                      <CFormInput
                        name="password"
                        type="password"
                        value={password}
                        placeholder="Contraseña"
                        autoComplete="current-password"
                        onChange={handlePasswordChange}
                      />
                    </CInputGroup>
                    <CRow>
                      <CCol xs={6}>
                        <CButton
                          color="primary"
                          className="px-4"
                          onClick={handleSubmit}
                          navigate={navigate}
                          seterrormessages={seterrormessages}
                        >
                          Iniciar sesión
                        </CButton>
                        <div className="error">{errorMessages.message}</div>
                      </CCol>
                    </CRow>
                  </CForm>
                </CCardBody>
              </CCard>
              <CCard className="py-5" style={{ width: '44%' }}>
                <CCardBody className="text-center">
                  <div>
                    <img id="logo" src={require('../../../assets/images/Icono.png')} />
                  </div>
                </CCardBody>
              </CCard>
            </CCardGroup>
          </CCol>
        </CRow>
      </CContainer>
    </div>
  )
}

export default Login
