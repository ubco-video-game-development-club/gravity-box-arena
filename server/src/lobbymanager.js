const userlist = require("./userlist");
const logger = require("./logger");

var openLobbies = []; //Lobbies that can be joined
var playerLobbies = { }; //Map of player ID to the lobby they're in

function joinOrCreateLobby(connection, key, responseCode) {
	if(!userlist.isUserAuthorized(key)) {
		logger.logWarning("Unauthorized user tried to join or create a lobby.");
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

	playerLobbies[key] = lobby;

	let buffer = Buffer.alloc(5);
	buffer.writeUInt8(responseCode, 0);
	buffer.writeInt32BE(lobby.id, 1);
	connection.sendBytes(buffer);
}

function createLobby(maxPlayers, id) {
	return {
		maxPlayers: maxPlayers,
		currentPlayers: [],
		lobbyId: id
	}
}

function listLobbies() {
	console.log("Lobbies:");
	console.log(JSON.stringify(playerLobbies, null, "\t"));
}

module.exports.joinOrCreateLobby = joinOrCreateLobby;
module.exports.listLobbies = listLobbies;