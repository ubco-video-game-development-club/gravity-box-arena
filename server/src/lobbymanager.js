const logger = require("./logger");
const userManager = require("./userManager");

var lobbyIdCounter = 0;
var openLobbies = []; //Lobbies that can be joined
var playerLobbies = { }; //Map of player ID to the lobby they're in

function joinOrCreateLobby(user, responseCode) {
	if(user == undefined) {
		logger.logWarning("Nonexistent user tried to create a lobby.");
		return;
	}

	var lobby;
	if(openLobbies.length > 0) {
		lobby = openLobbies[0];
	} else {
		lobby = new Lobby(4);
		openLobbies.push(lobby);
	}

	lobby.currentPlayers.push(user.key);
	playerLobbies[user.key] = lobby;

	if(lobby.currentPlayers.length >= lobby.maxPlayers) {
		let index = openLobbies.indexOf(lobby);
		if(index >= 0) {
			openLobbies.splice(index, 1);
		}
	}

	let buffer = Buffer.alloc(5);
	buffer.writeUInt8(responseCode, 0);
	buffer.writeInt32BE(lobby.id, 1);
	user.getConnection().sendBytes(buffer);
}

function listLobbies() {
	console.log("Lobbies:");
	console.log(JSON.stringify(playerLobbies, null, "\t"));
}

function syncData(user, responseCode, data) {
	if(user == undefined) {
		logger.logWarning("Nonexistent user tried to sync data.");
		return;
	}

	let key = user.key;
	let buf = Buffer.alloc(1 + key.length + data.length); //Layout: Response code, from user, data
	buf.writeUInt8(responseCode, 0);
	buf.write(key, 1);
	for(let i = 0; i < data.length; i++) {
		buf.writeUInt8(data[i], i + 2);
	}

	let lobby = playerLobbies[key];
	if(lobby == undefined) {
		logger.logWarning("User tried to sync data while not in a lobby.");
		return;
	}

	let players = lobby.currentPlayers;
	for(let i = 0; i < players.length; i++) {
		let playerKey = players[i];
		let currentPlayer = userManager.getUser(playerKey);
		if(currentPlayer != user) {
			let conn = currentPlayer.getConnection();
			conn.sendBytes(buf, (err) => {
				logger.logMessage(`User disconnected: ${err}`);
				currentPlayer.delete();
				lobby.removePlayer(currentPlayer);
			});
		}
	}
}

class Lobby {
	constructor(maxPlayers) {
		this.maxPlayers = maxPlayers;
		this.currentPlayers = [];
		this.lobbyId = lobbyIdCounter++;
	}

	removePlayer(player) {
		let index = this.currentPlayers.indexOf(player);
		if(index < 0) return;
		currentPlayers.splice(index, 1);
	}
}

module.exports.joinOrCreateLobby = joinOrCreateLobby;
module.exports.listLobbies = listLobbies;
module.exports.syncData = syncData;