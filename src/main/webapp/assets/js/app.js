const productsDB = [
    { id: 1, name: 'Brunello di Montalcino DOCG', type: 'Tinto', origin: 'Toscana, IT', price: 429.00, img: 'https://images.unsplash.com/photo-1681210744266-c6a23b8ed176?q=80&w=698&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D', desc: 'Estruturado e majestoso. Perfeito para uma macarronada em família no domingo.', featured: true },
    { id: 2, name: "Chardonnay Reserva", type: "Branco", origin: "Mendoza, AR", price: 145.00, img: "https://images.unsplash.com/photo-1587000098522-812f8896e8df?q=80&w=735&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", desc: "Fresco, com notas amanteigadas. Ideal para acompanhar peixes assados.", featured: true },
    { id: 3, name: "Côtes de Provence", type: "Rosé", origin: "Provence, FR", price: 180.00, img: "https://images.unsplash.com/photo-1593455427837-93e1f58b0df2?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", desc: "Leve, frutado e elegante. Para beber gelado em uma tarde na varanda.", featured: true },
    { id: 4, name: "Prosecco Valdobbiadene", type: "Espumante", origin: "Veneto, IT", price: 165.00, img: "https://images.unsplash.com/photo-1620421122564-760cfab6bd4a?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", desc: "Borbulhas finas e refrescantes. A melhor forma de celebrar conquistas.", featured: true },
    { id: 5, name: "Barolo DOCG", type: "Tinto", origin: "Piemonte, IT", price: 520.00, img: "https://images.unsplash.com/photo-1687952291665-8e46d794e0cd?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", desc: "O rei dos vinhos italianos. Complexo, com notas de trufas e rosas secas.", featured: false },
    { id: 6, name: "Sauvignon Blanc", type: "Branco", origin: "Marlborough, NZ", price: 130.00, img: "https://images.unsplash.com/photo-1736217292598-a200e3f0aae8?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", desc: "Acidez vibrante, notas de maracujá e um frescor inconfundível.", featured: false },
    { id: 7, name: "Malbec Gran Reserva", type: "Tinto", origin: "Salta, AR", price: 210.00, img: "https://images.unsplash.com/photo-1585867313424-06b0fd07d314?q=80&w=1100&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", desc: "Encorpado, taninos doces e muita fruta escura. Exige uma boa carne vermelha.", featured: false },
    { id: 8, name: "Pinot Noir", type: "Tinto", origin: "Borgonha, FR", price: 350.00, img: "https://images.unsplash.com/photo-1620679393872-2501651282c1?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", desc: "Elegante, sutil e delicado. Frutas vermelhas frescas que dançam na taça.", featured: false },
    { id: 9, name: "Primitivo di Manduria", type: "Tinto", origin: "Puglia, IT", price: 185.00, img: "https://images.unsplash.com/photo-1566065739035-8148a9aa8548?q=80&w=736&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", desc: "Intenso e aveludado, com toque de geleia de amora. Um abraço em forma de vinho.", featured: false },
    { id: 10, name: "Chianti Classico Riserva", type: "Tinto", origin: "Toscana, IT", price: 230.00, img: "https://images.unsplash.com/photo-1568930157403-9ad464e5f075?q=80&w=687&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", desc: "Equilibrado, acidez no ponto. Pede um prato com molho de tomate fresco.", featured: false },
    { id: 11, name: "Carmenère Estate", type: "Tinto", origin: "Valle de Colchagua, CL", price: 110.00, img: "https://plus.unsplash.com/premium_photo-1698430568354-d01ce48ea22f?q=80&w=688&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", desc: "Especiarias, pimenta e frutas maduras. Excelente relação qualidade e preço.", featured: false },
    { id: 12, name: "Champagne Brut", type: "Espumante", origin: "Champagne, FR", price: 490.00, img: "https://images.unsplash.com/photo-1666351028798-8920e510b705?q=80&w=735&auto=format&fit=crop&ixlib=rb-4.1.0&ixid=M3wxMjA3fDB8MHxwaG90by1wYWdlfHx8fGVufDB8fHx8fA%3D%3D", desc: "O clássico francês. Brioche, maçã verde e uma elegância inigualável.", featured: false }
];

