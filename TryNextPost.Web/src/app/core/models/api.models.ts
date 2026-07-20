export interface ApiResponse<T> {
  success: boolean;
  message: string;
  errors: string[] | null;
  data: T;
  statusCode: number;
}

export interface LoginRequest {
  email: string;
  password: string;
  deviceId: string;
}

export interface LoginSuccessResponse {
  message: string;
  token: string;
  expiresAt: string;
  roles: string[];
}

export interface WalletBalance {
  walletId: number;
  userId: string;
  balance: number;
}

export interface WalletRechargeResponse {
  paymentOrderId: number;
  gatewayOrderId: string;
  keyId: string;
  amount: number;
  amountInPaise: number;
  currency: string;
  receipt: string;
}

export interface VerifyPaymentRequest {
  razorpayOrderId: string;
  razorpayPaymentId: string;
  razorpaySignature: string;
}

export interface VerifyPaymentResponse {
  paymentOrderId: number;
  gatewayOrderId: string;
  gatewayPaymentId: string | null;
  status: string;
  amount: number;
  walletBalance: number;
  alreadyProcessed: boolean;
}
