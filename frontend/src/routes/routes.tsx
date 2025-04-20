import ConnectPage from '../pages/connectPage/ConnectPage'
import DashboardPage from '../pages/dashboardPage/DashboardPage'
import { CONNECT_ROUTE, HOME_ROUTE } from './paths'

export const publicRoutes = [
  {
    path: CONNECT_ROUTE,
    Component: ConnectPage,
  },
  {
    path: HOME_ROUTE,
    Component: DashboardPage,
  },
]
