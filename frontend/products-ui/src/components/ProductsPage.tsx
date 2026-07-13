import { useCallback, useEffect, useState } from "react";
import { getProducts } from "../api/productsApi";
import { ApiError } from "../api/client";
import type { ProductResponse } from "../types";
import { ProductForm } from "./ProductForm";
import { ProductList } from "./ProductList";

interface ProductsPageProps {
  token: string;
  onLogout: () => void;
}

export function ProductsPage({ token, onLogout }: ProductsPageProps) {
  const [products, setProducts] = useState<ProductResponse[]>([]);
  const [colourFilter, setColourFilter] = useState("");
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const loadProducts = useCallback(async () => {
    setIsLoading(true);
    setError(null);
    try {
      const result = await getProducts(token, colourFilter || undefined);
      setProducts(result);
    } catch (err) {
      if (err instanceof ApiError && err.status === 401) {
        onLogout();
        return;
      }
      setError(err instanceof ApiError ? err.message : "Unable to load products.");
    } finally {
      setIsLoading(false);
    }
  }, [token, colourFilter, onLogout]);

  useEffect(() => {
    void loadProducts();
  }, [loadProducts]);

  return (
    <div className="page">
      <div className="page-header">
        <h1>Products</h1>
        <button className="secondary" onClick={onLogout}>
          Sign out
        </button>
      </div>

      <ProductForm token={token} onCreated={loadProducts} />

      {error && <p className="error">{error}</p>}

      <ProductList
        products={products}
        colourFilter={colourFilter}
        onColourFilterChange={setColourFilter}
        isLoading={isLoading}
      />
    </div>
  );
}
