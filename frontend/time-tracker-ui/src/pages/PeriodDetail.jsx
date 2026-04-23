import { useEffect, useState } from "react";
import { useNavigate, useParams } from "react-router-dom";
import { apiFetch, setToken } from "../auth";

export default function PeriodDetail(){

    const [hoursWorked, setHoursWorked] = useState();
    const [shiftDate, setShiftDate] = useState();
    const [entries, setEntries ] = useState([]);
    const [form, setForm] = useState({date: '', hours: ''});
    const {id} = useParams();
    const navigate = useNavigate();

    useEffect(() => {loadEntries();}, []);

    async function loadEntries(){
        const data = await apiFetch(`/period/${id}/entries`);
        setEntries(data);
    }

    async function addEntry(){
        await apiFetch('/entries', {
            method: 'POST', 
            body: JSON.stringify({
                periodId: id,
                date: form.date,
                hours:parseFloat(form.hours)
            })
        });
        setForm({date:'', hours:''});
        loadEntries();
    }

    async function deleteEntry(entryId){
        await apiFetch(`/entries/${entryId}`, {method: 'DELETE'});
        loadEntries();
    }




    return(
        <div style={{padding:32}}>
            <button onClick={() => navigate('/')}>Back</button>
            <h1>Period Entries</h1>

            <div>
                <input type="date" value={form.date}
                    onChange={e => setForm({...form, date: e.target.value})}/>
                <input type="number" placeholder="Hours Worked" value={form.hours}
                    onChange={e => setForm({...form, hours: e.target.value})}/>
                <button onClick={addEntry}> Add Entry</button>
            </div>

            <table>
                <thead>
                    <tr>
                        <th>Date</th>
                        <th>Hours</th>
                        <th></th>
                    </tr>
                </thead>
                <tbody>
                    {entries.map(e=> (
                        <tr key={e.id}>
                            <td>{e.date}</td>
                            <td>{e.hours}</td>
                            <td><button onClick={() => deleteEntry(e.id)}>Delete</button></td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
}