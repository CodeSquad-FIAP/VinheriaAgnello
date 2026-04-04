package br.com.fiap.vinheriaagnello.web;

import br.com.fiap.vinheriaagnello.model.Cliente;
import br.com.fiap.vinheriaagnello.repository.InMemoryDatabase;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.HttpServlet;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

import java.io.IOException;

@WebServlet(urlPatterns = {WebRoutes.AUTH_LOGIN, WebRoutes.LOGIN})
public class LoginServlet extends HttpServlet {
	@Override
	protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException {
		req.getRequestDispatcher("/WEB-INF/jsp/auth/login.jsp").forward(req, resp);
	}

	@Override
	protected void doPost(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException {
		req.setCharacterEncoding("UTF-8");
		String email = req.getParameter("email");
		String senha = req.getParameter("senha");

		if (!InMemoryDatabase.authenticate(email, senha)) {
			req.setAttribute("error", "E-mail ou senha inválidos.");
			req.getRequestDispatcher("/WEB-INF/jsp/auth/login.jsp").forward(req, resp);
			return;
		}

		Cliente user = InMemoryDatabase.findClienteByEmail(email)
				.orElseGet(() -> InMemoryDatabase.createCliente("Usuário", email));
		req.getSession(true).setAttribute(InMemoryDatabase.SESSION_USER_ATTR, user);
		resp.sendRedirect(req.getContextPath() + "/home");
	}
}

