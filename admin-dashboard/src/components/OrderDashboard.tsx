import React, { useEffect, useState } from 'react';
import axios from 'axios';

const OrderDashboard = () => {
    const [orders, setOrders] = useState([]);

    useEffect(() => {
        const fetchOrders = async () => {
            const res = await axios.get('http://localhost:5001/api/orders');
            setOrders(res.data);
        };
        fetchOrders();
        const interval = setInterval(fetchOrders, 5000); // Poll every 5s
        return () => clearInterval(interval);
    }, []);

    return (
        <div className="p-6">
            <h1 className="text-2xl font-bold mb-4">Admin Order Dashboard</h1>
            <table className="min-w-full border">
                <thead>
                    <tr className="bg-gray-100">
                        <th>Order ID</th>
                        <th>Status</th>
                        <th>Total</th>
                        <th>Actions</th>
                    </tr>
                </thead>
                <tbody>
                    {orders.map(order => (
                        <tr key={order.id} className={order.status === 'Failed' ? 'bg-red-50' : ''}>
                            <td>{order.id}</td>
                            <td><StatusBadge status={order.status} /></td>
                            <td>${order.totalAmount}</td>
                            <td><button onClick={() => viewDetails(order.id)}>Details</button></td>
                        </tr>
                    ))}
                </tbody>
            </table>
        </div>
    );
};