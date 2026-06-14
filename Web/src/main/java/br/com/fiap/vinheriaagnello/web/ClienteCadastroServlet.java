package br.com.fiap.vinheriaagnello.web;

import br.com.fiap.vinheriaagnello.model.Cliente;
import br.com.fiap.vinheriaagnello.repository.InMemoryDatabase;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.HttpServlet;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

import java.io.IOException;

@WebServlet(urlPatterns = "/auth/cadastro")
public class ClienteCadastroServlet extends HttpServlet {
	@Override
	protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException {
		req.getRequestDispatcher("/WEB-INF/jsp/auth/cadastro.jsp").forward(req, resp);
	}

	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException {
		req.setCharacterEncoding("UTF-8");
		String nome = req.getParameter("nome");
		String email = req.getParameter("email");
		String senha = req.getParameter("senha");

		if (nome == null || nome.isBlank() || email == null || email.isBlank() || senha == null || senha.isBlank()) {
			req.setAttribute("error", "Preencha nome, e-mail e senha.");
			req.getRequestDispatcher("/WEB-INF/jsp/auth/cadastro.jsp").forward(req, resp);
			return;
		}

		if (InMemoryDatabase.findClienteByEmail(email).isPresent()) {
			req.setAttribute("error", "Já existe um usuário com este e-mail.");
			req.getRequestDispatcher("/WEB-INF/jsp/auth/cadastro.jsp").forward(req, resp);
			return;
		}

		Cliente cliente = InMemoryDatabase.createCliente(nome.trim(), email.trim().toLowerCase());
		InMemoryDatabase.upsertCredential(cliente.getEmail(), senha);

		req.getSession(true).setAttribute(InMemoryDatabase.SESSION_USER_ATTR, cliente);
		resp.sendRedirect(req.getContextPath() + "/home");
	}
}

