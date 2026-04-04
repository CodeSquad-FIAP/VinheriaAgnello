<%@ page contentType="text/html; charset=UTF-8" pageEncoding="UTF-8" %>

<footer id="main-footer" class="bg-wine-950 text-wine-100 py-16 border-t border-wine-900">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 grid grid-cols-1 md:grid-cols-3 gap-12">
        <div>
            <div class="flex items-center gap-2 mb-6 cursor-pointer" onclick="app.navigate('home')">
                <i data-lucide="wine" class="text-wine-400 h-6 w-6"></i>
                <span class="font-serif text-xl font-semibold text-white">Agnello</span>
            </div>
            <p class="text-sm text-wine-300 max-w-xs leading-relaxed">Trazendo a tradição e o calor do atendimento italiano para o ambiente digital. O mesmo cuidado da nossa loja física, agora onde você estiver.</p>
        </div>
        <div>
            <h4 class="text-white font-medium mb-4 uppercase tracking-wider text-sm">Nossa Casa</h4>
            <ul class="space-y-3 text-sm text-wine-300">
                <li><button onclick="app.navigate('home')" class="hover:text-white transition-colors">Início</button></li>
                <li><button onclick="app.navigate('adega')" class="hover:text-white transition-colors">A Adega</button></li>
                <li><button onclick="app.navigate('sobre')" class="hover:text-white transition-colors">Nossa História</button></li>
            </ul>
        </div>
        <div>
            <h4 class="text-white font-medium mb-4 uppercase tracking-wider text-sm">Fale Conosco</h4>
            <ul class="space-y-3 text-sm text-wine-300">
                <li class="flex items-center gap-3"><i data-lucide="map-pin" class="w-4 h-4 text-wine-500"></i> Rua das Videiras, 123 - SP</li>
                <li class="flex items-center gap-3"><i data-lucide="phone" class="w-4 h-4 text-wine-500"></i> (11) 99999-9999</li>
                <li>
                    <button onclick="app.mockSommelier()" class="mt-4 bg-wine-800 hover:bg-wine-700 text-white px-4 py-2 text-xs font-medium rounded-sm transition-colors flex items-center gap-2">
                        <i data-lucide="message-circle" class="w-4 h-4"></i> Chamar a Bianca
                    </button>
                </li>
            </ul>
        </div>
    </div>
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 mt-16 pt-8 border-t border-wine-900/50 flex flex-col md:flex-row justify-between items-center gap-4 text-xs text-wine-400/60">
        <p>&copy; 2026 Vinheria Agnello. Venda proibida para menores de 18 anos.</p>
        <p>Beba com moderação.</p>
    </div>
</footer>

<div id="cart-overlay" class="fixed inset-0 bg-stone-900/50 z-50 hidden opacity-0 transition-opacity duration-300" onclick="app.toggleCart()"></div>
<div id="cart-drawer" class="fixed inset-y-0 right-0 w-full md:w-96 bg-white z-50 transform translate-x-full transition-transform duration-300 shadow-2xl flex flex-col">
    <div class="p-4 border-b border-sand-200 flex justify-between items-center bg-sand-50">
        <h2 class="font-serif text-xl font-semibold text-wine-950 flex items-center gap-2">
            <i data-lucide="shopping-bag" class="w-5 h-5"></i> Sua Sacola
        </h2>
        <button onclick="app.toggleCart()" class="p-2 text-stone-400 hover:text-stone-700 transition-colors">
            <i data-lucide="x" class="w-6 h-6"></i>
        </button>
    </div>

    <div id="cart-items" class="flex-1 overflow-y-auto p-4 space-y-4">
    </div>

    <div class="p-6 border-t border-sand-200 bg-sand-50">
        <div class="flex justify-between items-center mb-4">
            <span class="text-stone-500 text-sm">Total</span>
            <span id="cart-total" class="text-2xl font-serif font-bold text-wine-900">R$ 0,00</span>
        </div>
        <button onclick="app.checkout()" class="w-full bg-wine-800 hover:bg-wine-900 text-white py-4 rounded-sm font-medium transition-colors flex justify-center items-center gap-2">
            Finalizar Compra <i data-lucide="arrow-right" class="w-4 h-4"></i>
        </button>
    </div>
</div>

<div id="toast" class="fixed bottom-20 md:bottom-10 left-1/2 transform -translate-x-1/2 bg-stone-900 text-white px-6 py-3 rounded-full text-sm font-medium shadow-xl z-50 transition-all duration-300 opacity-0 pointer-events-none translate-y-4 flex items-center gap-2">
    <i data-lucide="check-circle-2" class="w-4 h-4 text-green-400"></i>
    <span>Adicionado à sacola</span>
</div>

<script src="<%= request.getContextPath() %>/assets/js/app.js"></script>
</body>
</html>