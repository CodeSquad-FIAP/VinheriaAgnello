package com.example.myapplication.data.repository

import com.example.myapplication.data.local.dao.ProdutoDao
import com.example.myapplication.data.local.entity.Produto
import kotlinx.coroutines.flow.Flow

class ProdutoRepository(private val dao: ProdutoDao) {
    fun getAllProdutos(): Flow<List<Produto>> = dao.getAllProdutos()
    suspend fun inserir(produto: Produto): Long = dao.insert(produto)
    suspend fun atualizar(produto: Produto) = dao.update(produto)
    suspend fun deletar(produto: Produto) = dao.delete(produto)
}
