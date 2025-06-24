'use client'

import { useState } from 'react'
import c from './RegisterForm.module.scss'
import { register } from '@/utils/functions'

interface Props {
	setForm: (form: 'login' | 'register') => void
}

export default function RegisterForm({ setForm }: Props) {
	const [username, setUsername] = useState('')
	const [password, setPassword] = useState('')
	const [privateKey, setPrivateKey] = useState('')
	const [publicKey, setPublicKey] = useState('')

	const handleRegister = async () => {
		try {
			const data = await register({
				username,
				password,
				privateKey,
				publicKey,
			})
			if (data.success) {
				alert('Registered successfully!')
				setForm('login')
			} else {
				alert('Registration failed: ' + data.result)
			}
		} catch (err) {
			console.error('Register error:', err)
			alert('An error occurred during registration')
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
