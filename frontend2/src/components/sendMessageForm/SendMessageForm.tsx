'use client'
import { useState } from 'react'
import c from './SendMessageForm.module.scss'

export default function SendMessageForm() {
	const [to, setTo] = useState('0x0')
	const [msg, setMsg] = useState('')
	const [enc, setEnc] = useState(false)
	const [status, setStatus] = useState<string | null>(null)

	const send = async () => {
		setStatus('sending…')
		try {
			const r = await fetch('http://127.0.0.1:8071/api/addRecord', {
				method: 'POST',
				headers: { 'Content-Type': 'application/json' },
				credentials: 'include',
				body: JSON.stringify({
					to,
					message: msg,
					shouldBeEncrypted: enc,
				}),
			})
			const d = await r.json()
			setStatus(d.success ? 'sent ✅' : 'error: ' + d.result)
			setMsg('')
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
