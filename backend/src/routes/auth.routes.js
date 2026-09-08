const express = require('express');
const router = express.Router();
const authController = require('../controllers/auth.controller');
const requireAuth = require('../middleware/auth.middleware');
const rateLimit = require('../middleware/rateLimiter');

router.post('/signup', rateLimit, authController.signup);
router.post('/login', rateLimit, authController.login);
router.get('/me', requireAuth, authController.me);

module.exports = router;