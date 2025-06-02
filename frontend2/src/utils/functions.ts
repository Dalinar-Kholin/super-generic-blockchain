const BASE_URL = 'http://127.0.0.1:8071'

const logCookies = (label: string) => {
	console.log(`🍪 ${label} — document.cookie:`, document.cookie)
}

// -------------------------
// AUTH CHECK
// -------------------------
export const isAuth = async (): Promise<boolean> => {
	logCookies('isAuth')
	try {
		const r = await fetch(BASE_URL + '/api/getMessages', {
			credentials: 'include',
		})
		console.log('🔐 isAuth response status:', r.status)
		const d = await r.json()
		console.log('🔐 isAuth response json:', d)
		return d.success === true
	} catch (err) {
		console.error('❌ isAuth error:', err)
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
	logCookies('getPublicMessages')
	try {
		const res = await fetch(`${BASE_URL}/anon/getMessages`)
		const json = await res.json()
		console.log('📨 Public messages:', json)
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
	logCookies('getPrivateMessages')
	const res = await fetch(`${BASE_URL}/api/getMessages`, {
		credentials: 'include',
	})
	console.log('📦 Private response status:', res.status)
	const json = await res.json()
	console.log('🔒 Private messages:', json)
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
	logCookies('sendMessage')
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
	logCookies('login (before)')
	const res = await fetch(`${BASE_URL}/auth/login`, {
		method: 'POST',
		headers: { 'Content-Type': 'application/json' },
		credentials: 'include',
		body: JSON.stringify({ username, password }),
	})
	console.log('🔁 Login response status:', res.status)
	const json = await res.json()
	logCookies('login (after)')
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
	logCookies('register')
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
	logCookies('getStats')
	const res = await fetch(`${BASE_URL}/supervisor/getStats`)
	return res.json()
}

export const getFriendIp = async () => {
	logCookies('getFriendIp')
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
	logCookies('addNewNode')
	const url = new URL(`${BASE_URL}/supervisor/addNewNode`)
	url.searchParams.append('ip', ip)
	url.searchParams.append('port', port.toString())

	const res = await fetch(url.toString())
	return res.json()
}
