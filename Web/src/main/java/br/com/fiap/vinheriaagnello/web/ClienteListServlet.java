package br.com.fiap.vinheriaagnello.web;

import br.com.fiap.vinheriaagnello.repository.InMemoryDatabase;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.HttpServlet;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

import java.io.IOException;

@WebServlet(urlPatterns = "/app/clientes")
public class ClienteListServlet extends HttpServlet {
	@Override
	protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws ServletException, IOException {
		req.setAttribute("clientes", InMemoryDatabase.listClientes());
		req.getRequestDispatcher("/WEB-INF/jsp/app/clientes.jsp").forward(req, resp);
	}
}

