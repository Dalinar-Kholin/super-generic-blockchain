'use client'

import { useState } from 'react'
import c from './RegisterForm.module.scss'

interface Props {
	setForm: (form: 'login' | 'register') => void
}

export default function RegisterForm({ setForm }: Props) {
	const [username, setUsername] = useState('')
	const [password, setPassword] = useState('')
	const [privateKey, setPrivateKey] = useState('')
	const [publicKey, setPublicKey] = useState('')

	const handleRegister = async () => {
		const res = await fetch('/auth/register', {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			body: JSON.stringify({ username, password, privateKey, publicKey }),
		})

		const data = await res.json()
		if (data.success) {
			alert('Registered successfully!')
			setForm('login')
		} else {
			alert('Registration failed: ' + data.result)
		}
	}

	return (
		<div className={c.form}>
			<input
				placeholder="Username"
				className={c.input}
				value={username}
				onChange={(e) => setUsername(e.target.value)}
			/>
			<input
				placeholder="Password"
				type="password"
				className={c.input}
				value={password}
				onChange={(e) => setPassword(e.target.value)}
			/>
			<textarea
				placeholder="Private key (base64)"
				className={c.textarea}
				value={privateKey}
				onChange={(e) => setPrivateKey(e.target.value)}
				rows={4}
			/>
			<textarea
				placeholder="Public key (base64)"
				className={c.textarea}
				value={publicKey}
				onChange={(e) => setPublicKey(e.target.value)}
				rows={4}
			/>

			<button className={c.button} onClick={handleRegister}>
				Register
			</button>

			<p className={c.switch}>
				Already have an account?{' '}
				<span className={c.link} onClick={() => setForm('login')}>
					Login
				</span>
			</p>
		</div>
	)
}
