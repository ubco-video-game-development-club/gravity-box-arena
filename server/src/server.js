const logger = require("./logger");
const WebSocketServer = require("websocket").server;
const http = require("http");

var httpServer;
var wsServer;

function start(port) {
	httpServer = http.createServer((request, response) => {
		//Inform whoever is connecting that there are no pages
		logger.logMessage(`Received request for ${request.url}`);
		response.writeHead(404);
		response.end();
	});

	httpServer.listen(port, () => {
		logger.logMessage(`Server started on port ${port}.`);
	});

	wsServer = new WebSocketServer({
		httpServer: httpServer,
		//Always set autoAcceptConnections to false so that every connection can be verified
		autoAcceptConnections: false
	});

	wsServer.on("request", (request) => {
		if(!isOriginOK(request.origin)) {
			request.reject();
			logger.logMessage(`Request from ${origin} was rejected.`);
			return;
		}

		let connection = request.accept(null, request.origin);
		logger.logMessage(`Request accepted from ${request.origin}`);
		connection.on("message", (message) => {
			handleMessage(message, connection);
		});

		connection.on("close", (reason, description) => {
			logger.logMessage(`${connection.remoteAddress} disconnected. Reason: ${description} (${reason})`);
		});
	});
}

function handleMessage(message, connection) {
	//TODO: Actually process message
	switch(message.type) {
		case "utf8":
			connection.sendUTF(message.utf8Data);
			break;
		case "binary":
			connection.sendBytes(message.binaryData);
			break;
		default:
			logger.logError(`Invalid message type: ${message.type}`);
			break;
	}
}

function isOriginOK(origin) {
	//TODO: Check origin
	logger.logError("isOriginOK not implemented!");
	return true;
}

module.exports.start = start;