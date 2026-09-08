const Lobby = require('../models/Lobby');
const crypto = require('crypto');

async function generateCode() {
    let code = crypto.randomBytes(4).toString('hex').slice(0,6);
    return code;
}

async function createLobby(req, res) {
    try{
        const code = await generateCode();
        if (!code) {
            return res.status(400).json({
                message: `Invalid code ${code}`,
            })
        }
        const lobby = await Lobby.findOne({inviteCode: code});
        if (lobby) {
            return res.status(409).json({
                message: `Code already exists ${code}`,
            })
        }
        
        const hostUserId = req.userId;
        const newLobby = await Lobby.create({
            inviteCode: code,
            hostUserId: hostUserId,
            players: [hostUserId],
        });
        
        return res.status(201).json(
            {
                lobby: newLobby, 
                hostUserId: hostUserId,
                message: `Lobby created successfully. Here is the code: ${code}`,
        });
    }catch (err) {
        console.error('Create lobby error:', err);
        res.status(500).json({ message: 'Something went wrong' });
    }
}

async function joinLobby(req, res) {
    try{
        const {code} = req.params;
        const userId = req.userId;
        const cleanCode = code.trim().toLowerCase();
        
        let lobby = await Lobby.findOne({inviteCode: cleanCode});
        if (!lobby) {
            return res.status(404).json({
                message: 'Lobby not found',
            })
        }
        if(lobby.status !== 'waiting') {
            return res.status(400).json({
                error: 'Game already in progress',
                message: `Lobby joined invite failed: ${cleanCode}`,
            })
        }
        
        if(lobby.players.length >= lobby.maxPlayers) {
            return res.status(400).json({
                error: 'Lobby already full',
                message: 'Too many players in the lobby',
            })
        }
        
        if(lobby.players.some(p => p.toString() === userId)) {
            return res.status(200).json({
                message:`You are already a member of this lobby`,
                lobby,
            });
        }
        
        lobby.players.push(userId);
        const updatedLobby = await lobby.save();
        return res.status(200).json({
            lobby: updatedLobby,
        })
        
    }catch (err) {
        console.error('Join lobby error:', err);
        res.status(500).json({ message: 'Something went wrong' });
    }
}

async function getLobby(req, res) {
    try{
        const {code} = req.params;
        const cleanCode = code.trim().toLowerCase();
        
        let lobby = await Lobby.findOne({inviteCode: cleanCode}).populate('players', 'username').populate('hostUserId', 'username');
        if (lobby) {
            return res.status(200).json({
                lobby: lobby,
            });
        }
        else {
            return res.status(404).json({
                message: 'Lobby not found',
            });
        }
        
    }catch (err) {
        console.error('Get lobby error:', err);
        res.status(500).json({ message: 'Something went wrong' });
    }
}

async function startLobby(req, res) {
    try{
        const {code} = req.params;
        const userId = req.userId;
        const cleanCode = code.trim().toLowerCase();

        let lobby = await Lobby.findOne({inviteCode: cleanCode});
        if (!lobby) {
            return res.status(404).json({
                message: 'Lobby not found',
            })
        }
        
        if(lobby.hostUserId.toString() !== userId)
        {
            return res.status(403).json({
                message: 'Only the host can start the game',
            })
        }
        
        if(lobby.status === 'started') {
            return res.status(409).json({
                message: 'lobby already started', 
                lobby,
            })
        }
        
        if(lobby.players.length < 2)
        {
            return res.status(409).json({
                message: 'At least 2 players are required to start the game',
            })
        }
        
        lobby.status = 'started';
        await lobby.save();
        return res.status(200).json({
            lobby,
            message: 'Lobby started',
        });
        
    }catch (err) {
        console.error('Start lobby error:', err);
        res.status(500).json({ message: 'Something went wrong' });
    }
}
module.exports = {createLobby, joinLobby, getLobby, startLobby};