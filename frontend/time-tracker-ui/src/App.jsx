import { useState } from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { useGoogleLogin } from '@react-oauth/google'
import { setToken, getToken, apiFetch } from './auth'
import Dashboard from './pages/Dashboard'
import PeriodDetail from './pages/PeriodDetail'
import './App.css'

function LoginPage({onLogin}){
  const login = useGoogleLogin({
    onSuccess: async (response) => {
      //takes google token, sets it as my own token
      const data = await apiFetch('/auth/google',{
        method: 'POST',
        body: JSON.stringify({idToken: response.access_token})
      });
      console.log("Login response:", data);
      setToken(data.token);
      console.log("Token stored: ", data.token)
      onLogin();
    },
    flow: 'implicit'
  });
  return(
    <div style={{display: 'flex', flexDirection: 'column', alignItems: 'center', marginTop: 100}}>
      <h1>Time Tracker</h1>
      <button onClick={() => login()}>Sign in with Google OAuth</button>
    </div>
  );
}

export default function App(){
  const [loggedIn, setLoggedIn] = useState(!!getToken());
  return(
    <BrowserRouter>
      <Routes>
        <Route path='/login' element={
          loggedIn ? <Navigate to='/'/> : <LoginPage onLogin={() => setLoggedIn(true)} /> } />
        <Route path='/' element={loggedIn ? <Dashboard/> : <Navigate to='/login'/>}/>
        <Route path='/period/:id' element={loggedIn ? <PeriodDetail/> : <Navigate to='login'/>}/>
      </Routes>
    </BrowserRouter>
  )
}