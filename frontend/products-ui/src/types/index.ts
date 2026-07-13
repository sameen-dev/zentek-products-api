export const PRODUCT_COLOURS = [
  "Black",
  "White",
  "Red",
  "Blue",
  "Green",
  "Yellow",
  "Silver",
  "Grey",
  "Other",
] as const;

export type ProductColour = (typeof PRODUCT_COLOURS)[number];

export interface ProductResponse {
  id: string;
  name: string;
  sku: string;
  colour: string;
  price: number;
  description: string | null;
  createdUtc: string;
}

export interface CreateProductRequest {
  name: string;
  sku: string;
  colour: ProductColour;
  price: number;
  description?: string;
}

export interface TokenResponse {
  accessToken: string;
  expiresAtUtc: string;
}

export interface ProblemDetails {
  title?: string;
  status?: number;
  errors?: Record<string, string[]>;
}
