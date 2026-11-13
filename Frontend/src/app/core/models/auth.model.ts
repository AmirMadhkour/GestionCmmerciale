import { Role } from '../constants/roles';

export interface LoginRequest {
  email: string;
  password: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  fullName: string;
  role: string;
}

export interface CurrentUser {
  id: string;
  email: string;
  fullName: string;
  role: Role;
}

export interface JwtPayload {
  sub: string;
  email: string;
  role: string;
  exp: number;
}
