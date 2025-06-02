'use client'
import { useEffect, useState } from 'react'
import { getPrivateMessages } from '@/utils/functions'
import c from './PrivateMessages.module.scss'

interface Message {
	id: number
	from: string
	text: string
}

export default function PrivateMessages() {
	const [msgs, setMsgs] = useState<Message[]>([])

	useEffect(() => {
		const fetchMessages = async () => {
			try {
				const data = await getPrivateMessages()
				setMsgs(data.messages ?? [])
			} catch (err) {
				console.error('❌ Failed to fetch private messages:', err)
			}
		}

		fetchMessages()
	}, [])

	return (
		<ul className={c.list}>
			{msgs.map((m) => (
				<li key={m.id} className={c.item}>
					<span className={c.from}>{m.from}</span>
					<span className={c.text}>{m.text}</span>
				</li>
			))}
		</ul>
	)
}
