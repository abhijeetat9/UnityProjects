const User = require('../models/User');
const jwt = require("jsonwebtoken");

async function signup(req, res) {
    try {
        const {email, username, password} = req.body;
        if(!email || !username || !password) {
            return res.status(400).json({
                message: 'Email, username, and password are required'});
        }
        
        const user = await User.create({email, username,password})
        
        res.status(201).json({
            message: 'User successfully created!',
            user: {
                id: user._id,
                email: user.email,
                username: user.username
            }
        });
    }
    catch (err) {
        if(err.code === 11000) {
            return res.status(409).json({
                message: 'Email, username already exists!',
            })
        }
        if(err.name === "ValidationError")
        {
            return res.status(400).json({
                error: err.message
            })
        }
        console.error('Signup error:', err);
        res.status(500).json({ message: 'Something went wrong' });
    }
}

async function login(req, res) {
    try{
        const {username, password} = req.body;
        if(!username || !password) {
            return res.status(400).json({
                message: 'Username and password are required'
            })
        }
        
        const user = await User.findOne({username}).select('+password').exec();
        if(!user){
            return res.status(401).json({
                message: 'Invalid credentials',
            })
        }
        
        const matchPassword = await user.comparePassword(password);
        if(!matchPassword){
            return res.status(401).json({
                message: 'Invalid credentials',
            })
        }
        
        const token = jwt.sign(
            { userId: user._id }, 
            process.env.JWT_SECRET,
            {
                expiresIn: process.env.JWT_EXPIRES_IN
            })
        
        return res.status(200).json({
            token, 
            user: {id: user._id, username: user.username, email: user.email},
            message: 'Authenticated successfully!'
        })
    }
    catch (err) {
        console.error('Login error:', err);
        res.status(500).json({ message: 'Something went wrong' });
    }
}

async function me(req, res) {
    try{
        const user = await User.findById(req.userId);
        if(!user){
            return res.status(404).json({
                message: 'User not found!',
            });
        }
        
        res.status(200).json({
            id: user._id,
            email: user.email,
            username: user.username
        });
        
    }catch(err){
        console.error('Me error:', err);
        res.status(500).json({ message: 'Something went wrong'});
    }
}

module.exports = {signup, login, me};