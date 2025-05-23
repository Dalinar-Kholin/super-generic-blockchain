'use client'

import { useState } from 'react'
import LoginForm from '@/components/loginForm/LoginForm'
import RegisterForm from '@/components/registerForm/RegisterForm'
import { useRouter } from 'next/navigation'
import c from './AuthPage.module.scss'

export default function AuthPage() {
	const [form, setForm] = useState<'login' | 'register'>('login')
	const router = useRouter()

	const handleGuest = () => {
		localStorage.removeItem('username')
		document.cookie = 'uuid=; Max-Age=0; path=/'
		router.push('/dashboard')
	}

	return (
		<div className={c.page}>
			<main className={c.container}>
				<h1 className={c.title}>Welcome</h1>

				<button className={c.guestButton} onClick={handleGuest}>
					Continue as Guest
				</button>

				<h2 className={c.subtitle}>
					{form === 'login'
						? 'Login to your account'
						: 'Create a new account'}
				</h2>

				{form === 'login' ? (
					<LoginForm setForm={setForm} />
				) : (
					<RegisterForm setForm={setForm} />
				)}
			</main>
		</div>
	)
}
