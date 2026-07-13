import { useState, type FormEvent } from "react";
import { PRODUCT_COLOURS, type ProductColour } from "../types";
import { createProduct } from "../api/productsApi";
import { ApiError } from "../api/client";

interface ProductFormProps {
  token: string;
  onCreated: () => void;
}

export function ProductForm({ token, onCreated }: ProductFormProps) {
  const [name, setName] = useState("");
  const [sku, setSku] = useState("");
  const [colour, setColour] = useState<ProductColour>("Black");
  const [price, setPrice] = useState("");
  const [description, setDescription] = useState("");
  const [fieldErrors, setFieldErrors] = useState<Record<string, string[]>>({});
  const [error, setError] = useState<string | null>(null);
  const [isSubmitting, setIsSubmitting] = useState(false);

  async function handleSubmit(event: FormEvent) {
    event.preventDefault();
    setError(null);
    setFieldErrors({});
    setIsSubmitting(true);

    try {
      await createProduct(token, {
        name,
        sku,
        colour,
        price: Number(price),
        description: description || undefined,
      });
      setName("");
      setSku("");
      setPrice("");
      setDescription("");
      onCreated();
    } catch (err) {
      if (err instanceof ApiError && err.problem?.errors) {
        setFieldErrors(err.problem.errors);
      } else {
        setError(err instanceof ApiError ? err.message : "Unable to create the product.");
      }
    } finally {
      setIsSubmitting(false);
    }
  }

  return (
    <form className="card" onSubmit={handleSubmit}>
      <h2>Add a product</h2>
      <label>
        Name
        <input value={name} onChange={(e) => setName(e.target.value)} required />
      </label>
      {fieldErrors["Name"] && <p className="error">{fieldErrors["Name"].join(" ")}</p>}

      <label>
        SKU
        <input value={sku} onChange={(e) => setSku(e.target.value)} required />
      </label>
      {fieldErrors["Sku"] && <p className="error">{fieldErrors["Sku"].join(" ")}</p>}

      <label>
        Colour
        <select value={colour} onChange={(e) => setColour(e.target.value as ProductColour)}>
          {PRODUCT_COLOURS.map((c) => (
            <option key={c} value={c}>
              {c}
            </option>
          ))}
        </select>
      </label>
      {fieldErrors["Colour"] && <p className="error">{fieldErrors["Colour"].join(" ")}</p>}

      <label>
        Price
        <input type="number" step="0.01" min="0.01" value={price} onChange={(e) => setPrice(e.target.value)} required />
      </label>
      {fieldErrors["Price"] && <p className="error">{fieldErrors["Price"].join(" ")}</p>}

      <label>
        Description (optional)
        <textarea value={description} onChange={(e) => setDescription(e.target.value)} />
      </label>

      {error && <p className="error">{error}</p>}
      <button type="submit" disabled={isSubmitting}>
        {isSubmitting ? "Saving…" : "Create product"}
      </button>
    </form>
  );
}
