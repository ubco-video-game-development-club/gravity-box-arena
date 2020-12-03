const { connection } = require("websocket");
const logger = require("./logger");
const userlist = require("./userlist");

const packetTypes = {
	REQUEST_AUTH: 1,
};

const disconnectReasons = {
	BAD_PACKET: 1,
};

var users = [];

function handleMessage(message, connection) {
	switch(message.type) {
		case "utf8":
			logger.logMessage(`Received string from client: ${message.utf8Data}`);
			break;
		case "binary":
			if(!parsePacket(new Uint32Array(message.binaryData), connection)) {
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
1-: data
 */
function parsePacket(packet, connection) {
	let packetType = packet[0];
	switch(packetType) {
		case packetTypes.REQUEST_AUTH:
			authorize(connection);
			break;
		default:
			logger.logWarning(`Invalid packet type: ${packetType}`);
			return false;
	}

	return true;
}

function authorize(connection) {
	let key = userlist.createKey();
	connection.sendUTF(key);
}

module.exports.handleMessage = handleMessage;