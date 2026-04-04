package br.com.fiap.vinheriaagnello.web;

import br.com.fiap.vinheriaagnello.repository.InMemoryDatabase;
import jakarta.servlet.FilterChain;
import jakarta.servlet.ServletException;
import jakarta.servlet.annotation.WebFilter;
import jakarta.servlet.http.HttpFilter;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

import java.io.IOException;

@WebFilter(urlPatterns = "/app/*")
public class AuthFilter extends HttpFilter {
	@Override
	protected void doFilter(HttpServletRequest req, HttpServletResponse res, FilterChain chain)
			throws IOException, ServletException {
		String path = req.getRequestURI().substring(req.getContextPath().length());
		Object user = req.getSession(true).getAttribute(InMemoryDatabase.SESSION_USER_ATTR);
		if (user == null) {
			String ctx = req.getContextPath();
			res.sendRedirect(ctx + WebRoutes.AUTH_LOGIN);
			return;
		}
		chain.doFilter(req, res);
	}
}

