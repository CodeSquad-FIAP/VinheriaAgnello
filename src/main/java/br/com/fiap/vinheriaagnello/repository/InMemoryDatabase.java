package br.com.fiap.vinheriaagnello.repository;

import br.com.fiap.vinheriaagnello.model.Cliente;
import br.com.fiap.vinheriaagnello.model.Vinho;

import java.math.BigDecimal;
import java.util.ArrayList;
import java.util.Collections;
import java.util.List;
import java.util.Optional;
import java.util.concurrent.CopyOnWriteArrayList;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.atomic.AtomicLong;

public final class InMemoryDatabase {
	public static final String SESSION_USER_ATTR = "user";

	private static final AtomicLong CLIENTE_SEQ = new AtomicLong(1);
	private static final AtomicLong VINHO_SEQ = new AtomicLong(1);

	private static final CopyOnWriteArrayList<Cliente> CLIENTES = new CopyOnWriteArrayList<>();
	private static final CopyOnWriteArrayList<Vinho> VINHOS = new CopyOnWriteArrayList<>();

	private static final ConcurrentHashMap<String, String> CREDENTIALS = new ConcurrentHashMap<>();

	static {
		seed();
	}

	private InMemoryDatabase() {
	}

	private static void seed() {
		Cliente admin = createCliente("Admin", "admin@agnello.com");
		CREDENTIALS.put(admin.getEmail(), "admin");

		createVinho("Brunello di Montalcino", "Tinto", "Toscana, IT", new BigDecimal("429.00"));
		createVinho("Chardonnay Reserva", "Branco", "Mendoza, AR", new BigDecimal("145.00"));
		createVinho("Côtes de Provence", "Rosé", "Provence, FR", new BigDecimal("180.00"));
	}

	public static Cliente createCliente(String nome, String email) {
		Cliente c = new Cliente(CLIENTE_SEQ.getAndIncrement(), nome, email);
		CLIENTES.add(c);
		return c;
	}

	public static List<Cliente> listClientes() {
		return Collections.unmodifiableList(new ArrayList<>(CLIENTES));
	}

	public static Optional<Cliente> findClienteByEmail(String email) {
		return CLIENTES.stream().filter(c -> c.getEmail().equalsIgnoreCase(email)).findFirst();
	}

	public static void upsertCredential(String email, String senha) {
		CREDENTIALS.put(email, senha);
	}

	public static boolean authenticate(String email, String senha) {
		if (email == null || senha == null) return false;
		String expected = CREDENTIALS.get(email);
		return expected != null && expected.equals(senha);
	}

	public static Vinho createVinho(String nome, String tipo, String origem, BigDecimal preco) {
		Vinho v = new Vinho(VINHO_SEQ.getAndIncrement(), nome, tipo, origem, preco);
		VINHOS.add(v);
		return v;
	}

	public static List<Vinho> listVinhos() {
		return Collections.unmodifiableList(new ArrayList<>(VINHOS));
	}
}

