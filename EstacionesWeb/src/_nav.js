import React from 'react'
import CIcon from '@coreui/icons-react'
import { cilCalculator, cilSpeedometer, cilUser, cilPlus, cilCog, cilContact } from '@coreui/icons'
import { CNavGroup, CNavItem, CNavTitle } from '@coreui/react'

const _nav = [
  {
    component: CNavItem,
    name: 'Estaciones',
    to: '/dashboard',
    icon: <CIcon icon={cilSpeedometer} customClassName="nav-icon" />,
    level: 2,
  },
  {
    component: CNavItem,
    name: 'Turnos',
    to: '/Turnos',
    icon: <CIcon icon={cilCalculator} customClassName="nav-icon" />,
    level: 1,
  },
  {
    component: CNavItem,
    name: 'Canastilla',
    to: '/Canastilla',
    icon: <CIcon icon={cilCalculator} customClassName="nav-icon" />,
    level: 1,
  },
  {
    component: CNavItem,
    name: 'Cupo por cliente',
    to: '/PorCliente',
    icon: <CIcon icon={cilCalculator} customClassName="nav-icon" />,
    level: 1,
  },
  {
    component: CNavItem,
    name: 'Cupo por automotores',
    to: '/PorAutomotores',
    icon: <CIcon icon={cilCalculator} customClassName="nav-icon" />,
    level: 1,
  },
  {
    component: CNavItem,
    name: 'Ventas por clientes',
    to: '/VentasClientes',
    icon: <CIcon icon={cilCalculator} customClassName="nav-icon" />,
    level: 1,
  },
]

export default _nav
