'use client'

import { useState } from 'react'
import { useRouter } from 'next/navigation'
import c from './LoginForm.module.scss'
import { login } from '@/utils/functions'

interface Props {
	setForm: (form: 'login' | 'register') => void
}

export default function LoginForm({ setForm }: Props) {
	const [username, setUsername] = useState('')
	const [password, setPassword] = useState('')
	const router = useRouter()

	const handleLogin = async () => {
		try {
			const data = await login({ username, password })
			if (data.success) {
				localStorage.setItem('username', username)
				router.push('/dashboard')
			} else {
				alert('Login failed')
			}
		} catch (err) {
			console.error('Login error:', err)
			alert('Login error occurred')
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
