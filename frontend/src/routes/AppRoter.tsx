import { Routes, Route, Navigate } from 'react-router-dom'
import { publicRoutes } from './routes'
import { CONNECT_ROUTE, HOME_ROUTE } from './paths'

const AppRouter = () => {
  return (
    <Routes>
      {publicRoutes.map(({ path, Component }) => (
        <Route key={path} path={path} element={<Component />} />
      ))}
      <Route path="*" element={<Navigate to={`/${CONNECT_ROUTE}`} />} />
    </Routes>
  )
}

export default AppRouter
