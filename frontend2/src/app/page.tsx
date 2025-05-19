'use client'

import { useEffect } from 'react'
import { useRouter } from 'next/navigation'
import c from './page.module.scss'

export default function Home() {
	const router = useRouter()

	useEffect(() => {
		const timeout = setTimeout(() => {
			router.push('/auth')
		}, 3000)
		return () => clearTimeout(timeout)
	}, [router])

	return (
		<div className={c.loader}>
			<div className={c.leftImageWrapper}>
				<img src="/logo_left.png" alt="Left" className={c.image} />
			</div>

			<div className={c.centerLine}></div>

			<div className={c.rightImageWrapper}>
				<img src="/logo_right.png" alt="Right" className={c.image} />
			</div>
		</div>
	)
}
