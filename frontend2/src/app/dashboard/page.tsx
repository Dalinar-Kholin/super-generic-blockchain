'use client'

import { useEffect, useState } from 'react'
import { getUsername } from '@/utils/functions'
import PublicMessages from '@/components/publicMessages/PublicMessages'
import PrivateMessages from '@/components/privateMessages/PrivateMessages'
import SendMessageForm from '@/components/sendMessageForm/SendMessageForm'
import c from './DashboardPage.module.scss'

export default function Dashboard() {
	const [username, setUsername] = useState('guest')

	useEffect(() => {
		setUsername(getUsername())
	}, [])

	const isGuest = username === 'guest'

	return (
		<div className={c.page}>
			<div className={c.card}>
				<header className={c.header}>
					hello,&nbsp;<strong>{username}</strong>
				</header>

				<section className={c.section}>
					<h3>🌐 Public messages</h3>
					<PublicMessages />
				</section>

				{!isGuest ? (
					<>
						<section className={c.section}>
							<h3>🔒 Your private messages</h3>
							<PrivateMessages />
						</section>

						<section className={c.section}>
							<h3>✉️ Send a message</h3>
							<SendMessageForm />
						</section>
					</>
				) : (
					<p className={c.note}>
						Log in to view private inbox &amp; send messages
					</p>
				)}
			</div>
		</div>
	)
}
