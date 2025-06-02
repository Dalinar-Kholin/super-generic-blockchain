'use client'
import { useEffect, useState } from 'react'
import { getPublicMessages } from '@/utils/functions'
import c from './PublicMessages.module.scss'

interface Message {
	id: number
	from: string
	text: string
}

export default function PublicMessages() {
	const [msgs, setMsgs] = useState<Message[]>([])

	useEffect(() => {
		const fetchMessages = async () => {
			try {
				const data = await getPublicMessages()
				setMsgs(data.messages ?? [])
			} catch (err) {
				console.error('❌ Failed to fetch public messages:', err)
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