const app = {
    state: {
        cart: [],
        currentView: 'home',
        searchQuery: '',
        currentCategory: 'all'
    },

    init() {
        lucide.createIcons();
        this.renderHomeProducts();
        this.renderAdegaProducts();
        this.updateCartUI();
    },

    formatMoney(value) {
        return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
    },

    navigate(view, focusSearch = false) {
        this.state.currentView = view;

        document.querySelectorAll('.view-section').forEach(el => el.classList.add('hidden'));
        document.getElementById(`view-${view}`).classList.remove('hidden'); // CORRIGIDO: crases em vez de aspas

        const footer = document.getElementById('main-footer');
        if (footer) {
            if (view === 'adega') {
                footer.classList.add('hidden');
            } else {
                footer.classList.remove('hidden');
            }
        }

        document.querySelectorAll('.nav-link, .mobile-nav-link').forEach(el => { // CORRIGIDO: faltando o ponto na classe nav-link
            const isTarget = el.dataset.target === view;
            if (el.classList.contains('mobile-nav-link')) {
                el.className = `mobile-nav-link flex flex-col items-center gap-1 w-full transition-colors ${isTarget ? 'text-wine-800' : 'text-stone-400 hover:text-wine-800'}`; // CORRIGIDO: className em vez de classList
            } else {
                el.className = `nav-link font-medium transition-colors ${isTarget ? 'text-wine-800' : 'text-stone-500 hover:text-wine-800'}`;
            }
        });

        window.scrollTo(0, 0);

        if (focusSearch && view === 'adega') {
            setTimeout(() => document.getElementById('search-input').focus(), 100);
        }
    },

    createProductCard(product, isCompact = false) {
        const compactClasses = isCompact ? 'min-w-[260px] md:min-w-0 snap-center' : '';
        const highlightBadge = product.featured ? `<div class="absolute top-3 left-3 bg-wine-900 text-white text-[9px] font-bold px-2 py-1 uppercase tracking-wider z-10">Destaque</div>` : ''; // CORRIGIDO: left-3

        const fallbackImg = 'https://images.unsplash.com/photo-1510812431401-41d2bd2722f3?ixlib=rb-4.0.3&auto=format&fit=crop&w=600&q=80';

        return `
            <div class="group cursor-pointer flex flex-col h-full bg-white rounded-md border-sand-100 overflow-hidden ${compactClasses}">
                <div class="relative aspect-[4/5] overflow-hidden bg-sand-200">
                    ${highlightBadge}
                    <img loading="lazy" src="${product.img}" alt="${product.name}" onerror="this.onerror=null;this.src='${fallbackImg}'" class="absolute inset-0 w-full h-full object-cover transition-transform duration-700 group-hover:scale-105" />
                    <div class="absolute inset-0 bg-stone-900/10 group-hover:bg-transparent transition-colors duration-500"></div>
                </div>
                <div class="p-5 flex flex-col flex-grow">
                    <span class="text-[10px] md:text-xs text-stone-400 uppercase tracking-wider mb-2 block font-medium">${product.type} • ${product.origin}</span>
                    <h3 class="font-serif text-lg md:text-xl text-stone-900 mb-2 leading-tight">${product.name}</h3>
                    <p class="text-stone-500 text-xs md:text-sm mb-5 line-clamp-2 flex-grow">${product.desc}</p>
                    <div class="flex justify-between items-center mt-auto pt-4 border-t border-sand-100">
                        <span class="text-lg font-semibold text-wine-900">${this.formatMoney(product.price)}</span>
                        <button onclick="app.addToCart(${product.id})" aria-label="Adicionar ${product.name}" class="bg-wine-50 text-wine-800 p-2 rounded-full hover:bg-wine-800 hover:text-white transition-colors">
                            <i data-lucide="plus" class="w-5 h-5"></i>
                        </button>
                    </div>
                </div>
            </div>
        `;
    },

    renderHomeProducts() {
        const container = document.getElementById('home-products');
        const featured = productsDB.filter(p => p.featured).slice(0, 4);
        container.innerHTML = featured.map(p => this.createProductCard(p, true)).join('');
        lucide.createIcons();
    },

    renderAdegaProducts() {
        const container = document.getElementById('adega-products');
        const emptyState = document.getElementById('empty-state');

        let filtered = productsDB;

        if (this.state.currentCategory !== 'all') {
            filtered = filtered.filter(p => p.type === this.state.currentCategory);
        }

        if (this.state.searchQuery) {
            const q = this.state.searchQuery.toLowerCase(); // CORRIGIDO: estava this.create.searchQuery
            filtered = filtered.filter(p =>
                p.name.toLowerCase().includes(q) ||
                p.type.toLowerCase().includes(q) ||
                p.origin.toLowerCase().includes(q)
            );
        }

        if (filtered.length === 0) {
            container.innerHTML = '';
            emptyState.classList.remove('hidden');
        } else {
            emptyState.classList.add('hidden');
            container.innerHTML = filtered.map(p => this.createProductCard(p)).join('');
        }
        lucide.createIcons();
    },

    filterProducts(query) {
        this.state.searchQuery = query;
        this.renderAdegaProducts();
    },

    filterCategory(cat) {
        this.state.currentCategory = cat;
        document.querySelectorAll('.filter-btn').forEach(btn => {
            if (btn.dataset.cat === cat) {
                btn.className = 'filter-btn active px-4 py-1.5 rounded-full border border-wine-800 bg-wine-800 text-white text-xs whitespace-nowrap transition-colors';
            } else {
                btn.className = 'filter-btn px-4 py-1.5 rounded-full border border-sand-300 text-stone-600 hover:border-wine-800 hover:text-wine-800 text-xs whitespace-nowrap transition-colors';
            }
        });
        this.renderAdegaProducts();
    },

    addToCart(productId) {
        const product = productsDB.find(p => p.id === productId);
        if (!product) return;

        const existingItem = this.state.cart.find(item => item.id === productId);
        if (existingItem) {
            existingItem.qtd++;
        } else {
            this.state.cart.push({ ...product, qtd: 1 });
        }

        this.updateCartUI();
        this.showToast();

    },

    removeFromCart(productId) {
        this.state.cart = this.state.cart.filter(item => item.id !== productId);
        this.updateCartUI();
    },

    changeQtd(productId, delta) {
        const item = this.state.cart.find(item => item.id === productId);
        if (!item) return;

        item.qtd += delta;
        if (item.qtd <= 0) {
            this.removeFromCart(productId);
        } else {
            this.updateCartUI();
        }
    },

    toggleCart() {
        const drawer = document.getElementById('cart-drawer');
        const overlay = document.getElementById('cart-overlay');
        const isClosed = drawer.classList.contains('translate-x-full');

        if (isClosed) {
            overlay.classList.remove('hidden');
            setTimeout(() => overlay.classList.remove('opacity-0'), 10);
            drawer.classList.remove('translate-x-full');
        } else {
            overlay.classList.add('opacity-0');
            drawer.classList.add('translate-x-full');
            setTimeout(() => overlay.classList.add('hidden'), 300);
        }
    },

    updateCartUI() {
        const container = document.getElementById('cart-items');
        const totalEl = document.getElementById('cart-total');
        const badges = [document.getElementById('cart-badge'), document.getElementById('mobile-cart-badge')];

        const totalItems = this.state.cart.reduce((sum, item) => sum + item.qtd, 0);
        const totalPrice = this.state.cart.reduce((sum, item) => sum + (item.price * item.qtd), 0);

        badges.forEach(badge => {
            if (totalItems > 0) {
                badge.textContent = totalItems;
                badge.classList.remove('hidden');
            } else {
                badge.classList.add('hidden');
            }
        });

        totalEl.textContent = this.formatMoney(totalPrice);

        const fallbackImg = 'https://images.unsplash.com/photo-1510812431401-41d2bd2722f3?ixlib=rb-4.0.3&auto=format&fit=crop&w=600&q=80';

        if (this.state.cart.length === 0) {
            container.innerHTML = `
                <div class="h-full flex flex-col items-center justify-center text-stone-400">
                    <i data-lucide="shopping-cart" class="w-12 h-12 mb-4 opacity-50"></i>
                    <p>Sua sacola está vazia.</p>
                </div>
            `;
        } else {
            container.innerHTML = this.state.cart.map(item => `
                <div class="flex gap-4 bg-white p-3 rounded-md border border-sand-100 shadow-sm">
                    <div class="w-16 h-16 bg-sand-100 rounded overflow-hidden flex-shrink-0">
                        <img src="${item.img}" alt="${item.name}" onerror="this.onerror=null;this.src='${fallbackImg}'" class="w-full h-full object-cover">
                    </div>
                    <div class="flex-1">
                        <h4 class="font-serif text-sm font-semibold text-stone-900 line-clamp-1">${item.name}</h4>
                        <div class="text-wine-800 font-medium text-sm mt-1">${this.formatMoney(item.price)}</div>
                        <div class="flex items-center gap-3 mt-2">
                            <div class="flex items-center bg-sand-100 rounded-sm">
                                <button onclick="app.changeQtd(${item.id}, -1)" class="px-2 py-0.5 text-stone-500 hover:text-stone-900">-</button>
                                <span class="text-xs font-medium w-4 text-center">${item.qtd}</span>
                                <button onclick="app.changeQtd(${item.id}, 1)" class="px-2 py-0.5 text-stone-500 hover:text-stone-900">+</button>
                            </div>
                            <button onclick="app.removeFromCart(${item.id})" class="text-xs text-stone-400 hover:text-red-500 underline ml-auto">Remover</button>
                        </div>
                    </div>
                </div>
            `).join('');
        }
        lucide.createIcons();
    },

    showToast() {
        const toast = document.getElementById('toast');
        toast.classList.remove('opacity-0', 'translate-y-4', 'pointer-events-none');

        if (this.toastTimeout) clearTimeout(this.toastTimeout);

        this.toastTimeout = setTimeout(() => {
            toast.classList.add('opacity-0', 'translate-y-4', 'pointer-events-none');
        }, 2500);
    },

    mockSommelier() {
        alert('Redirecionando para o WhatsApp da Bianca (Sommelier)...');
    },

    checkout() {
        if (this.state.cart.length === 0) {
            alert('Sua sacola está vazia.');
            return;
        }
        alert('Redirecionando para o gateway de pagamento simulado...');
    }
};

document.addEventListener('DOMContentLoaded', () => {
    app.init();
});