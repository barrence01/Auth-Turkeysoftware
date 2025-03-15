"use client";
import { logoutUser, getUserInfo, refreshToken } from "../../../utils/auth";
import { useState, useEffect } from "react";
import { useRouter } from 'next/navigation';

export default function Home() {
	const [user, setUser] = useState(null);
	const router = useRouter();
	
	useEffect(() => {
		const getUser = async () => {
			const userDetails = await getUserInfo();
			if (userDetails && userDetails.data) {
				setUser(userDetails.data);
			}
		};
		getUser();
	}, []);

	

	const handleLogout = async () => {
		await logoutUser();
		router.push('../');
	};

	const handleRefresh = async () => {
		await refreshToken();
	}

	const handleGetInfo = async () => {
		await getUserInfo();
	}

	return (
		<div className="min-h-screen bg-gray-100 items-center flex flex-col justify-center">
			<div className="bg-gray-600 p-8 flex flex-col rounded-lg">
			{user ? <h1>Hi, {user.name}</h1> : <h1>Welcome stranger!</h1>}

			<button className="bg-blue-400 p-1 rounded-sm m-1" onClick={handleGetInfo}>Simular Requisição</button>

			<button className="bg-blue-400 p-1 rounded-sm m-1" onClick={handleLogout}>Logout</button>
			
			<button className="bg-blue-400 p-1 rounded-sm m-1" onClick={handleRefresh}>refresh token</button>
			</div>
		</div>
	);
}
