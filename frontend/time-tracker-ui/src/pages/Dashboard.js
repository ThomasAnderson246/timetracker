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
        const data = await apiFetch('/periods');
        setPeriods(data);
    }

    async function createPeriod(){
        await apiFetch('/periods',{ method: 'POST', body: JSON.stringify(form)});
        setShowForm(false);
        setForm({label: '', startDate:'', endDate: ''});
        loadPeriods();
    }

    async function deletePeriod(id) {
        await apiFetch(`/periods/${id}`, {method: 'DELETE'});
        loadPeriods();
    }

    return("");
}