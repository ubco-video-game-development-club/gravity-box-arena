const logger = require("./logger");

var users = []

function createKey() {
	let key = generateSecretKey();
	
	//This should almost never happen
	while(users.includes(key)) {
		logger.logWarning("Created duplicate key!");
		key = generateSecretKey();
	}

	users.push(key);
	return key;
}

function isUserAuthorized(key) {
	return users.includes(key);
}

function removeKey(key) {
	let index = users.indexOf(key);
	if(index < 0) return;
	users.splice(index, 1);
}

function generateSecretKey() {
	let index = users.length & 11111; //Take the last 5 bits of the index
	let now = Date.now() & 0xfffff; //Take the last 20 bits of the current time
	let key = index.toString(36) + now.toString(36).padStart(5, "0");
	console.log(`Generating secret key. Index: ${index}; Time: ${now}; Key: ${key}`);
	return key;
}

module.exports.createKey = createKey;
module.exports.removeKey = removeKey;
module.exports.isUserAuthorized = isUserAuthorized;