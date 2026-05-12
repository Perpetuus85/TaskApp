import './App.css';
import { useAuth } from './context/AuthProvider';
import AppRoutes from "./routes/AppRoutes.tsx";

function App() {
  const { isLoading } = useAuth();

  if (isLoading) {
    return <p>Loading...</p>;
  }


  return (
      <>
          <AppRoutes />
      </>
  );
}

export default App;
