const rateLimit = require('express-rate-limit');

const authLimiter = rateLimit({
    windowMs: 15*60*1000,
    max:10,
    message:'Too many requests, you have been rate limited',
    standardHeaders: true,
    legacyHeaders: false
});

module.exports = authLimiter;
