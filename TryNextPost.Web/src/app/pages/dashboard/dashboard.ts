import { Component, OnInit, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { CurrencyPipe, DecimalPipe } from '@angular/common';
import { AuthService } from '../../core/services/auth.service';
import { WalletService } from '../../core/services/wallet.service';

type ModalStep = 'amount' | 'gateway';

@Component({
  selector: 'app-dashboard',
  standalone: true,
  imports: [FormsModule, CurrencyPipe, DecimalPipe],
  templateUrl: './dashboard.html',
  styleUrl: './dashboard.scss',
})
export class DashboardComponent implements OnInit {
  readonly balance = signal(0);
  readonly loadingBalance = signal(false);
  readonly paying = signal(false);
  readonly error = signal<string | null>(null);
  readonly success = signal<string | null>(null);

  readonly modalOpen = signal(false);
  readonly step = signal<ModalStep>('amount');
  readonly amount = signal(200);
  readonly presets = [200, 500, 1000, 2000, 5000, 10000];
  contact = '';

  constructor(
    readonly auth: AuthService,
    private readonly wallet: WalletService,
  ) {}

  ngOnInit(): void {
    this.refreshBalance();
  }

  refreshBalance(): void {
    this.loadingBalance.set(true);
    this.wallet.getBalance().subscribe({
      next: (data) => {
        this.balance.set(data.balance);
        this.loadingBalance.set(false);
      },
      error: (err) => {
        this.loadingBalance.set(false);
        this.error.set(err?.error?.message || 'Could not load wallet balance');
      },
    });
  }

  openRecharge(): void {
    this.error.set(null);
    this.success.set(null);
    this.step.set('amount');
    this.amount.set(200);
    this.modalOpen.set(true);
  }

  closeModal(): void {
    if (this.paying()) {
      return;
    }
    this.modalOpen.set(false);
  }

  selectAmount(value: number): void {
    this.amount.set(value);
  }

  goToGateway(): void {
    if (this.amount() <= 0) {
      this.error.set('Enter a valid amount');
      return;
    }
    this.step.set('gateway');
  }

  backToAmount(): void {
    this.step.set('amount');
  }

  payWithRazorpay(): void {
    this.error.set(null);
    this.success.set(null);
    this.paying.set(true);

    this.wallet.createRecharge(this.amount()).subscribe({
      next: (order) => {
        if (typeof window.Razorpay !== 'function') {
          this.paying.set(false);
          this.error.set('Razorpay Checkout script failed to load. Refresh the page.');
          return;
        }

        const rzp = new window.Razorpay({
          key: order.keyId,
          amount: order.amountInPaise,
          currency: order.currency || 'INR',
          name: 'TryNextPost',
          description: `Wallet recharge ₹${order.amount}`,
          order_id: order.gatewayOrderId,
          prefill: {
            email: this.auth.email() || undefined,
            contact: this.contact.trim() || undefined,
          },
          theme: { color: '#1d4ed8' },
          handler: (response) => {
            this.wallet
              .verifyPayment({
                razorpayOrderId: response.razorpay_order_id,
                razorpayPaymentId: response.razorpay_payment_id,
                razorpaySignature: response.razorpay_signature,
              })
              .subscribe({
                next: (verified) => {
                  this.paying.set(false);
                  this.modalOpen.set(false);
                  this.balance.set(verified.walletBalance);
                  this.success.set(
                    `₹${verified.amount} added. New balance: ₹${verified.walletBalance}`,
                  );
                },
                error: (err) => {
                  this.paying.set(false);
                  this.error.set(
                    err?.error?.message ||
                      'Payment captured but verify failed. Check wallet / contact support.',
                  );
                },
              });
          },
          modal: {
            ondismiss: () => {
              this.paying.set(false);
            },
          },
        });

        rzp.open();
      },
      error: (err) => {
        this.paying.set(false);
        this.error.set(err?.error?.message || 'Could not create Razorpay order');
      },
    });
  }
}
