package br.com.fiap.vinheriaagnello.model;

import java.io.Serializable;
import java.util.Objects;

public class Cliente implements Serializable {
	private final long id;
	private final String nome;
	private final String email;

	public Cliente(long id, String nome, String email) {
		this.id = id;
		this.nome = Objects.requireNonNull(nome);
		this.email = Objects.requireNonNull(email);
	}

	public long getId() {
		return id;
	}

	public String getNome() {
		return nome;
	}

	public String getEmail() {
		return email;
	}
}

