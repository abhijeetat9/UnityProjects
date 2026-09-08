# Cabboo Backend

Auth + lobby API for Cabboo.

## Setup

```
cd backend
npm install
cp .env.example .env   # fill in real values
npm run dev
```

`GET /health` should return `{ "status": "ok" }` once the server and MongoDB connection are both up.

## Structure

- `src/config/` - database connection setup
- `src/models/` - Mongoose schemas
- `src/routes/` - Express route definitions
- `src/controllers/` - request handler logic
- `src/middleware/` - auth verification, error handling
- `src/utils/` - shared helpers (JWT signing/verification, etc.)
