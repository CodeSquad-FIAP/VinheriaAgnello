<%@ page contentType="text/html; charset=UTF-8" pageEncoding="UTF-8" %>
<%@ taglib prefix="c" uri="http://java.sun.com/jsp/jstl/core" %>

<jsp:include page="/WEB-INF/jsp/components/header.jsp" />

<main class="pt-20 md:pt-24 flex-grow bg-sand-50">
  <div class="max-w-7xl mx-auto px-4 sm:px-6 lg:px-8 py-10">
    <div class="flex flex-col md:flex-row md:items-end md:justify-between gap-4 mb-8 border-b border-sand-200 pb-6">
      <div>
        <span class="text-wine-700 tracking-widest uppercase text-[10px] md:text-xs font-semibold mb-2 block">Admin</span>
        <h1 class="font-serif text-3xl md:text-4xl text-stone-900">Clientes</h1>
      </div>
      <div class="flex flex-wrap gap-3 text-sm">
        <a class="px-4 py-2 rounded-sm border border-sand-300 bg-white hover:bg-sand-100 transition-colors" href="${pageContext.request.contextPath}/app/vinhos">Vinhos</a>
        <a class="px-4 py-2 rounded-sm border border-wine-800 bg-wine-800 text-white hover:bg-wine-900 transition-colors" href="${pageContext.request.contextPath}/app/home">Voltar para a vitrine</a>
        <a class="px-4 py-2 rounded-sm border border-red-200 bg-red-50 text-red-800 hover:bg-red-100 transition-colors" href="${pageContext.request.contextPath}/auth/logout">Sair</a>
      </div>
    </div>

    <div class="bg-white border border-sand-200 rounded-sm shadow-sm overflow-hidden">
      <div class="overflow-x-auto">
        <table class="w-full text-sm">
          <thead class="bg-sand-100 text-stone-600">
          <tr>
            <th class="text-left p-4">ID</th>
            <th class="text-left p-4">Nome</th>
            <th class="text-left p-4">E-mail</th>
          </tr>
          </thead>
          <tbody>
          <c:forEach var="c" items="${clientes}">
            <tr class="border-t border-sand-100 hover:bg-sand-50/70 transition-colors">
              <td class="p-4 text-stone-500">${c.id}</td>
              <td class="p-4 font-medium text-stone-900">${c.nome}</td>
              <td class="p-4 text-stone-600">${c.email}</td>
            </tr>
          </c:forEach>
          </tbody>
        </table>
      </div>
    </div>
  </div>
</main>

<jsp:include page="/WEB-INF/jsp/components/footer.jsp" />

