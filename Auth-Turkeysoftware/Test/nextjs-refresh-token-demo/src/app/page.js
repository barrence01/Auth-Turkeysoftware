"use client";
import React, { useState } from "react";
import { loginUser } from "../../utils/auth";
import { useRouter } from 'next/navigation';

export default function LoginPage() {
	const [password, setPassword] = useState("");
	const [email, setEmail] = useState("");
	const [error, setError] = useState("");
	const router = useRouter();

	const handleSubmit = async (e) => {
		e.preventDefault();
		setError(""); 
		if (password === "" || email === "") {
			setError("Please fill in both email and password.");
			return;
		}
		try {
			await loginUser(email, password);
			router.push('/panel');
		} catch (e) {
			console.error(e);
			setError("Login failed. Please check your credentials.");
		}
	};

	const handleRegister = async (e) => {
		e.preventDefault();
		router.push('/register');
	};

	return (
		<div className="min-h-screen bg-gray-100 items-center flex flex-col justify-center">
			<form
				onSubmit={handleSubmit}
				className="bg-gray-600 p-8 flex flex-col rounded-lg"
			>
				<label>Email</label>
				<input
					className="text-gray-600"
					type="email"
					value={email}
					required
					onChange={(e) => setEmail(e.target.value)}
				/>
				<br />

				<label>Password</label>
				<input
					className="text-gray-600"
					type="password"
					value={password}
					required
					onChange={(e) => setPassword(e.target.value)}
				/>
				<br />
				
				{error && <p className="text-red-500 text-sm">{error}</p>} 

				<button
					className="bg-blue-400 p-1 rounded-sm"
					type="submit"
				>
					Login
				</button>
			</form>

			<button
				className="bg-blue-400 p-1 rounded-sm m-1"
				onClick={handleRegister}
			>
				Register
			</button>
		</div>
	);
}
