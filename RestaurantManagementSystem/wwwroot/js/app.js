const app = (() => {
    // API endpoint prefix
    const API_BASE = '/api';

    // Global application state
    const state = {
        tables: [],
        menu: [],
        inventory: [],
        orders: [],
        reservations: [],
        dashboard: {},
        cart: [], // items in POS cart: { menuItemId, name, price, quantity, notes }
        currentView: 'dashboard'
    };

    // Initialize application
    async function init() {
        setupNavigation();
        setupEventListeners();
        await syncData();
        
        // Refresh dashboard every 30 seconds silently
        setInterval(() => {
            if (state.currentView === 'dashboard') {
                syncDashboard();
            }
        }, 30000);
    }

    // Navigation logic between SPA views
    function setupNavigation() {
        document.querySelectorAll('.nav-link').forEach(link => {
            link.addEventListener('click', (e) => {
                e.preventDefault();
                const view = link.getAttribute('data-view');
                switchView(view);
            });
        });
    }

    function switchView(viewName) {
        state.currentView = viewName;

        // Toggle active navigation link
        document.querySelectorAll('.nav-link').forEach(link => {
            if (link.getAttribute('data-view') === viewName) {
                link.classList.add('active');
            } else {
                link.classList.remove('active');
            }
        });

        // Toggle active section view
        document.querySelectorAll('.view-section').forEach(section => {
            if (section.id === `view-${viewName}`) {
                section.classList.add('active');
            } else {
                section.classList.remove('active');
            }
        });

        // Update header texts
        const titleEl = document.getElementById('header-view-title');
        const subtitleEl = document.getElementById('header-view-subtitle');

        switch (viewName) {
            case 'dashboard':
                titleEl.textContent = 'Dashboard Summary';
                subtitleEl.textContent = 'Overview of restaurant performance and active tasks';
                syncDashboard();
                break;
            case 'orders':
                titleEl.textContent = 'Orders Terminal';
                subtitleEl.textContent = 'Track and advance customer orders from kitchen to cashier';
                renderOrdersBoard();
                break;
            case 'reservations':
                titleEl.textContent = 'Reservations & Tables';
                subtitleEl.textContent = 'Manage bookings and track live dining table occupancy';
                renderTablesAndReservations();
                break;
            case 'inventory':
                titleEl.textContent = 'Ingredients Inventory';
                subtitleEl.textContent = 'Monitor stock levels, set reorder levels, and restock raw items';
                renderInventory();
                break;
            case 'menu':
                titleEl.textContent = 'Menu Items';
                subtitleEl.textContent = 'Configure dishes, pricing, availability, and recipes';
                renderMenuPage();
                break;
        }
    }

    // Sync all dataset from API
    async function syncData() {
        showToast('Syncing restaurant data...', 'info');
        try {
            await Promise.all([
                fetchTables(),
                fetchMenu(),
                fetchInventory(),
                fetchOrders(),
                fetchReservations()
            ]);
            
            // Render active view
            switchView(state.currentView);
            showToast('All data successfully synchronized.', 'success');
        } catch (err) {
            console.error(err);
            showToast('Failed to load data. Ensure API server is running.', 'danger');
        }
    }

    async function syncDashboard() {
        try {
            const res = await fetch(`${API_BASE}/reports/dashboard`);
            if (!res.ok) throw new Error();
            state.dashboard = await res.json();
            renderDashboard();
        } catch (err) {
            showToast('Failed to refresh dashboard stats.', 'danger');
        }
    }

    // Fetch Calls
    async function fetchTables() {
        const res = await fetch(`${API_BASE}/tables`);
        state.tables = await res.json();
    }

    async function fetchMenu() {
        const res = await fetch(`${API_BASE}/menu`);
        state.menu = await res.json();
    }

    async function fetchInventory() {
        const res = await fetch(`${API_BASE}/inventory`);
        state.inventory = await res.json();
    }

    async function fetchOrders() {
        const res = await fetch(`${API_BASE}/orders`);
        state.orders = await res.json();
    }

    async function fetchReservations() {
        const res = await fetch(`${API_BASE}/reservations`);
        state.reservations = await res.json();
    }

    // Setup Event Listeners
    function setupEventListeners() {
        document.getElementById('btn-refresh-data').addEventListener('click', syncData);
        document.getElementById('btn-quick-order').addEventListener('click', () => openPOSModal());
        document.getElementById('btn-create-order-pos').addEventListener('click', () => openPOSModal());

        // Reservation Form Submission
        document.getElementById('form-add-reservation').addEventListener('submit', async (e) => {
            e.preventDefault();
            const reservation = {
                tableId: parseInt(document.getElementById('res-table-select').value),
                customerName: document.getElementById('res-customer-name').value,
                customerPhone: document.getElementById('res-customer-phone').value,
                reservationTime: new Date(document.getElementById('res-time').value).toISOString(),
                numberOfGuests: parseInt(document.getElementById('res-guests').value)
            };

            try {
                const res = await fetch(`${API_BASE}/reservations`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(reservation)
                });

                if (res.status === 409) {
                    const data = await res.json();
                    showToast(data.message || 'Table conflict.', 'danger');
                    return;
                }
                if (!res.ok) throw new Error();

                showToast('Table reserved successfully!', 'success');
                document.getElementById('form-add-reservation').reset();
                await syncData();
            } catch (err) {
                showToast('Error placing reservation.', 'danger');
            }
        });

        // Inventory Form Submission
        document.getElementById('form-add-inventory').addEventListener('submit', async (e) => {
            e.preventDefault();
            const item = {
                name: document.getElementById('inv-name').value,
                quantityInStock: parseFloat(document.getElementById('inv-qty').value),
                unit: document.getElementById('inv-unit').value,
                reorderLevel: parseFloat(document.getElementById('inv-reorder').value)
            };

            try {
                const res = await fetch(`${API_BASE}/inventory`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(item)
                });
                if (!res.ok) throw new Error();

                showToast('Ingredient added to inventory.', 'success');
                document.getElementById('form-add-inventory').reset();
                await syncData();
            } catch (err) {
                showToast('Error adding ingredient.', 'danger');
            }
        });

        // Restock Form Submission
        document.getElementById('form-restock-item').addEventListener('submit', async (e) => {
            e.preventDefault();
            const id = document.getElementById('restock-item-id').value;
            const quantity = parseFloat(document.getElementById('restock-quantity').value);

            try {
                const res = await fetch(`${API_BASE}/inventory/${id}/restock`, {
                    method: 'PUT',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(quantity)
                });
                if (!res.ok) throw new Error();

                closeModal('modal-restock');
                showToast('Stock level updated successfully.', 'success');
                await syncData();
            } catch (err) {
                showToast('Error updating stock.', 'danger');
            }
        });

        // POS Tab Filtering
        document.querySelectorAll('#pos-category-tabs .category-tab').forEach(tab => {
            tab.addEventListener('click', () => {
                document.querySelectorAll('#pos-category-tabs .category-tab').forEach(t => t.classList.remove('active'));
                tab.classList.add('active');
                renderPOSGrid(tab.getAttribute('data-cat'));
            });
        });

        // Recipe Ingredient Row Builder
        document.getElementById('btn-add-recipe-ingredient-row').addEventListener('click', addRecipeIngredientRow);

        // Menu Form Submission
        document.getElementById('form-add-menu-item').addEventListener('submit', async (e) => {
            e.preventDefault();
            
            // Build ingredients list
            const ingredients = [];
            document.querySelectorAll('.recipe-row').forEach(row => {
                const invId = parseInt(row.querySelector('.recipe-inv-select').value);
                const qty = parseFloat(row.querySelector('.recipe-qty-input').value);
                if (invId && qty > 0) {
                    ingredients.push({
                        inventoryItemId: invId,
                        quantityRequired: qty
                    });
                }
            });

            const menuItem = {
                name: document.getElementById('menu-name').value,
                price: parseFloat(document.getElementById('menu-price').value),
                category: document.getElementById('menu-category').value,
                description: document.getElementById('menu-description').value,
                isAvailable: true,
                ingredients: ingredients
            };

            try {
                const res = await fetch(`${API_BASE}/menu`, {
                    method: 'POST',
                    headers: { 'Content-Type': 'application/json' },
                    body: JSON.stringify(menuItem)
                });
                if (!res.ok) throw new Error();

                showToast('New dish added to menu.', 'success');
                document.getElementById('form-add-menu-item').reset();
                document.getElementById('recipe-ingredients-list').innerHTML = '';
                await syncData();
            } catch (err) {
                showToast('Error adding menu item.', 'danger');
            }
        });

        // Cart Order Process Button
        document.getElementById('btn-submit-pos-order').addEventListener('click', submitPOSOrder);
    }

    // ================= RENDERING VIEWS =================

    // 1. Dashboard View
    function renderDashboard() {
        const stats = state.dashboard;
        if (!stats) return;

        // Animate count increments nicely
        document.getElementById('stat-revenue').textContent = `$${(stats.dailyRevenue || 0).toFixed(2)}`;
        document.getElementById('stat-orders').textContent = stats.dailyOrdersCount || 0;
        document.getElementById('stat-active-tables').textContent = stats.activeTablesCount || 0;
        document.getElementById('stat-alerts').textContent = stats.lowStockItemsCount || 0;

        // Render stock alerts table
        const alertsTbody = document.getElementById('dashboard-stock-alerts');
        if (stats.lowStockAlerts && stats.lowStockAlerts.length > 0) {
            alertsTbody.innerHTML = stats.lowStockAlerts.map(item => `
                <tr>
                    <td style="font-weight: 500;">${item.name}</td>
                    <td style="color: var(--danger); font-weight: 600;">${item.quantityInStock} ${item.unit}</td>
                    <td style="color: var(--text-secondary);">${item.reorderLevel} ${item.unit}</td>
                </tr>
            `).join('');
        } else {
            alertsTbody.innerHTML = `
                <tr>
                    <td colspan="3" style="text-align: center; color: var(--text-secondary); padding: 1rem;">All ingredients are well stocked.</td>
                </tr>
            `;
        }

        // Render popular items table
        const popTbody = document.getElementById('dashboard-popular-items');
        if (stats.popularItems && stats.popularItems.length > 0) {
            popTbody.innerHTML = stats.popularItems.map(item => `
                <tr>
                    <td style="font-weight: 600; color: #fff;">${item.itemName}</td>
                    <td>${item.quantitySold} units</td>
                    <td style="color: var(--success); font-weight: 600;">$${item.totalRevenue.toFixed(2)}</td>
                </tr>
            `).join('');
        } else {
            popTbody.innerHTML = `
                <tr>
                    <td colspan="3" style="text-align: center; color: var(--text-secondary); padding: 1.5rem;">No menu item sales recorded today.</td>
                </tr>
            `;
        }

        // Fetch hourly report details and draw chart
        drawHourlySalesChart();
    }

    async function drawHourlySalesChart() {
        const container = document.getElementById('chart-hourly-sales');
        try {
            const res = await fetch(`${API_BASE}/reports/daily`);
            if (!res.ok) throw new Error();
            const data = await res.json();
            
            if (!data.hourlySales || data.hourlySales.length === 0) {
                container.innerHTML = `<div style="margin: auto; color: var(--text-secondary);">No sales transactions processed yet today.</div>`;
                return;
            }

            const maxRevenue = Math.max(...data.hourlySales.map(h => h.Revenue), 1);
            
            container.innerHTML = data.hourlySales.map(h => {
                const heightPercent = (h.Revenue / maxRevenue) * 85; // cap height at 85% for labels
                const formattedHour = h.Hour === 0 ? '12 AM' : h.Hour === 12 ? '12 PM' : h.Hour > 12 ? `${h.Hour - 12} PM` : `${h.Hour} AM`;
                return `
                    <div class="chart-bar-wrapper">
                        <div class="chart-bar" style="height: ${heightPercent}%;">
                            <div class="chart-tooltip">
                                <strong>$${h.Revenue.toFixed(2)}</strong><br>
                                ${h.OrderCount} order${h.OrderCount > 1 ? 's' : ''}
                            </div>
                        </div>
                        <div class="chart-label">${formattedHour}</div>
                    </div>
                `;
            }).join('');
        } catch (err) {
            container.innerHTML = `<div style="margin: auto; color: var(--danger);">Failed to load sales chart metrics.</div>`;
        }
    }

    // 2. Orders Kanban Board View
    function renderOrdersBoard() {
        const columns = {
            'Pending': document.getElementById('kb-pending'),
            'Preparing': document.getElementById('kb-preparing'),
            'Served': document.getElementById('kb-served'),
            'Paid': document.getElementById('kb-paid')
        };

        // Clear columns
        Object.values(columns).forEach(el => el.innerHTML = '');

        const counts = { Pending: 0, Preparing: 0, Served: 0, Paid: 0 };

        state.orders.forEach(order => {
            const statusStr = order.status;
            counts[statusStr] = (counts[statusStr] || 0) + 1;

            if (columns[statusStr]) {
                const card = document.createElement('div');
                card.className = 'order-card-kb';
                
                const tableStr = order.tableId ? `Table #${order.table.tableNumber}` : 'Takeaway';
                const itemsList = order.orderItems.map(oi => `${oi.quantity}x ${oi.menuItem.name}`).join(', ');
                const orderTimeStr = new Date(order.orderTime).toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });

                card.innerHTML = `
                    <div class="card-header">
                        <span style="font-weight:600;">Order #${order.id}</span>
                        <span class="badge ${order.tableId ? 'badge-available' : 'badge-cancelled'}">${tableStr}</span>
                    </div>
                    <div class="card-items-list" title="${itemsList}">${itemsList}</div>
                    <div class="card-footer">
                        <span class="card-time">${orderTimeStr}</span>
                        <span class="card-amount">$${order.totalAmount.toFixed(2)}</span>
                    </div>
                `;

                card.addEventListener('click', () => openOrderDetailsModal(order));
                columns[statusStr].appendChild(card);
            }
        });

        // Set counts headers
        document.getElementById('count-pending').textContent = counts.Pending;
        document.getElementById('count-preparing').textContent = counts.Preparing;
        document.getElementById('count-served').textContent = counts.Served;
        document.getElementById('count-paid').textContent = counts.Paid;
    }

    // 3. Reservations & Tables layout
    function renderTablesAndReservations() {
        // Render visual tables layout
        const grid = document.getElementById('layout-table-grid');
        grid.innerHTML = state.tables.map(table => {
            let statusClass = 'status-available';
            let statusLabel = 'Available';
            let icon = '🪑';

            if (table.status === 'Occupied') {
                statusClass = 'status-occupied';
                statusLabel = 'Occupied';
                icon = '🍽️';
            } else if (table.status === 'Reserved') {
                statusClass = 'status-reserved';
                statusLabel = 'Reserved';
                icon = '🎟️';
            }

            return `
                <div class="table-element ${statusClass}" onclick="app.showTableDetails(${table.id})">
                    <span class="table-icon">${icon}</span>
                    <span class="table-num">T-${table.tableNumber}</span>
                    <span class="table-cap">Capacity: ${table.capacity} guests</span>
                    <span class="badge ${table.status === 'Available' ? 'badge-available' : table.status === 'Occupied' ? 'badge-occupied' : 'badge-reserved'}">${statusLabel}</span>
                </div>
            `;
        }).join('');

        // Populate table select in reservation form
        const select = document.getElementById('res-table-select');
        select.innerHTML = state.tables
            .filter(t => t.status === 'Available')
            .map(t => `<option value="${t.id}">Table ${t.tableNumber} (Cap: ${t.capacity})</option>`)
            .join('');

        if (select.children.length === 0) {
            select.innerHTML = `<option value="">No tables available</option>`;
        }

        // Render upcoming bookings
        const tbody = document.getElementById('reservations-table-body');
        if (state.reservations.length === 0) {
            tbody.innerHTML = `<tr><td colspan="6" style="text-align:center; color:var(--text-secondary);">No bookings recorded.</td></tr>`;
            return;
        }

        tbody.innerHTML = state.reservations.map(res => {
            const timeStr = new Date(res.reservationTime).toLocaleString([], { dateStyle: 'short', timeStyle: 'short' });
            
            let actionBtn = '';
            if (res.status === 'Pending' || res.status === 'Confirmed') {
                actionBtn = `
                    <button class="btn btn-success" style="padding: 0.25rem 0.5rem; font-size: 0.75rem;" onclick="app.updateReservationStatus(${res.id}, 'complete')">Seat / Complete</button>
                    <button class="btn btn-danger" style="padding: 0.25rem 0.5rem; font-size: 0.75rem;" onclick="app.updateReservationStatus(${res.id}, 'cancel')">Cancel</button>
                `;
            }

            return `
                <tr>
                    <td style="font-weight: 500;">${res.customerName}</td>
                    <td>Table ${res.table.tableNumber}</td>
                    <td>${res.numberOfGuests} guests</td>
                    <td>${timeStr}</td>
                    <td><span class="badge ${res.status === 'Confirmed' ? 'badge-reserved' : res.status === 'Completed' ? 'badge-paid' : res.status === 'Cancelled' ? 'badge-cancelled' : 'badge-pending'}">${res.status}</span></td>
                    <td>
                        <div style="display: flex; gap: 0.35rem;">
                            ${actionBtn}
                        </div>
                    </td>
                </tr>
            `;
        }).join('');
    }

    // 4. Inventory Page
    function renderInventory() {
        const tbody = document.getElementById('inventory-table-body');
        if (state.inventory.length === 0) {
            tbody.innerHTML = `<tr><td colspan="4" style="text-align:center;">No inventory items found.</td></tr>`;
            return;
        }

        tbody.innerHTML = state.inventory.map(item => {
            const isLow = item.quantityInStock <= item.reorderLevel;
            return `
                <tr>
                    <td style="font-weight: 600; color: #fff;">${item.name}</td>
                    <td style="font-weight: 700; color: ${isLow ? 'var(--danger)' : 'var(--success)'};">
                        ${item.quantityInStock} ${item.unit}
                        ${isLow ? ' <span style="font-size:0.75rem; font-weight:normal; color:var(--warning);">⚠️ Low</span>' : ''}
                    </td>
                    <td style="color: var(--text-secondary);">${item.reorderLevel} ${item.unit}</td>
                    <td>
                        <button class="btn btn-secondary" style="padding:0.35rem 0.65rem; font-size:0.75rem;" onclick="app.showRestockModal(${item.id}, '${item.name}')">Restock</button>
                    </td>
                </tr>
            `;
        }).join('');

        // Populate dropdowns in recipe ingredient lists in Menu tab
        populateRecipeIngredientSelectors();
    }

    // 5. Menu Manager View
    function renderMenuPage() {
        const tbody = document.getElementById('menu-table-body');
        if (state.menu.length === 0) {
            tbody.innerHTML = `<tr><td colspan="5" style="text-align:center;">No dishes configured.</td></tr>`;
            return;
        }

        tbody.innerHTML = state.menu.map(item => {
            return `
                <tr>
                    <td style="font-size: 0.8rem; color: var(--text-secondary); font-weight:600; text-transform:uppercase;">${item.category}</td>
                    <td style="font-weight: 600; color:#fff;">${item.name}</td>
                    <td style="font-weight: 700; color: var(--accent);">$${item.price.toFixed(2)}</td>
                    <td>
                        <span class="badge ${item.isAvailable ? 'badge-available' : 'badge-occupied'}">${item.isAvailable ? 'Available' : 'Out of Stock'}</span>
                    </td>
                    <td>
                        <button class="btn btn-secondary" style="padding: 0.35rem 0.65rem; font-size: 0.75rem;" onclick="app.toggleMenuItemAvailability(${item.id})">Toggle Status</button>
                    </td>
                </tr>
            `;
        }).join('');
    }

    // ================= RECIPE ROW BUILDERS =================
    function populateRecipeIngredientSelectors() {
        // Helper to update dynamic recipe selector lists if they exist in forms
        document.querySelectorAll('.recipe-inv-select').forEach(sel => {
            const val = sel.value;
            sel.innerHTML = '<option value="">-- Choose Ingredient --</option>' + 
                state.inventory.map(i => `<option value="${i.id}">${i.name} (${i.unit})</option>`).join('');
            sel.value = val;
        });
    }

    function addRecipeIngredientRow() {
        const container = document.getElementById('recipe-ingredients-list');
        const row = document.createElement('div');
        row.className = 'form-row recipe-row';
        row.style.alignItems = 'center';
        
        row.innerHTML = `
            <div style="flex:2;">
                <select class="form-control recipe-inv-select" required>
                    <option value="">-- Choose Ingredient --</option>
                    ${state.inventory.map(i => `<option value="${i.id}">${i.name} (${i.unit})</option>`).join('')}
                </select>
            </div>
            <div style="flex:1;">
                <input type="number" class="form-control recipe-qty-input" min="0.01" step="any" placeholder="Qty" required>
            </div>
            <div>
                <button type="button" class="btn btn-danger" style="padding: 0.65rem; font-size:0.85rem;" onclick="this.parentElement.parentElement.remove()">&times;</button>
            </div>
        `;
        container.appendChild(row);
    }

    // ================= POS CART & MODAL TERMINAL =================
    function openPOSModal() {
        // Populate tables dropdown
        const select = document.getElementById('pos-table-select');
        select.innerHTML = '<option value="">Takeaway / Delivery</option>' + 
            state.tables.map(t => `<option value="${t.id}">Table ${t.tableNumber} (Cap: ${t.capacity} - ${t.status})</option>`).join('');
        
        // Reset cart
        state.cart = [];
        updatePOSCart();

        // Render menu grid
        renderPOSGrid('all');

        openModal('modal-pos');
    }

    function renderPOSGrid(category) {
        const grid = document.getElementById('pos-menu-grid');
        grid.innerHTML = '';

        const items = category === 'all' ? state.menu : state.menu.filter(m => m.category === category);
        
        if (items.length === 0) {
            grid.innerHTML = `<div style="text-align:center; padding:2rem; width:100%; color:var(--text-secondary);">No items available in this category.</div>`;
            return;
        }

        items.forEach(item => {
            const card = document.createElement('div');
            card.className = `menu-item-card ${item.isAvailable ? '' : 'unavailable'}`;
            card.innerHTML = `
                <span class="item-category">${item.category}</span>
                <span class="item-name">${item.name}</span>
                <span class="item-description" title="${item.description}">${item.description}</span>
                <div class="item-footer">
                    <span class="item-price">$${item.price.toFixed(2)}</span>
                    <span style="font-size:0.75rem; color: ${item.isAvailable ? 'var(--success)' : 'var(--danger)'};">
                        ${item.isAvailable ? '＋ Add' : 'Sold Out'}
                    </span>
                </div>
            `;

            if (item.isAvailable) {
                card.addEventListener('click', () => addToCart(item));
            }
            grid.appendChild(card);
        });
    }

    function addToCart(menuItem) {
        const existing = state.cart.find(c => c.menuItemId === menuItem.Id || c.menuItemId === menuItem.id);
        if (existing) {
            existing.quantity++;
        } else {
            state.cart.push({
                menuItemId: menuItem.id || menuItem.Id,
                name: menuItem.name,
                price: menuItem.price,
                quantity: 1,
                notes: ''
            });
        }
        updatePOSCart();
    }

    function changeCartQty(index, delta) {
        state.cart[index].quantity += delta;
        if (state.cart[index].quantity <= 0) {
            state.cart.splice(index, 1);
        }
        updatePOSCart();
    }

    function updatePOSCart() {
        const container = document.getElementById('pos-cart-items');
        if (state.cart.length === 0) {
            container.innerHTML = `<div style="margin: auto; color: var(--text-secondary);">Cart is empty. Click menu items to add.</div>`;
            document.getElementById('pos-cart-total').textContent = '$0.00';
            return;
        }

        let total = 0;
        container.innerHTML = state.cart.map((item, idx) => {
            const itemTotal = item.price * item.quantity;
            total += itemTotal;
            return `
                <div class="cart-item">
                    <div class="cart-item-info">
                        <span class="cart-item-name">${item.name}</span>
                        <span class="cart-item-price">$${item.price.toFixed(2)}</span>
                        <div class="cart-item-quantity">
                            <button class="cart-qty-btn" onclick="app.changeCartQty(${idx}, -1)">-</button>
                            <span style="font-size: 0.85rem; font-weight:600; min-width:15px; text-align:center;">${item.quantity}</span>
                            <button class="cart-qty-btn" onclick="app.changeCartQty(${idx}, 1)">+</button>
                        </div>
                    </div>
                    <div style="display:flex; flex-direction:column; align-items:flex-end;">
                        <span class="cart-item-total">$${itemTotal.toFixed(2)}</span>
                        <input type="text" placeholder="Notes..." class="form-control" style="font-size: 0.7rem; padding: 0.15rem 0.35rem; margin-top: 0.35rem; max-width:80px;" value="${item.notes}" onchange="app.updateCartNotes(${idx}, this.value)">
                    </div>
                </div>
            `;
        }).join('');

        document.getElementById('pos-cart-total').textContent = `$${total.toFixed(2)}`;
    }

    function updateCartNotes(index, val) {
        state.cart[index].notes = val;
    }

    async function submitPOSOrder() {
        if (state.cart.length === 0) {
            showToast('Cannot place order: Cart is empty.', 'warning');
            return;
        }

        const tableIdVal = document.getElementById('pos-table-select').value;
        const payload = {
            tableId: tableIdVal ? parseInt(tableIdVal) : null,
            orderItems: state.cart.map(item => ({
                menuItemId: item.menuItemId,
                quantity: item.quantity,
                notes: item.notes
            }))
        };

        try {
            const res = await fetch(`${API_BASE}/orders`, {
                method: 'POST',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(payload)
            });

            if (res.status === 409 || res.status === 400) {
                const err = await res.json();
                showToast(err.message || 'Validation error placing order.', 'danger');
                return;
            }
            if (!res.ok) throw new Error();

            closeModal('modal-pos');
            showToast('Order created successfully.', 'success');
            await syncData();
        } catch (err) {
            showToast('Error processing order: insufficient stock or system error.', 'danger');
        }
    }

    // ================= DETAILS POPUPS & MODALS =================
    async function openOrderDetailsModal(order) {
        document.getElementById('order-details-title').textContent = `Order #${order.id} Details`;
        
        const tableStr = order.tableId ? `Table #${order.table.tableNumber}` : 'Takeaway / Delivery';
        document.getElementById('order-details-table').textContent = `Destination: ${tableStr}`;
        
        const orderTimeStr = new Date(order.orderTime).toLocaleString([], { dateStyle: 'short', timeStyle: 'short' });
        document.getElementById('order-details-time').textContent = `Ordered: ${orderTimeStr}`;

        const statusBadge = document.getElementById('order-details-status');
        statusBadge.innerHTML = `<span class="badge badge-${order.status.toLowerCase()}">${order.status}</span>`;

        // Load items list
        const tbody = document.getElementById('order-details-items-body');
        tbody.innerHTML = order.orderItems.map(oi => {
            const lineTotal = oi.quantity * oi.unitPrice;
            return `
                <tr>
                    <td style="font-weight: 500;">
                        ${oi.menuItem.name}
                        ${oi.notes ? `<div style="font-size:0.75rem; color:var(--warning); font-style:italic;">Notes: ${oi.notes}</div>` : ''}
                    </td>
                    <td>${oi.quantity}</td>
                    <td>$${oi.unitPrice.toFixed(2)}</td>
                    <td style="font-weight: 600;">$${lineTotal.toFixed(2)}</td>
                </tr>
            `;
        }).join('');

        document.getElementById('order-details-total').textContent = `$${order.totalAmount.toFixed(2)}`;

        // Build status transitions buttons
        const actionArea = document.getElementById('order-details-action-buttons');
        actionArea.innerHTML = '';

        if (order.status === 'Pending') {
            actionArea.innerHTML = `
                <button class="btn btn-warning" onclick="app.updateOrderStatus(${order.id}, 'Preparing')">Start Cooking</button>
                <button class="btn btn-danger" onclick="app.updateOrderStatus(${order.id}, 'Cancelled')">Cancel Order</button>
            `;
        } else if (order.status === 'Preparing') {
            actionArea.innerHTML = `
                <button class="btn btn-primary" onclick="app.updateOrderStatus(${order.id}, 'Served')">Mark Served</button>
                <button class="btn btn-danger" onclick="app.updateOrderStatus(${order.id}, 'Cancelled')">Cancel Order</button>
            `;
        } else if (order.status === 'Served') {
            actionArea.innerHTML = `
                <button class="btn btn-success" onclick="app.updateOrderStatus(${order.id}, 'Paid')">Collect Payment</button>
                <button class="btn btn-danger" onclick="app.updateOrderStatus(${order.id}, 'Cancelled')">Cancel Order</button>
            `;
        }

        openModal('modal-order-details');
    }

    async function updateOrderStatus(orderId, newStatus) {
        try {
            const res = await fetch(`${API_BASE}/orders/${orderId}/status`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(newStatus)
            });
            if (!res.ok) throw new Error();

            closeModal('modal-order-details');
            showToast(`Order #${orderId} status advanced to ${newStatus}.`, 'success');
            await syncData();
        } catch (err) {
            showToast('Failed to update order status.', 'danger');
        }
    }

    async function toggleMenuItemAvailability(id) {
        const item = state.menu.find(m => m.id === id);
        if (!item) return;

        const updated = { ...item, isAvailable: !item.isAvailable };
        
        try {
            const res = await fetch(`${API_BASE}/menu/${id}`, {
                method: 'PUT',
                headers: { 'Content-Type': 'application/json' },
                body: JSON.stringify(updated)
            });
            if (!res.ok) throw new Error();

            showToast(`Status updated for '${item.name}'.`, 'success');
            await syncData();
        } catch (err) {
            showToast('Failed to update menu item status.', 'danger');
        }
    }

    async function updateReservationStatus(id, action) {
        try {
            const res = await fetch(`${API_BASE}/reservations/${id}/${action}`, {
                method: 'PUT'
            });
            if (!res.ok) throw new Error();

            showToast(`Reservation successfully updated.`, 'success');
            await syncData();
        } catch (err) {
            showToast('Error updating reservation.', 'danger');
        }
    }

    function showRestockModal(id, name) {
        if (!id) {
            // Pick first item from stock alerts if clicked generic button
            const stats = state.dashboard;
            if (stats && stats.lowStockAlerts && stats.lowStockAlerts.length > 0) {
                id = stats.lowStockAlerts[0].id;
                name = stats.lowStockAlerts[0].name;
            } else if (state.inventory.length > 0) {
                id = state.inventory[0].id;
                name = state.inventory[0].name;
            } else {
                showToast('Inventory is empty.', 'warning');
                return;
            }
        }

        document.getElementById('restock-item-id').value = id;
        document.getElementById('restock-item-label').textContent = `Restocking Item: ${name}`;
        document.getElementById('restock-quantity').value = '20';
        openModal('modal-restock');
    }

    function showTableDetails(id) {
        const table = state.tables.find(t => t.id === id);
        if (!table) return;

        // Check if there are active orders for this table
        const activeOrder = state.orders.find(o => o.tableId === id && o.status !== 'Paid' && o.status !== 'Cancelled');
        
        if (activeOrder) {
            openOrderDetailsModal(activeOrder);
        } else {
            showToast(`Table ${table.tableNumber} is ${table.status}. No active orders right now.`, 'info');
        }
    }

    // Modal Helpers
    function openModal(modalId) {
        document.getElementById(modalId).classList.add('active');
    }

    function closeModal(modalId) {
        document.getElementById(modalId).classList.remove('active');
    }

    // Alert Notification Toast System
    let toastTimeout = null;
    function showToast(message, type = 'info') {
        const toast = document.getElementById('app-alert-toast');
        const iconEl = document.getElementById('alert-toast-icon');
        const msgEl = document.getElementById('alert-toast-message');

        toast.className = `alert-toast ${type}`;
        msgEl.textContent = message;

        switch (type) {
            case 'success': iconEl.textContent = '✅'; break;
            case 'warning': iconEl.textContent = '⚠️'; break;
            case 'danger': iconEl.textContent = '❌'; break;
            case 'info':
            default: iconEl.textContent = 'ℹ️'; break;
        }

        toast.classList.add('active');

        if (toastTimeout) clearTimeout(toastTimeout);
        
        toastTimeout = setTimeout(() => {
            toast.classList.remove('active');
        }, 4000);
    }

    // Page-load trigger
    window.addEventListener('DOMContentLoaded', init);

    // Public exposure for inline onclick handlers
    return {
        changeCartQty,
        updateCartNotes,
        closeModal,
        updateOrderStatus,
        toggleMenuItemAvailability,
        updateReservationStatus,
        showRestockModal,
        showTableDetails
    };
})();
