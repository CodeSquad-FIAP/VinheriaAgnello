package br.com.fiap.vinheriaagnello.web;

import jakarta.servlet.annotation.WebServlet;
import jakarta.servlet.http.HttpServlet;
import jakarta.servlet.http.HttpServletRequest;
import jakarta.servlet.http.HttpServletResponse;

import java.io.IOException;

@WebServlet(urlPatterns = {WebRoutes.AUTH_LOGOUT, WebRoutes.LOGOUT})
public class LogoutServlet extends HttpServlet {
	@Override
	protected void doGet(HttpServletRequest req, HttpServletResponse resp) throws IOException {
		if (req.getSession(false) != null) {
			req.getSession(false).invalidate();
		}
		resp.sendRedirect(req.getContextPath() + WebRoutes.LOGIN);
	}
}

