const logErrors = 1;
const logWarnings = 1 << 1;
const logInfo = 1 << 2;

var logLevel = logErrors | logWarnings | logInfo;

function logError(error) {
	if(checkLogLevel(logErrors)) {
		console.log(`\x1b[31m[ERROR] ${(new Date())}: ${error}\x1b[0m`);
	}
}

function logWarning(warning) {
	if(checkLogLevel(logWarnings)) {
		console.log(`\x1b[33m[WARN]\x1b[0m ${(new Date())}: ${warning}`);
	}
}

function logMessage(message) {
	if(checkLogLevel(logInfo)) {
		console.log(`[INFO] ${(new Date())}: ${message}`);
	}
}

function checkLogLevel(level) {
	return (level & logLevel) == level;
}

module.exports.logErrors = logErrors;
module.exports.logWarnings = logWarnings;
module.exports.logInfo = logInfo;

module.exports.logError = logError;
module.exports.logWarning = logWarning;
module.exports.logMessage = logMessage;

module.exports.setLogLevel = (level) => {
	logLevel = level;
}