package br.com.fiap.vinheriaagnello.model;

import java.io.Serializable;
import java.math.BigDecimal;
import java.util.Objects;

public class Vinho implements Serializable {
	private final long id;
	private final String nome;
	private final String tipo;
	private final String origem;
	private final BigDecimal preco;

	public Vinho(long id, String nome, String tipo, String origem, BigDecimal preco) {
		this.id = id;
		this.nome = Objects.requireNonNull(nome);
		this.tipo = Objects.requireNonNull(tipo);
		this.origem = Objects.requireNonNull(origem);
		this.preco = Objects.requireNonNull(preco);
	}

	public long getId() {
		return id;
	}

	public String getNome() {
		return nome;
	}

	public String getTipo() {
		return tipo;
	}

	public String getOrigem() {
		return origem;
	}

	public BigDecimal getPreco() {
		return preco;
	}
}

