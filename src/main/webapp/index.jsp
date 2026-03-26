<%@ page contentType="text/html; charset=UTF-8" pageEncoding="UTF-8" %>

<jsp:include page="/WEB-INF/jsp/components/header.jsp" />

<main class="pt-16 md:pt-20 flex-grow">
    
    <div id="view-home" class="view-section block">
        <section class="relative h-[85vh] md:h-[90vh] flex items-center overflow-hidden">
            <div class="absolute inset-0 z-0">
                <img src="https://images.unsplash.com/photo-1510812431401-41d2bd2722f3?ixlib=rb-4.0.3&auto=format&fit=crop&w=2070&q=80" alt="Adega e Degustação" class="w-full h-full object-cover" />
                <div class="absolute inset-0 bg-gradient-to-t md:bg-gradient-to-r from-stone-950/90 via-stone-900/60 to-transparent"></div>
            </div>
            <div class="relative z-10 px-6 sm:px-12 md:px-24 w-full max-w-7xl mx-auto">
                <div class="max-w-2xl">
                    <span class="text-wine-300 tracking-[0.2em] uppercase text-xs font-bold mb-4 block flex items-center gap-2">
                        <i class="w-6 h-px bg-wine-300"></i> Tradição Italiana
                    </span>
                    <h1 class="font-serif text-5xl md:text-7xl lg:text-8xl text-white mb-6 leading-[1.1]">
                        O vinho certo,<br>para a hora <span class="text-wine-400 italic">certa</span>.
                    </h1>
                    <p class="text-sand-100 text-base md:text-xl mb-10 font-light leading-relaxed max-w-lg">
                        Deguste a curadoria exclusiva do Sr. Giulio com rótulos selecionados para momentos reais da sua vida.
                    </p>
                    <div class="flex flex-col sm:flex-row gap-4">
                        <button onclick="app.navigate('adega')" class="bg-wine-800 hover:bg-wine-700 text-white px-8 py-4 text-sm tracking-wide uppercase font-semibold transition-all duration-300 flex items-center justify-center gap-2">
                            Explorar a Adega
                        </button>
                        <button onclick="app.mockSommelier()" class="bg-transparent border border-sand-100/50 hover:bg-white/10 hover:border-white text-white px-8 py-4 text-sm tracking-wide uppercase font-semibold transition-all duration-300 backdrop-blur-sm w-full sm:w-auto">
                            Consultar Sommelier
                        </button>
                    </div>
                </div>
            </div>
        </section>

        <section class="py-16 md:py-24 bg-white">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8">
                <div class="flex justify-between items-end mb-10 border-b border-sand-200 pb-6">
                    <div>
                        <span class="text-wine-700 tracking-widest uppercase text-[10px] md:text-xs font-semibold mb-2 block">Seleção do Giulio</span>
                        <h2 class="font-serif text-3xl md:text-4xl text-stone-900">Destaques da Semana</h2>
                    </div>
                    <button onclick="app.navigate('adega')" class="hidden md:flex items-center gap-2 text-wine-900 font-medium hover:text-wine-600 transition-colors text-sm group">
                        Ver todos <i data-lucide="arrow-right" class="w-4 h-4 group-hover:translate-x-1 transition-transform"></i>
                    </button>
                </div>

                <div id="home-products" class="flex md:grid md:grid-cols-4 gap-6 overflow-x-auto no-scrollbar pb-4 snap-x"></div>
                
                <div class="mt-8 text-center md:hidden">
                    <button onclick="app.navigate('adega')" class="w-full border border-wine-800 text-wine-900 py-4 font-medium hover:bg-wine-50 transition-colors text-sm uppercase tracking-wide">
                        Ver Adega Completa
                    </button>
                </div>
            </div>
        </section>
    </div>
    
    <div id="view-adega" class="view-section hidden min-h-screen bg-sand-50 pb-10">
        <div class="bg-white border-b border-sand-200 sticky top-16 md:top-20 z-30 shadow-sm">
            <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-4">
                <div class="relative">
                    <i data-lucide="search" class="absolute left-3 top-1/2 transform -translate-y-1/2 text-stone-400 w-5 h-5"></i>
                    <input type="text" id="search-input" placeholder="Busque por uva, país ou nome..." 
                        class="w-full bg-sand-100 border-transparent focus:border-wine-500 focus:bg-white focus:ring-0 rounded-sm py-3 pl-10 pr-4 text-sm transition-all outline-none"
                        oninput="app.filterProducts(this.value)">
                </div>
                <div class="flex gap-2 mt-4 overflow-x-auto no-scrollbar pb-1">
                    <button onclick="app.filterCategory('all')" class="filter-btn active px-4 py-1.5 rounded-full border border-wine-800 bg-wine-800 text-white text-xs whitespace-nowrap transition-colors" data-cat="all">Todos</button>
                    <button onclick="app.filterCategory('Tinto')" class="filter-btn px-4 py-1.5 rounded-full border border-sand-300 text-stone-600 hover:border-wine-800 hover:text-wine-800 text-xs whitespace-nowrap transition-colors" data-cat="Tinto">Tintos</button>
                    <button onclick="app.filterCategory('Branco')" class="filter-btn px-4 py-1.5 rounded-full border border-sand-300 text-stone-600 hover:border-wine-800 hover:text-wine-800 text-xs whitespace-nowrap transition-colors" data-cat="Branco">Brancos</button>
                    <button onclick="app.filterCategory('Rosé')" class="filter-btn px-4 py-1.5 rounded-full border border-sand-300 text-stone-600 hover:border-wine-800 hover:text-wine-800 text-xs whitespace-nowrap transition-colors" data-cat="Rosé">Rosés</button>
                    <button onclick="app.filterCategory('Espumante')" class="filter-btn px-4 py-1.5 rounded-full border border-sand-300 text-stone-600 hover:border-wine-800 hover:text-wine-800 text-xs whitespace-nowrap transition-colors" data-cat="Espumante">Espumantes</button>
                </div>
            </div>
        </div>

        <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-8">
            <h2 class="font-serif text-2xl text-stone-900 mb-6 hidden md:block">Nossa Coleção</h2>
            <div id="adega-products" class="grid grid-cols-2 lg:grid-cols-4 md:gap-8 gap-4"></div>
            <div id="empty-state" class="hidden text-center py-20">
                <i data-lucide="wine-off" class="w-12 h-12 text-sand-400 mx-auto mb-4"></i>
                <p class="text-stone-500 font-medium">Nenhum vinho encontrado com este termo.</p>
            </div>
        </div>
    </div>
    
    <div id="view-sobre" class="view-section hidden bg-white">
        <section class="py-16 md:py-24 max-w-4xl mx-auto px-4 sm:px-6 lg:px-8">
            <div class="text-center mb-16">
                <span class="text-wine-700 tracking-widest uppercase text-xs font-semibold mb-3 block">Tradição & Família</span>
                <h1 class="font-serif text-4xl md:text-5xl text-stone-900 mb-6">Agnello, muito além da taça.</h1>
            </div>

            <div class="relative h-64 md:h-96 w-full mb-12 overflow-hidden rounded-sm shadow-xl">
                <img src="${pageContext.request.contextPath}/assets/img/giulio-bianca.png" alt="Giulio e Bianca na Vinheria Agnello" class="w-full h-full object-cover"/>
            </div>

            <div class="prose prose-stone prose-lg mx-auto text-stone-600 leading-relaxed">
                <p class="mb-6">Tudo começou com o balcão de madeira rústica, onde o Sr. Giulio, com seu sorriso acolhedor, passava horas entendendo não apenas o paladar, mas a história de cada cliente que entrava na Vinheria Agnello. <em>"O que você vai jantar hoje?"</em>, ele costumava perguntar. Essa era a chave para a garrafa perfeita.</p>
                
                <p class="mb-6">Por anos, fomos exclusivamente uma loja física. Acreditávamos que a experiência de escolher um vinho exigia o toque, o olhar e a conversa. Porém, o mundo mudou, e a saudade dos nossos clientes que se mudaram ou que não tinham mais tempo para visitas nos fez refletir.</p>

                <h3 class="font-serif text-2xl text-stone-900 mt-10 mb-4">O Desafio do Digital</h3>
                <p class="mb-6">Como traduzir a nossa alma para uma tela fria de vidro? A internet está cheia de catálogos intermináveis e filtros complexos que mais confundem do que ajudam. Bianca, filha do Sr. Giulio, percebeu que precisávamos de uma abordagem diferente: <strong>o digital focado na experiência humana.</strong></p>

                <p class="mb-6">Nós não transportamos o nosso estoque inteiro para a internet. Nós transportamos a nossa <em>curadoria</em>. Cada vinho que você encontra aqui passou pelo crivo rigoroso da família. Nossa missão é evitar a fadiga de decisão. Você não precisa ser um sommelier profissional para escolher um bom vinho; nós fazemos o trabalho duro para que você apenas desfrute o momento.</p>

                <blockquote class="border-l-4 border-wine-700 pl-6 my-8 italic text-stone-500">
                    "O melhor vinho não é o mais caro. É aquele que você abre cercado de boas companhias, sem medo de errar na escolha." — Giulio Agnello
                </blockquote>

                <p>Seja bem-vindo à nossa casa digital. E se precisar de qualquer ajuda, a Bianca está à disposição no botão do Sommelier. Salute!</p>
            </div>
        </section>
    </div>
</main>

<jsp:include page="/WEB-INF/jsp/components/footer.jsp" />