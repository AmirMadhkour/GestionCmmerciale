import { HttpClient } from '@angular/common/http';
import { Injectable, computed, inject, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ROLE_LABELS, Role, RoleInput, toRole } from '../constants/roles';
import { AuthResponse, CurrentUser, LoginRequest } from '../models/auth.model';
import { TokenStorageService } from './token-storage.service';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly router = inject(Router);
  private readonly storage = inject(TokenStorageService);
  private readonly baseUrl = `${environment.apiUrl}/auth`;

  private readonly user = signal<CurrentUser | null>(this.restoreUser());

  readonly currentUser = this.user.asReadonly();
  readonly isAuthenticated = computed(() => this.user() !== null);
  readonly isAdmin = computed(() => this.user()?.role === Role.Admin);
  readonly roleLabel = computed(() => {
    const current = this.user();
    return current ? (ROLE_LABELS[current.role] ?? current.role) : '';
  });

  login(credentials: LoginRequest): Observable<CurrentUser> {
    return this.http.post<AuthResponse>(`${this.baseUrl}/login`, credentials).pipe(
      map((response) => {
        const user = this.buildUser(response);
        this.storage.saveToken(response.token);
        this.storage.saveUser(user);
        this.user.set(user);
        return user;
      })
    );
  }

  logout(): void {
    this.clearSession();
    void this.router.navigate(['/login']);
  }

  clearSession(): void {
    this.storage.clear();
    this.user.set(null);
  }

  isSessionValid(): boolean {
    if (this.storage.isTokenExpired()) {
      if (this.user()) {
        this.clearSession();
      }
      return false;
    }
    return this.user() !== null;
  }

  hasRole(roles: RoleInput | RoleInput[]): boolean {
    const current = this.user();
    if (!current) {
      return false;
    }
    const allowed = (Array.isArray(roles) ? roles : [roles]).map(toRole);
    return allowed.includes(current.role);
  }

  private restoreUser(): CurrentUser | null {
    if (this.storage.isTokenExpired()) {
      this.storage.clear();
      return null;
    }
    return this.storage.getUser();
  }

  private buildUser(response: AuthResponse): CurrentUser {
    const payload = this.storage.decode(response.token);
    return {
      id: payload?.sub ?? '',
      email: response.email,
      fullName: response.fullName,
      role: toRole(response.role)
    };
  }
}
