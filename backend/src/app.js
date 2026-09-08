const express = require('express');
const cors = require('cors');
const helmet = require('helmet');

const app = express();

app.use(helmet());
app.use(cors({ origin: process.env.CORS_ORIGIN?.split(',') }));
app.use(express.json());

app.get('/health', (req, res) => {
  res.json({ status: 'ok' });
});

// Auth routes get wired up here once src/routes/auth.routes.js exists:
app.use('/auth', require('./routes/auth.routes'));

// Centralized error handler goes here once src/middleware/errorHandler.js exists:
// app.use(require('./middleware/errorHandler'));
app.use('/lobbies', require('./routes/lobby.routes'));
module.exports = app;
