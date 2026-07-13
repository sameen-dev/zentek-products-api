import { useState } from "react";
import "./App.css";
import { LoginForm } from "./components/LoginForm";
import { ProductsPage } from "./components/ProductsPage";

const TOKEN_STORAGE_KEY = "products-ui.accessToken";

function App() {
  const [token, setToken] = useState<string | null>(() => sessionStorage.getItem(TOKEN_STORAGE_KEY));

  function handleLoggedIn(accessToken: string) {
    sessionStorage.setItem(TOKEN_STORAGE_KEY, accessToken);
    setToken(accessToken);
  }

  function handleLogout() {
    sessionStorage.removeItem(TOKEN_STORAGE_KEY);
    setToken(null);
  }

  return (
    <main className="app">
      {token ? <ProductsPage token={token} onLogout={handleLogout} /> : <LoginForm onLoggedIn={handleLoggedIn} />}
    </main>
  );
}

export default App;
