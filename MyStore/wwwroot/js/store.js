document.addEventListener('DOMContentLoaded', function () {
    // --- CONFIGURATION & STATE ---
    const WHATSAPP_NUMBER = "201010475455"; // Your WhatsApp number
    let cart = [];

    // --- SELECTORS ---
    const currentStoreIdInput = document.getElementById('current-store-id');
    const checkoutModalEl = document.getElementById('checkoutModal');
    const checkoutModal = checkoutModalEl ? new bootstrap.Modal(checkoutModalEl) : null;
    const productDetailsModalEl = document.getElementById('productDetailsModal');
    const productDetailsModal = productDetailsModalEl ? new bootstrap.Modal(productDetailsModalEl) : null;

    // Common Selectors for both views
    const allProductWrappers = document.querySelectorAll('.product-card-wrapper');
    const allCompanyButtons = document.querySelectorAll('.company-card-btn');

    // Desktop
    const desktopSearchInput = document.getElementById('product-search-input-desktop');
    const desktopCartContainer = document.getElementById('cart-items-desktop');
    const desktopCartTotal = document.getElementById('cart-total-desktop');
    const desktopEmptyMsg = document.querySelector('#cart-sidebar .cart-empty-msg');

    // Mobile
    const mobileSearchInput = document.getElementById('product-search-input-mobile');
    const mobileCartContainer = document.getElementById('cart-items-mobile');
    const mobileCartTotal = document.getElementById('cart-total-mobile');
    const mobileEmptyMsg = document.querySelector('.cart-empty-msg-mobile');
    const bottomNavButtons = document.querySelectorAll('.app-bottom-nav .nav-btn');
    const appViews = document.querySelectorAll('.app-view');
    const mobileCartCount = document.getElementById('bottom-nav-cart-count');
    const mobileCartFooter = document.querySelector('.cart-footer-mobile');

    // --- CORE FUNCTIONS (Cart, Modals, Order) ---

    function showProductDetails(productData) {
        if (!productDetailsModalEl) return;
        document.getElementById('modal-product-image').src = productData.image;
        document.getElementById('modal-product-name').textContent = productData.name;
        document.getElementById('modal-product-description').textContent = productData.description;
        document.getElementById('modal-product-price').textContent = `${parseFloat(productData.price).toFixed(2)} جنيه`;
        const modalAddToCartBtn = document.getElementById('modal-add-to-cart-btn');
        modalAddToCartBtn.dataset.productId = productData.id;
        modalAddToCartBtn.dataset.productName = productData.name;
        modalAddToCartBtn.dataset.productPrice = productData.price;
        modalAddToCartBtn.dataset.productImage = productData.image;
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
        if (action === 'increment') cart[itemIndex].quantity++;
        else if (action === 'decrement') {
            cart[itemIndex].quantity--;
            if (cart[itemIndex].quantity <= 0) cart.splice(itemIndex, 1);
        } else if (action === 'remove') cart.splice(itemIndex, 1);
        updateCartDisplay();
    }

    function updateCartDisplay() {
        const total = cart.reduce((sum, item) => sum + item.price * item.quantity, 0);
        const totalItems = cart.reduce((sum, item) => sum + item.quantity, 0);
        const hasItems = cart.length > 0;
        const cartHtml = hasItems ? cart.map(item => `
            <div class="cart-item">
                <div class="item-info"><span class="item-name">${item.name}</span><span class="item-price">${item.quantity} × ${item.price.toFixed(2)} جنيه</span></div>
                <div class="item-controls">
                    <button class="btn btn-sm btn-outline-secondary qty-btn" data-product-id="${item.id}" data-action="increment">+</button>
                    <span class="item-qty">${item.quantity}</span>
                    <button class="btn btn-sm btn-outline-secondary qty-btn" data-product-id="${item.id}" data-action="decrement">-</button>
                    <button class="btn btn-sm text-danger remove-item" data-product-id="${item.id}"><i class="fas fa-trash-alt"></i></button>
                </div>
            </div>`).join('') : '';

        if (desktopCartContainer) {
            desktopCartContainer.innerHTML = cartHtml;
            desktopCartTotal.textContent = total.toFixed(2);
            document.querySelectorAll('.desktop-container .checkout-btn').forEach(btn => btn.disabled = !hasItems);
            if (desktopEmptyMsg) desktopEmptyMsg.style.display = hasItems ? 'none' : 'block';
        }

        if (mobileCartContainer) {
            mobileCartContainer.innerHTML = cartHtml;
            mobileCartTotal.textContent = total.toFixed(2);
            document.querySelectorAll('.app-shell .checkout-btn').forEach(btn => btn.disabled = !hasItems);
            if (mobileEmptyMsg) mobileEmptyMsg.style.display = hasItems ? 'none' : 'block';
            if (mobileCartCount) mobileCartCount.textContent = totalItems;
            const cartViewIsActive = document.getElementById('view-cart')?.classList.contains('active');
            if (mobileCartFooter) mobileCartFooter.style.display = (cartViewIsActive && hasItems) ? 'block' : 'none';
        }
    }

    async function submitOrder() {
        const confirmOrderBtn = document.getElementById('confirm-order-btn');
        const checkoutForm = document.getElementById('checkout-form');
        if (!checkoutForm.checkValidity()) {
            checkoutForm.classList.add('was-validated');
            return;
        }
        confirmOrderBtn.disabled = true;
        confirmOrderBtn.innerHTML = `<span class="spinner-border spinner-border-sm"></span> جارٍ التأكيد...`;
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
                const totalAmount = cart.reduce((s, i) => s + i.price * i.quantity, 0);
                let message = `*طلب جديد برقم: ${result.orderNumber}*\n\n*الاسم:* ${orderData.customerName}\n*الهاتف:* ${orderData.customerPhone}\n*العنوان:* ${orderData.customerAddress}\n\n*-- تفاصيل الطلب --*\n`;
                cart.forEach(item => { message += `- ${item.name} (الكمية: ${item.quantity}) - ${(item.price * item.quantity).toFixed(2)} جنيه\n`; });
                message += `\n*الإجمالي: ${totalAmount.toFixed(2)} جنيه*`;
                window.location.href = `https://wa.me/${WHATSAPP_NUMBER}?text=${encodeURIComponent(message)}`;
                cart = [];
                updateCartDisplay();
                checkoutModal.hide();
            } else {
                alert(`حدث خطأ: ${result.message}`);
            }
        } catch (error) {
            console.error("Order creation failed:", error);
            alert("فشل الاتصال بالخادم.");
        } finally {
            confirmOrderBtn.disabled = false;
            confirmOrderBtn.innerHTML = 'تأكيد الطلب';
        }
    }

    // --- FILTER & SEARCH ---
    function filterAndSearch() {
        const activeCompanyBtn = document.querySelector('.company-card-btn.active');
        const companyId = activeCompanyBtn ? activeCompanyBtn.dataset.companyId : '0';
        const desktopQuery = desktopSearchInput ? desktopSearchInput.value.toLowerCase() : '';
        const mobileQuery = mobileSearchInput ? mobileSearchInput.value.toLowerCase() : '';
        const currentQuery = window.innerWidth >= 992 ? desktopQuery : mobileQuery;

        allProductWrappers.forEach(card => {
            const companyMatch = companyId === '0' || card.dataset.companyId === companyId;
            const nameMatch = card.dataset.productName.toLowerCase().includes(currentQuery);
            card.style.display = companyMatch && nameMatch ? 'block' : 'none';
        });
    }

    // --- EVENT LISTENERS ---

    document.body.addEventListener('click', function (e) {
        const addToCartBtn = e.target.closest('.add-to-cart-btn');
        const productCardWrapper = e.target.closest('.product-card-wrapper');

        if (addToCartBtn) {
            e.stopPropagation();
            let productDataContainer = addToCartBtn.closest('.product-card-wrapper') || addToCartBtn;
            const product = {
                id: parseInt(productDataContainer.dataset.productId),
                name: productDataContainer.dataset.productName,
                price: parseFloat(productDataContainer.dataset.productPrice),
                imageUrl: productDataContainer.dataset.productImage
            };
            if (product.id) {
                addToCart(product);
                if (addToCartBtn.id === 'modal-add-to-cart-btn') {
                    productDetailsModal.hide();
                } else {
                    addToCartBtn.innerHTML = '<i class="fas fa-check"></i>';
                    setTimeout(() => {
                        const originalContent = addToCartBtn.closest('.product-card-wrapper') ? '<i class="fas fa-cart-plus"></i>' : '<i class="fas fa-cart-plus me-1"></i> إضافة للسلة';
                        addToCartBtn.innerHTML = originalContent;
                    }, 1000);
                }
            }
        } else if (productCardWrapper) {
            const productData = {
                id: parseInt(productCardWrapper.dataset.productId),
                name: productCardWrapper.dataset.productName,
                price: parseFloat(productCardWrapper.dataset.productPrice),
                image: productCardWrapper.dataset.productImage,
                description: productCardWrapper.dataset.productDescription
            };
            if (productData.id) showProductDetails(productData);
        }

        const qtyBtn = e.target.closest('.qty-btn');
        if (qtyBtn) updateQuantity(parseInt(qtyBtn.dataset.productId), qtyBtn.dataset.action);

        const removeItemBtn = e.target.closest('.remove-item');
        if (removeItemBtn) updateQuantity(parseInt(removeItemBtn.dataset.productId), 'remove');

        const checkoutBtn = e.target.closest('.checkout-btn');
        if (checkoutBtn && cart.length > 0 && checkoutModal) checkoutModal.show();

        const confirmOrderBtn = e.target.closest('#confirm-order-btn');
        if (confirmOrderBtn) submitOrder();
    });

    // Mobile Bottom Navigation
    bottomNavButtons.forEach(button => {
        button.addEventListener('click', () => {
            const targetViewId = button.dataset.view;
            appViews.forEach(view => view.classList.toggle('active', view.id === targetViewId));
            bottomNavButtons.forEach(btn => btn.classList.toggle('active', btn === button));
            updateCartDisplay();
        });
    });

    // Company/Brand Filter Buttons
    allCompanyButtons.forEach(button => {
        button.addEventListener('click', function () {
            const companyId = this.dataset.companyId;
            allCompanyButtons.forEach(btn => btn.classList.remove('active'));
            document.querySelectorAll(`.company-card-btn[data-company-id="${companyId}"]`).forEach(btn => btn.classList.add('active'));
            filterAndSearch();
        });
    });

    // Search Input Listeners
    desktopSearchInput?.addEventListener('input', filterAndSearch);
    mobileSearchInput?.addEventListener('input', filterAndSearch);

    // --- MANUAL CAROUSEL SCROLLING LOGIC ---
    function setupCarousel(containerId, prevBtnId, nextBtnId) {
        const container = document.getElementById(containerId);
        const prevBtn = document.getElementById(prevBtnId);
        const nextBtn = document.getElementById(nextBtnId);

        if (container && prevBtn && nextBtn) {
            const scrollAmount = 300;

            nextBtn.addEventListener('click', () => {
                container.scrollBy({ left: scrollAmount, behavior: 'smooth' });
            });

            prevBtn.addEventListener('click', () => {
                container.scrollBy({ left: -scrollAmount, behavior: 'smooth' });
            });
        }
    }

    // --- INITIALIZATION ---
    updateCartDisplay();
    setupCarousel('brands-scroll-desktop', 'scroll-prev-desktop', 'scroll-next-desktop');
    setupCarousel('brands-scroll-mobile', 'scroll-prev-mobile', 'scroll-next-mobile');
});