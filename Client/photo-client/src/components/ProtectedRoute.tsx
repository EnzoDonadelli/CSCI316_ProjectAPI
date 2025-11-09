import React from 'react'
import { Navigate } from 'react-router-dom'
import { useAppSelector } from '../store/hooks'

export default function ProtectedRoute({ children }: { children: JSX.Element }) {
  const token = useAppSelector(s => (s as any).auth?.token)
  if (!token) {
    return <Navigate to="/login" replace />
  }
  return children
}
