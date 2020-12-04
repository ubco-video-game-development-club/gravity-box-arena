var userList = [];

class User {
	constructor(connection) {
		this.connection = connection;
		this.key = generateSecretKey();
		userList.push(this);
		userList[this.key] = this;
	}

	getConnection() {
		return this.connection;
	}

	getKey() {
		return this.key;
	}

	delete() {
		userList[this.key] = undefined;
		let index = userList.indexOf(this);
		if(index >= 0) {
			userList.splice(index, 1);
		}
	}
}

function generateSecretKey() {
	let index = users.length & 11111; //Take the last 5 bits of the index
	let now = Date.now() & 0xfffff; //Take the last 20 bits of the current time
	let key = index.toString(36) + now.toString(36).padStart(4, "0");
	return key;
}

function getUser(key) {
	return userList[key];
}

module.exports.User = User;
module.exports.getUser = getUser;