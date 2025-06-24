'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import c from './LoginForm.module.scss'

interface Props {
	setForm: (form: 'login' | 'register') => void
}

export default function LoginForm({ setForm }: Props) {
	const [username, setUsername] = useState('')
	const [password, setPassword] = useState('')
	const router = useRouter()

	const handleLogin = async () => {
		const res = await fetch('/auth/login', {
			method: 'POST',
			headers: { 'Content-Type': 'application/json' },
			credentials: 'include',
			body: JSON.stringify({ username, password }),
		})
		const data = await res.json()
		if (data.success) {
			localStorage.setItem('username', username)
			router.push('/dashboard')
		} else alert('Login failed')
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
			<button className={c.button} onClick={handleLogin}>
				Login
			</button>

			<p className={c.switch}>
				Don't have an account?{' '}
				<span className={c.link} onClick={() => setForm('register')}>
					Register
				</span>
			</p>
		</div>
	)
}
