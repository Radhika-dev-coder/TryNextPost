import { Injectable, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Router } from '@angular/router';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import { LoginRequest, LoginSuccessResponse } from '../models/api.models';

const TOKEN_KEY = 'tnp_token';
const EMAIL_KEY = 'tnp_email';

@Injectable({ providedIn: 'root' })
export class AuthService {
  readonly token = signal<string | null>(localStorage.getItem(TOKEN_KEY));
  readonly email = signal<string | null>(localStorage.getItem(EMAIL_KEY));

  constructor(
    private readonly http: HttpClient,
    private readonly router: Router,
  ) {}

  login(request: LoginRequest): Observable<LoginSuccessResponse> {
    return this.http
      .post<LoginSuccessResponse>(`${environment.apiBaseUrl}/api/Auth/login`, request)
      .pipe(
        tap((res) => {
          localStorage.setItem(TOKEN_KEY, res.token);
          localStorage.setItem(EMAIL_KEY, request.email);
          this.token.set(res.token);
          this.email.set(request.email);
        }),
      );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(EMAIL_KEY);
    this.token.set(null);
    this.email.set(null);
    void this.router.navigateByUrl('/login');
  }

  isLoggedIn(): boolean {
    return !!this.token();
  }
}
