package br.com.fiap.vinheriaagnello.web;

import br.com.fiap.vinheriaagnello.repository.InMemoryDatabase;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.HttpServlet;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

import java.io.IOException;
import java.math.BigDecimal;

@WebServlet(urlPatterns = "/app/vinhos")
public class VinhoServlet extends HttpServlet {
	@Override
	protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException {
		req.setAttribute("vinhos", InMemoryDatabase.listVinhos());
		req.getRequestDispatcher("/WEB-INF/jsp/app/vinhos.jsp").forward(req, resp);
	}

	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException {
		req.setCharacterEncoding("UTF-8");
		String nome = req.getParameter("nome");
		String tipo = req.getParameter("tipo");
		String origem = req.getParameter("origem");
		String precoStr = req.getParameter("preco");

		if (nome == null || nome.isBlank() || origem == null || origem.isBlank() || precoStr == null || precoStr.isBlank()) {
			req.setAttribute("error", "Preencha nome, origem e preço.");
			doGet(req, resp);
			return;
		}

		BigDecimal preco;
		try {

			preco = new BigDecimal(precoStr.trim().replace(',', '.'));
			if (preco.compareTo(BigDecimal.ZERO) <= 0) {
				throw new NumberFormatException("Preço deve ser maior que zero");
			}
		} catch (NumberFormatException ex) {
			req.setAttribute("error", "Preço inválido. Ex: 199.90");
			doGet(req, resp);
			return;
		}

		InMemoryDatabase.createVinho(nome.trim(), (tipo == null || tipo.isBlank()) ? "Tinto" : tipo.trim(), origem.trim(), preco);
		resp.sendRedirect(req.getContextPath() + "/app/vinhos");
	}
}


