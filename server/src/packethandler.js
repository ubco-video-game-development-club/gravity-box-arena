const logger = require("./logger");
const userlist = require("./userlist");
const lobbymanager = require("./lobbymanager");
const { StringDecoder } = require("string_decoder");

const stringDecoder = new StringDecoder("utf8");

const packetTypes = {
	REQUEST_AUTH: 1,
	REQUEST_JOIN_OR_CREATE_LOBBY: 2,
};

const disconnectReasons = {
	BAD_PACKET: 1,
};

function handleMessage(message, connection) {
	switch(message.type) {
		case "utf8":
			logger.logMessage(`Received string from client: ${message.utf8Data}`);
			break;
		case "binary":
			if(!parsePacket(new Uint8Array(message.binaryData), connection)) {
				connection.close(disconnectReasons.BAD_PACKET, "Bad packet.");
				logger.logMessage("Disconnected user due to bad packet.");
			}
			break;
		default:
			logger.logWarning(`Invalid message type: ${message.type}`);
			break;
	}
}


/*
PACKET LAYOUT
0: type
1-5: secret key (if applicable)
6-: data
 */
function parsePacket(packet, connection) {
	let packetType = packet[0];
	let key = stringDecoder.write(packet.slice(1, 6));
	let data = packet.slice(6);
	switch(packetType) {
		case packetTypes.REQUEST_AUTH:
			authorize(connection);
			break;
		case packetTypes.REQUEST_JOIN_OR_CREATE_LOBBY:
			lobbymanager.joinOrCreateLobby(connection, key, packetTypes.REQUEST_JOIN_OR_CREATE_LOBBY);
			break;
		default:
			logger.logWarning(`Invalid packet type: ${packetType}`);
			return false;
	}

	return true;
}

function authorize(connection) {
	let key = userlist.createKey();

	let buffer = Buffer.alloc(1 + key.length);
	buffer.writeUInt8(packetTypes.REQUEST_AUTH, 0);
	buffer.write(key, 1);

	console.log(stringDecoder.write(buffer.slice(1,6)));

	connection.sendBytes(buffer);
}

module.exports.handleMessage = handleMessage;