import { StrictMode } from 'react'
import { createRoot } from 'react-dom/client'
import './index.css'
import App from './App.tsx'
import AuthProvider from './context/AuthProvider.tsx'
import BoardProvider from './context/BoardProvider.tsx'
import {BrowserRouter} from "react-router";

createRoot(document.getElementById('root')!).render(
  <StrictMode>
    <AuthProvider>
      <BoardProvider>
          <BrowserRouter>
              <App />
          </BrowserRouter>
      </BoardProvider>
    </AuthProvider>
  </StrictMode>,
)
