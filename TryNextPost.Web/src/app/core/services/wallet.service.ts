import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  ApiResponse,
  VerifyPaymentRequest,
  VerifyPaymentResponse,
  WalletBalance,
  WalletRechargeResponse,
} from '../models/api.models';

@Injectable({ providedIn: 'root' })
export class WalletService {
  constructor(private readonly http: HttpClient) {}

  getBalance(): Observable<WalletBalance> {
    return this.http
      .get<ApiResponse<WalletBalance>>(`${environment.apiBaseUrl}/api/Wallet/balance`)
      .pipe(map((r) => r.data));
  }

  createRecharge(amount: number): Observable<WalletRechargeResponse> {
    return this.http
      .post<ApiResponse<WalletRechargeResponse>>(`${environment.apiBaseUrl}/api/Wallet/recharge`, {
        amount,
      })
      .pipe(map((r) => r.data));
  }

  verifyPayment(payload: VerifyPaymentRequest): Observable<VerifyPaymentResponse> {
    return this.http
      .post<ApiResponse<VerifyPaymentResponse>>(
        `${environment.apiBaseUrl}/api/Wallet/verify-payment`,
        payload,
      )
      .pipe(map((r) => r.data));
  }
}
