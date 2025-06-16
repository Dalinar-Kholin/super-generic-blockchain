const BASE_URL = 'http://127.0.0.1:8071'

// -------------------------
// AUTH CHECK
// -------------------------
export const isAuth = async (): Promise<boolean> => {
	try {
		const r = await fetch(BASE_URL + '/api/getMessages', {
			credentials: 'include',
		})
		const d = await r.json()
		return d.success === true
	} catch (err) {
		return false
	}
}

// -------------------------
// USERNAME
// -------------------------
export const getUsername = (): string => {
	return localStorage.getItem('username') ?? 'guest'
}

// -------------------------
// PUBLIC / ANON ROUTES
// -------------------------
export const getPublicMessages = async () => {
	try {
		const res = await fetch(`${BASE_URL}/anon/getMessages`)
		const json = await res.json()
		console.log('public', json)
		return json
	} catch (err) {
		console.error('❌ getPublicMessages error:', err)
		throw err
	}
}

// -------------------------
// AUTH ROUTES
// -------------------------
export const getPrivateMessages = async () => {
	const res = await fetch(`${BASE_URL}/api/getMessages`, {
		credentials: 'include',
	})
	const json = await res.json()
	console.log('private', json)
	return json
}

export const sendMessage = async ({
	to,
	message,
	shouldBeEncrypted = false,
}: {
	to: string
	message: string
	shouldBeEncrypted: boolean
}) => {
	const res = await fetch(`${BASE_URL}/api/addRecord`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include',
		body: JSON.stringify({ to, message, shouldBeEncrypted }),
	})
	console.log('📤 Send message status:', res.status)
	return res.json()
}

// -------------------------
// AUTH SYSTEM
// -------------------------
export const login = async ({
	username,
	password,
}: {
	username: string
	password: string
}) => {
	const res = await fetch(`${BASE_URL}/auth/login`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include',
		body: JSON.stringify({ username, password }),
	})
	console.log('🔁 Login response status:', res.status)
	const json = await res.json()
	console.log('✅ Login response:', json)
	return json
}

export const register = async ({
	username,
	password,
	privateKey,
	publicKey,
}: {
	username: string
	password: string
	privateKey: string
	publicKey: string
}) => {
	const res = await fetch(`${BASE_URL}/auth/register`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		body: JSON.stringify({ username, password, privateKey, publicKey }),
	})
	console.log('📝 Register response status:', res.status)
	return res.json()
}

// -------------------------
// SUPERVISOR ROUTES
// -------------------------
export const getStats = async () => {
	const res = await fetch(`${BASE_URL}/supervisor/getStats`)
	return res.json()
}

export const getFriendIp = async () => {
	const res = await fetch(`${BASE_URL}/supervisor/getFriendIp`)
	return res.json()
}

export const addNewNode = async ({
	ip,
	port,
}: {
	ip: string
	port: number
}) => {
	const url = new URL(`${BASE_URL}/supervisor/addNewNode`)
	url.searchParams.append('ip', ip)
	url.searchParams.append('port', port.toString())

	const res = await fetch(url.toString())
	return res.json()
}
