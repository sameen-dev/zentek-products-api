import { apiRequest } from "./client";
import type { CreateProductRequest, ProductResponse } from "../types";

export function getProducts(token: string, colour?: string): Promise<ProductResponse[]> {
  const query = colour ? `?colour=${encodeURIComponent(colour)}` : "";
  return apiRequest<ProductResponse[]>(`/api/products${query}`, { token });
}

export function createProduct(token: string, request: CreateProductRequest): Promise<ProductResponse> {
  return apiRequest<ProductResponse>("/api/products", {
    method: "POST",
    token,
    body: request,
  });
}
