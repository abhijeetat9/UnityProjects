const jwt = require('jsonwebtoken');

function requireAuth(req, res, next) {
    const authHeader = req.headers.authorization;
    if (!authHeader || !authHeader.startsWith('Bearer ')) {
        return res.status(401).json({
            error: 'No token provided',
        });
    }
    const token = authHeader.split('Bearer ')[1];
    try{
        const decodedToken = jwt.verify(token, process.env.JWT_SECRET);
        req.userId = decodedToken.userId;
        next();
    }catch(err){
        res.status(401).json({
            error: 'Invalid or expired token',
        })
    }
}

module.exports = requireAuth;
