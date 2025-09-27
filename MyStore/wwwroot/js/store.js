/**
 * MyStore Front-End Logic
 * Handles searching, company selection, product display, and shopping cart.
 */
document.addEventListener('DOMContentLoaded', function () {

    // --- CONFIGURATION ---
    const WHATSAPP_NUMBER = "201010475455"; // ⚠️ غيّر هذا الرقم إلى رقمك مع كود الدولة

    // --- DOM ELEMENT SELECTORS ---
    const storeContainer = document.querySelector('.store-container');
    if (!storeContainer) return; // Exit if not on the store page

    const searchInput = document.getElementById('product-search-input');
    const companyButtons = document.querySelectorAll('.company-card-btn');
    const productsList = document.getElementById('products-list');
    const loadingSpinner = document.getElementById('loading-spinner');
    const productsPlaceholder = document.getElementById('products-placeholder');
    const productsSectionTitle = document.querySelector('#products-section .section-title');

    // Cart Elements
    const desktopCartItemsContainer = document.querySelector('#cart-sidebar #cart-items');
    const desktopCartTotal = document.querySelector('#cart-sidebar #cart-total');
    const desktopCheckoutBtn = document.querySelector('#cart-sidebar #checkout-btn');
    const desktopEmptyCartMsg = document.querySelector('#cart-sidebar .cart-empty-msg');
    const mobileCartContent = document.getElementById('mobile-cart-content');
    const mobileCartBtn = document.getElementById('mobile-cart-btn');
    const mobileCartCount = document.getElementById('mobile-cart-count');

    // === FIX: Initialize Modals safely by checking if the element exists first ===
    const checkoutModalEl = document.getElementById('checkoutModal');
    const checkoutModal = checkoutModalEl ? new bootstrap.Modal(checkoutModalEl) : null;

    const productDetailsModalEl = document.getElementById('productDetailsModal');
    const productDetailsModal = productDetailsModalEl ? new bootstrap.Modal(productDetailsModalEl) : null;


    // --- STATE MANAGEMENT ---
    let cart = [];
    let allProducts = [];
    let searchTimeout;

    // --- EVENT LISTENERS ---

    // 1. Handle Product Search
    searchInput.addEventListener('input', function () {
        const query = this.value.trim();
        clearTimeout(searchTimeout);

        if (query.length < 2) {
            productsPlaceholder.classList.remove('d-none');
            productsList.innerHTML = '';
            productsSectionTitle.textContent = 'المنتجات';
            companyButtons.forEach(btn => btn.classList.remove('active'));
            return;
        }

        searchTimeout = setTimeout(() => {
            searchProducts(query);
        }, 300);
    });

    // 2. Handle Company Selection
    companyButtons.forEach(button => {
        button.addEventListener('click', function () {
            const companyId = this.dataset.companyId;
            companyButtons.forEach(btn => btn.classList.remove('active'));
            this.classList.add('active');
            searchInput.value = '';
            productsSectionTitle.textContent = 'المنتجات';
            fetchProducts(companyId);
        });
    });

    // 3. Handle Product Card Clicks (Add to Cart vs. View Details)
    productsList.addEventListener('click', function (e) {
        const addToCartBtn = e.target.closest('.add-to-cart-btn');
        if (addToCartBtn) {
            e.stopPropagation();
            const productId = parseInt(addToCartBtn.dataset.productId);
            const product = allProducts.find(p => p.id === productId);
            if (product) addToCart(product);
            return;
        }
        const productCard = e.target.closest('.product-card');
        if (productCard) {
            const productId = parseInt(productCard.dataset.productId);
            const product = allProducts.find(p => p.id === productId);
            if (product) showProductDetailsModal(product);
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

    // 5. Handle Checkout Process
    document.body.addEventListener('click', function (e) {
        if (e.target.closest('#checkout-btn')) {
            if (cart.length > 0 && checkoutModal) { // Check if modal exists
                const mobileCartModalEl = document.getElementById('cartModal');
                const mobileCartModal = bootstrap.Modal.getInstance(mobileCartModalEl);
                if (mobileCartModal && mobileCartModal._isShown) {
                    mobileCartModalEl.addEventListener('hidden.bs.modal', () => {
                        checkoutModal.show();
                    }, { once: true });
                    mobileCartModal.hide();
                } else {
                    checkoutModal.show();
                }
            }
        }
    });

    // 6. Handle Confirm Order Button Click
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
                    cartItems: cart.map(item => ({ id: item.id, quantity: item.quantity }))
                };
                try {
                    const response = await fetch(`/?handler=CreateOrder`, {
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

    // 7. Handle "Add to Cart" from within the Product Details Modal
    if (productDetailsModalEl) {
        productDetailsModalEl.addEventListener('click', function (e) {
            if (e.target.closest('.modal-add-to-cart-btn')) {
                const button = e.target.closest('.modal-add-to-cart-btn');
                const productId = parseInt(button.dataset.productId);
                const product = allProducts.find(p => p.id === productId);
                if (product) {
                    addToCart(product);
                    button.innerHTML = '<i class="fas fa-check"></i> تمت الإضافة!';
                    button.classList.replace('btn-primary', 'btn-success');
                    setTimeout(() => {
                        button.innerHTML = '<i class="fas fa-cart-plus me-1"></i> إضافة للسلة';
                        button.classList.replace('btn-success', 'btn-primary');
                        productDetailsModal.hide();
                    }, 1200);
                }
            }
        });
    }

    // --- FUNCTIONS ---

    async function searchProducts(query) {
        productsPlaceholder.classList.add('d-none');
        loadingSpinner.classList.remove('d-none');
        productsList.innerHTML = '';
        allProducts = [];
        companyButtons.forEach(btn => btn.classList.remove('active'));
        productsSectionTitle.textContent = `نتائج البحث عن "${query}"`;
        try {
            const response = await fetch(`/?handler=SearchProducts&query=${encodeURIComponent(query)}`);
            if (!response.ok) throw new Error('Network response was not ok');
            const products = await response.json();
            allProducts = products;
            displayProducts(products);
        } catch (error) {
            console.error('Failed to search products:', error);
            productsList.innerHTML = `<div class="col-12"><p class="text-danger text-center p-5">عفواً، حدث خطأ أثناء البحث.</p></div>`;
        } finally {
            loadingSpinner.classList.add('d-none');
        }
    }

    async function fetchProducts(companyId) {
        productsPlaceholder.classList.add('d-none');
        loadingSpinner.classList.remove('d-none');
        productsList.innerHTML = '';
        allProducts = [];
        try {
            const response = await fetch(`/?handler=Products&companyId=${companyId}`);
            if (!response.ok) throw new Error('Network response was not ok');
            const products = await response.json();
            allProducts = products;
            displayProducts(products);
        } catch (error) {
            console.error('Failed to fetch products:', error);
            productsList.innerHTML = `<div class="col-12"><p class="text-danger text-center p-5">عفواً، حدث خطأ أثناء جلب المنتجات.</p></div>`;
        } finally {
            loadingSpinner.classList.add('d-none');
        }
    }

    function displayProducts(products) {
        productsList.innerHTML = '';
        if (products.length === 0) {
            productsList.innerHTML = `<div class="col-12"><p class="text-muted text-center p-5">لا توجد منتجات تطابق بحثك أو لهذه الشركة حالياً.</p></div>`;
            return;
        }
        products.forEach(product => {
            const productCol = document.createElement('div');
            productCol.className = 'col-lg-4 col-md-6';
            productCol.innerHTML = `
                <div class="product-card" data-product-id="${product.id}">
                    <div class="product-image-container">
                        <img src="${product.imageUrl || '/images/placeholder.png'}" alt="${product.name}" class="product-image">
                    </div>
                    <div class="product-info">
                        <h5 class="product-name">${product.name}</h5>
                        <div class="product-footer">
                           <p class="product-price">${product.price.toFixed(2)} جنيه</p>
                           <button class="btn btn-sm btn-primary add-to-cart-btn" data-product-id="${product.id}">
                                <i class="fas fa-cart-plus"></i>
                           </button>
                        </div>
                    </div>
                </div>`;
            productsList.appendChild(productCol);
        });
    }

    function showProductDetailsModal(product) {
        if (!productDetailsModalEl) return;
        productDetailsModalEl.querySelector('.modal-product-image').src = product.imageUrl || '/images/placeholder.png';
        productDetailsModalEl.querySelector('.modal-product-title').textContent = product.name;
        productDetailsModalEl.querySelector('.modal-product-description').textContent = product.description || "لا يوجد وصف متاح لهذا المنتج.";
        productDetailsModalEl.querySelector('.modal-product-price').textContent = `${product.price.toFixed(2)} جنيه`;
        productDetailsModalEl.querySelector('.modal-add-to-cart-btn').dataset.productId = product.id;
        productDetailsModal.show();
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

        if (desktopCartItemsContainer) {
            desktopCartItemsContainer.innerHTML = cartHtml;
            desktopCartTotal.textContent = `${total.toFixed(2)}`;
            desktopCheckoutBtn.disabled = !cartHasItems;
            desktopEmptyCartMsg.classList.toggle('d-none', cartHasItems);
        }

        if (mobileCartContent) {
            mobileCartContent.innerHTML = `
                <div class="modal-header">
                    <h5 class="modal-title"><i class="fas fa-shopping-cart me-2"></i>سلة المشتريات</h5>
                    <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                </div>
                <div class="modal-body">${cartHasItems ? cartHtml : '<p class="cart-empty-msg text-center text-muted">سلتك فارغة حالياً.</p>'}</div>
                <div class="modal-footer">
                    <div class="w-100">
                        <div class="d-flex justify-content-between"><strong>الإجمالي</strong><strong><span>${total.toFixed(2)}</span> جنيه</strong></div>
                        <div class="d-grid mt-3"><button id="checkout-btn" class="btn btn-success btn-lg" ${!cartHasItems ? 'disabled' : ''}><i class="fab fa-whatsapp me-2"></i>إتمام الطلب</button></div>
                    </div>
                </div>`;
        }

        if (mobileCartBtn && mobileCartCount) {
            mobileCartCount.textContent = totalItems;
            mobileCartBtn.classList.toggle('d-none', !cartHasItems);
        }
    }
    updateCartDisplay();
});

