const logger = require("./logger");

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
		lobby = createLobby(4, openLobbies.length); //Set the lobby ID to the current lobby ID and then increment
		openLobbies.push(lobby);
	}

	if(lobby.currentPlayers.length >= lobby.maxPlayers) {
		let index = openLobbies.indexOf(lobby);
		if(index >= 0) {
			openLobbies.splice(index, 1);
		}
	}

	playerLobbies[user.key] = lobby;

	let buffer = Buffer.alloc(5);
	buffer.writeUInt8(responseCode, 0);
	buffer.writeInt32BE(lobby.id, 1);
	connection.sendBytes(buffer);
}

function createLobby(maxPlayers, id) {
	return {
		maxPlayers: maxPlayers,
		currentPlayers: [],
		lobbyId: id,
		removePlayer: (player) => {
			let index = currentPlayers.indexOf(player);
			if(index < 0) return;
			currentPlayers.splice(index, 1);
		}
	}
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
	let players = lobby.currentPlayers;
	for(let i = 0; i < players.length; i++) {
		let currentPlayer = players[i];
		if(currentPlayer != user) {
			let conn = currentPlayer.getConnection();
			conn.sendBytes(buf, (err) => {
				logger.logInfo(`User disconnected: ${err}`);
				currentPlayer.delete();
				lobby.removePlayer(currentPlayer);
			});
		}
	}
}

module.exports.joinOrCreateLobby = joinOrCreateLobby;
module.exports.listLobbies = listLobbies;
module.exports.syncData = syncData;