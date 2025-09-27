/**
 * MyStore Front-End Logic (Multi-Store Version)
 * Handles client-side filtering, searching, and the complete shopping cart functionality.
 */
document.addEventListener('DOMContentLoaded', function () {
    // --- CONFIGURATION ---
    const WHATSAPP_NUMBER = "201010475455"; // ⚠️ غيّر هذا الرقم إلى رقمك مع كود الدولة

    // --- DOM ELEMENT SELECTORS ---
    const storeContainer = document.querySelector('.store-container');
    if (!storeContainer) return; // Exit if not on a store page

    const currentStoreIdInput = document.getElementById('current-store-id');
    const searchInput = document.getElementById('product-search-input');
    const companyButtons = document.querySelectorAll('.company-card-btn');
    const productsList = document.getElementById('products-list');
    const productCardWrappers = document.querySelectorAll('.product-card-wrapper');

    // Cart Elements
    const desktopCartItemsContainer = document.querySelector('#cart-sidebar #cart-items');
    const desktopCartTotal = document.querySelector('#cart-sidebar #cart-total');
    const desktopEmptyCartMsg = document.querySelector('#cart-sidebar .cart-empty-msg');
    const mobileCartContent = document.getElementById('mobile-cart-content');
    const mobileCartBtn = document.getElementById('mobile-cart-btn');
    const mobileCartCount = document.getElementById('mobile-cart-count');
    const checkoutModalEl = document.getElementById('checkoutModal');
    const checkoutModal = checkoutModalEl ? new bootstrap.Modal(checkoutModalEl) : null;

    // --- STATE MANAGEMENT ---
    let cart = [];

    // --- EVENT LISTENERS ---

    // 1. Handle Company Button Filtering
    companyButtons.forEach(button => {
        button.addEventListener('click', function () {
            companyButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            const selectedCompanyId = this.dataset.companyId;
            filterProductsByCompany(selectedCompanyId);
        });
    });

    // 2. Handle Product Search (Client-side)
    let searchTimeout;
    searchInput.addEventListener('input', function () {
        clearTimeout(searchTimeout);
        searchTimeout = setTimeout(() => {
            filterProductsBySearch(this.value.trim());
        }, 300);
    });

    // 3. Handle Add to Cart Clicks
    productsList.addEventListener('click', function (e) {
        const addToCartBtn = e.target.closest('.add-to-cart-btn');
        if (addToCartBtn) {
            const product = {
                id: parseInt(addToCartBtn.dataset.productId),
                name: addToCartBtn.dataset.productName,
                price: parseFloat(addToCartBtn.dataset.productPrice),
                imageUrl: addToCartBtn.dataset.productImage
            };
            if (product && product.id) {
                addToCart(product);
                addToCartBtn.innerHTML = '<i class="fas fa-check"></i> تمت الإضافة';
                setTimeout(() => {
                    addToCartBtn.innerHTML = '<i class="fas fa-cart-plus me-1"></i> إضافة للسلة';
                }, 1000);
            }
        }
    });

    // 4. Handle Cart Quantity Changes & Deletions
    document.body.addEventListener('click', function (e) {
        if (e.target.closest('.qty-btn')) {
            const button = e.target.closest('.qty-btn');
            updateQuantity(parseInt(button.dataset.productId), button.dataset.action);
        }
        if (e.target.closest('.remove-item')) {
            const button = e.target.closest('.remove-item');
            updateQuantity(parseInt(button.dataset.productId), 'remove');
        }
    });

    // 5. Handle Checkout Button Click (Opens Modal) - Corrected to use class
    document.body.addEventListener('click', function (e) {
        if (e.target.closest('.checkout-btn')) {
            if (cart.length > 0 && checkoutModal) {
                checkoutModal.show();
            }
        }
    });

    // 6. Handle Confirm Order Button Click (Sends Data)
    if (checkoutModalEl) {
        checkoutModalEl.addEventListener('click', async function (e) {
            if (e.target.id === 'confirm-order-btn') {
                const confirmOrderBtn = e.target;
                const checkoutForm = document.getElementById('checkout-form');
                if (!checkoutForm.checkValidity()) {
                    checkoutForm.classList.add('was-validated');
                    return;
                }

                confirmOrderBtn.disabled = true;
                confirmOrderBtn.innerHTML = `<span class="spinner-border spinner-border-sm" role="status" aria-hidden="true"></span> جارٍ التأكيد...`;

                const orderData = {
                    customerName: document.getElementById('customer-name').value,
                    customerPhone: document.getElementById('customer-phone').value,
                    customerAddress: document.getElementById('customer-address').value,
                    cartItems: cart.map(item => ({ id: item.id, quantity: item.quantity })),
                    storeId: parseInt(currentStoreIdInput.value)
                };

                try {
                    const response = await fetch(`${window.location.pathname}?handler=CreateOrder`, {
                        method: 'POST',
                        headers: { 'Content-Type': 'application/json', 'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value },
                        body: JSON.stringify(orderData)
                    });

                    const result = await response.json();
                    if (result.success) {
                        const totalAmount = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);
                        let message = `*طلب جديد برقم: ${result.orderNumber}*\n\n*الاسم:* ${orderData.customerName}\n*الهاتف:* ${orderData.customerPhone}\n*العنوان:* ${orderData.customerAddress}\n\n*-- تفاصيل الطلب --*\n`;
                        cart.forEach(item => { message += `- ${item.name} (الكمية: ${item.quantity}) - ${(item.price * item.quantity).toFixed(2)} جنيه\n`; });
                        message += `\n*الإجمالي: ${totalAmount.toFixed(2)} جنيه*`;

                        window.location.href = `https://wa.me/${WHATSAPP_NUMBER}?text=${encodeURIComponent(message)}`;

                        cart = [];
                        updateCartDisplay();
                        checkoutModal.hide();
                    } else {
                        alert(`حدث خطأ:\n${result.message}`);
                    }
                } catch (error) {
                    console.error("Order creation failed:", error);
                    alert("فشل الاتصال بالخادم. الرجاء التحقق من اتصالك بالإنترنت.");
                } finally {
                    confirmOrderBtn.disabled = false;
                    confirmOrderBtn.innerHTML = 'تأكيد الطلب عبر واتساب';
                }
            }
        });
    }

    // --- FUNCTIONS ---

    function filterProductsByCompany(companyId) {
        productCardWrappers.forEach(card => {
            const cardCompanyId = card.dataset.companyId;
            if (companyId === '0' || cardCompanyId === companyId) {
                card.style.display = 'block';
            } else {
                card.style.display = 'none';
            }
        });
    }

    function filterProductsBySearch(query) {
        const lowerCaseQuery = query.toLowerCase();
        companyButtons.forEach(btn => btn.classList.remove('active'));

        productCardWrappers.forEach(card => {
            const productName = card.querySelector('.card-title')?.textContent.toLowerCase() || '';
            if (productName.includes(lowerCaseQuery)) {
                card.style.display = 'block';
            } else {
                card.style.display = 'none';
            }
        });
    }

    function addToCart(product) {
        const existingItem = cart.find(item => item.id === product.id);
        if (existingItem) {
            existingItem.quantity++;
        } else {
            cart.push({ ...product, quantity: 1 });
        }
        updateCartDisplay();
    }

    function updateQuantity(productId, action) {
        const itemIndex = cart.findIndex(item => item.id === productId);
        if (itemIndex === -1) return;

        if (action === 'increment') {
            cart[itemIndex].quantity++;
        } else if (action === 'decrement') {
            cart[itemIndex].quantity--;
            if (cart[itemIndex].quantity <= 0) {
                cart.splice(itemIndex, 1);
            }
        } else if (action === 'remove') {
            cart.splice(itemIndex, 1);
        }
        updateCartDisplay();
    }

    function updateCartDisplay() {
        const total = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);
        const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
        const cartHasItems = cart.length > 0;

        const cartHtml = cartHasItems ? cart.map(item => `
            <div class="cart-item">
                <div class="item-info">
                    <span class="item-name">${item.name}</span>
                    <span class="item-price">${item.quantity} × ${item.price.toFixed(2)} جنيه</span>
                </div>
                <div class="item-controls">
                    <button class="btn btn-sm btn-outline-secondary qty-btn" data-product-id="${item.id}" data-action="increment">+</button>
                    <span class="item-qty">${item.quantity}</span>
                    <button class="btn btn-sm btn-outline-secondary qty-btn" data-product-id="${item.id}" data-action="decrement">-</button>
                    <button class="btn btn-sm text-danger remove-item" data-product-id="${item.id}"><i class="fas fa-trash-alt"></i></button>
                </div>
            </div>`).join('') : '';

        const desktopCheckoutBtn = document.querySelector('#cart-sidebar .checkout-btn');
        if (desktopCartItemsContainer) {
            desktopCartItemsContainer.innerHTML = cartHtml;
            desktopCartTotal.textContent = `${total.toFixed(2)}`;
            if (desktopCheckoutBtn) desktopCheckoutBtn.disabled = !cartHasItems;
            desktopEmptyCartMsg.classList.toggle('d-none', cartHasItems);
        }

        if (mobileCartBtn && mobileCartCount) {
            mobileCartCount.textContent = totalItems;
            mobileCartBtn.style.display = cartHasItems ? 'flex' : 'none';
        }

        if (mobileCartContent) {
            // Corrected to use class for the checkout button
            mobileCartContent.innerHTML = `
                <div class="modal-header">
                    <h5 class="modal-title"><i class="fas fa-shopping-cart me-2"></i>سلة المشتريات</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">${cartHasItems ? cartHtml : '<p class="cart-empty-msg text-center text-muted">سلتك فارغة حالياً.</p>'}</div>
                <div class="modal-footer">
                    <div class="w-100">
                        <div class="d-flex justify-content-between"><strong>الإجمالي</strong><strong><span>${total.toFixed(2)}</span> جنيه</strong></div>
                        <div class="d-grid mt-3"><button class="btn btn-success btn-lg checkout-btn" ${!cartHasItems ? 'disabled' : ''}><i class="fab fa-whatsapp me-2"></i>إتمام الطلب</button></div>
                    </div>
                </div>`;
        }
    }

    // --- Initialize ---
    updateCartDisplay();
});
