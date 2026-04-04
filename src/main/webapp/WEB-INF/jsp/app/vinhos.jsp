<%@ page contentType="text/html; charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>

<jsp:include page="/WEB-INF/jsp/components/header.jsp" />

<main class="pt-20 md:pt-24 flex-grow bg-sand-50">
    <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
        <div class="flex flex-col md:flex-row md:items-end md:justify-between gap-4 mb-8 border-b border-sand-200 pb-6">
            <div>
                <span class="text-wine-700 tracking-widest uppercase text-[10px] md:text-xs font-semibold mb-2 block">Admin</span>
                <h1 class="font-serif text-3xl md:text-4xl text-stone-900">Vinhos</h1>
            </div>
            <div class="flex flex-wrap gap-3 text-sm">
                <a class="px-4 py-2 rounded-sm border border-sand-300 bg-white hover:bg-sand-100 transition-colors" href="${pageContext.request.contextPath}/app/clientes">Clientes</a>
                <a class="px-4 py-2 rounded-sm border border-wine-800 bg-wine-800 text-white hover:bg-wine-900 transition-colors" href="${pageContext.request.contextPath}/home">Voltar para a vitrine</a>
                <a class="px-4 py-2 rounded-sm border border-red-200 bg-red-50 text-red-800 hover:bg-red-100 transition-colors" href="${pageContext.request.contextPath}/auth/logout">Sair</a>
            </div>
        </div>

        <c:if test="${not empty error}">
            <div class="mb-6 p-4 rounded-sm bg-red-50 text-red-800 text-sm border border-red-200">${error}</div>
        </c:if>

        <div class="grid lg:grid-cols-5 gap-6">
            <div class="lg:col-span-2 bg-white border border-sand-200 rounded-sm shadow-sm p-6">
                <h2 class="font-serif text-xl text-wine-950 mb-1 flex items-center gap-2">
                    Cadastrar vinho
                </h2>
                <p class="text-sm text-stone-500 mb-6">Adicione um novo rótulo ao catálogo.</p>

                <form method="post" action="${pageContext.request.contextPath}/app/vinhos" class="space-y-4">
                    <div>
                        <label class="block text-xs font-semibold uppercase tracking-wide text-stone-500 mb-2">Nome</label>
                        <input name="nome" required class="w-full border border-sand-300 rounded-sm px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-wine-300 focus:border-wine-500" placeholder="Ex: Barolo Riserva" />
                    </div>
                    <div>
                        <label class="block text-xs font-semibold uppercase tracking-wide text-stone-500 mb-2">Tipo</label>
                        <select name="tipo" class="w-full border border-sand-300 rounded-sm px-3 py-2.5 bg-white focus:outline-none focus:ring-2 focus:ring-wine-300 focus:border-wine-500">
                            <option>Tinto</option>
                            <option>Branco</option>
                            <option>Rosé</option>
                            <option>Espumante</option>
                        </select>
                    </div>
                    <div>
                        <label class="block text-xs font-semibold uppercase tracking-wide text-stone-500 mb-2">Origem</label>
                        <input name="origem" required class="w-full border border-sand-300 rounded-sm px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-wine-300 focus:border-wine-500" placeholder="Região, País" />
                    </div>
                    <div>
                        <label class="block text-xs font-semibold uppercase tracking-wide text-stone-500 mb-2">Preço</label>
                        <input name="preco" required class="w-full border border-sand-300 rounded-sm px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-wine-300 focus:border-wine-500" placeholder="199.90" />
                        <p class="mt-2 text-xs text-stone-400">Aceita ponto ou vírgula. Ex.: 199.90 ou 199,90</p>
                    </div>
                    <button class="w-full bg-wine-800 hover:bg-wine-900 text-white py-3 rounded-sm font-medium transition-colors">Salvar</button>
                </form>
            </div>

            <div class="lg:col-span-3 bg-white border border-sand-200 rounded-sm shadow-sm overflow-hidden">
                <div class="p-6 border-b border-sand-200 bg-sand-50 flex items-center justify-between">
                    <h3 class="font-serif text-xl text-stone-900">Catálogo</h3>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-sm">
                        <thead class="bg-sand-100 text-stone-600">
                        <tr>
                            <th class="text-left p-4">ID</th>
                            <th class="text-left p-4">Nome</th>
                            <th class="text-left p-4">Tipo</th>
                            <th class="text-left p-4">Origem</th>
                            <th class="text-left p-4">Preço</th>
                        </tr>
                        </thead>
                        <tbody>
                        <c:forEach var="v" items="${vinhos}">
                            <tr class="border-t border-sand-100 hover:bg-sand-50/70 transition-colors">
                                <td class="p-4 text-stone-500">${v.id}</td>
                                <td class="p-4 font-medium text-stone-900">${v.nome}</td>
                                <td class="p-4 text-stone-600">${v.tipo}</td>
                                <td class="p-4 text-stone-600">${v.origem}</td>
                                <td class="p-4 font-medium text-wine-900">R$ ${v.preco}</td>
                            </tr>
                        </c:forEach>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</main>

<jsp:include page="/WEB-INF/jsp/components/footer.jsp" />
