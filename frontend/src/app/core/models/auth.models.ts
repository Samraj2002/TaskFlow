// src/app/core/models/auth.models.ts
export interface RegisterDto {
  name: string;
  email: string;
  password: string;
}

export interface LoginDto {
  email: string;
  password: string;
}

export interface AuthResponse {
  accessToken: string;
  refreshToken: string;
  name: string;
  email: string;
  role: 'Admin' | 'Manager' | 'Member';
  expiresAt: string;
}

export interface CurrentUser {
  id: string;
  name: string;
  email: string;
  role: 'Admin' | 'Manager' | 'Member';
  initials: string;
}

// Generic API response wrapper (needed by auth.service.ts)
export interface ApiResponse<T> {
  success: boolean;
  message: string;
  data: T;
  errors: string[];
}
