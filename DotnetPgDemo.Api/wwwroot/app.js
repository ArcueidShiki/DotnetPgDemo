const API_BASE = 'https://localhost:7203/api';

let currentUser = null;
let users = [];
let orders = [];

// Initialize
document.addEventListener('DOMContentLoaded', async () => {
    await loadUsers();
    await loadOrders();
    
    document.getElementById('userSelect').addEventListener('change', (e) => {
        const userId = parseInt(e.target.value);
        currentUser = users.find(u => u.id === userId);
        updateUserInfo();
        renderOrders();
    });
});

async function loadUsers() {
    try {
        const response = await fetch(`${API_BASE}/users`);
        users = await response.json();
        
        const select = document.getElementById('userSelect');
        select.innerHTML = '';
        
        users.forEach(user => {
            const option = document.createElement('option');
            option.value = user.id;
            option.textContent = `${user.username} (${user.role}${user.role === 1 ? ` - Lvl ${user.adminLevel}` : ''})`;
            select.appendChild(option);
        });

        // Select first user by default
        if (users.length > 0) {
            currentUser = users[0];
            updateUserInfo();
        }
    } catch (error) {
        console.error('Error loading users:', error);
    }
}

function updateUserInfo() {
    const infoSpan = document.getElementById('userInfo');
    if (currentUser) {
        infoSpan.textContent = currentUser.role === 1 
            ? `Admin Level ${currentUser.adminLevel}` 
            : 'Standard User';
    }
}

async function loadOrders() {
    try {
        const response = await fetch(`${API_BASE}/orders`);
        orders = await response.json();
        renderOrders();
    } catch (error) {
        console.error('Error loading orders:', error);
        document.getElementById('ordersList').innerHTML = '<p>Error loading orders.</p>';
    }
}

function renderOrders() {
    const container = document.getElementById('ordersList');
    container.innerHTML = '';

    if (orders.length === 0) {
        container.innerHTML = '<p>No orders found.</p>';
        return;
    }

    orders.forEach(order => {
        const card = createOrderCard(order);
        container.appendChild(card);
    });
}

function createOrderCard(order) {
    const div = document.createElement('div');
    div.className = 'order-card';
    
    const statusClass = getStatusClass(order.status);
    const statusText = getStatusText(order.status);

    // Determine if current user can approve
    const canApprove = canUserApprove(order);

    div.innerHTML = `
        <div class="order-header">
            <span class="order-number">#${order.orderNumber}</span>
            <span class="order-amount">$${order.amount.toFixed(2)}</span>
        </div>
        <div class="order-details">
            <p><strong>Created By:</strong> ${order.createdByUsername}</p>
            <p><strong>Date:</strong> ${new Date(order.createdAt).toLocaleDateString()}</p>
            <p><strong>Description:</strong> ${order.description}</p>
            <p><strong>Status:</strong> <span class="status-badge ${statusClass}">${statusText}</span></p>
            <p><strong>Approval Status:</strong> ${order.approvalStatusMessage}</p>
        </div>
        
        <div class="approval-section">
            <div class="approval-title">Approval Progress</div>
            <div class="approval-steps">
                ${renderApprovalSteps(order)}
            </div>
        </div>

        ${canApprove ? `
            <div class="actions">
                <button onclick="handleApproval(${order.id}, 2)" class="btn-reject">Reject</button>
                <button onclick="handleApproval(${order.id}, 1)" class="btn-approve">Approve</button>
            </div>
        ` : ''}
    `;

    return div;
}

function getStatusClass(status) {
    switch(status) {
        case 0: return 'status-pending';
        case 1: return 'status-approved';
        case 2: return 'status-rejected';
        case 3: return 'status-cancelled';
        default: return '';
    }
}

function getStatusText(status) {
    switch(status) {
        case 0: return 'Pending';
        case 1: return 'Approved';
        case 2: return 'Rejected';
        case 3: return 'Cancelled';
        default: return 'Unknown';
    }
}

function renderApprovalSteps(order) {
    // Combine required levels and existing approvals to show status
    // This is a simplified view
    let html = '';
    
    // Show existing approvals
    order.approvals.forEach(approval => {
        const decisionClass = approval.decision === 1 ? 'step-approved' : (approval.decision === 2 ? 'step-rejected' : 'step-pending');
        const decisionText = approval.decision === 1 ? 'Approved' : (approval.decision === 2 ? 'Rejected' : 'Pending');
        
        html += `
            <div class="approval-step ${decisionClass}">
                <span>Level ${approval.adminLevel}</span>
                <span>${decisionText} by ${approval.approvedByUsername}</span>
            </div>
        `;
    });

    // Show pending required levels that haven't been created/approved yet
    // Note: The API returns 'approvals' which includes pending ones created at start?
    // Let's check the API logic. It creates approval records for all required levels at start.
    // So iterating order.approvals should be enough if they are sorted.
    
    return html;
}

function canUserApprove(order) {
    if (!currentUser || currentUser.role !== 1) return false; // Not admin
    if (order.status !== 0) return false; // Not pending

    // Find the pending approval for this user's level
    const pendingApproval = order.approvals.find(a => 
        a.adminLevel === currentUser.adminLevel && 
        a.decision === 0 // Pending
    );

    // Also need to check if lower levels are approved?
    // The API logic seems to enforce sequential approval via 'ApprovalStatus' on the order
    // e.g. AwaitingLevel1, AwaitingLevel2.
    
    // Let's check order.approvalStatus
    // ApprovalStatus enum: Pending=0, AwaitingLevel1=1, AwaitingLevel2=2, AwaitingLevel3=3...
    
    // If order is AwaitingLevelX, and user is LevelX, they can approve.
    
    // Map ApprovalStatus to Level
    let awaitingLevel = 0;
    if (order.approvalStatus === 1) awaitingLevel = 1;
    else if (order.approvalStatus === 2) awaitingLevel = 2;
    else if (order.approvalStatus === 3) awaitingLevel = 3;
    
    // If specific awaiting status
    if (awaitingLevel > 0) {
        return currentUser.adminLevel === awaitingLevel;
    }
    
    // Fallback: check if there is a pending approval for this level and it's the "next" one
    // But relying on order.approvalStatus is safer as per backend logic.
    
    return false;
}

async function handleApproval(orderId, decision) {
    if (!currentUser) return;

    const comment = prompt(decision === 1 ? "Enter approval comments (optional):" : "Enter rejection reason:");
    if (decision === 2 && !comment) {
        alert("Comment is required for rejection.");
        return;
    }

    try {
        const response = await fetch(`${API_BASE}/orders/${orderId}/approve`, {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json'
            },
            body: JSON.stringify({
                orderId: orderId,
                approvedByUserId: currentUser.id,
                decision: decision, // 1=Approved, 2=Rejected
                comments: comment || ''
            })
        });

        if (response.ok) {
            await loadOrders(); // Refresh list
        } else {
            const errorText = await response.text();
            alert(`Error: ${errorText}`);
        }
    } catch (error) {
        console.error('Error submitting approval:', error);
        alert('Failed to submit approval.');
    }
}
