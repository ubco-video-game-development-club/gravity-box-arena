const logger = require("./logger");
const server = require("./server");
const lobbymanager = require("./lobbymanager");

const fs = require("fs");
const readline = require("readline").createInterface({
	input: process.stdin,
	output: process.stdout
});

const commands = {
	"stop": shutdown,
	"list lobbies": lobbymanager.listLobbies
}

const defaultConfig = {
	logLevel: 7,
	port: 8080,
}

let config = loadConfig();
logger.logMessage(`Starting with log level ${config.logLevel}`);
logger.setLogLevel(config.logLevel);

server.start(config.port);
logger.logMessage("To stop the server, type \"stop\"");
readline.question("> ", processCommand);

function processCommand(command) {
	let cmd = command.toLowerCase().trim();
	let func = commands[cmd];
	if(func) {
		func();
	} else {
		logger.logError(`Command ${cmd} not found.`);
	}

	readline.question("> ", processCommand);
}

function loadConfig() {
	var config;
	if(fs.existsSync("config.json")) {
		config = JSON.parse(fs.readFileSync("config.json"));
	} else {
		config = defaultConfig;
		logger.logError("Config file was not found. Please create one.");
	}

	return config;
}

function shutdown() {
	process.exit(0);
}