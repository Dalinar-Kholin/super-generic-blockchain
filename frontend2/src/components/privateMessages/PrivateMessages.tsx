'use client'
import c from './PrivateMessages.module.scss'

export default function PrivateMessages() {
	const msgs = [
		{ id: 1, from: '0xFriend', text: '🤫 secret msg' },
		{ id: 2, from: '0xSelf', text: 'my private note' },
	]

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
