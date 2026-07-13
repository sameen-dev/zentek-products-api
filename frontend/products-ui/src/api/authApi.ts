import { apiRequest } from "./client";
import type { TokenResponse } from "../types";

export function login(username: string, password: string): Promise<TokenResponse> {
  return apiRequest<TokenResponse>("/api/auth/token", {
    method: "POST",
    body: { username, password },
  });
}
