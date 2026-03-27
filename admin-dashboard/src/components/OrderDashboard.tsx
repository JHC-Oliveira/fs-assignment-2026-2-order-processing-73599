import React, { useEffect, useState } from 'react';
import axios from 'axios';

interface Order {
    id: string;
    customerId: string;
    status: string;
    totalAmount: number;
    createdAt: string;
}

const statusColors: Record<string, { bg: string; color: string }> = {
    Submitted: { bg: '#dbeafe', color: '#1e40af' },
    InventoryPending: { bg: '#fef9c3', color: '#854d0e' },
    PaymentPending: { bg: '#ffedd5', color: '#9a3412' },
    ShippingPending: { bg: '#ede9fe', color: '#5b21b6' },
    Completed: { bg: '#dcfce7', color: '#166534' },
    Failed: { bg: '#fee2e2', color: '#991b1b' },
};

const StatusBadge = ({ status }: { status: string }) => {
    const style = statusColors[status] ?? { bg: '#e5e7eb', color: '#374151' };
    return (
        <span style={{
            backgroundColor: style.bg,
            color: style.color,
            padding: '3px 10px',
            borderRadius: '12px',
            fontSize: '12px',
            fontWeight: 600,
        }}>{status}</span>
    );
};

const OrderDashboard = () => {
    const [orders, setOrders] = useState<Order[]>([]);
    const [selected, setSelected] = useState<Order | null>(null);
    const [lastUpdated, setLastUpdated] = useState<string>('');
    const [filter, setFilter] = useState<'all' | 'completed' | 'failed'>('all');

    useEffect(() => {
        const fetchOrders = async () => {
            const res = await axios.get('http://localhost:5000/api/orders');
            setOrders(res.data);
            setLastUpdated(new Date().toLocaleTimeString());
        };
        fetchOrders();
        const interval = setInterval(fetchOrders, 5000);
        return () => clearInterval(interval);
    }, []);

    const completed = orders.filter(o => o.status === 'Completed').length;
    const failed = orders.filter(o => o.status === 'Failed').length;

    const filteredOrders = orders.filter(function(o) {
            if (filter === 'completed') return o.status === 'Completed';
            if (filter === 'failed') return o.status === 'Failed';
            return true;
        });

    const cards = [
        { key: 'all', label: 'Total Orders', value: orders.length, bg: '#fff', color: '#111827', border: '#3b82f6' },
        { key: 'completed', label: 'Completed', value: completed, bg: '#dcfce7', color: '#166534', border: '#16a34a' },
        { key: 'failed', label: 'Failed', value: failed, bg: '#fee2e2', color: '#991b1b', border: '#dc2626' },
    ];

    return (
        <div style={{ minHeight: '100vh', backgroundColor: '#f0f2f5', padding: '32px' }}>
            {/* Header */}
            <div style={{ marginBottom: '24px' }}>
                <h1 style={{ fontSize: '24px', fontWeight: 700, color: '#111827', margin: 0 }}>
                    📦 Order Management Dashboard
                </h1>
                <p style={{ color: '#6b7280', marginTop: '4px', fontSize: '13px' }}>
                    Auto-refreshes every 5s · Last updated: {lastUpdated}
                </p>
            </div>

            {/* Stats Cards */}
            <div style={{ display: 'grid', gridTemplateColumns: 'repeat(3, 1fr)', gap: '16px', marginBottom: '24px' }}>
                {cards.map(card => (
                    <div key={card.key} onClick={() => setFilter(card.key as any)} style={{
                        backgroundColor: card.bg,
                        borderRadius: '12px',
                        padding: '20px 24px',
                        boxShadow: filter === card.key
                            ? `0 0 0 3px ${card.border}`
                            : '0 1px 3px rgba(0,0,0,0.08)',
                        cursor: 'pointer',
                        transition: 'box-shadow 0.2s',
                    }}>
                        <p style={{ margin: 0, fontSize: '13px', color: '#6b7280' }}>{card.label}</p>
                        <p style={{ margin: '4px 0 0', fontSize: '32px', fontWeight: 700, color: card.color }}>{card.value}</p>
                        {filter === card.key && (
                            <p style={{ margin: '6px 0 0', fontSize: '11px', color: card.border, fontWeight: 600 }}>● Active filter</p>
                        )}
                    </div>
                ))}
            </div>

            {/* Filter label */}
            <div style={{ marginBottom: '12px', fontSize: '13px', color: '#6b7280' }}>
                Showing <strong>{filteredOrders.length}</strong> {filter === 'all' ? 'total' : filter} order{filteredOrders.length !== 1 ? 's' : ''}
                {filter !== 'all' && (
                    <button onClick={() => setFilter('all')} style={{
                        marginLeft: '10px', background: 'none', border: 'none',
                        color: '#3b82f6', cursor: 'pointer', fontSize: '13px', textDecoration: 'underline'
                    }}>Clear filter</button>
                )}
            </div>

            {/* Table */}
            <div style={{ backgroundColor: '#fff', borderRadius: '12px', boxShadow: '0 1px 3px rgba(0,0,0,0.08)', overflow: 'hidden' }}>
                <table style={{ width: '100%', borderCollapse: 'collapse' }}>
                    <thead>
                        <tr style={{ backgroundColor: '#f9fafb', borderBottom: '1px solid #e5e7eb' }}>
                            {['Order ID', 'Status', 'Total', 'Created At', 'Actions'].map(h => (
                                <th key={h} style={{ padding: '12px 16px', textAlign: 'left', fontSize: '12px', fontWeight: 600, color: '#6b7280', textTransform: 'uppercase' }}>{h}</th>
                            ))}
                        </tr>
                    </thead>
                    <tbody>
                        {filteredOrders.map((order, i) => (
                            <tr key={order.id} style={{
                                borderBottom: '1px solid #f3f4f6',
                                backgroundColor: order.status === 'Failed' ? '#fff5f5' : i % 2 === 0 ? '#fff' : '#fafafa',
                            }}>
                                <td style={{ padding: '12px 16px', fontSize: '12px', color: '#6b7280', fontFamily: 'monospace' }}>
                                    {order.id.substring(0, 8)}...
                                </td>
                                <td style={{ padding: '12px 16px' }}><StatusBadge status={order.status} /></td>
                                <td style={{ padding: '12px 16px', fontWeight: 600, color: '#111827' }}>${order.totalAmount.toFixed(2)}</td>
                                <td style={{ padding: '12px 16px', fontSize: '13px', color: '#6b7280' }}>
                                    {new Date(order.createdAt).toLocaleString()}
                                </td>
                                <td style={{ padding: '12px 16px' }}>
                                    <button onClick={() => setSelected(order)} style={{
                                        backgroundColor: '#3b82f6', color: '#fff', border: 'none',
                                        borderRadius: '6px', padding: '5px 12px', fontSize: '12px', cursor: 'pointer',
                                    }}>Details</button>
                                </td>
                            </tr>
                        ))}
                    </tbody>
                </table>
            </div>

            {/* Detail Modal */}
            {selected && (
                <div style={{
                    position: 'fixed', inset: 0, backgroundColor: 'rgba(0,0,0,0.4)',
                    display: 'flex', alignItems: 'center', justifyContent: 'center', zIndex: 50,
                }}>
                    <div style={{ backgroundColor: '#fff', borderRadius: '12px', padding: '28px', minWidth: '400px', boxShadow: '0 10px 40px rgba(0,0,0,0.2)' }}>
                        <h2 style={{ margin: '0 0 16px', fontSize: '18px', fontWeight: 700 }}>Order Details</h2>
                        <p><strong>ID:</strong> {selected.id}</p>
                        <p><strong>Customer ID:</strong> {selected.customerId}</p>
                        <p><strong>Status:</strong> <StatusBadge status={selected.status} /></p>
                        <p><strong>Total:</strong> ${selected.totalAmount.toFixed(2)}</p>
                        <p><strong>Created:</strong> {new Date(selected.createdAt).toLocaleString()}</p>
                        <button onClick={() => setSelected(null)} style={{
                            marginTop: '16px', backgroundColor: '#6b7280', color: '#fff',
                            border: 'none', borderRadius: '6px', padding: '8px 16px', cursor: 'pointer',
                        }}>Close</button>
                    </div>
                </div>
            )}
        </div>
    );
};

export default OrderDashboard;
