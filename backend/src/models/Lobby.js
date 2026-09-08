const mongoose = require('mongoose');
const lobbySchema = new mongoose.Schema(
    {
        hostUserId: {
            type: mongoose.Schema.Types.ObjectId,
            ref: 'User',
            required: true
        },
        inviteCode: {
            type: String,
            required: true,
            unique: true,
            trim: true,
            lowercase: true,
            match: [/^[a-zA-Z0-9_]+$/, 'Invalid code'],
        },
        players: [
            {
                type: mongoose.Schema.Types.ObjectId, 
                ref: 'User',
            },
        ],
        maxPlayers:{
            type: Number,
            default: 5,
        },
        status:{
            type: String,
            enum: ['waiting', 'started'],
            default: 'waiting',
        }
    },
    {timestamps: true}
);

module.exports = mongoose.model('Lobby', lobbySchema);

