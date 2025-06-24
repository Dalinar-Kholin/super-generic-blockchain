'use client'
import { useState } from 'react'
import c from './SendMessageForm.module.scss'
import { sendMessage } from '@/utils/functions'

export default function SendMessageForm() {
	const [to, setTo] = useState('0x0')
	const [msg, setMsg] = useState('')
	const [enc, setEnc] = useState(false)
	const [status, setStatus] = useState<string | null>(null)

	const send = async () => {
		setStatus('sending…')
		try {
			const d = await sendMessage({
				to,
				message: msg,
				shouldBeEncrypted: enc,
			})
			setStatus(d.success ? 'sent ✅' : 'error: ' + d.result)
			if (d.success) setMsg('')
		} catch (e) {
			setStatus('network error')
		}
	}

	return (
		<div className={c.form}>
			<input
				value={to}
				onChange={(e) => setTo(e.target.value)}
				className={c.input}
				placeholder="Recipient (address)"
			/>
			<textarea
				value={msg}
				onChange={(e) => setMsg(e.target.value)}
				className={c.textarea}
				rows={3}
				placeholder="Your message"
			/>
			<label className={c.chk}>
				<input
					type="checkbox"
					checked={enc}
					onChange={(e) => setEnc(e.target.checked)}
				/>
				encrypt
			</label>
			<button className={c.button} onClick={send}>
				Send
			</button>
			{status && <p className={c.status}>{status}</p>}
		</div>
	)
}
