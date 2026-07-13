import { PRODUCT_COLOURS } from "../types";
import type { ProductResponse } from "../types";

interface ProductListProps {
  products: ProductResponse[];
  colourFilter: string;
  onColourFilterChange: (colour: string) => void;
  isLoading: boolean;
}

export function ProductList({ products, colourFilter, onColourFilterChange, isLoading }: ProductListProps) {
  return (
    <div className="card">
      <div className="list-header">
        <h2>Products</h2>
        <label className="filter">
          Colour
          <select value={colourFilter} onChange={(e) => onColourFilterChange(e.target.value)}>
            <option value="">All</option>
            {PRODUCT_COLOURS.map((c) => (
              <option key={c} value={c}>
                {c}
              </option>
            ))}
          </select>
        </label>
      </div>

      {isLoading ? (
        <p>Loading…</p>
      ) : products.length === 0 ? (
        <p>No products found.</p>
      ) : (
        <table>
          <thead>
            <tr>
              <th>Name</th>
              <th>SKU</th>
              <th>Colour</th>
              <th>Price</th>
              <th>Description</th>
            </tr>
          </thead>
          <tbody>
            {products.map((product) => (
              <tr key={product.id}>
                <td>{product.name}</td>
                <td>{product.sku}</td>
                <td>{product.colour}</td>
                <td>{product.price.toFixed(2)}</td>
                <td>{product.description ?? "—"}</td>
              </tr>
            ))}
          </tbody>
        </table>
      )}
    </div>
  );
}
