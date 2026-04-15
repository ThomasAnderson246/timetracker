// stores token
let token = sessionStorage.getItem('token');

export const setToken = (t) => {
    token = t;
    sessionStorage.setItem('token', t);
}

export const getToken = () => token;

export const clearToken = () => {
    token = null;
    sessionStorage.removeItem('token');
}

export async function apiFetch(path, options = {}){
    console.log('apiFetch called, token:', token);
    const res = await fetch(`${import.meta.env.VITE_API_URL}${path}`, {
        ...options, 
        headers: {
            'Content-Type': 'application/json',
            'Authorization': `Bearer ${token}`,
            ...options.headers
        }
    });
    if (!res.ok) throw new Error(`API error: ${res.status}`);
    if (res.status === 204) return null; //no content was returned
    return res.json();
}