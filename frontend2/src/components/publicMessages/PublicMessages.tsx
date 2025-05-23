'use client'
import c from './PublicMessages.module.scss'

export default function PublicMessages() {
	const msgs = [
		{ id: 1, from: '0xAlice', text: 'Hello world!' },
		{ id: 2, from: '0xBob', text: 'Witaj świecie!' },
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
