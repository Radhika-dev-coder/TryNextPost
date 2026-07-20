# TryNextPost.Web

Angular dashboard for wallet recharge (NimbusPost-like flow + Razorpay Checkout).

## Run

1. Start API (`https` profile) — `https://localhost:7167`
2. Ensure User Secrets have `Razorpay:KeyId` and `Razorpay:KeySecret`
3. Frontend:

```bash
cd TryNextPost.Web
npm start
```

Open `http://localhost:4200`

## Flow

1. Login as **Seller** (wallet owner)
2. Click **Recharge** → choose amount → **Pay** → Razorpay
3. After Checkout success → `POST /api/Wallet/verify-payment` → balance updates

API base URL: `src/environments/environment.ts`
