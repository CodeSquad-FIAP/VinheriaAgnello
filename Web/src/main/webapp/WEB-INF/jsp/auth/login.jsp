<%@ page contentType="text/html; charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="jakarta.tags.core" %>

<jsp:include page="/WEB-INF/jsp/components/header.jsp" />

<main class="pt-20 md:pt-24 flex-grow bg-sand-50">
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
    <div class="grid lg:grid-cols-2 gap-8 items-stretch">
      <section class="hidden lg:block relative overflow-hidden rounded-sm border border-sand-200 bg-white shadow-sm">
        <div class="absolute inset-0">
          <img
                  src="https://images.unsplash.com/photo-1510812431401-41d2bd2722f3?auto=format&fit=crop&w=2000&q=80"
                  alt="Adega"
                  class="w-full h-full object-cover"
          />
          <div class="absolute inset-0 bg-gradient-to-t from-stone-950/80 via-stone-900/40 to-transparent"></div>
        </div>
        <div class="relative p-8 h-full flex flex-col justify-end">
          <span class="text-wine-300 tracking-[0.2em] uppercase text-xs font-bold mb-3 block">Vinheria Agnello</span>
          <h1 class="font-serif text-4xl text-white leading-tight">Bem-vindo de volta</h1>
          <p class="mt-3 text-sand-100/90">Entre para acessar as telas administrativas (vinhos e clientes).</p>
        </div>
      </section>

      <section class="bg-white border border-sand-200 rounded-sm shadow-sm p-6 md:p-8">
        <span class="text-wine-700 tracking-widest uppercase text-[10px] md:text-xs font-semibold mb-2 block">Acesso</span>
        <h2 class="font-serif text-3xl text-stone-900 mb-2">Entrar</h2>
        <p class="text-sm text-stone-500 mb-6">Use <strong>admin@agnello.com</strong> / <strong>admin</strong> para testar.</p>

        <c:if test="${not empty error}">
          <div class="mb-6 p-4 rounded-sm bg-red-50 text-red-800 text-sm border border-red-200">${error}</div>
        </c:if>

        <form method="post" action="${pageContext.request.contextPath}/auth/login" class="space-y-4">
          <div>
            <label class="block text-xs font-semibold uppercase tracking-wide text-stone-500 mb-2">E-mail</label>
            <input name="email" type="email" required class="w-full border border-sand-300 rounded-sm px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-wine-300 focus:border-wine-500" placeholder="seuemail@exemplo.com" />
          </div>
          <div>
            <label class="block text-xs font-semibold uppercase tracking-wide text-stone-500 mb-2">Senha</label>
            <input name="senha" type="password" required class="w-full border border-sand-300 rounded-sm px-3 py-2.5 focus:outline-none focus:ring-2 focus:ring-wine-300 focus:border-wine-500" placeholder="••••••••" />
          </div>
          <button class="w-full bg-wine-800 hover:bg-wine-900 text-white py-3 rounded-sm font-medium transition-colors">Entrar</button>
        </form>

        <div class="mt-6 text-sm text-stone-600">
          Não tem conta?
          <a class="text-wine-800 underline" href="${pageContext.request.contextPath}/auth/cadastro">Cadastrar</a>
        </div>
      </section>
    </div>
  </div>
</main>

<jsp:include page="/WEB-INF/jsp/components/footer.jsp" />