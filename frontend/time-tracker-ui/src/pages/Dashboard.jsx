import { useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { apiFetch } from "../auth";

export default function Dashboard(){

    const [periods, setPeriods] = useState([]);
    const [showForm, setShowForm] = useState(false);
    const [form, setForm] = useState({label: '', startDate:'', endDate:''});
    const navigate = useNavigate();

    useEffect(() => { loadPeriods();},[]);

    async function loadPeriods() {
        const data = await apiFetch('/period');
        setPeriods(data);
    }

    async function createPeriod(){
        await apiFetch('/period',{ method: 'POST', body: JSON.stringify(form)});
        setShowForm(false);
        setForm({label: '', startDate:'', endDate: ''});
        loadPeriods();
    }

    async function deletePeriod(id) {
        await apiFetch(`/period/${id}`, {method: 'DELETE'});
        loadPeriods();
    }

    return(
        <div style={{padding:32}}>
        <h1>My Periods</h1>
        <button onClick={ () => setShowForm(true)}>+ New Period</button>
        {showForm && (
            <div>
                <input placeholder='Label' value={form.label}
                    onChange={e => setForm({...form, label: e.target.value})}/>
                <input type='date' value={form.startDate}
                    onChange={e => setForm({...form, startDate: e.target.value})}/>
                <input type='date' value={form.endDate}
                    onChange={e => setForm({...form, endDate: e.target.value})}/>
                <button onClick={createPeriod}>Save</button>
                <button onClick={() => setShowForm(false)}>Cancel</button>
            </div>
        )}

        <table>
            <thead>
                <tr>
                    <th>Label</th><th>Start</th><th>Total Hours</th><th></th>
                </tr>
            </thead>
            <tbody>
                {periods.map(p=> (
                    <tr key={p.id}>
                        <td><button onClick={() =>
                        navigate(`/period/${p.id}`)}>{p.label}</button></td>
                        <td>{p.startDate}</td>
                        <td>{p.endDate}</td>
                        <td>{p.totalHours}</td>
                        <td><button onClick={() => deletePeriod(p.id)}>Delete</button></td>
                    </tr>
                ))}
            </tbody>
        </table>
        </div>
    );
}