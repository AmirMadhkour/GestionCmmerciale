import { Injectable } from '@angular/core';
import { STORAGE_KEYS } from '../constants/app.constants';
import { CurrentUser, JwtPayload } from '../models/auth.model';

@Injectable({ providedIn: 'root' })
export class TokenStorageService {
  getToken(): string | null {
    return this.read(STORAGE_KEYS.token);
  }

  saveToken(token: string): void {
    this.write(STORAGE_KEYS.token, token);
  }

  getUser(): CurrentUser | null {
    const raw = this.read(STORAGE_KEYS.user);
    if (!raw) {
      return null;
    }
    try {
      return JSON.parse(raw) as CurrentUser;
    } catch {
      return null;
    }
  }

  saveUser(user: CurrentUser): void {
    this.write(STORAGE_KEYS.user, JSON.stringify(user));
  }

  clear(): void {
    try {
      localStorage.removeItem(STORAGE_KEYS.token);
      localStorage.removeItem(STORAGE_KEYS.user);
    } catch {
      return;
    }
  }

  decode(token: string): JwtPayload | null {
    const payload = token.split('.')[1];
    if (!payload) {
      return null;
    }
    try {
      const base64 = payload.replace(/-/g, '+').replace(/_/g, '/');
      const padded = base64.padEnd(base64.length + ((4 - (base64.length % 4)) % 4), '=');
      const bytes = Uint8Array.from(atob(padded), (char) => char.charCodeAt(0));
      return JSON.parse(new TextDecoder().decode(bytes)) as JwtPayload;
    } catch {
      return null;
    }
  }

  isTokenExpired(token: string | null = this.getToken()): boolean {
    if (!token) {
      return true;
    }
    const payload = this.decode(token);
    if (!payload?.exp) {
      return true;
    }
    return payload.exp * 1000 <= Date.now();
  }

  private read(key: string): string | null {
    try {
      return localStorage.getItem(key);
    } catch {
      return null;
    }
  }

  private write(key: string, value: string): void {
    try {
      localStorage.setItem(key, value);
    } catch {
      return;
    }
  }
}
