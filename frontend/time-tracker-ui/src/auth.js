// stores token
let token = null;

export const setToken = (t) => {token = t;}
export const getToken = () => token;
export const clearToken = () => {token = null };

export async function apiFetch(path, options = {}){
    const res = await fetch(`${import.meta.env.VITE_API_URL}${path}`, {
        ...options, 
        headers: {
            'Content-Type': 'application/json',
            'Authorization': 'Bearer ${token}',
            ...options.headers
        }
    });
    if (!res.ok) throw new Error(`API error: ${res.status}`);
    if (res.status === 204) return null; //no content was returned
    return res.json();
}