const express = require('express');
const router = express.Router();
const lobbyController = require('../controllers/lobby.controller');
const requireAuth = require('../middleware/auth.middleware');

router.post('/', requireAuth, lobbyController.createLobby);

router.post('/:code/join', requireAuth, lobbyController.joinLobby);

router.get('/:code', requireAuth, lobbyController.getLobby);

router.post('/:code/start', requireAuth, lobbyController.startLobby);

module.exports = router;