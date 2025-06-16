'use client'

import { useEffect, useState } from 'react'
import { getPrivateMessages } from '@/utils/functions'
import c from './PrivateMessages.module.scss'

interface Message {
	from: string
	to: string
	message: string
}

export default function PrivateMessages() {
	const [msgs, setMsgs] = useState<Message[]>([])

	useEffect(() => {
		const fetchMessages = async () => {
			try {
				const data = await getPrivateMessages()
				setMsgs(data.result ?? [])
			} catch (err) {
				console.error('❌ Failed to fetch private messages:', err)
			}
		}

		fetchMessages()
	}, [])

	return (
		<div className={c.wrapper}>
			<ul className={c.list}>
				{msgs.map((m, i) => (
					<li key={i} className={c.item}>
						<div className={c.meta}>
							<span className={c.from}>{m.from}</span>
							<span className={c.to}>→ {m.to}</span>
						</div>
						<div className={c.text}>{m.message}</div>
					</li>
				))}
			</ul>
		</div>
	)
}
